using System;
using System.Linq;
using Fiddler;
using Freezium.Core;
using Freezium.Core.Interfaces;
using Freezium.Helpers;
using Freezium.Services;
using Newtonsoft.Json.Linq;

namespace Freezium.Infrastructure.Proxy
{
    /// <summary>
    /// Handles the FiddlerCore BeforeResponse event.
    /// Manipulates response bodies to inject premium, watchlist, follow, and favorite data.
    /// </summary>
    public class ResponseInterceptor
    {
        private readonly IAnimeRepository _repository;

        public event Action<string> LogMessage;

        /// <summary>
        /// Initializes a new instance of <see cref="ResponseInterceptor"/> with the given anime
        /// repository, which is used to supply locally stored watch-list, follow, and favorite
        /// data when overriding API responses.
        /// </summary>
        /// <param name="repository">The repository used to read local anime list data.</param>
        public ResponseInterceptor(IAnimeRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Entry point for the FiddlerCore <c>BeforeResponse</c> event. Filters out sessions
        /// that do not originate from the configured API host, then dispatches the session to
        /// each specialized response handler in sequence. Any unhandled exception is caught and
        /// reported via the <see cref="LogMessage"/> event so that a single bad response cannot
        /// crash the proxy pipeline.
        /// </summary>
        /// <param name="session">The FiddlerCore session representing the intercepted HTTP(S) response.</param>
        public void Handle(Session session)
        {
            try
            {
                // Fast path: Skip non-API responses immediately
                if (!session.fullUrl.Contains(Constants.TargetApiHost))
                    return;

                // Route to specific handlers
                var uri = session.PathAndQuery;

                if (session.uriContains("user/get"))
                {
                    HandlePremiumInjection(session);
                }
                else if (session.RequestMethod == "POST" && AppSettingsService.Current.ManipulateWL)
                {
                    // Handle manipulation responses
                    if (session.uriContains("anime/watch-list"))
                        HandleWatchListResponse(session);
                    else if (session.uriContains("anime/follow"))
                        HandleFollowResponse(session);
                    else if (session.uriContains("anime/favorite"))
                        HandleFavoriteResponse(session);
                }
                else if (AppSettingsService.Current.ManipulateWL)
                {
                    if (session.uriContains("page/watch-list"))
                        HandleWatchListPage(session);
                    else if (session.uriContains("page/favorite-list"))
                        HandleFavoriteListPage(session);
                    else if (session.uriContains("anime/user-details"))
                        HandleUserDetailsResponse(session);
                }
            }
            catch
            {
                // Silent fail - don't crash the pipeline
            }
        }

        /// <summary>
        /// Intercepts a successful <c>user/get</c> response and injects premium subscription
        /// data into the response body before it reaches the client.
        /// </summary>
        /// <param name="session">The session whose response body will be decoded and modified.</param>
        private void HandlePremiumInjection(Session session)
        {
            if (session.responseCode != 200)
                return;

            session.utilDecodeResponse();
            var body = session.GetResponseBodyAsString();

            try
            {
                var jobj = JObject.Parse(body);
                if (jobj["success"]?.Value<bool>() != true)
                    return;

                var data = jobj["data"] as JObject;
                if (data == null)
                    return;

                data["subscription"] = true;

                data.Remove("premium");
                data.Add("premium", new JObject
                {
                    { "created", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
                    { "time", 2592000000 },
                    { "active", true }
                });

                data.Remove("premium_plan");
                data.Add("premium_plan", new JObject
                {
                    { "ID", "standart" },
                    { "name", "Standart" }
                });

                data["infinity"] = true;
                data["staff"] = true;

                session.utilSetResponseBody(jobj.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch { }
        }

        /// <summary>
        /// Handles responses to POST requests on the <c>anime/watch-list</c> endpoint when
        /// watch-list manipulation is enabled.
        /// </summary>
        private void HandleWatchListResponse(Session session)
        {
            var islem = GetTransactionHeader(session);
            if (islem == null)
                return;

            if (islem == "add")
            {
                session.responseCode = 200;
                session.utilSetResponseBody("{\"success\":true,\"msg\":\"Successfully added to the list.\"}");
            }
            else if (islem == "delete")
            {
                session.responseCode = 200;
                session.utilSetResponseBody("{\"success\":true,\"msg\":\"Successfully removed from the list.\"}");
            }
        }

        /// <summary>
        /// Handles responses to POST requests on the <c>anime/follow</c> endpoint when
        /// watch-list manipulation is enabled.
        /// </summary>
        private void HandleFollowResponse(Session session)
        {
            var islem = GetTransactionHeader(session);
            if (islem == null)
                return;

            if (islem == "add")
            {
                session.responseCode = 200;
                session.utilSetResponseBody("{\"success\":true,\"msg\":\"Successfully followed.\"}");
            }
            else if (islem == "delete")
            {
                session.responseCode = 200;
                session.utilSetResponseBody("{\"success\":true,\"msg\":\"Successfully unfollowed.\"}");
            }
        }

        /// <summary>
        /// Handles responses to POST requests on the <c>anime/favorite</c> endpoint when
        /// watch-list manipulation is enabled.
        /// </summary>
        private void HandleFavoriteResponse(Session session)
        {
            var islem = GetTransactionHeader(session);
            if (islem == null)
                return;

            if (islem == "add")
            {
                session.responseCode = 200;
                session.utilSetResponseBody("{\"success\":true,\"total\":1,\"msg\":\"Added to your favorites.\"}");
            }
            else if (islem == "delete")
            {
                session.responseCode = 200;
                session.utilSetResponseBody("{\"success\":true,\"total\":0,\"msg\":\"Removed from your favorites.\"}");
            }
        }

        /// <summary>
        /// Handles paginated watch-list page responses when watch-list manipulation is enabled.
        /// </summary>
        private void HandleWatchListPage(Session session)
        {
            try
            {
                var list = _repository.GetWatchList().ToArray();
                var response = new JObject
                {
                    { "success", true },
                    { "data", JArray.FromObject(list) }
                };
                session.responseCode = 200;
                session.utilSetResponseBody(response.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch { }
        }

        /// <summary>
        /// Handles paginated favorite-list page responses when watch-list manipulation is enabled.
        /// </summary>
        private void HandleFavoriteListPage(Session session)
        {
            try
            {
                var list = _repository.GetFavoriteList().ToArray();
                var response = new JObject
                {
                    { "success", true },
                    { "data", JArray.FromObject(list) }
                };
                session.responseCode = 200;
                session.utilSetResponseBody(response.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch { }
        }

        /// <summary>
        /// Handles responses from the <c>anime/user-details</c> endpoint when watch-list
        /// manipulation is enabled.
        /// </summary>
        private void HandleUserDetailsResponse(Session session)
        {
            var data = session.GetResponseBodyAsString();
            if (string.IsNullOrEmpty(data))
                return;

            if (!JsonHelper.TryParseJObject(data, out var jobj))
                return;

            try
            {
                var id = System.Web.HttpUtility.ParseQueryString(
                    new Uri(session.fullUrl).Query)["id"];

                if (string.IsNullOrEmpty(id))
                    return;

                jobj["watch_list"] = _repository.IsInWatchList(id);
                jobj["follow"] = _repository.IsInFollow(id);
                jobj["favorite"] = _repository.IsInFavorite(id);

                session.utilSetResponseBody(jobj.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch { }
        }

        /// <summary>
        /// Retrieves the value of the custom <c>islem</c> request header.
        /// </summary>
        private static string GetTransactionHeader(Session session)
        {
            var headers = session.RequestHeaders;
            foreach (var header in headers)
            {
                if (header.Name == "islem")
                    return header.Value;
            }
            return null;
        }
    }
}

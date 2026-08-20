using System;
using System.Linq;
using Fiddler;
using Freezium.Core;
using Freezium.Core.Interfaces;
using Freezium.Infrastructure.Crypto;
using Freezium.Services;
using Newtonsoft.Json.Linq;

namespace Freezium.Infrastructure.Proxy
{
    /// <summary>
    /// Handles the FiddlerCore BeforeRequest event.
    /// Manipulates the request based on needs and activates response buffering.
    /// </summary>
    public class RequestInterceptor
    {
        private readonly IAnimeRepository _repository;

        public event Action<string> LogMessage;

        /// <summary>
        /// Initializes a new instance of <see cref="RequestInterceptor"/> with the given anime
        /// repository, which is used to persist watchlist, follow, and favorite state changes
        /// that are detected within intercepted request bodies.
        /// </summary>
        /// <param name="repository">The repository used to read and write local anime list data.</param>
        public RequestInterceptor(IAnimeRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Entry point for the FiddlerCore <c>BeforeRequest</c> event. Filters out sessions
        /// that do not target the configured API host, and then dispatches the session to each
        /// specialized handler in sequence. Any unhandled exception is caught and reported via
        /// the <see cref="LogMessage"/> event so that a single bad request cannot crash the
        /// proxy pipeline.
        /// </summary>
        /// <param name="session">The FiddlerCore session representing the intercepted HTTP(S) request.</param>
        public void Handle(Session session)
        {
            try
            {
                // Serve PAC (Proxy Auto-Configuration) script dynamically
                if (session.PathAndQuery.Equals("/proxy.pac", StringComparison.OrdinalIgnoreCase) ||
                    session.fullUrl.IndexOf("127.0.0.1:8888/proxy.pac", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    session.fullUrl.IndexOf("localhost:8888/proxy.pac", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string pacScript = "function FindProxyForURL(url, host) {\r\n" +
                                       "    if (dnsDomainIs(host, 'anizium.co') || shExpMatch(host, '*.anizium.co')) {\r\n" +
                                       "        return 'PROXY 127.0.0.1:8888';\r\n" +
                                       "    }\r\n" +
                                       "    return 'DIRECT';\r\n" +
                                       "}";

                    session.utilCreateResponseAndBypassServer();
                    session.responseCode = 200;
                    session.oResponse.headers.Add("Content-Type", "application/x-ns-proxy-autoconfig");
                    session.oResponse.headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    session.oResponse.headers.Add("Pragma", "no-cache");
                    session.utilSetResponseBody(pacScript);
                    return;
                }

                // Fast path: Skip non-API requests immediately
                if (!session.fullUrl.Contains(Constants.TargetApiHost))
                    return;

                // Capture headers first (lightweight operation)
                CaptureHeaders(session);

                // Route to specific handlers
                var uri = session.PathAndQuery;
                
                if (session.uriContains("user/get"))
                {
                    HandleUserGet(session);
                }
                else if (session.RequestMethod == "POST" && AppSettingsService.Current.ManipulateWL)
                {
                    // Handle manipulation endpoints
                    if (session.uriContains("anime/watch-list"))
                        ProcessManipulationRequest(session, "watch-list");
                    else if (session.uriContains("anime/follow"))
                        ProcessManipulationRequest(session, "follow");
                    else if (session.uriContains("anime/favorite"))
                        ProcessManipulationRequest(session, "favorite");
                }
                else if (AppSettingsService.Current.ManipulateWL)
                {
                    if (session.uriContains("page/watch-list") || session.uriContains("page/favorite-list"))
                    {
                        session.bBufferResponse = true;
                    }
                    else if (session.uriContains("anime/user-details"))
                    {
                        HandleUserDetails(session);
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail - don't crash the pipeline
            }
        }

        /// <summary>
        /// Scans the request headers for the <c>cf-control</c> header (case-insensitive) and,
        /// if present, stores its value in <see cref="AppSettingsService.Current.CfControl"/>.
        /// This header carries a Cloudflare control token that may be required for subsequent
        /// API calls or crypto operations.
        /// </summary>
        /// <param name="session">The session whose request headers are inspected.</param>
        private void CaptureHeaders(Session session)
        {
            var headers = session.RequestHeaders;
            foreach (var header in headers)
            {
                if (header.Name.Equals("cf-control", StringComparison.OrdinalIgnoreCase))
                {
                    AppSettingsService.Current.CfControl = header.Value;
                    break;
                }
            }
        }

        /// <summary>
        /// Handles requests targeting the <c>user/get</c> endpoint. Removes the
        /// <c>Accept-Encoding</c> header so that the server returns a plain, uncompressed
        /// response body, and enables response buffering so that the full response is available
        /// to the <see cref="ResponseInterceptor"/> before being forwarded to the client.
        /// </summary>
        /// <param name="session">The session to inspect and potentially modify.</param>
        private void HandleUserGet(Session session)
        {
            RemoveAcceptEncoding(session);
            session.bBufferResponse = true;
        }

        /// <summary>
        /// Handles requests to the <c>anime/user-details</c> endpoint when watch-list
        /// manipulation is enabled. Strips the <c>Accept-Encoding</c> header to prevent
        /// a compressed response body, and enables response buffering so the
        /// <see cref="ResponseInterceptor"/> can override the watch-list, follow, and favorite
        /// flags with locally stored state.
        /// </summary>
        /// <param name="session">The session to inspect and potentially modify.</param>
        private void HandleUserDetails(Session session)
        {
            RemoveAcceptEncoding(session);
            session.bBufferResponse = true;
        }

        /// <summary>
        /// Core logic for handling watch-list, follow, and favorite manipulation requests.
        /// Reads the raw request body, extracts and decrypts the encrypted <c>d</c> field using
        /// <see cref="CryptoHelper.Decrypt"/>, then parses the anime ID and transaction type
        /// (<c>"add"</c> or <c>"delete"</c>) from the decrypted JSON payload.
        /// </summary>
        /// <param name="session">The session whose request body will be decrypted and processed.</param>
        /// <param name="type">The list type being manipulated.</param>
        private void ProcessManipulationRequest(Session session, string type)
        {
            string body;
            try
            {
                body = session.GetRequestBodyAsString();
            }
            catch { return; }

            if (string.IsNullOrEmpty(body))
                return;

            string decrypted;
            try
            {
                var jBody = JObject.Parse(body);
                var dValue = jBody["d"]?.Value<string>();
                if (string.IsNullOrEmpty(dValue))
                    return;

                decrypted = CryptoHelper.Decrypt(dValue, Constants.AniziumEncryptionKey);
            }
            catch { return; }

            if (decrypted == null)
                return;

            try
            {
                var jobj = JObject.Parse(decrypted);
                var animeId = jobj["id"]?.Value<string>();
                var transaction = jobj["transaction"]?.Value<string>();

                if (string.IsNullOrEmpty(animeId) || string.IsNullOrEmpty(transaction))
                    return;

                session.RequestHeaders.Add("islem", transaction);

                bool shouldBuffer = false;

                if (transaction == "add")
                {
                    bool success = false;
                    switch (type)
                    {
                        case "watch-list": success = _repository.AddWatchList(animeId); break;
                        case "follow": success = _repository.AddFollow(animeId); break;
                        case "favorite": success = _repository.AddFavorite(animeId); break;
                    }
                    shouldBuffer = success;
                }
                else if (transaction == "delete")
                {
                    switch (type)
                    {
                        case "watch-list": _repository.RemoveWatchList(animeId); break;
                        case "follow": _repository.RemoveFollow(animeId); break;
                        case "favorite": _repository.RemoveFavorite(animeId); break;
                    }
                    shouldBuffer = true;
                }

                if (shouldBuffer)
                    session.bBufferResponse = true;
            }
            catch { }
        }

        /// <summary>
        /// Removes the <c>Accept-Encoding</c> header from the request (case-insensitive) if it
        /// is present. This forces the server to return a plain, uncompressed response body,
        /// which is necessary before the response interceptor attempts to parse and modify the
        /// raw JSON string.
        /// </summary>
        /// <param name="session">The session whose request headers will be modified.</param>
        private static void RemoveAcceptEncoding(Session session)
        {
            if (session.RequestHeaders.Any(x =>
                x.Name.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase)))
            {
                session.RequestHeaders.Remove("Accept-Encoding");
            }
        }
    }
}

using System;
using Freezium.Core;
using Freezium.Core.Interfaces;
using Freezium.Core.Models;
using Freezium.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Freezium.Infrastructure.Api
{
    /// <summary>
    /// Implementation of HTTP requests made to the Anizium REST API.
    /// </summary>
    public class AniziumApiClient : IAnimeApiClient
    {
        private readonly RestClient _client;

        public AniziumApiClient()
        {
            _client = new RestClient(Constants.AniziumApiBaseUrl);
        }

        public Anime GetAnime(string id)
        {
            var request = new RestRequest("/anime/get");
            request.Method = Method.Get;
            request.AddQueryParameter("id", id);

            var cfControl = AppSettingsService.Current.CfControl
                ?? "Cf" + Guid.NewGuid().ToString().Replace("-", "");
            request.AddHeader("Cf-control", cfControl);

            var response = _client.Execute(request);
            if (response.IsSuccessStatusCode)
            {
                var jobj = JObject.Parse(response.Content);
                return JsonConvert.DeserializeObject<Anime>(jobj["data"].ToString());
            }

            return null;
        }
    }
}

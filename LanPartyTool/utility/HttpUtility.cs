using System;
using System.Net.Http;
using log4net;
using LanPartyTool.agent;
using Newtonsoft.Json;
using RestSharp;

namespace LanPartyTool.utility
{
    public class HttpUtility
    {
        private static readonly int HTTP_CLIENT_TIMEOUT = 3000;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpUtility));
        private static readonly HttpClient HttpClient = new HttpClient();

        static HttpUtility()
        {
            HttpClient.Timeout = TimeSpan.FromMilliseconds(HTTP_CLIENT_TIMEOUT);
        }

        public static JsonResponse DoPost(string url, dynamic requestBody = null)
        {
            Logger.Debug("Doing POST HTTP Request");

            if (requestBody == null) requestBody = new { };

            var request = new RestRequest(Method.POST);
            request.AddJsonBody(requestBody);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");

            var client = new RestClient(url) {Timeout = HTTP_CLIENT_TIMEOUT };
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                Logger.Debug("Unable to complete HTTP request");
                return null;
            }

            var responseString = response.Content;

            var responseJson = JsonConvert.DeserializeObject<JsonResponse>(responseString);
            return responseJson;
        }
    }
}
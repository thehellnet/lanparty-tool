using System;
using System.Net.Http;
using log4net;
using LanPartyTool.agent;
using Newtonsoft.Json;
using RestSharp;

namespace LanPartyTool.utility
{
    internal class HttpUtility
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpUtility));
        private static readonly HttpClient HttpClient = new HttpClient();

        static HttpUtility()
        {
            HttpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        public static JsonResponse DoPost(string url, dynamic requestBody = null)
        {
            Logger.Debug("Doing POST HTTP Request");

            if (requestBody == null) requestBody = new { };

            var requestData = new
            {
                jsonrpc = "2.0",
                method = "action",
                @params = requestBody,
                id = 0
            };

            var request = new RestRequest(Method.POST);
            request.AddJsonBody(requestData);
            request.AddHeader("Content-Type", "application/json");

            var client = new RestClient(url) {Timeout = 2000};
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                Logger.Debug("Unable to complete HTTP request");
                return null;
            }

            var responseString = response.Content;

            var responseJson = JsonConvert.DeserializeObject<JsonRpcResponse>(responseString);
            return responseJson.Result;
        }
    }
}
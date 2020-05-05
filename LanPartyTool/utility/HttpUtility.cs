using System;
using System.Net;
using System.Net.Http;
using System.Windows;
using LanPartyTool.agent;
using log4net;
using Newtonsoft.Json;
using RestSharp;

namespace LanPartyTool.utility
{
    public static class HttpUtility
    {
        private const int HttpClientTimeout = 3000;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpUtility));
        private static readonly HttpClient HttpClient = new HttpClient();

        static HttpUtility()
        {
            HttpClient.Timeout = TimeSpan.FromMilliseconds(HttpClientTimeout);
        }

        public static dynamic DoPost(string url, dynamic requestBody = null)
        {
            Logger.Debug("Doing POST HTTP Request");

            if (requestBody == null) requestBody = new { };

            var request = new RestRequest(Method.POST);
            request.AddJsonBody(requestBody);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");

            RestClient client;

            try
            {
                client = new RestClient(url);
            }
            catch (UriFormatException e)
            {
                Logger.Error($"Error executing http call: {e.Message}");
                return null;
            }

            client.Timeout = HttpClientTimeout;
            client.UserAgent = Application.ResourceAssembly.FullName;

            var response = client.Execute(request);


            if (response.StatusCode != HttpStatusCode.OK)
            {
                Logger.Debug($"Unable to complete HTTP request. StatusCode {response.StatusCode}");
                return null;
            }

            var responseJson = JsonConvert.DeserializeObject(response.Content);
            return responseJson;
        }
    }
}
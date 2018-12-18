﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using log4net;
using LanPartyTool.agent;
using Newtonsoft.Json;

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

        public static JsonResponse doPost(string url, dynamic requestBody)
        {
            Logger.Debug("Doing POST HTTP Request");

            var requestData = new Dictionary<string, dynamic>
            {
                {"jsonrpc", "2.0"},
                {"method", "action"},
                {"params", requestBody},
                {"id", 0}
            };

            var jsonString = JsonConvert.SerializeObject(requestData, Formatting.None);
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage;
            try
            {
                responseMessage = HttpClient.PostAsync(url, stringContent).Result;
            }
            catch (AggregateException)
            {
                Logger.Debug("Unable to complete HTTP request");
                return null;
            }

            if (!responseMessage.IsSuccessStatusCode) return null;

            string responseString;
            try
            {
                responseString = responseMessage.Content.ReadAsStringAsync().Result;
            }
            catch (AggregateException)
            {
                Logger.Debug("Unable to read HTTP response");
                return null;
            }

            var responseJson = JsonConvert.DeserializeObject<JsonRpcResponse>(responseString);
            return responseJson.Result;
        }
    }
}
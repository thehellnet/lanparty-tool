using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using log4net;
using Newtonsoft.Json.Linq;

namespace LanPartyTool.utility
{
    internal class ServerUtility
    {
        private const string ServerProtocol = "http";
        private const int ServerPort = 8080;
        private const string ServerEndPoint = "/lanparty_manager/api/public/v1/tool";

        private const string PingEndPoint = "/ping";
        private const string WelcomeEndPoint = "/welcome";
        private const string GetCfgEndPoint = "/getCfg";
        private const string SaveCfgEndPoint = "/saveCfg";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerUtility));
        public static string DefaultServerUrl()
        {
            foreach (var serverUrl in PossibleServerUrls())
            {
                var result = HttpUtility.DoPost($"{serverUrl}{PingEndPoint}");
                if (result == null)
                {
                    Logger.Debug($"\"{serverUrl}\" is not valid");
                    continue;
                }

                Logger.Info($"Server URL: {serverUrl}");
                return serverUrl;
            }

            return "";
        }

        public static void Welcome(string serverUrl)
        {
            var result = HttpUtility.DoPost($"{serverUrl}{WelcomeEndPoint}");
            if (result == null) Logger.Error($"\"{serverUrl}\" is not valid");
        }

        public static List<string> GetCfg(string serverUrl, string barcode)
        {
            var result = HttpUtility.DoPost($"{serverUrl}{GetCfgEndPoint}", new {barcode});
            if (result == null)
            {
                Logger.Debug($"Null response... \"{serverUrl}\" is not valid");
                return null;
            }

            var rawData = result as JArray;
            var cfgLines = rawData?.ToObject<List<string>>();
            return cfgLines;
        }

        public static bool SaveCfg(string serverUrl, string barcode, List<string> cfgLines)
        {
            var result = HttpUtility.DoPost($"{serverUrl}{SaveCfgEndPoint}", new {barcode, cfgLines});
            if (result == null)
            {
                Logger.Debug($"Null response... \"{serverUrl}\" is not valid");
                return false;
            }

            return true;
        }

        private static IEnumerable<string> PossibleServerUrls()
        {
            var serverUrls = new List<string>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;

                Logger.Debug($"Parsing interface \"{networkInterface.Name}\"");

                foreach (var addressInformation in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    var address = addressInformation.Address;
                    var networkMask = addressInformation.IPv4Mask;
                    var ipNetwork = IPNetwork.Parse(address, networkMask);
                    var ipAddress = ipNetwork.LastUsable;

                    if (ipAddress.IsIPv6LinkLocal
                        || ipAddress.IsIPv6Multicast
                        || ipAddress.IsIPv6SiteLocal
                        || ipAddress.IsIPv6Teredo)
                        continue;

                    var serverAddress = ipAddress.AddressFamily == AddressFamily.InterNetworkV6
                        ? $"[{ipAddress}]"
                        : ipAddress.ToString();

                    var serverUrl = PrepareServerUrl(serverAddress);
                    serverUrls.Add(serverUrl);
                    Logger.Debug($"New possible URL: {serverUrl}");
                }
            }

            serverUrls.Add(PrepareServerUrl("127.0.0.1"));

            return serverUrls.ToArray();
        }

        private static string PrepareServerUrl(string serverAddress)
        {
            return $"{ServerProtocol}://{serverAddress}:{ServerPort}{ServerEndPoint}";
        }
    }
}
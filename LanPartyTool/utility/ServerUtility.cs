using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using log4net;
using LanPartyTool.agent;

namespace LanPartyTool.utility
{
    internal class ServerUtility
    {
        private const string ServerProtocol = "http";
        private const int ServerPort = 8069;
        private const string ServerEndPoint = "/ap1/v1/tool";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerUtility));

        public static string DefaultServerAddress()
        {
            foreach (var serverUrl in possibleServerUrls())
            {
                var result = HttpUtility.doPost($"{serverUrl}/welcome", new {test = "Test"});
                if (result == null)
                {
                    Logger.Debug($"\"{serverUrl}\" is not valid");
                    continue;
                }

                Logger.Info(result);
            }

            return "";
        }

        private static IEnumerable<string> possibleServerUrls()
        {
            var serverUrls = new List<string>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                {
                    continue;
                }

                Logger.Debug($"Parsing interface \"{networkInterface.Name}\"");

                foreach (var addressInformation in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    var address = addressInformation.Address;
                    var netmask = addressInformation.IPv4Mask;
                    var ipNetwork = IPNetwork.Parse(address, netmask);
                    var ipAddress = ipNetwork.LastUsable;

                    if (ipAddress.IsIPv6LinkLocal
                        || ipAddress.IsIPv6Multicast
                        || ipAddress.IsIPv6SiteLocal
                        || ipAddress.IsIPv6Teredo)
                    {
                        continue;
                    }

                    var serverAddress = ipAddress.AddressFamily == AddressFamily.InterNetworkV6
                        ? $"[{ipAddress}]"
                        : ipAddress.ToString();

                    var serverUrl = $"{ServerProtocol}://{serverAddress}:{ServerPort}{ServerEndPoint}";
                    serverUrls.Add(serverUrl);
                    Logger.Debug($"New possible URL: {serverUrl}");
                }
            }

            return serverUrls.ToArray();
        }
    }
}
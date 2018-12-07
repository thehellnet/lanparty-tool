using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using LanPartyTool.config;
using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    class Agent
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Agent));

        private readonly Config _config = Config.GetInstance();

        private readonly ServerSocket _serverSocket = new ServerSocket();

        private Timer _timer;

        public void Start()
        {
            Logger.Info("Agent start");

            _serverSocket.Start();
            _serverSocket.OnConnectionAccepted += NewConnectionHandler;

            _timer = new Timer((obj) =>
            {
                Logger.Info("Setting values");
                _config.GameExe = "Game EXE";
                _config.CfgFile = "CFG Path";
            }, null, 3000, Timeout.Infinite);
        }

        public void Stop()
        {
            Logger.Info("Agent stop");

            _serverSocket.OnConnectionAccepted -= NewConnectionHandler;
            _serverSocket.Stop();
        }

        private void NewConnectionHandler(Socket socket)
        {
            Logger.Debug("Starting client loop");

            while (socket.Connected)
            {
                var memoryStream = new MemoryStream();

                do
                {
                    var buff = new byte[1024];
                    int readSize;

                    try
                    {
                        readSize = socket.Receive(buff);
                    }
                    catch (SocketException e)
                    {
                        Logger.Error(e.Message);
                        break;
                    }

                    if (readSize == 0)
                    {
                        break;
                    }

                    memoryStream.Write(buff, (int) memoryStream.Length, readSize);
                } while (socket.Available > 0);

                if (memoryStream.Length == 0)
                {
                    break;
                }

                var rawData = memoryStream.ToArray();
                Logger.Debug($"Received new command of {rawData.Length} bytes");

                var payload = Encoding.UTF8.GetString(rawData);
                var request = JsonConvert.DeserializeObject<dynamic>(payload);
                var response = ParsePayload(request);
                var ret = JsonConvert.SerializeObject(response);

                Logger.Debug($"Response: {ret}");

                var retBytes = Encoding.ASCII.GetBytes(ret);
                socket.Send(retBytes);
            }

            Logger.Debug("Closing client loop");
        }

        dynamic ParsePayload(dynamic request)
        {
            Logger.Debug("Parsing payload");
            Logger.Debug($"Action: {request.action}");

            if (request.action == "ping")
            {
                return JsonResponse.GetSuccessInstance("pong");
            }

            if (request.action == "cfg")
            {
                return JsonResponse.GetErrorInstance("Not implemented yet");
            }

            return JsonResponse.GetErrorInstance("Action not recognized");
        }
    }
}
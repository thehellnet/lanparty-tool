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
using LanPartyTool.utility;
using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    internal class Agent
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Agent));

        private readonly Config _config = Config.GetInstance();
        private readonly ServerSocket _serverSocket = new ServerSocket();

        public void Start()
        {
            new Thread(StartAgent).Start();
        }

        public void Stop()
        {
            new Thread(StopAgent).Start();
        }

        private void StartAgent()
        {
            Logger.Info("Agent start");

            if (!CheckConfig())
            {
                return;
            }

            CheckEntryPoint();

            _serverSocket.Start();
            _serverSocket.OnConnectionAccepted += NewConnectionHandler;

            Logger.Debug("Agent start sequence completed");
        }

        private void StopAgent()
        {
            Logger.Info("Agent stop");

            _serverSocket.OnConnectionAccepted -= NewConnectionHandler;
            _serverSocket.Stop();

            Logger.Debug("Agent stop sequence completed");
        }

        private bool CheckConfig()
        {
            Logger.Info("Checking configuration");

            if (_config.GameExe == "")
            {
                Logger.Debug("Getting default game exe path");
                _config.GameExe = GameUtility.DefaultGameExe();

                if (_config.GameExe == "")
                {
                    Logger.Error("Invalid game exe path");
                    return false;
                }
            }

            Logger.Debug("Game exe path correct");

            if (_config.ToolCfg == "")
            {
                Logger.Debug("Getting default tool cfg path");
                _config.ToolCfg = GameUtility.DefaultToolCfg();

                if (_config.ToolCfg == "")
                {
                    Logger.Error("Invalid tool cfg path");
                    return false;
                }
            }

            Logger.Debug("Tool cfg path correct");

            if (_config.ProfileCfg == "")
            {
                Logger.Debug("Getting default profile cfg path");
                _config.ProfileCfg = GameUtility.DefaultProfileCfg();

                if (_config.ProfileCfg == "")
                {
                    Logger.Error("Invalid profile cfg path");
                    return false;
                }
            }

            Logger.Debug("Profile cfg path correct");

            return true;
        }

        private void CheckEntryPoint()
        {
            Logger.Info("Checking entry point presence");
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
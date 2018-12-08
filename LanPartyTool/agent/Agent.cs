using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            InitConfig();

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

        private void InitConfig()
        {
            Logger.Info("Initializing configuration");

            if (_config.GameExe == "")
            {
                Logger.Debug("Getting default game exe path");
                _config.GameExe = GameUtility.DefaultGameExe();
            }

            if (_config.ToolCfg == "")
            {
                Logger.Debug("Getting default tool cfg path");
                _config.ToolCfg = GameUtility.DefaultToolCfg();
            }

            if (_config.ProfileCfg == "")
            {
                Logger.Debug("Getting default profile cfg path");
                _config.ProfileCfg = GameUtility.DefaultProfileCfg();
            }
        }

        private bool CheckConfig()
        {
            Logger.Info("Checking configuration");

            if (_config.GameExe == "" || !File.Exists(_config.GameExe))
            {
                Logger.Error("Invalid game exe path");
                return false;
            }

            Logger.Debug("Game exe path correct");

            if (_config.ToolCfg == "")
            {
                Logger.Error("Invalid tool cfg path");
                return false;
            }

            var toolCfgDirPath = Path.GetDirectoryName(_config.ToolCfg);
            if (toolCfgDirPath == null)
            {
                Logger.Error("Unable to compute tool cfg directory path");
                return false;
            }

            if (!Directory.Exists(toolCfgDirPath))
            {
                Directory.CreateDirectory(toolCfgDirPath);
            }

            if (!File.Exists(_config.ToolCfg))
            {
                File.Create(_config.ToolCfg).Dispose();
            }

            if (!File.Exists(_config.ToolCfg))
            {
                Logger.Error("Tool cfg file not found");
                return false;
            }

            Logger.Debug("Tool cfg path correct");

            if (_config.ProfileCfg == "" || !File.Exists(_config.ProfileCfg))
            {
                Logger.Error("Invalid profile cfg path");
                return false;
            }

            Logger.Debug("Profile cfg path correct");

            return true;
        }

        private void CheckEntryPoint()
        {
            Logger.Info("Checking entry point presence");

            var entrypointLines = new List<int>();

            var profileCfgPath = _config.ProfileCfg;
            var profileCfgRows = GameUtility.ReadCfg(profileCfgPath);

            for (var i = 0; i < profileCfgRows.Count; i++)
            {
                var row = profileCfgRows.ElementAt(i);
                //var items = GameUtility.splitCfgLine(row);
                if (row.StartsWith(@"bind ."))
                {
                    entrypointLines.Add(i);
                }
            }

            var entrypointRemove = false;
            var entrypointAdd = false;

            switch (entrypointLines.Count)
            {
                case 1:
                    Logger.Debug($"Entrypoint bind command found at line {entrypointLines[0] + 1}");
                    break;
                case 0:
                    Logger.Warn("Entrypoint bind command not found. Adding...");
                    entrypointAdd = true;
                    break;
                default:
                    Logger.Warn("Multiple entrypoint bind command found. Removing unuseful...");
                    entrypointRemove = true;
                    entrypointAdd = true;
                    break;
            }

            if (entrypointRemove)
            {
                profileCfgRows.RemoveAll(row => row.StartsWith(@"bind ."));
                GameUtility.WriteCfg(profileCfgPath, profileCfgRows);
                Logger.Debug("Entrypoint bind commands removed from list");
            }

            if (entrypointAdd)
            {
                profileCfgRows.Add($"bind . \"exec {Constants.ToolCfgName}\"");
                GameUtility.WriteCfg(profileCfgPath, profileCfgRows);
                Logger.Debug("Entrypoint bind command added in profile cfg");
            }
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

        private dynamic ParsePayload(dynamic request)
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using log4net;
using LanPartyTool.config;
using LanPartyTool.utility;
using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    internal class Agent
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Agent));

        private readonly Status _status = Status.GetInstance();
        private readonly Config _config = Config.GetInstance();

        private readonly ServerSocket _serverSocket = new ServerSocket();

        public Agent()
        {
            _serverSocket.OnNewStatus += NewSocketStatusHandler;
        }

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

        private bool CheckEntryPoint()
        {
            Logger.Info("Checking entry point presence");

            var entrypointLines = new List<int>();

            var profileCfgPath = _config.ProfileCfg;
            var profileCfgRows = GameUtility.ReadCfg(profileCfgPath);

            for (var i = 0; i < profileCfgRows.Count; i++)
            {
                var row = profileCfgRows.ElementAt(i);
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

            if (entrypointRemove || entrypointAdd)
            {
                var result = MessageBox.Show(
                    "Some changes are required in profile CFG.\n" +
                    "If you select NO, no changes will be applied.\n" +
                    "Do you want to continue?",
                    "LanPartyTool CFG editing",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);

                if (result == MessageBoxResult.No)
                {
                    return false;
                }
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

            return true;
        }

        private void NewSocketStatusHandler(ServerSocket.Status status)
        {
            _status.SocketStatus = status;
        }

        private void NewConnectionHandler(Socket socket)
        {
            new ClientSocket(socket).Start();
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using log4net;
using LanPartyTool.config;
using LanPartyTool.utility;

namespace LanPartyTool.agent
{
    internal class Agent
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Agent));

        private readonly Config _config = Config.GetInstance();
        private readonly SerialPortReader _serialPortReader = new SerialPortReader();
        private readonly ServerSocket _serverSocket = new ServerSocket();
        private readonly Status _status = Status.GetInstance();

        public Agent()
        {
            _serverSocket.OnNewStatus += NewSocketStatusHandler;
            _serialPortReader.OnNewStatus += NewSerialPortStatusHandler;
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

            if (!CheckConfig()) return;

            if (!CheckEntryPoint()) return;

            _serverSocket.OnConnectionAccepted += NewConnectionHandler;
            _serverSocket.Start();

            _serialPortReader.OnNewBarcode += NewBarcodeHandler;
            _serialPortReader.Start();

            Logger.Debug("Agent start sequence completed");
        }

        private void StopAgent()
        {
            Logger.Info("Agent stop");

            _serialPortReader.OnNewBarcode -= NewBarcodeHandler;
            _serialPortReader.Stop();

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

            if (_config.ServerUrl == "")
            {
                Logger.Debug("Getting default server url");
                _config.ServerUrl = ServerUtility.DefaultServerUrl();
            }

            if (_config.SerialPort == "")
            {
                Logger.Debug("Getting default serial port");
                var portNames = SerialPort.GetPortNames();
                var portName = portNames.Length > 0 ? portNames[0] : "";
                _config.SerialPort = portName;
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

            if (!Directory.Exists(toolCfgDirPath)) Directory.CreateDirectory(toolCfgDirPath);

            if (!File.Exists(_config.ToolCfg)) File.Create(_config.ToolCfg).Dispose();

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

            if (_config.ServerUrl == "")
            {
                Logger.Error("Invalid Server Address");
                return false;
            }

            Logger.Debug("Server Address correct correct");

            if (_config.SerialPort == "")
            {
                Logger.Error("Invalid Serial Port");
                return false;
            }

            Logger.Debug("Serial Port correct");

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
                if (row.StartsWith(@"bind .")) entrypointLines.Add(i);
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

                if (result == MessageBoxResult.No) return false;
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

        private void NewSerialPortStatusHandler(SerialPortReader.PortStatus status)
        {
            _status.SerialPortStatus = status;
        }

        private void NewConnectionHandler(Socket socket)
        {
            new ClientSocket(socket).Start();
        }

        public void NewBarcodeHandler(string barcode)
        {
            Logger.Info($"New barcode scan: {barcode}");

            _status.LastBarcode = barcode;

            PlayPing();

            var cfgLines = ServerUtility.GetCfg(_config.ServerUrl, barcode);
            if (cfgLines == null) return;

            ApplyNewCfg(cfgLines);
        }

        private void ApplyNewCfg(List<string> cfgLines)
        {
            if (cfgLines == null) return;

            Logger.Debug($"New CFG with {cfgLines.Count} lines");

            Logger.Debug("Updating tool CFG");
            GameUtility.WriteCfg(_config.ToolCfg, cfgLines);

            Thread.Sleep(1000);

            Logger.Debug("Triggering keyboard keypress");
            WindowsUtility.SendKeyDown(Constants.GameExeName);
        }

        private static void PlayPing()
        {
            //Console.Beep(1000, 500);

            new Thread(() =>
            {
                Logger.Debug("Playing ping sound");

                var player = new SoundPlayer {SoundLocation = @"sounds/ping.wav"};
                player.Load();
                player.PlaySync();
                player.Stop();
                player.Dispose();
            }).Start();
        }
    }
}
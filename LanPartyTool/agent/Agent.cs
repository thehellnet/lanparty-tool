using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using LanPartyTool.config;
using LanPartyTool.utility;
using log4net;

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

            if (!CheckEntryPoints()) return;

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

        private bool CheckEntryPoints()
        {
            Logger.Info("Checking entry point presence");

            var entrypointExecLines = new List<int>();
            var entrypointDumpLines = new List<int>();

            var profileCfgPath = _config.ProfileCfg;
            var profileCfgRows = GameUtility.ReadCfg(profileCfgPath);

            var execBindCmd = $"bind {Constants.CommandExecKey}";
            var dumpBindCmd = $"bind {Constants.CommandDumpKey}";

            for (var i = 0; i < profileCfgRows.Count; i++)
            {
                var row = profileCfgRows.ElementAt(i);
                if (row.StartsWith(execBindCmd))
                {
                    Logger.Debug($"Entrypoint bind command for exec found at line {i + 1}");
                    entrypointExecLines.Add(i);
                }

                if (row.StartsWith(dumpBindCmd))
                {
                    Logger.Debug($"Entrypoint bind command for dump found at line {i + 1}");
                    entrypointDumpLines.Add(i);
                }
            }

            if (entrypointExecLines.Count > 1) Logger.Warn("Multiple entrypoint bind command for exec found");
            if (entrypointDumpLines.Count > 1) Logger.Warn("Multiple entrypoint bind command for dump found");

            if (entrypointExecLines.Count == 0) Logger.Warn("Entrypoint bind command for exec not found");
            if (entrypointDumpLines.Count == 0) Logger.Warn("Entrypoint bind command for dump not found");

            var entrypointExecRemove = entrypointExecLines.Count > 1;
            var entrypointDumpRemove = entrypointDumpLines.Count > 1;
            var entrypointExecAdd = entrypointExecLines.Count == 0 || entrypointExecRemove;
            var entrypointDumpAdd = entrypointDumpLines.Count == 0 || entrypointDumpRemove;

            if (entrypointExecRemove || entrypointDumpRemove || entrypointExecAdd || entrypointDumpAdd)
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

            if (entrypointExecRemove)
            {
                profileCfgRows.RemoveAll(row => row.StartsWith(execBindCmd));
                GameUtility.WriteCfg(profileCfgPath, profileCfgRows);
                Logger.Debug("Entrypoint bind commands for exec removed from list");
            }

            if (entrypointExecAdd)
            {
                profileCfgRows.Add($"{execBindCmd} \"exec {Constants.ToolCfgExecName}\"");
                GameUtility.WriteCfg(profileCfgPath, profileCfgRows);
                Logger.Debug("Entrypoint bind command for exec added in profile cfg");
            }

            if (entrypointDumpRemove)
            {
                profileCfgRows.RemoveAll(row => row.StartsWith(dumpBindCmd));
                GameUtility.WriteCfg(profileCfgPath, profileCfgRows);
                Logger.Debug("Entrypoint bind commands for dump removed from list");
            }

            if (entrypointDumpAdd)
            {
                profileCfgRows.Add($"{dumpBindCmd} \"writeconfig {Constants.ToolCfgDumpName}\"");
                GameUtility.WriteCfg(profileCfgPath, profileCfgRows);
                Logger.Debug("Entrypoint bind command for dump added in profile cfg");
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

        public void DumpConfigHandler()
        {
            Logger.Info("Manual config dump");

            var cfgLines = GameUtility.DumpCfg();

            Logger.Debug($"CFG lines: {cfgLines.Count}");
        }

        private void ApplyNewCfg(List<string> cfgLines)
        {
            if (cfgLines == null) return;

            Logger.Debug($"New CFG with {cfgLines.Count} lines");

            Logger.Debug("Updating tool CFG");
            GameUtility.WriteCfg(_config.ToolCfg, cfgLines);

            Thread.Sleep(1000);

            Logger.Debug("Triggering keyboard keypress");
            WindowsUtility.SendKeyDownExec();
        }

        private static void PlayPing()
        {
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
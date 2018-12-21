﻿using System.ComponentModel;
using System.IO;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LanPartyTool.config
{
    internal sealed class Config : INotifyPropertyChanged
    {
        private const string ConfigFileName = "config.json";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Config));

        private static Config _instance;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _gameExe = "";

        public string GameExe
        {
            get => _gameExe;
            set
            {
                if (_gameExe == value) return;
                _gameExe = value ?? "";
                OnPropertyChanged("GameExe");
                Save();
            }
        }

        private string _toolCfg = "";

        public string ToolCfg
        {
            get => _toolCfg;
            set
            {
                if (_toolCfg == value) return;
                _toolCfg = value ?? "";
                OnPropertyChanged("ToolCfg");
                Save();
            }
        }

        private string _profileCfg = "";

        public string ProfileCfg
        {
            get => _profileCfg;
            set
            {
                if (_profileCfg == value) return;
                _profileCfg = value ?? "";
                OnPropertyChanged("ProfileCfg");
                Save();
            }
        }

        private string _serverUrl = "";

        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                if (_serverUrl == value) return;
                _serverUrl = value ?? "";
                OnPropertyChanged("ServerUrl");
                Save();
            }
        }

        private string _serialPort = "";

        public string SerialPort
        {
            get => _serialPort;
            set
            {
                if (_serialPort == value) return;
                _serialPort = value ?? "";
                OnPropertyChanged("SerialPort");
                Save();
            }
        }

        private Config()
        {
            Load();
        }

        public static Config GetInstance()
        {
            return _instance ?? (_instance = new Config());
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Load()
        {
            Logger.Debug("Loading configuration to file");

            if (!File.Exists(ConfigFileName))
            {
                return;
            }

            var jsonString = File.ReadAllText(ConfigFileName);
            var json = JObject.Parse(jsonString);
            GameExe = json.GetValue("gameExe")?.ToString();
            ToolCfg = json.GetValue("toolCfg")?.ToString();
            ProfileCfg = json.GetValue("profileCfg")?.ToString();
            ServerUrl = json.GetValue("serverUrl")?.ToString();
            SerialPort = json.GetValue("serialPort")?.ToString();
        }

        private void Save()
        {
            Logger.Debug("Saving configuration from file");

            var json = new
            {
                gameExe = _gameExe,
                toolCfg = _toolCfg,
                profileCfg = _profileCfg,
                serverUrl = _serverUrl,
                serialPort = _serialPort
            };
            var jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(ConfigFileName, jsonString);
        }
    }
}
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace LanPartyTool.config
{
    internal sealed class Config : INotifyPropertyChanged
    {
        private const string ConfigFileName = "config.json";

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
            if (!File.Exists(ConfigFileName))
            {
                return;
            }

            var jsonString = File.ReadAllText(ConfigFileName);
            dynamic json = JsonConvert.DeserializeObject(jsonString);
            _gameExe = json.gameExe.ToString();
            _toolCfg = json.toolCfg.ToString();
            _profileCfg = json.profileCfg.ToString();
            _serverUrl = json.serverUrl.ToString();
        }

        private void Save()
        {
            var json = new
            {
                gameExe = _gameExe,
                toolCfg = _toolCfg,
                profileCfg = _profileCfg,
                serverUrl = _serverUrl
            };
            var jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(ConfigFileName, jsonString);
        }
    }
}
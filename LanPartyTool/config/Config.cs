using System.ComponentModel;

namespace LanPartyTool.config
{
    internal sealed class Config : INotifyPropertyChanged
    {
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
            }
        }

        private Config()
        {
        }

        public static Config GetInstance()
        {
            return _instance ?? (_instance = new Config());
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
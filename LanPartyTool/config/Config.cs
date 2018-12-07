using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LanPartyTool.config
{
    internal class Config : INotifyPropertyChanged
    {
        private static Config _instance;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _gameExe;

        public string GameExe
        {
            get => _gameExe;
            set
            {
                if (_gameExe == value) return;

                _gameExe = value;
                OnPropertyChanged("GameExe");
            }
        }

        private string _cfgFile;

        public string CfgFile
        {
            get => _cfgFile;
            set
            {
                if (_cfgFile == value) return;

                _cfgFile = value;
                OnPropertyChanged("CfgFile");
            }
        }

        private Config()
        {
        }

        public static Config GetInstance()
        {
            return _instance ?? (_instance = new Config());
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
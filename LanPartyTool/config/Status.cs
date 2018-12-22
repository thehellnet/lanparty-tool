using System.ComponentModel;
using LanPartyTool.agent;

namespace LanPartyTool.config
{
    internal sealed class Status : INotifyPropertyChanged
    {
        public delegate void SocketStatusChangedHandler();

        private static Status _instance;

        private ServerSocket.Status _socketStatus = ServerSocket.Status.Closed;

        private Status()
        {
        }

        public ServerSocket.Status SocketStatus
        {
            get => _socketStatus;
            set
            {
                if (_socketStatus == value) return;
                _socketStatus = value;
                OnPropertyChanged("SocketStatus");
                OnSocketStatusChanged?.Invoke();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event SocketStatusChangedHandler OnSocketStatusChanged;

        public static Status GetInstance()
        {
            return _instance ?? (_instance = new Status());
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
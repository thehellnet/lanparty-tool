using System.ComponentModel;
using LanPartyTool.agent;

namespace LanPartyTool.config
{
    internal class Status : INotifyPropertyChanged
    {
        private static Status _instance;

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void SocketStatusChangedHandler();

        public event SocketStatusChangedHandler OnSocketStatusChanged;

        private ServerSocket.Status _socketStatus = ServerSocket.Status.Closed;

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

        private Status()
        {
        }

        public static Status GetInstance()
        {
            return _instance ?? (_instance = new Status());
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
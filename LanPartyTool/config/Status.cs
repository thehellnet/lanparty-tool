using LanPartyTool.agent;

namespace LanPartyTool.config
{
    internal sealed class Status
    {
        public delegate void SerialPortStatusChangedHandler();

        public delegate void SocketStatusChangedHandler();

        private static Status _instance;

        private SerialPortReader.Status _serialPortStatus = SerialPortReader.Status.Closed;

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
                OnSocketStatusChanged?.Invoke();
            }
        }

        public SerialPortReader.Status SerialPortStatus
        {
            get => _serialPortStatus;
            set
            {
                if (_serialPortStatus == value) return;
                _serialPortStatus = value;
                OnSerialPortStatusChanged?.Invoke();
            }
        }

        public event SocketStatusChangedHandler OnSocketStatusChanged;
        public event SerialPortStatusChangedHandler OnSerialPortStatusChanged;

        public static Status GetInstance()
        {
            return _instance ?? (_instance = new Status());
        }
    }
}
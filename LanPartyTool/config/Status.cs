﻿using System.ComponentModel;
using LanPartyTool.agent;

namespace LanPartyTool.config
{
    internal sealed class Status : INotifyPropertyChanged
    {
        public delegate void SerialPortStatusChangedHandler();

        public delegate void SocketStatusChangedHandler();

        private static Status _instance;

        private string _lastBarcode = "";

        private SerialPortReader.PortStatus _serialPortStatus = SerialPortReader.PortStatus.Closed;

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

        public SerialPortReader.PortStatus SerialPortStatus
        {
            get => _serialPortStatus;
            set
            {
                if (_serialPortStatus == value) return;
                _serialPortStatus = value;
                OnSerialPortStatusChanged?.Invoke();
            }
        }

        public string LastBarcode
        {
            get => _lastBarcode;
            set
            {
                if (_lastBarcode == value) return;
                _lastBarcode = value ?? "";
                OnPropertyChanged("LastBarcode");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event SocketStatusChangedHandler OnSocketStatusChanged;
        public event SerialPortStatusChangedHandler OnSerialPortStatusChanged;

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
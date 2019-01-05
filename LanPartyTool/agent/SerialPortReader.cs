using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using log4net;
using LanPartyTool.config;

namespace LanPartyTool.agent
{
    internal class SerialPortReader
    {
        public delegate void NewBarcodeHandler(string barcode);

        public delegate void NewStatusHandler(PortStatus portStatus);

        public enum PortStatus
        {
            Closed,
            Preparing,
            Open,
            Parsing,
            Closing
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(SerialPortReader));
        private readonly Config _config = Config.GetInstance();
        private readonly ServerSocket _serverSocket = new ServerSocket();
        private readonly Status _status = Status.GetInstance();

        private SerialPort _serialPort;
        private Thread _thread;

        public event NewBarcodeHandler OnNewBarcode;
        public event NewStatusHandler OnNewStatus;

        public void Start()
        {
            Logger.Info("SerialPortReader start");

            Logger.Debug("Preparing serial port");
            OnNewStatus?.Invoke(PortStatus.Preparing);

            _serialPort = new SerialPort(_config.SerialPort)
            {
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = SerialPort.InfiniteTimeout
            };

            Logger.Debug("Opening serial port");
            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                OnNewStatus?.Invoke(PortStatus.Closed);
                return;
            }

            OnNewStatus?.Invoke(PortStatus.Open);

            Logger.Debug("Launching serial port loop");
            _thread = new Thread(SerialLoop);
            _thread.Start();
        }

        public void Stop()
        {
            Logger.Info("SerialPortReader stop");

            OnNewStatus?.Invoke(PortStatus.Closing);

            Logger.Debug("Closing serial port");

            if (_serialPort != null && _serialPort.IsOpen)
                try
                {
                    _serialPort.Close();
                }
                catch (IOException)
                {
                }

            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    _thread.Interrupt();
                    _thread.Join();
                }

                _thread = null;
            }

            _serialPort = null;
        }

        private void SerialLoop()
        {
            Logger.Debug("Starting loop");

            var line = "";

            while (_serialPort.IsOpen)
            {
                int c;

                try
                {
                    c = _serialPort.ReadChar();
                }
                catch (Exception e)
                {
                    Logger.Error($"Error on serial port: {e.Message}");
                    continue;
                }

                if (c == '\n' || c == '\r')
                {
                    if (line.Length > 0) NewSerialLine(line);

                    line = "";
                    continue;
                }

                line += (char) c;
            }

            Logger.Debug("Loop end");
        }

        private void NewSerialLine(string line)
        {
            Logger.Debug($"New line on serial port: {line}");
            OnNewBarcode?.Invoke(line);
        }
    }
}
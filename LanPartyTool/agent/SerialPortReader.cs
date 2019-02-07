using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private DateTime lastBarcodeDateTime = DateTime.Now;

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

            var line = new List<byte>();

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

                if (line.Count == 0 && c != 0x02)
                    continue;

                line.Add((byte) c);

                if (c != 0x03)
                    continue;

                var barcode = ParseString(line);
                if (!string.IsNullOrEmpty(barcode)) NewSerialLine(barcode);

                line.Clear();
            }

            Logger.Debug("Loop end");
        }

        private void NewSerialLine(string line)
        {
            Logger.Debug($"New line on serial port: {line}");

            if (lastBarcodeDateTime.AddSeconds(2) >= DateTime.Now)
                return;

            lastBarcodeDateTime = DateTime.Now;
            new Thread(() => { OnNewBarcode?.Invoke(line); }).Start();
        }

        private static string ParseString(IReadOnlyCollection<byte> line)
        {
            if (line == null)
                return null;
            if (line.Count != 14)
                return null;
            if (line.ElementAt(0) != 0x02)
                return null;
            if (line.ElementAt(13) != 0x03)
                return null;

            uint computedChecksum = 0;
            for (var i = 1; i < 11; i += 2)
            {
                var rawVal = line.Skip(i).Take(2).ToArray();
                var asciiVal = Encoding.UTF8.GetString(rawVal);
                var val = Convert.ToUInt16(asciiVal, 16);
                if (i == 1)
                    computedChecksum = val;
                else
                    computedChecksum ^= val;
            }

            var rawVersion = line.Skip(1).Take(2).ToArray();
            var rawNumber = line.Skip(3).Take(8).ToArray();
            var rawChecksum = line.Skip(11).Take(2).ToArray();

            var asciiVersion = Encoding.UTF8.GetString(rawVersion);
            var asciiNumber = Encoding.UTF8.GetString(rawNumber);
            var asciiChecksum = Encoding.UTF8.GetString(rawChecksum);

            var number = Convert.ToUInt64(asciiNumber, 16);
            var checksum = Convert.ToUInt64(asciiChecksum, 16);

            if (checksum != computedChecksum)
                return null;

            var barcode = number.ToString("D10");

            Logger.Debug($"New barcode {barcode} - Version {asciiVersion}");

            return barcode;
        }
    }
}
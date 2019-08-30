using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using LanPartyTool.config;
using LanPartyTool.utility;
using log4net;

namespace LanPartyTool.agent
{
    internal class SerialPortReader
    {
        public delegate void NewBarcodeHandler(string barcode, int counts);

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

        private readonly List<byte> line = new List<byte>();
        private string _lastBarcode;
        private DateTime _lastBarcodeDateTime = DateTime.Now;

        private SerialPort _serialPort;
        private int _tokenCounts;

        private TokenStatus _tokenStatus = TokenStatus.Waiting;
        private readonly Watchdog _tokenWatchdog = new Watchdog();

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

            _serialPort.DataReceived += ReadData;

            _tokenWatchdog.OnWatchdogTimeout += TokenRemoved;
        }

        public void Stop()
        {
            Logger.Info("SerialPortReader stop");

            OnNewStatus?.Invoke(PortStatus.Closing);

            _tokenWatchdog.OnWatchdogTimeout -= TokenRemoved;

            Logger.Debug("Closing serial port");

            _serialPort.DataReceived -= ReadData;

            if (_serialPort != null && _serialPort.IsOpen)
                try
                {
                    _serialPort.Close();
                }
                catch (IOException)
                {
                }

            _serialPort = null;
        }

        private void ReadData(object sender, SerialDataReceivedEventArgs e)
        {
            //            new Thread(ReadDataLoop).Start();
            ReadDataLoop();
        }

        private void ReadDataLoop()
        {
            Logger.Debug("Reading data");

            while (true)
            {
                int c;
                try
                {
                    c = _serialPort.ReadByte();
                }
                catch (IOException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error on serial port: {ex.Message}");
                    break;
                }

                if (c == -1) break;

                if (line.Count == 0 && c != 0x02)
                    continue;

                line.Add((byte) c);

                if (c != 0x03)
                    continue;

                var barcode = ParseString(line);
                if (!string.IsNullOrEmpty(barcode))
                {
                    new Thread(() => { NewSerialLine(barcode); }).Start();
                }

                line.Clear();
            }
        }

        private void NewSerialLine(string line)
        {
            Logger.Debug($"New line on serial port: {line}");

            _lastBarcode = line;

            switch (_tokenStatus)
            {
                case TokenStatus.Waiting:
                    _tokenStatus = TokenStatus.Counting;
                    _tokenWatchdog.Start();
                    break;

                case TokenStatus.Counting:
                    _tokenWatchdog.Ping();
                    break;

                default:
                    return;
            }

            if (_tokenStatus != TokenStatus.Counting) return;

            if (_lastBarcodeDateTime.AddSeconds(Constants.SerialBarcodeDebounceTimeout) >= DateTime.Now) return;
            _lastBarcodeDateTime = DateTime.Now;

            _tokenCounts++;
            SoundUtility.Play(SoundUtility.Sound.Ping);
        }

        private void TokenRemoved()
        {
            _tokenStatus = TokenStatus.Parsing;
            _tokenWatchdog.Stop();

            Logger.Info($"New Barcode command: barcode {_lastBarcode} - counts: {_tokenCounts}");

            var barcode = _lastBarcode;
            var counts = _tokenCounts;
            new Thread(() => { OnNewBarcode?.Invoke(barcode, counts); }).Start();

            _tokenStatus = TokenStatus.Waiting;
            _tokenCounts = 0;
        }

        private static string ParseString(IReadOnlyCollection<byte> line)
        {
            if (line == null) return null;
            if (line.Count != 14) return null;
            if (line.ElementAt(0) != 0x02) return null;
            if (line.ElementAt(13) != 0x03) return null;

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

        private enum TokenStatus
        {
            Waiting,
            Counting,
            Parsing
        }
    }
}
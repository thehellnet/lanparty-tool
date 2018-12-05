using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace LanPartyTool.agent
{
    class ServerSocket
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerSocket));

        private const int SocketPort = 19642;
        private const int SocketBacklog = 128;

        public delegate void NewConnectionHandler(Socket socket);
        public event NewConnectionHandler OnConnectionAccepted;

        private Socket _socket;
        private Thread _thread;

        public void Start()
        {
            Logger.Info("ServerSocket start");

            Logger.Debug("Preparing socket");
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, SocketPort));

            try
            {
                Logger.Debug("Socket Listen");
                _socket.Listen(SocketBacklog);
            }
            catch (SocketException e)
            {
                Logger.Error(e.Message);
                return;
            }

            Logger.Debug("Preparing main loop thread");
            _thread = new Thread(Loop);
            _thread.Start();
        }

        public void Stop()
        {
            Logger.Info("ServerSocket stop");

            if (_socket.IsBound)
            {
                _socket.Close();
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

            _socket = null;
        }

        private void Loop()
        {
            Logger.Debug("Starting loop");

            while (_socket.IsBound)
            {
                Socket newSocket;

                try
                {
                    newSocket = _socket.Accept();
                }
                catch (SocketException)
                {
                    break;
                }

                var remoteAddress = newSocket.RemoteEndPoint.ToString();
                Logger.Info($"New socket connection from {remoteAddress} accepted");

                Logger.Debug("Preparing ClientSocket instance");
                OnConnectionAccepted?.Invoke(newSocket);
            }
        }
    }
}
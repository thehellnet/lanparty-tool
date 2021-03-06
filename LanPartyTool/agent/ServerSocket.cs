﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace LanPartyTool.agent
{
    internal class ServerSocket : IDisposable
    {
        public delegate void NewConnectionHandler(Socket socket);

        public delegate void NewStatusHandler(Status status);

        public enum Status
        {
            Closed,
            Preparing,
            Listening,
            Accepting,
            Closing
        }

        private const int SocketPort = 19642;
        private const int SocketBacklog = 128;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerSocket));

        private readonly object SYNC = new object();

        private Socket _socket;
        private Thread _thread;

        public event NewConnectionHandler OnConnectionAccepted;
        public event NewStatusHandler OnNewStatus;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            lock (SYNC)
            {
                Logger.Info("ServerSocket start");

                if (_socket != null)
                {
                    Logger.Error("Already running");
                    return;
                }

                Logger.Debug("Preparing socket");
                OnNewStatus?.Invoke(Status.Preparing);

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

                OnNewStatus?.Invoke(Status.Listening);

                Logger.Debug("Preparing main loop thread");
                _thread = new Thread(Loop);
                _thread.Start();
            }
        }

        public void Stop()
        {
            lock (SYNC)
            {
                Logger.Info("ServerSocket stop");

                OnNewStatus?.Invoke(Status.Closing);

                if (_socket != null)
                    try
                    {
                        _socket.Close();
                    }
                    catch (SocketException)
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

                _socket = null;

                OnNewStatus?.Invoke(Status.Closed);
            }
        }

        private void Loop()
        {
            Logger.Debug("Starting loop");

            while (_socket.IsBound)
            {
                Socket newSocket;

                OnNewStatus?.Invoke(Status.Listening);

                try
                {
                    newSocket = _socket.Accept();
                }
                catch (Exception)
                {
                    break;
                }

                OnNewStatus?.Invoke(Status.Accepting);

                var remoteAddress = newSocket.RemoteEndPoint.ToString();
                Logger.Info($"New socket connection from {remoteAddress} accepted");

                Logger.Debug("Preparing ClientSocket instance");
                OnConnectionAccepted?.Invoke(newSocket);
            }
        }
    }
}
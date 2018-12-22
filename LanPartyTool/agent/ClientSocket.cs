using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;
using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    internal class ClientSocket
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerSocket));

        private readonly Socket _socket;

        public ClientSocket(Socket socket)
        {
            _socket = socket;
        }

        public void Start()
        {
            new Thread(Loop).Start();
        }

        private void Loop()
        {
            Logger.Debug("Starting client loop");

            while (_socket.Connected)
            {
                var memoryStream = new MemoryStream();

                do
                {
                    var buff = new byte[1024];
                    int readSize;

                    try
                    {
                        readSize = _socket.Receive(buff);
                    }
                    catch (SocketException e)
                    {
                        Logger.Error(e.Message);
                        break;
                    }

                    if (readSize == 0) break;

                    memoryStream.Write(buff, (int) memoryStream.Length, readSize);
                } while (_socket.Available > 0);

                if (memoryStream.Length == 0) break;

                var rawData = memoryStream.ToArray();
                Logger.Debug($"Received new command of {rawData.Length} bytes");

                var payload = Encoding.UTF8.GetString(rawData);
                var request = JsonConvert.DeserializeObject<dynamic>(payload);

                var executor = new Executor(request);
                var response = executor.Execute();

                var ret = JsonConvert.SerializeObject(response);

                Logger.Debug($"Response: {ret}");

                var retBytes = Encoding.ASCII.GetBytes(ret);

                try
                {
                    _socket.Send(retBytes);
                }
                catch (SocketException e)
                {
                    Logger.Warn(e.Message);
                }
            }

            Logger.Debug("Closing client loop");
        }
    }
}
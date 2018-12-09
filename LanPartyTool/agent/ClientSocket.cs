using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    internal class ClientSocket
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServerSocket));

        private Socket _socket;

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

                    if (readSize == 0)
                    {
                        break;
                    }

                    memoryStream.Write(buff, (int) memoryStream.Length, readSize);
                } while (_socket.Available > 0);

                if (memoryStream.Length == 0)
                {
                    break;
                }

                var rawData = memoryStream.ToArray();
                Logger.Debug($"Received new command of {rawData.Length} bytes");

                var payload = Encoding.UTF8.GetString(rawData);
                var request = JsonConvert.DeserializeObject<dynamic>(payload);
                var response = ParsePayload(request);
                var ret = JsonConvert.SerializeObject(response);

                Logger.Debug($"Response: {ret}");

                var retBytes = Encoding.ASCII.GetBytes(ret);
                _socket.Send(retBytes);
            }

            Logger.Debug("Closing client loop");
        }

        private JsonResponse ParsePayload(dynamic request)
        {
            Logger.Debug("Parsing payload");
            Logger.Debug($"Action: {request.action}");

            if (request.action == "ping")
            {
                return JsonResponse.GetSuccessInstance("pong");
            }

            if (request.action == "cfg")
            {
                return JsonResponse.GetErrorInstance("Not implemented yet");
            }

            return JsonResponse.GetErrorInstance("Action not recognized");
        }
    }
}
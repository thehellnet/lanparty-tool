using System.Net.Sockets;

namespace LanPartyTool.agent
{
    class ClientSocket
    {
        private Socket _socket;

        public ClientSocket(Socket socket)
        {
            _socket = socket;
        }
    }
}
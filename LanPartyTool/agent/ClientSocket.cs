using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
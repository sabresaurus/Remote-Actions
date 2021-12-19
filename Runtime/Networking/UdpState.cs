using System.Net;
using System.Net.Sockets;

namespace Sabresaurus.RemoteActions
{
    public struct UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint endPoint;
    }
}
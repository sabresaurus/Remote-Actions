using System.Net;
using System.Net.Sockets;

namespace Sabresaurus.RemoteActions
{
    public struct UdpState
    {
        public readonly UdpClient udpClient;
        public readonly IPEndPoint endPoint;

        public UdpState(UdpClient udpClient, IPEndPoint endPoint)
        {
            this.udpClient = udpClient;
            this.endPoint = endPoint;
        }
    }
}
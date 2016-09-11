using System.Net;
using System.Net.Sockets;

namespace SelfHostingWebServer.Helper
{
    static class Tcp
    {
        public static int GetFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

    }
}

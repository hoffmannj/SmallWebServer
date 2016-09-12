using SelfHostingWebServer.Implementations;
using SelfHostingWebServer.Interfaces;

namespace SelfHostingWebServer
{
    public sealed class ServerParameters
    {
        public int HttpPort { get; set; }
        public int HttpsPort { get; set; }
        public bool LocalhostOnly { get; set; }
        public string BasePath { get; set; }

        public ISerializer Serializer { get; set; }

        public ServerParameters()
        {
            HttpPort = -1;
            HttpsPort = -1;
            LocalhostOnly = false;
            BasePath = string.Empty;
            Serializer = new DefaultSerializer();
        }
    }
}

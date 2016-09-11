using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfHostingWebServer
{
    public class ServerParameters
    {
        public int HttpPort { get; set; }
        public int HttpsPort { get; set; }
        public bool LocalhostOnly { get; set; }
        public string BasePath { get; set; }

        public ServerParameters()
        {
            HttpPort = -1;
            HttpsPort = -1;
            LocalhostOnly = false;
            BasePath = string.Empty;
        }
    }
}

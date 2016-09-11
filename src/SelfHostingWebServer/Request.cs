using System.Net;

namespace SelfHostingWebServer
{
    public class Request
    {
        public HttpListenerRequest OriginalRequest { get; private set; }
        public string QueryString { get; private set; }
        public string BodyString { get; private set; }

        public Request(HttpListenerRequest originalRequest)
        {
            OriginalRequest = originalRequest;
            QueryString = originalRequest.QueryString.ToString();
        }

        internal void SetBodyString(string bodyString)
        {
            BodyString = bodyString;
        }
    }
}

using System.Net;

namespace SelfHostingWebServer
{
    public sealed class Request
    {
        public HttpListenerRequest OriginalRequest { get; private set; }
        public string PathAndQueryString { get; private set; }
        public string BodyString { get; private set; }

        public Request(HttpListenerRequest originalRequest)
        {
            OriginalRequest = originalRequest;
            PathAndQueryString = originalRequest.Url.PathAndQuery;
        }

        internal void SetBodyString(string bodyString)
        {
            BodyString = bodyString;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SelfHostingWebServer.Handler
{
    internal class RequestHandlerStore
    {
        private readonly List<RequestHandlerInfo> _handlers = new List<RequestHandlerInfo>();
        private readonly object _locker = new object();

        public void Register(RequestHandlerInfo handler)
        {
            lock (_locker)
            {
                if (_handlers.Any(h => h == handler)) throw new Exception("RequestHandler already registered");
                _handlers.Add(handler);
            }
        }

        public RequestHandlerInfo GetHandler(string uri, string method)
        {
            lock (_locker)
            {
                return _handlers.OrderByDescending(handler => handler.Path.Length).Where(handler => handler.IsMatch(uri, method)).FirstOrDefault();
            }
        }
    }
}

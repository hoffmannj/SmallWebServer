using SelfHostingWebServer.Handler;
using System;

namespace SelfHostingWebServer
{
    public sealed class Router
    {
        private RequestHandlerStore _requestHandlerStore;

        internal Router SetRequestHandlerStore(RequestHandlerStore requestHandlerStore)
        {
            _requestHandlerStore = requestHandlerStore;
            return this;
        }

        public void GET(string path, Func<Context, object> func)
        {
            GET(path, func, false);
        }

        public void GET(string path, Func<Context, object> func, bool serializeResult)
        {
            var mi = new RequestHandlerInfo(path, WebMethod.GET, func, serializeResult, null);
            _requestHandlerStore.Register(mi);
        }

        public void POST(string path, Func<Context, object> func)
        {
            POST(path, func, false, null);
        }

        public void POST(string path, Func<Context, object> func, bool serializeResult)
        {
            POST(path, func, serializeResult, null);
        }

        public void POST(string path, Func<Context, object> func, Func<string, object> bodyDeserializer)
        {
            POST(path, func, false, bodyDeserializer);
        }

        public void POST(string path, Func<Context, object> func, bool serializeResult, Func<string, object> bodyDeserializer)
        {
            var mi = new RequestHandlerInfo(path, WebMethod.POST, func, serializeResult, bodyDeserializer);
            _requestHandlerStore.Register(mi);
        }

        public void DELETE(string path, Func<Context, object> func)
        {
            DELETE(path, func, false, null);
        }

        public void DELETE(string path, Func<Context, object> func, bool serializeResult)
        {
            DELETE(path, func, serializeResult, null);
        }

        public void DELETE(string path, Func<Context, object> func, Func<string, object> bodyDeserializer)
        {
            DELETE(path, func, false, bodyDeserializer);
        }

        public void DELETE(string path, Func<Context, object> func, bool serializeResult, Func<string, object> bodyDeserializer)
        {
            var mi = new RequestHandlerInfo(path, WebMethod.DELETE, func, serializeResult, bodyDeserializer);
            _requestHandlerStore.Register(mi);
        }

        public void PUT(string path, Func<Context, object> func)
        {
            PUT(path, func, false, null);
        }

        public void PUT(string path, Func<Context, object> func, bool serializeResult)
        {
            PUT(path, func, serializeResult, null);
        }

        public void PUT(string path, Func<Context, object> func, Func<string, object> bodyDeserializer)
        {
            PUT(path, func, false, bodyDeserializer);
        }

        public void PUT(string path, Func<Context, object> func, bool serializeResult, Func<string, object> bodyDeserializer)
        {
            var mi = new RequestHandlerInfo(path, WebMethod.PUT, func, serializeResult, bodyDeserializer);
            _requestHandlerStore.Register(mi);
        }

    }
}

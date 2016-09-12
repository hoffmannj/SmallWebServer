using SelfHostingWebServer.Handler;
using SelfHostingWebServer.HandlerEventArgs;
using System;
using System.Net;

namespace SelfHostingWebServer
{
    public delegate void PreProcessHandler(object sender, PreProcessHandlerEventArgs e);
    public delegate void PostProcessHandler(object sender, PostProcessHandlerEventArgs e);
    public delegate void ProcessErrorHandler(object sender, ProcessErrorHandlerEventArgs e);

    public sealed class WebServer : IDisposable
    {
        private PreProcessHandler _preProcess;
        public event PreProcessHandler PreProcess
        {
            add
            {
                _preProcess += value;
            }
            remove
            {
                _preProcess -= value;
            }
        }

        private PostProcessHandler _postProcess;
        public event PostProcessHandler PostProcess
        {
            add
            {
                _postProcess += value;
            }
            remove
            {
                _postProcess -= value;
            }
        }

        private ProcessErrorHandler _errorProcess;
        public event ProcessErrorHandler ErrorProcess
        {
            add
            {
                _errorProcess += value;
            }
            remove
            {
                _errorProcess -= value;
            }
        }

        private readonly Listener _listener;
        private readonly RequestHandlerStore _requestHandlerStore;
        private readonly SessionHandler _sessionHandler;
        public VirtualPathHandler VirtualPath { get; }
        public Router Router { get; }

        public WebServer(ServerParameters parameters)
        {
            _sessionHandler = new SessionHandler();
            _requestHandlerStore = new RequestHandlerStore();
            VirtualPath = new VirtualPathHandler();
            Router = new Router().SetRequestHandlerStore(_requestHandlerStore);

            if (string.IsNullOrEmpty(parameters.BasePath)) VirtualPath.SetDefaultBasePath();
            else VirtualPath.SetBasePath(parameters.BasePath);

            _listener = new Listener(parameters, ListenerContextHandler);
        }

        public WebServer() : this(new ServerParameters())
        { }

        public int HttpPort
        {
            get
            {
                return _listener.Parameters.HttpPort;
            }
        }

        public int HttpsPort
        {
            get
            {
                return _listener.Parameters.HttpsPort;
            }
        }

        public void Start()
        {
            try
            {
                _listener.Stop();
                _listener.Start();
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                _listener.Stop();
                throw new UnauthorizedAccessException("Error in changing certificate settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("ERROR: " + ex.Message);
                _listener.Stop();
                throw;
            }
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public void Dispose()
        {
            _listener.Dispose();
        }

        private void ListenerContextHandler(HttpListenerContext ctx)
        {
            var handler = new HttpRequestHandler(ctx, _sessionHandler, _requestHandlerStore, _listener.Parameters.Serializer, _preProcess, _postProcess, _errorProcess);
            handler.Handle();
        }
    }
}

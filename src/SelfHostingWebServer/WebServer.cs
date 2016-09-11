using SelfHostingWebServer.Handler;
using SelfHostingWebServer.HandlerEventArgs;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace SelfHostingWebServer
{
    public delegate void PreProcessHandler(object sender, PreProcessHandlerEventArgs e);
    public delegate void PostProcessHandler(object sender, PostProcessHandlerEventArgs e);
    public delegate void ProcessErrorHandler(object sender, ProcessErrorHandlerEventArgs e);

    public sealed class WebServer : IDisposable
    {
        public event PreProcessHandler PreProcess;
        public event PostProcessHandler PostProcess;
        public event ProcessErrorHandler ErrorProcess;

        private HttpListener _listener;
        private ServerParameters _parameters;
        private Thread _workerTask;
        private readonly RequestHandlerStore _requestHandlerStore;
        private readonly SessionHandler _sessionHandler;
        private VirtualPathHandler VirtualPath { get; }
        private Router Router { get; }

        public WebServer(ServerParameters parameters)
        {
            _sessionHandler = new SessionHandler();
            _requestHandlerStore = new RequestHandlerStore();
            _parameters = parameters;
            VirtualPath = new VirtualPathHandler();
            Router = new Router().SetRequestHandlerStore(_requestHandlerStore);

            if (string.IsNullOrEmpty(parameters.BasePath)) VirtualPath.SetDefaultBasePath();
            else VirtualPath.SetBasePath(parameters.BasePath);

            if (_parameters.HttpPort == 0) _parameters.HttpPort = Helper.Tcp.GetFreeTcpPort();
            if (_parameters.HttpsPort == 0) _parameters.HttpsPort = Helper.Tcp.GetFreeTcpPort();
            if (_parameters.HttpPort < 0 && _parameters.HttpsPort < 0) throw new Exception("At least one port has to be used");

            CreateHttpListener();
        }

        public WebServer() : this(new ServerParameters())
        { }

        public int HttpPort
        {
            get
            {
                return _parameters.HttpPort;
            }
        }

        public int HttpsPort
        {
            get
            {
                return _parameters.HttpsPort;
            }
        }

        public void Run()
        {
            try
            {
                ReleaseListener();
                CreateHttpListener();
                _listener.Start();
                _workerTask = new Thread(new ThreadStart(WorkerTask));
                _workerTask.Start();
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                ReleaseListener();
                throw new UnauthorizedAccessException("Error in changing certificate settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("ERROR: " + ex.Message);
                ReleaseListener();
            }
        }

        public void Dispose()
        {
            ReleaseListener();
        }

        private void ReleaseListener()
        {
            if (_workerTask != null)
            {
                _workerTask.Abort();
                _workerTask = null;
            }
            if (_listener != null)
            {
                if (_listener.IsListening) _listener.Stop();
                _listener.Close();
                _listener = null;
            }
        }

        private void CreateHttpListener()
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            _listener = new HttpListener();

            SetHttpPrefix();
            SetHttpsPrefix();
        }

        private void SetHttpPrefix()
        {
            if (_parameters.HttpPort > 0)
            {
                var host = _parameters.LocalhostOnly ? "localhost" : "+";
                var prefix = CreatePrefix("", host, _parameters.HttpsPort);
                _listener.Prefixes.Add(prefix);
            }
        }

        private void SetHttpsPrefix()
        {
            if (_parameters.HttpsPort > 0)
            {
                var host = _parameters.LocalhostOnly ? "localhost" : "+";
                var prefix = CreatePrefix("s", host, _parameters.HttpsPort);
                _listener.Prefixes.Add(prefix);
            }
        }

        private string CreatePrefix(string schemeEnd, string host, int port)
        {
            return string.Format("http{0}://{1}:{2}/", schemeEnd, host, _parameters.HttpsPort);
        }

        private void WorkerTask()
        {
            while (_listener.IsListening)
            {
                ThreadPool.QueueUserWorkItem(ListenerContextHandler, _listener.GetContext());
            }
        }

        private void ListenerContextHandler(object contextObject)
        {
            var ctx = contextObject as HttpListenerContext;
            Context context = null;
            try
            {
                context = CreateContext(ctx);
                string rstr = RequestHandler(context);
                WriteStringToResponse(ctx.Response, rstr);
            }
            catch (Exceptions.ErrorCodeException ex)
            {
                ctx.Response.StatusCode = ex.ErrorCode;
                System.Diagnostics.Trace.WriteLine(ex.ErrorCode);
                ErrorProcess?.Invoke(this, new ProcessErrorHandlerEventArgs(context, ex));
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                System.Diagnostics.Trace.WriteLine(ex.Message);
                ErrorProcess?.Invoke(this, new ProcessErrorHandlerEventArgs(context, ex));
            }
            finally
            {
                ctx.Response.OutputStream.Close();
            }
        }

        private void WriteStringToResponse(HttpListenerResponse response, string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            byte[] buf = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buf.Length;
            response.OutputStream.Write(buf, 0, buf.Length);
        }

        private void InitializeSession(Context ctx, HttpListenerContext context)
        {
            var request = ctx.Request.OriginalRequest;
            if (request.Cookies != null && request.Cookies["__SessionId__"] != null)
            {
                var sessionId = request.Cookies["__SessionId__"].Value;
                var sessionIdGuid = Guid.Parse(sessionId);
                var session = _sessionHandler.GetOrCreate(sessionIdGuid);
                ctx.SetSession(session);
            }
            else
            {
                var sessionIdGuid = Guid.NewGuid();
                var session = new Session();
                _sessionHandler.Set(sessionIdGuid, session);
                ctx.SetSession(session);
                context.Response.Cookies.Add(new Cookie("__SessionId__", sessionIdGuid.ToString(), "/"));
            }
        }

        private RequestHandlerInfo GetMatchingHandler(Context ctx)
        {
            var uri = ctx.Request.OriginalRequest.Url.AbsoluteUri;
            var method = ctx.Request.OriginalRequest.HttpMethod.ToUpper();
            return _requestHandlerStore.GetHandler(uri, method);
        }

        private Context CreateContext(HttpListenerContext context)
        {
            var request = context.Request;
            var ctx = new Context(new Request(request), context.Response);
            context.Response.Cookies = context.Response.Cookies ?? new CookieCollection();
            InitializeSession(ctx, context);
            return ctx;
        }

        private string RequestHandler(Context context)
        {
            _sessionHandler.CleanTimedOutSessions();
            var handler = GetMatchingHandler(context);
            if (handler != null)
            {
                context.SetForHandler(handler);
                PreProcess?.Invoke(this, new PreProcessHandlerEventArgs(context));
                var retVal = handler.Call(context);
                PostProcess?.Invoke(this, new PostProcessHandlerEventArgs(context, retVal));
                if (handler.ShouldSerializeResult) return Newtonsoft.Json.JsonConvert.SerializeObject(retVal); 
                else return retVal.ToString();
            }
            throw new Exceptions.ErrorCodeException(404);
        }
    }
}

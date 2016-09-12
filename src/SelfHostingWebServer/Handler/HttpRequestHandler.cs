using SelfHostingWebServer.HandlerEventArgs;
using SelfHostingWebServer.Interfaces;
using System;
using System.Net;
using System.Text;

namespace SelfHostingWebServer.Handler
{
    internal sealed class HttpRequestHandler
    {
        private HttpListenerContext _context;
        private SessionHandler _sessionHandler;
        private RequestHandlerStore _requestHandlerStore;
        private ISerializer _serializer;

        private PreProcessHandler _preProcess;
        private PostProcessHandler _postProcess;
        private ProcessErrorHandler _errorProcess;

        public HttpRequestHandler(
            HttpListenerContext ctx, 
            SessionHandler sessionHandler,
            RequestHandlerStore requestHandlerStore,
            ISerializer serializer,
            PreProcessHandler preProcess, 
            PostProcessHandler postProcess, 
            ProcessErrorHandler errorProcess)
        {
            _context = ctx;
            _sessionHandler = sessionHandler;
            _requestHandlerStore = requestHandlerStore;
            _serializer = serializer;
            _preProcess = preProcess;
            _postProcess = postProcess;
            _errorProcess = errorProcess;
        }

        public void Handle()
        {
            Context context = null;
            try
            {
                context = CreateContext(_context);
                WriteStringToResponse(_context.Response, RequestHandler(context));
            }
            catch (Exceptions.ErrorCodeException ex)
            {
                _context.Response.StatusCode = ex.ErrorCode;
                _errorProcess?.Invoke(this, new ProcessErrorHandlerEventArgs(context, ex));
            }
            catch (Exception ex)
            {
                _context.Response.StatusCode = 500;
                _errorProcess?.Invoke(this, new ProcessErrorHandlerEventArgs(context, ex));
            }
            finally
            {
                _context.Response.OutputStream.Close();
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
                var session = _sessionHandler.GetOrCreate(Guid.Parse(sessionId));
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
                _preProcess?.Invoke(this, new PreProcessHandlerEventArgs(context));
                var retVal = handler.Call(context);
                _postProcess?.Invoke(this, new PostProcessHandlerEventArgs(context, retVal));
                if (handler.ShouldSerializeResult) return _serializer.Serialize(retVal);
                else return retVal.ToString();
            }
            throw new Exceptions.ErrorCodeException(404);
        }
    }
}

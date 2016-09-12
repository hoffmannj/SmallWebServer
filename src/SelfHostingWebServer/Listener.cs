using System;
using System.Net;
using System.Threading;

namespace SelfHostingWebServer
{
    internal sealed class Listener : IDisposable
    {
        private HttpListener _listener;
        private Action<HttpListenerContext> _contextHandler;
        private Thread _workerTask;
        public ServerParameters Parameters { get; }

        public Listener(ServerParameters parameters, Action<HttpListenerContext> contextHandler)
        {
            Parameters = parameters;
            _contextHandler = contextHandler;

            if (Parameters.HttpPort == 0) Parameters.HttpPort = Helper.Tcp.GetFreeTcpPort();
            if (Parameters.HttpsPort == 0) Parameters.HttpsPort = Helper.Tcp.GetFreeTcpPort();
            if (Parameters.HttpPort < 0 && Parameters.HttpsPort < 0) throw new Exception("At least one port has to be used");

            CreateHttpListener();
        }

        public void Start()
        {
            _listener.Start();
            _workerTask = new Thread(new ThreadStart(WorkerTask));
            _workerTask.Start();
        }

        public void Stop()
        {
            if (_listener.IsListening) _listener.Stop();
        }

        public void Dispose()
        {
            if (_workerTask != null)
            {
                _workerTask.Abort();
                _workerTask = null;
            }
            if (_listener != null)
            {
                if (_listener.IsListening) Stop();
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
            if (Parameters.HttpPort > 0)
            {
                var host = Parameters.LocalhostOnly ? "localhost" : "+";
                var prefix = CreatePrefix("", host, Parameters.HttpPort);
                System.Diagnostics.Trace.WriteLine("Prefix: " + prefix);
                _listener.Prefixes.Add(prefix);
            }
        }

        private void SetHttpsPrefix()
        {
            if (Parameters.HttpsPort > 0)
            {
                var host = Parameters.LocalhostOnly ? "localhost" : "+";
                var prefix = CreatePrefix("s", host, Parameters.HttpsPort);
                System.Diagnostics.Trace.WriteLine("Prefix: " + prefix);
                _listener.Prefixes.Add(prefix);
            }
        }

        private string CreatePrefix(string schemeEnd, string host, int port)
        {
            return string.Format("http{0}://{1}:{2}/", schemeEnd, host, port);
        }

        private void WorkerTask()
        {
            while (_listener.IsListening)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(ListenerContextHandler, _listener.GetContext());
                }
                catch (HttpListenerException ex)
                {
                    //ErrorCode 995: ERROR_OPERATION_ABORTED
                    //URL: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681388(v=vs.85).aspx
                    //Error message: The I/O operation has been aborted because of either a thread exit or an application request.
                    if (ex.ErrorCode != 995) throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void ListenerContextHandler(object contextObject)
        {
            var ctx = contextObject as HttpListenerContext;
            if (ctx == null) return;
            _contextHandler(ctx);
        }
    }
}

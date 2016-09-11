using System;

namespace SelfHostingWebServer.HandlerEventArgs
{
    public class ProcessErrorHandlerEventArgs : EventArgs
    {
        public Context Context { get; private set; }
        public Exception Exception { get; private set; }

        public ProcessErrorHandlerEventArgs(Context context, Exception exception)
        {
            Context = context;
            Exception = exception;
        }
    }
}

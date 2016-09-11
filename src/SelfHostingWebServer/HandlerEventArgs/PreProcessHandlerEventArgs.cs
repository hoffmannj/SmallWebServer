using System;

namespace SelfHostingWebServer.HandlerEventArgs
{
    public class PreProcessHandlerEventArgs : EventArgs
    {
        public Context Context { get; private set; }

        public PreProcessHandlerEventArgs(Context context)
        {
            Context = context;
        }
    }
}

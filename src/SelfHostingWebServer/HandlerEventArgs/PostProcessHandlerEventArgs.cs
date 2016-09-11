using System;

namespace SelfHostingWebServer.HandlerEventArgs
{
    public class PostProcessHandlerEventArgs : EventArgs
    {
        public Context Context { get; private set; }
        public object ResultValue { get; private set; }

        public PostProcessHandlerEventArgs(Context context, object resultValue)
        {
            Context = context;
            ResultValue = resultValue;
        }
    }
}

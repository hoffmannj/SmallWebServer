using SelfHostingWebServer.HandlerEventArgs;
using System;

namespace SelfHostingWebServer.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var parameters = new ServerParameters
            {
                HttpPort = 3000,
                LocalhostOnly = true
            };
            using (var server = new WebServer(parameters))
            {
                //set routes
                server.Router.GET("/", GetHandler);
                server.Router.GET("/json/{value}", GetHandlerJson, true);
                server.Router.GET("/error", GetHandlerWithError);

                //set pre/post event handlers
                server.PreProcess += PreEvent;
                server.PostProcess += PostEvent;
                server.ErrorProcess += ErrorEvent;

                server.Start();
                Console.WriteLine("Started on port: {0}", server.HttpPort);
                Console.ReadLine();
                server.Stop();
            }
        }

        private static object GetHandler(Context context)
        {
            return "<html><body><h1>Handled ;-)</h1></body></html>";
        }

        private static object GetHandlerJson(Context context)
        {
            return new
            {
                doubleField = Math.PI,
                dateField = DateTime.Now,
                stringField = "Some content",
                parameterField = "Value: " + context.QueryParameters["value"],
                objectField = new
                {
                    charField = '!',
                    intField = 13
                }
            };
        }

        private static object GetHandlerWithError(Context context)
        {
            throw new Exception("Some error happened");
        }

        private static void PreEvent(object sender, PreProcessHandlerEventArgs e)
        {
            Console.WriteLine("Handling: " + e.Context.Request.PathAndQueryString);
        }

        private static void PostEvent(object sender, PostProcessHandlerEventArgs e)
        {
            Console.WriteLine("Handled: " + e.Context.Request.PathAndQueryString);
            Console.WriteLine("Result: " + e.ResultValue.ToString());
        }

        private static void ErrorEvent(object sender, ProcessErrorHandlerEventArgs e)
        {
            Console.WriteLine("Error: " + e.Exception.Message);
        }
    }
}

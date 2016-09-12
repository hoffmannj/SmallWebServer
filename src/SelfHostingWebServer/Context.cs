using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;

namespace SelfHostingWebServer
{
    public sealed class Context
    {
        public Request Request { get; private set; }
        public IReadOnlyDictionary<string, object> QueryParameters { get; private set; }
        public dynamic BodyObject { get; private set; }
        public Session Session { get; private set; }

        internal HttpListenerResponse Response { get; private set; }

        public Context(Request request, HttpListenerResponse response)
        {
            Request = request;
            Response = response;
        }

        internal void SetQueryParameters(IReadOnlyDictionary<string, object> queryParameters)
        {
            QueryParameters = queryParameters;
        }

        internal void SetBodyObject(dynamic bodyObject)
        {
            BodyObject = bodyObject;
        }

        internal void SetSession(Session session)
        {
            Session = session;
        }

        internal void SetForHandler(RequestHandlerInfo handler)
        {
            var match = handler.GetMatch(Request.OriginalRequest.Url.AbsoluteUri);
            InitializeQueryParameters(handler, match);
            InitializeBodyObject(handler);

        }

        private void InitializeQueryParameters(RequestHandlerInfo handler, Match match)
        {
            var qsDict = new Dictionary<string, object>();
            var groupNames = handler.GetGroupNames();
            foreach (var n in groupNames)
            {
                if (n == "0") continue;
                if (qsDict.ContainsKey(n))
                {
                    if (qsDict[n].GetType() != typeof(List<object>))
                    {
                        qsDict[n] = new List<object> { qsDict[n] };
                    }
                    else
                    {
                        (qsDict[n] as List<object>).Add(match.Groups[n].Value);
                    }
                }
                else
                {
                    qsDict[n] = match.Groups[n].Value;
                }
            }
            SetQueryParameters(new ReadOnlyDictionary<string, object>(qsDict));
        }

        private void InitializeBodyObject(RequestHandlerInfo handler)
        {
            if (Request.OriginalRequest.HasEntityBody)
            {
                using (var sr = new System.IO.StreamReader(Request.OriginalRequest.InputStream, System.Text.Encoding.UTF8))
                {
                    var body = sr.ReadToEnd();
                    Request.SetBodyString(body);
                    var contentType = Request.OriginalRequest.ContentType;
                    if (handler.BodyDeserializer != null)
                    {
                        SetBodyObject(handler.BodyDeserializer(body));
                    }
                    else if (contentType == "application/json")
                    {
                        SetJsonBody(body);
                    }
                    else if (contentType == "application/x-www-form-urlencoded")
                    {
                        SetFormBody(body);
                    }
                    else SetBodyObject(body);
                }
            }
        }

        private void SetJsonBody(string body)
        {
            SetBodyObject((dynamic)Newtonsoft.Json.JsonConvert.DeserializeObject(body));
        }

        private void SetFormBody(string body)
        {
            var nameValuePairs = Helper.HttpUtility.ParseQueryString(body);
            var d = new Dictionary<string, string>();
            var keys = nameValuePairs.AllKeys;
            foreach (string key in keys)
            {
                d[key] = nameValuePairs[key];
            }
            SetBodyObject(d);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SelfHostingWebServer
{
    internal class RequestHandlerInfo
    {
        private static readonly Regex _mainRegex = new Regex(@"\{[^\}]+\}");

        private string _path;
        private Regex _parametersRegex;
        private List<string> _groupNames;
        private WebMethod _method;
        private Func<Context, object> _handlerFunc;

        public bool ShouldSerializeResult { get; }
        public Func<string, object> BodyDeserializer { get; }

        public RequestHandlerInfo(string path, WebMethod method, Func<Context, object> handlerFunc, bool shouldSerializeResult, Func<string, object> bodyDeserializer)
        {
            _path = path;
            _handlerFunc = handlerFunc;
            _method = method;
            ShouldSerializeResult = shouldSerializeResult;
            BodyDeserializer = bodyDeserializer;
            CreateParametersRegex();
        }


        public object GetResult(Context context)
        {
            return _handlerFunc(context);
        }

        public bool IsMatch(string path, string method)
        {
            if (Enum.GetName(typeof(WebMethod), _method) != method) return false;
            var match = _parametersRegex.Match(path);
            return match.Success;
        }

        public Match GetMatch(string path)
        {
            return _parametersRegex.Match(path);
        }

        public object Call(Context context)
        {
            return _handlerFunc(context);
        }

        public List<string> GetGroupNames()
        {
            return _groupNames;
        }


        private void CreateParametersRegex()
        {
            var parts = _mainRegex.Matches(_path);
            var temp = _path;
            foreach (Match m in parts)
            {
                var name = m.Value.Substring(1, m.Value.Length - 2);
                if (name == "*fullPath")
                {
                    var pos = temp.IndexOf(m.Value);
                    temp = temp.Substring(0, pos) + @"(?<__fullPath__>.+)";
                    break;
                }
                temp = temp.Replace(m.Value, @"(?<" + name + @">[^/]+)");
            }
            _parametersRegex = new Regex(temp);
            _groupNames = new List<string>(_parametersRegex.GetGroupNames());
        }

    }
}

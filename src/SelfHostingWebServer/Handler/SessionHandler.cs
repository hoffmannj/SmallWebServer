using System;
using System.Collections.Generic;
using System.Linq;

namespace SelfHostingWebServer.Handler
{
    internal sealed class SessionHandler
    {
        private readonly Dictionary<Guid, Session> _sessions;
        private readonly object _sessionLocker;

        public SessionHandler()
        {
            _sessions = new Dictionary<Guid, Session>();
            _sessionLocker = new object();
        }

        public void Set(Guid key, Session value)
        {
            lock (_sessionLocker)
            {
                _sessions[key] = value;
            }
        }

        public Session GetOrCreate(Guid key)
        {
            lock (_sessionLocker)
            {
                if (!_sessions.ContainsKey(key))
                {
                    _sessions[key] = new Session();
                }
                return _sessions[key];
            }
        }

        public void CleanTimedOutSessions()
        {
            lock (_sessionLocker)
            {
                var todelete = _sessions.Where(s => s.Value.IsTimedOut()).Select(s => s.Key).ToList();
                todelete.ForEach(key => _sessions.Remove(key));
            }
        }

    }
}

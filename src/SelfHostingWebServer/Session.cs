using System;
using System.Collections.Generic;

namespace SelfHostingWebServer
{
    public class Session
    {
        private readonly Dictionary<string, object> _storage;
        private DateTime _lastAccess;
        private TimeSpan _timeout;
        private readonly object _locker;

        public Session()
        {
            System.Diagnostics.Trace.WriteLine("new session");
            _locker = new object();
            _storage = new Dictionary<string, object>();
            _lastAccess = DateTime.Now;
            _timeout = TimeSpan.FromHours(1);
        }

        internal void SetTimeOut(TimeSpan newTimeOut)
        {
            lock (_locker)
            {
                _timeout = newTimeOut;
                _lastAccess = DateTime.Now;
            }
        }

        internal bool IsTimedOut()
        {
            lock (_locker)
            {
                return DateTime.Now - _lastAccess >= _timeout;
            }
        }

        internal void Touch()
        {
            lock (_locker)
            {
                _lastAccess = DateTime.Now;
            }
        }

        public object this[string index]
        {
            get
            {
                lock (_locker)
                {
                    _lastAccess = DateTime.Now;
                    return _storage[index];
                }
            }
            set
            {
                lock (_locker)
                {
                    _lastAccess = DateTime.Now;
                    _storage[index] = value;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            lock (_locker)
            {
                _lastAccess = DateTime.Now;
                return _storage.ContainsKey(key);
            }
        }

        public void DeleteKey(string key)
        {
            lock (_locker)
            {
                _lastAccess = DateTime.Now;
                if (_storage.ContainsKey(key)) return;
                _storage.Remove(key);
            }
        }
    }
}

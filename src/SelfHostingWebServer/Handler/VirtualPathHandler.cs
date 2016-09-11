using SelfHostingWebServer.Exceptions;
using System.Collections.Generic;
using System.IO;

namespace SelfHostingWebServer.Handler
{
    internal sealed class VirtualPathHandler
    {
        private string _basePath = string.Empty;

        internal void SetDefaultBasePath()
        {
            _basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        internal void SetBasePath(string path)
        {
            _basePath = Path.GetFullPath(path);
        }

        public string GetCurrentPath()
        {
            return _basePath;
        }

        public bool IsValidRelativePath(string relativePath)
        {
            return RelativePathToFullPath(relativePath).StartsWith(GetCurrentPath());
        }

        public string RelativePathToFullPath(string relativePath)
        {
            var path = GetCurrentPath();
            path = Path.Combine(path, relativePath);
            path = Path.GetFullPath(path);
            return path;
        }

        public string ReadFromLocalFile(string relativePath, Dictionary<string, string> cache)
        {
            if (cache != null && cache.ContainsKey(relativePath)) return cache[relativePath];
            var content = File.ReadAllText(RelativePathToFullPath(relativePath));
            if (cache != null) cache[relativePath] = content;
            return content;
        }

        public bool LocalFileExists(string relativePath, Dictionary<string, string> cache)
        {
            if (cache != null && cache.ContainsKey(relativePath)) return true;
            return File.Exists(RelativePathToFullPath(relativePath));
        }


        public string GetStaticFile(string relativePath, Dictionary<string, string> cache)
        {
            if (!IsValidRelativePath(relativePath) || !LocalFileExists(relativePath, cache)) throw new ErrorCodeException(404);
            if (cache != null && cache.ContainsKey(relativePath)) return cache[relativePath];
            var content = ReadFromLocalFile(relativePath, cache);
            if (cache != null) cache[relativePath] = content;
            return content;
        }
    }
}

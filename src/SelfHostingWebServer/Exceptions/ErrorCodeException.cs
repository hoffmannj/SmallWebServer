using System;
using System.Runtime.Serialization;

namespace SelfHostingWebServer.Exceptions
{
    [Serializable]
    public class ErrorCodeException : Exception
    {
        public int ErrorCode { get; }

        public ErrorCodeException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", ErrorCode);
        }
    }
}

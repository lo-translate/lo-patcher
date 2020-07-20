using System;
using System.Runtime.Serialization;

namespace LoPatcher.Patcher.Exceptions
{
    public class PatchFailedException : Exception
    {
        public PatchFailedException()
        {
        }

        public PatchFailedException(string message) : base(message)
        {
        }

        public PatchFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PatchFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
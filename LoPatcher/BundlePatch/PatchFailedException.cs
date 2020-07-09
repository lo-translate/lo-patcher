using System;
using System.Collections.Generic;
using System.Text;

namespace LoPatcher.BundlePatch
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
    }
}

using System;

namespace LoPatcher.LanguageUpdate
{
    public class UpdateResponse
    {
        public bool Success { get; internal set; }
        public Exception Error { get; internal set; }
    }
}
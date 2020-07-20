using System;

namespace LoPatcher.LanguageUpdate
{
    public class UpdateCheckResponse
    {
        public bool Success { get; internal set; }
        public Exception Error { get; internal set; }
        public Version Version { get; internal set; }
        public string UpdateLocation { get; internal set; }
    }
}
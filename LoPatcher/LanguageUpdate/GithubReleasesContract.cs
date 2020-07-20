using System.Runtime.Serialization;

#pragma warning disable CA1051 // Do not declare visible instance fields

namespace LoPatcher.LanguageUpdate
{
    [DataContract]
    public class GithubReleasesContract
    {
        [DataMember]
        public string tag_name;

        [DataMember]
        public string zipball_url;
    }
}

#pragma warning disable CA1051 // Do not declare visible instance fields
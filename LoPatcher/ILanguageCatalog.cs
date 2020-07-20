using System.Collections.Generic;

namespace LoPatcher
{
    public interface ILanguageCatalog
    {
        public string FindTranslation(string text);

        public IDictionary<string, string> AsDictionary();
    }
}
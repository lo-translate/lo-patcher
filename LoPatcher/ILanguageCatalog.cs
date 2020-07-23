using System.Collections.Generic;

namespace LoPatcher
{
    public interface ILanguageCatalog
    {
        public Dictionary<string, string> Catalog { get; }

        public string FindTranslation(string text);
    }
}
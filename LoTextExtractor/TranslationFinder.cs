using FileHelpers;
using LoPatcher;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoTextExtractor
{
    public class TranslationFinder
    {
        private readonly LanguageCatalog languageCatalog = new LanguageCatalog() { LoadComments = true };
        private readonly Dictionary<string, string> knownText = new Dictionary<string, string>();
        private readonly Dictionary<Regex, string> knownRegex = new Dictionary<Regex, string>();

        public TranslationFinder()
        {
            LoadKnownTextFromTsv("Resources/binfilepatcher-142_new.tsv");
            LoadKnownTextFromTsv("Resources/binfilepatcher-library.tsv");
            LoadKnownTextFromTsv("Resources/localefilepatcher-library.tsv");

            LoadKnownTextFromCsv("Resources/skilltool-known-effects.csv");
            LoadKnownTextFromCsv("Resources/skilltool-known-skills.csv");
            LoadKnownTextFromCsv("Resources/skilltool-skill-trans.csv");
            LoadKnownTextFromCsv("Resources/skilltool-strings.csv");

            LoadKnownRegexFromTsv("Resources/binfilepatcher-regex.tsv");

            var localFile = "LoTranslation.zip";
            var localFolder = @"..\..\..\..\..\LoTranslation";

            if (File.Exists(localFile))
            {
                languageCatalog.LoadTranslations(new FileInfo(localFile));
            }
            else if (Directory.Exists(localFolder))
            {
                languageCatalog.LoadTranslations(new DirectoryInfo(localFolder));
            }

            if (languageCatalog.Catalog.Any())
            {
                LoadKnownTextFromDictionary(languageCatalog.Catalog);
            }
        }

        public string FindComment(string koreanText, string japaneseText)
        {
            foreach (var foreignText in new[] { japaneseText, koreanText })
            {
                if (foreignText == null)
                {
                    continue;
                }

                if (languageCatalog.Comments.ContainsKey(foreignText))
                {
                    return languageCatalog.Comments[foreignText];
                }
            }

            return null;
        }

        public string FindTranslation(string[] foreignTexts)
        {
            foreach (var foreignText in foreignTexts)
            {
                if (foreignText == null)
                {
                    continue;
                }

                var translation = knownText.ContainsKey(foreignText) ? knownText[foreignText] : null;
                if (string.IsNullOrEmpty(translation) && foreignText.Contains('\n', System.StringComparison.Ordinal))
                {
                    // If we can't find a known translation try again using Windows style new lines. This is needed
                    // due to Karambolo.PO using it while parsing the PO file.
                    var normalizedForeignText = Regex.Replace(foreignText, @"\r\n|\n\r|\n|\r", "\r\n");
                    if (!normalizedForeignText.Equals(foreignText, System.StringComparison.Ordinal))
                    {
                        translation = knownText.ContainsKey(normalizedForeignText) ? knownText[normalizedForeignText] : null;
                    }
                }

                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }

                foreach (var regexKvp in knownRegex)
                {
                    var match = regexKvp.Key.Match(foreignText);
                    if (match.Success)
                    {
                        var replaced = regexKvp.Value.Replace("`", match.Groups[1].Value, System.StringComparison.Ordinal);

                        // Make sure we replaced something
                        if (replaced != match.Groups[1].Value)
                        {
                            return replaced;
                        }
                    }
                }
            }

            return null;
        }

        public string FindPartialTranslation(string[] foreignTexts)
        {
            foreach (var foreignText in foreignTexts)
            {

                var start = @"(^|[:：/]{1})";
                var end = @"($|[/\(（]{1})";

                var partialTranslation = foreignText;
                var knownParts = new Dictionary<Regex, string>()
                {
                    { new Regex("^きゃはは![:：]{1}"), "Hahaha!:" },
                    { new Regex("^サディズム[:：]{1}"), "Sadistic Nature:" },
                    { new Regex("^リベンジャー[:：]{1}"), "Revengeance:" },
                    { new Regex("^代謝促進[:：]{1}"), "Metabolism Boost:" },
                    { new Regex("^突進[:：]{1}"), "Rush:" },
                    { new Regex("^擬態[:：]{1}"), "Mimesis:" },
                    { new Regex("^超高速突進[:：]{1}"), "High-speed Rush:" },

                    { new Regex(start + @"ダメージ量アップ" + end), "$1 Increase Damage $2" },
                    { new Regex(start + @"スキル強化" + end), "$1 Skill Enhancement $2" },
                    { new Regex(start + @"標的指定" + end), "$1 Mark Target $2" },
                    { new Regex(start + @"対象挑発" + end), "$1 Taunt Target $2" },
                    { new Regex(start + @"バリア/ダメージ減少無視" + end), "$1 Ignore Shields / Damage Reduction $2" },
                    { new Regex(start + @"攻撃力/クリティカル率/行動力増加" + end), "$1 Attack / Crit Chance / Action Speed Up $2" },
                    { new Regex(start + @"\{0\}%の威力で反撃" + end), "$1 Counterattack with {0}% power $2" },

                    { new Regex(start + @"行動力(\+)?\{0\}%" + end), "$1 Action Speed $2{0}% $3" },
                    { new Regex(start + @"命中率(\+)?\{0\}%" + end), "$1 Accuracy $2{0}% $3" },
                    { new Regex(start + @"攻撃力(\+)?\{0\}%" + end), "$1 Attack $2{0}% $3" },
                    { new Regex(start + @"対軽装型ダメージ量(\+)?\{0\}%" + end), "$1 Anti-Light Damage $2{0}% $3" },
                    { new Regex(start + @"クリティカル率(\+)?\{0\}%" + end), "$1 Critical Hit Rate $2{0}% $3" },
                    { new Regex(start + @"ダメージ量(\+)?\{0\}%" + end), "$1 Damage $2{0}% $3" },
                    { new Regex(start + @"受けるダメージ(\+)?\{0\}%" + end), "$1 Damage Taken $2{0}% $3" },
                    { new Regex(start + @"防御力(\+)?\{0\}%" + end), "$1 Defense $2{0}% $3" },
                    { new Regex(start + @"防御貫通(\+)?\{0\}%" + end), "$1 Defense Penetration $2{0}% $3" },
                    { new Regex(start + @"経験値(\+)?\{0\}%" + end), "$1 Experience $2{0}% $3" },
                    { new Regex(start + @"射程距離(\+)?\{0\}" + end), "$1 Range $2{0} $3" },
                    { new Regex(start + @"対機動型ダメージ量(\+)\{0\}%" + end), "$1 $2{0}% Damage against Flying units $3" },

                    { new Regex(start + @"追加火炎ダメージ(\+)?\{0\}%" + end), "$1 Additional Fire Damage $2{0}% $3" },

                    { new Regex(start + @"火炎耐性(\+)?\{0\}%" + end), "$1 Resist Fire $2{0}% $3" },
                    { new Regex(start + @"電気耐性(\+)?\{0\}%" + end), "$1 Resist Electric $2{0}% $3" },
                    { new Regex(start + @"冷気耐性(\+)?\{0\}%" + end), "$1 Resist Ice $2{0}% $3" },
                    { new Regex(start + @"火全耐性(\+)?\{0\}%" + end), "$1 Resist Total $2{0}% $3" },
                    { new Regex(start + @"状態異常耐性(\+)?\{0\}%" + end), "$1 Resist Effects $2{0}% $3" },

                    { new Regex(start + @"受けるダメージ\{0\}%減少" + end), "$1 {0}% Damage Reduction $2" },
                    { new Regex(start + @"受けるダメージ\{0\}%ダウン" + end), "$1 {0}% Damage Reduction $2" },

                    { new Regex(start + @"回避率(\+)?\{0\}%" + end), "$1 Evasion $2{0}% $3" },
                    { new Regex(start + @"AP増加" + end), "$1 AP Up $2" },
                    { new Regex(start + @"攻撃支援" + end), "$1 Follow-up Attack $2" },
                    { new Regex(start + @"指定対象保護" + end), "$1 Protect Target $2" },
                    { new Regex(start + @"受けるダメージ減少" + end), "$1 Damage Reduction $2" },
                    { new Regex(start + @"防御力" + end), "$1 Defense $2" },
                    { new Regex(start + @"ダメージ減少" + end), "$1 Damage Reduction $2" },
                    { new Regex(start + @"防御力増加" + end), "$1 Defense Up $2" },
                    { new Regex(start + @"移動不可" + end), "$1 Rooted $2" },
                    { new Regex(start + @"行保護" + end), "$1 Row Protection $2" },
                    { new Regex(start + @"回避率減少" + end), "$1 Evasion Reduction $2" },
                    { new Regex(start + @"回避率増加" + end), "$1 Evasion Up $2" },
                    { new Regex(start + @"回避率" + end), "$1 Evasion $2" },
                    { new Regex(start + @"前に引き寄せ" + end), "$1 Pull Forward $2" },
                    { new Regex(start + @"バリア効果" + end), "$1 Shield Effect $2" },
                    { new Regex(start + @"反撃" + end), "$1 Counterattack $2" },
                    { new Regex(start + @"標的" + end), "$1 Mark $2" },
                    { new Regex(start + @"命中率" + end), "$1 Accuracy $2" },
                    { new Regex(start + @"電気耐性" + end), "$1 Resist Electric $2" },
                    { new Regex(start + @"状態異常耐性増加" + end), "$1 Effect Resist Up $2" },
                    { new Regex(start + @"有害な効果解除" + end), "$1 Remove Harmful Effects $2" },
                    { new Regex(start + @"行動力増加" + end), "$1 Action Speed Up $2" },
                    { new Regex(start + @"ダメージ増幅" + end), "$1 Increase Damage $2" },
                    { new Regex(start + @"行動力減少" + end), "$1 Reduced Action Speed $2" },
                    { new Regex(start + @"行動力" + end), "$1 Action Speed $2" },
                    { new Regex(start + @"攻撃力" + end), "$1 Attack $2" },
                    { new Regex(start + @"クリティカル率" + end), "$1 Critical Chance $2" },
                    { new Regex(start + @"クリティカル率増加" + end), "$1 Critical Hit Rate $2" },
                    { new Regex(start + @"受けるダメージ最小化" + end), "$1 Minimize Damage Received $2" },
                    { new Regex(start + @"ダメージ最小化" + end), "$1 Minimize Damage $2" },
                    { new Regex(start + @"後ろから攻撃するほどダメージ増加" + end), "$1 Increase damage dealt to further enemies $2" },

                    { new Regex(@"[\(（]バイオ[\)）]"), " (Bio)" },
                    { new Regex(@"[\(（]自分[\)）]"), " (Self)" },
                    { new Regex(@"[\(（]ロボット[\)）]"), " (Robot)" },
                    { new Regex(@"[\(（]保護機[\)）]"), " (Defender)" },
                    { new Regex(@"[\(（]腐食時[\)）]"), " (Corrosion)" },
                    { new Regex(@"[\(（]機動[\)）]"), " (Flying)" },
                    { new Regex(@"[\(（]攻撃機[\)）]"), " (Attacker)" },
                    { new Regex(@"[\(（]軽装/重装[\)）]"), " (Light/Heavy)" },
                    { new Regex(@"[\(（]重装[\)）]"), " (Heavy)" },
                    { new Regex(@"[\(（]軽装[\)）]"), " (Light)" },
                    { new Regex(@"[\(（]最大[\)）]"), " (Max)" },

                    { new Regex(@"[\(（]敵撃破時[\)）]"), " (On kill)" },
                    { new Regex(@"[\(（]攻撃時[\)）]"), " (On attack)" },
                    { new Regex(@"[\(（]味方撃破時[\)）]"), " (On ally death)" },
                    { new Regex(@"[\(（]クリティカル時[\)）]"), " (On critical)" },

                    { new Regex(@"[\(（]浸水時[\)）]"), " (When wet)" },
                };

                foreach (var kvpPart in knownParts)
                {
                    if (kvpPart.Key.IsMatch(partialTranslation))
                    {
                        partialTranslation = kvpPart.Key.Replace(partialTranslation, kvpPart.Value);
                    }
                }

                if (!partialTranslation.Equals(foreignText, System.StringComparison.Ordinal))
                {
                    return partialTranslation
                        // Just in case we added any, replace double spaces with single
                        .Replace("  ", " ")
                        // Some of the matches don't replace Unicode colons
                        .Replace("：", ":")
                        .Trim();
                }
            }

            return null;
        }

        public void LoadKnownTextFromDictionary(Dictionary<string, string> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var japaneseText = kvp.Key;
                var translation = kvp.Value;
                if (string.IsNullOrEmpty(translation))
                {
                    continue;
                }

                japaneseText = japaneseText.Replace("`n", "\n", System.StringComparison.Ordinal);
                translation = translation.Replace("`n", "\n", System.StringComparison.Ordinal);

                if (knownText.ContainsKey(japaneseText))
                {
                    if (!translation.Equals(knownText[japaneseText], System.StringComparison.Ordinal))
                    {
                        Debug.WriteLine($"Duplicate translation: '{translation}' != '{knownText[japaneseText]}'");
                    }

                    // We intentionally don't prevent the translation from being overwritten under the assumption
                    // the translation file contains the most up to date translations.
                }

                knownText[japaneseText] = translation;
            }
        }

        public void LoadKnownTextFromTsv(string input)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();

            using var stream = File.OpenRead(input);
            using var reader = new StreamReader(stream);

            AddTranslations(engine.ReadString(reader.ReadToEnd().Replace("`n", "\n", System.StringComparison.Ordinal)));
        }

        public void LoadKnownTextFromCsv(string input)
        {
            var engine = new FileHelperEngine<Translation.CsvTranslation>();

            using var stream = File.OpenRead(input);
            using var reader = new StreamReader(stream);

            AddTranslations(engine.ReadString(reader.ReadToEnd().Replace("`n", "\n", System.StringComparison.Ordinal)));
        }

        private void AddTranslations(Translation[] translations)
        {
            foreach (var translation in translations)
            {
                var englishText = translation.English;
                var koreanText = translation.Korean;

                englishText = englishText.Replace("…", "...", System.StringComparison.Ordinal);

                if (knownText.ContainsKey(koreanText))
                {
                    if (!englishText.Equals(knownText[koreanText], System.StringComparison.Ordinal))
                    {
                        Debug.WriteLine($"Duplicate translation: '{knownText[koreanText]}' != '{englishText}'");
                    }
                    continue;
                }

                knownText.Add(koreanText, englishText);
            }
        }

        public void LoadKnownRegexFromTsv(string input)
        {
            var engine = new FileHelperEngine<Translation.TsvTranslation>();
            var lines = engine.ReadFile(input);

            foreach (var translation in lines)
            {
                knownRegex.Add(new Regex($"^\\s+?{translation.Korean}\\s+?$"), translation.English);
            }
        }
    }
}
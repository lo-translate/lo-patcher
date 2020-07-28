using FileHelpers;
using LoPatcher;
using System;
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

        public string[] FindComments(string koreanText, string japaneseText)
        {
            foreach (var foreignText in new[] { japaneseText, koreanText })
            {
                if (foreignText == null)
                {
                    continue;
                }

                if (languageCatalog.Comments.ContainsKey(foreignText))
                {
                    return Regex.Split(languageCatalog.Comments[foreignText], @";\s?");
                }
            }

            return Array.Empty<string>();
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
                var end = @"($|[:：/\(（]{1})";

                var partialTranslation = foreignText;
                var knownParts = new Dictionary<Regex, string>()
                {
                    { new Regex(start + @"きゃはは!" + end), "$1 Hahaha! $2" },
                    { new Regex(start + @"サディズム" + end), "$1 Sadistic Nature $2" },
                    { new Regex(start + @"リベンジャー" + end), "$1 Revengeance $2" },
                    { new Regex(start + @"代謝促進" + end), "$1 Metabolism Boost $2" },
                    { new Regex(start + @"突進" + end), "$1 Rush $2" },
                    { new Regex(start + @"擬態" + end), "$1 Mimesis $2" },
                    { new Regex(start + @"超高速突進" + end), "$1 High-speed Rush $2" },

                    { new Regex(start + @"ダメージ量アップ" + end), "$1 Increase Damage $2" },
                    { new Regex(start + @"スキル強化" + end), "$1 Skill Enhancement $2" },
                    { new Regex(start + @"標的指定" + end), "$1 Mark Target $2" },
                    { new Regex(start + @"標的設定" + end), "$1 Mark Target $2" },
                    { new Regex(start + @"対象挑発" + end), "$1 Taunt Target $2" },
                    { new Regex(start + @"挑発" + end), "$1 Taunt $2" },
                    { new Regex(start + @"バリア/ダメージ減少無視" + end), "$1 Ignore Shields / Damage Reduction $2" },
                    { new Regex(start + @"攻撃力/クリティカル率/行動力増加" + end), "$1 Attack / Crit Chance / Action Speed Up $2" },
                    { new Regex(start + @"\{0\}%の威力で反撃" + end), "$1 Counterattack with {0}% power $2" },
                    { new Regex(start + @"AP\{0\}に変更" + end), "$1 Change AP to {0} $2" },
                    { new Regex(start + @"攻撃力増加" + end), "$1 Attack Up $2" },
                    { new Regex(start + @"後ろに突き飛ばし" + end), "$1 Push Back $2" },
                    { new Regex(start + @"AP増加" + end), "$1 AP Up $2" },
                    { new Regex(start + @"APアップ" + end), "$1 AP Up $2" },
                    { new Regex(start + @"AP減少" + end), "$1 AP Down $2" },
                    { new Regex(start + @"攻撃支援" + end), "$1 Follow-up Attack $2" },
                    { new Regex(start + @"指定対象保護" + end), "$1 Protect Target $2" },
                    { new Regex(start + @"受けるダメージ減少" + end), "$1 Damage Reduction $2" },
                    { new Regex(start + @"防御力" + end), "$1 Defense $2" },
                    { new Regex(start + @"防御力減少" + end), "$1 Defense Reduction $2" },
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
                    { new Regex(start + @"命中率増加" + end), "$1 Accuracy Up $2" },
                    { new Regex(start + @"命中率減少" + end), "$1 Accuracy Down $2" },
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
                    { new Regex(start + @"行動力減少解除" + end), "$1 Remove Action Speed Reduction $2" },
                    { new Regex(start + @"回避率減少解除" + end), "$1 Remove Evasion Reduction $2" },
                    { new Regex(start + @"命中率強化解除" + end), "$1 Remove Accuracy Buff $2" },
                    { new Regex(start + @"命中率増加解除" + end), "$1 Remove Hit Rate Buff $2" },
                    { new Regex(start + @"ダメージ減少効果解除" + end), "$1 Remove Damage Reduction $2" },
                    { new Regex(start + @"クリティカル率減少" + end), "$1 Reduced Crit Rate $2" },
                    { new Regex(start + @"行保護解除" + end), "$1 Remove Row Protection $2" },
                    { new Regex(start + @"列保護解除" + end), "$1 Remove Column Protection $2" },
                    { new Regex(start + @"混乱効果" + end), "$1 Confusion Effect $2" },
                    { new Regex(start + @"射程距離減少" + end), "$1 Reduced Range $2" },
                    { new Regex(start + @"後ろに\{0\}マス突き飛ばし" + end), "$1 Push Back {0} Spaces $2" },
                    { new Regex(start + @"一定確率で行動不可" + end), "$1 Chance to Stun $2" },

                    { new Regex(start + @"\{0\}HPで戦闘続行" + end), "$1 Continue Battle at {0} HP $2" },
                    { new Regex(start + @"自分のHP%が低いほどダメージ量増加" + end), "$1 Deal more damage as HP decreases $2" },
                    { new Regex(start + @"敵のHP%低いほどダメージ量増加" + end), "$1 Damage level increases as enemy HP decreases $2" },
                    { new Regex(start + @"敵HP%が低いほどダメージ量増加" + end), "$1 Damage amount increases as enemy HP decreases $2" },
                    { new Regex(start + @"敵HP%が低いほどダメージ量(\+)?\{0\}%" + end), "$1 Deal $2{0}% more damage as enemy HP decreases $3" },

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

                    { new Regex(start + @"浸水" + end), "$1 Wet $2" },
                    
                    // Guesses based on Wiki and partial results

                    // Alexandra
                    { new Regex(start + @"電荷集束" + end), "$1 Electric Charge $2" },
                    { new Regex(start + @"模範教師" + end), "$1 Role Model Teacher $2" },
                    { new Regex(start + @"電荷放出" + end), "$1 Charge Release $2" },
                    { new Regex(start + @"電磁場刺激(!)?" + end), "$1 Electric Field Stimulation$2 $3" },
                    { new Regex(start + @"電荷放出状態に変身" + end), "$1 Entered Electric Emission state $2" },
                    { new Regex(start + @"戦闘終了時、味方の獲得経験値増加" + end), "$1 After battle, increase EXP to allies $2" },
                    { new Regex(start + @"クリティカル時、ダメージ量増加" + end), "$1 Increase Critical Damage $2" },

                    // Alice
                    { new Regex(start + @"敵攻撃時、行動力増加/撃破時、一定確率でAP回復" + end), "$1 Small chance to refund AP on destroying enemy $2" },
                    { new Regex(start + @"対軽装型ダメージ量増加" + end), "$1 Increased Anti-Light Damage $2" },
                    { new Regex(start + @"バリア&ダメージ減少効果無視" + end), "$1 Ignore Shield and Damage Reduction $2" },
                    { new Regex(start + @"一定確率でAP回復" + end), "$1 Small chance to refund AP $2" },
                    
                    // Aqua
                    { new Regex(start + @"攻撃力\{0\}% 固定ダメージ" + end), "$1 Fixed damage {0}% $2" },

                    // Black Lilith
                    { new Regex(start + @"挑発対象ダメージ増加" + end), "$1 Additional damage to Provoked enemies $2" },
                    { new Regex(start + @"ダメージ無効\{0\}回" + end), "$1 Nullify Damage {0} times $2" },
                    { new Regex(start + @"ダメージ無効1回" + end), "$1 Nullify Damage once $2" },
                    { new Regex(start + @"ダメージ無効" + end), "$1 Nullify Damage $2" },
                    
                    // CS Perrault
                    { new Regex(start + @"猫の手を貸す" + end), "$1 Lend a Hand $2" },
                    { new Regex(start + @"柔軟性" + end), "$1 Flexibility $2" },
                    { new Regex(start + @"目覚めた本能" + end), "$1 Awakened Instinct $2" },
                    
                    // Daphne
                    { new Regex(start + @"隣接するバイオロイドの攻撃力" + end), "$1 Grant allies in Area of Effect Attack Up $2" },
                    
                    // Aeda
                    { new Regex(start + @"精密分析" + end), "$1 Accurate Analysis $2" },
                    { new Regex(start + @"隣接した味方の攻撃力" + end), "$1 Grant allies in Area of Effect Attack Up $2" },
                    { new Regex(start + @"防御貫通効果" + end), "$1 Defense Penetration $2" },
                    
                    // Albatross
                    { new Regex(start + @"AGS戦術指揮" + end), "$1 AGS Tactical Command $2" },
                    { new Regex(start + @"バリア\+\{0\}" + end), "$1 Shield +{0} $2" },

                    // Bulgasari
                    { new Regex(start + @"突撃支援" + end), "$1 Assault Support $2" },
                    { new Regex(start + @"\{0\}%確率攻撃支援" + end), "$1 Follow Up Attack {0}% $2" },

                    // Calista
                    { new Regex(start + @"精密砲撃" + end), "$1 Accurate Bombardment $2" },
                    { new Regex(start + @"指定対象保護解除" + end), "$1 Remove Target Protect $2" },
                    { new Regex(start + @"指定保護解除" + end), "$1 Remove Target Protect $2" },

                    // Daika
                    { new Regex(start + @"高性能レーダー" + end), "$1 High-Performance Radar $2" },
                    { new Regex(start + @"次の敵情報獲得" + end), "$1 Grant Recon effect $2" },
                    { new Regex(start + @"偵察機能" + end), "$1 Recon $2" }, // Recon Function?
                    { new Regex(start + @"命中率減少解除" + end), "$1 Remove Accuracy Reduction $2" },

                    // Djinnia
                    { new Regex(start + @"味方の回避率増加" + end), "$1 Grant allies in Area of Effect Evasion Up $2" },

                    // Doctor
                    { new Regex(start + @"実験対象募集!" + end), "$1 Recruiting Test Subjects! $2" },
                    { new Regex(start + @"50%強化解除" + end), "$1 50% chance to remove buffs $2" },
                    { new Regex(start + @"前に引き寄せる" + end), "$1 Pull Forward $2" },
                    { new Regex(start + @"ランダム強化効果" + end), "$1 Random Buff $2" },
                    { new Regex(start + @"敵の情報獲得" + end), "$1 Grant Recon $2" },
                    
                    // Emily
                    { new Regex(start + @"急速充電" + end), "$1 Quick Charge $2" },
                    { new Regex(start + @"出力強化" + end), "$1 Enhance Output $2" },
                    { new Regex(start + @"バリア/ダメージ減少効果無視" + end), "$1 Ignore Shield and Damage Reduction $2" },
                    { new Regex(start + @"防御貫通" + end), "$1 Defense Penetration $2" },
                    { new Regex(start + @"特殊防御無視" + end), "$1 Ignore Shield and Damage Reduction $2" },
                    { new Regex(start + @"バリア/ダメージ減少解除" + end), "$1 Remove Shield and Damage Reduction $2" },
                    //{ new Regex(start + @"ダメージ減少効果無視" + end), "$1 Ignore Damage Reduction $2" },
                    
                    // Gnome
                    { new Regex(start + @"援護射撃" + end), "$1 Support Fire $2" },

                    // At the end so replacing ( doesn't break matching of parts
                    { new Regex(@"[\(（]バイオ[\)）]"), " (Bio)" },
                    { new Regex(@"[\(（]自分[\)）]"), " (Self)" },
                    { new Regex(@"[\(（]ロボット[\)）]"), " (Robot)" },
                    { new Regex(@"[\(（]保護機[\)）]"), " (Defender)" },
                    { new Regex(@"[\(（]腐食時[\)）]"), " (Corrosion)" },
                    { new Regex(@"[\(（]機動[\)）]"), " (Flying)" },
                    { new Regex(@"[\(（]攻撃機[\)）]"), " (Attacker)" },
                    { new Regex(@"[\(（]攻撃機/支援機[\)）]"), " (Attacker/Supporter)" },
                    { new Regex(@"[\(（]支援機[\)）]"), " (Supporter)" },
                    { new Regex(@"[\(（]軽装/重装[\)）]"), " (Light/Heavy)" },
                    { new Regex(@"[\(（]重装[\)）]"), " (Heavy)" },
                    { new Regex(@"[\(（]軽装[\)）]"), " (Light)" },
                    { new Regex(@"[\(（]最大[\)）]"), " (Max)" },

                    { new Regex(@"[\(（]ハチコ[\)）]"), " (Hachiko)" },
                    { new Regex(@"[\(（]ブラックハウンド[\)）]"), " (Blackhound)" },
                    { new Regex(@"[\(（]サンドガール[\)）]"), " (Sand Girl)" },
                    { new Regex(@"[\(（]ウンディーネ[\)）]"), " (Undine)" },
                    { new Regex(@"[\(（]アタランテ[\)）]"), " (Atalanta)" },

                    { new Regex(@"[\(（]最大\+\{0\}%[\)）]"), " (Max +{0}%)" },

                    { new Regex(@"[\(（]3重複[\)）]"), " (3 stacks)" },
                    { new Regex(@"[\(（]5重複[\)）]"), " (5 stacks)" },

                    { new Regex(@"[\(（]敵撃破時[\)）]"), " (On kill)" },
                    { new Regex(@"[\(（]攻撃時[\)）]"), " (On attack)" },
                    { new Regex(@"[\(（]味方撃破時[\)）]"), " (On ally death)" },
                    { new Regex(@"[\(（]クリティカル時[\)）]"), " (On critical)" },

                    { new Regex(@"[\(（]浸水時[\)）]"), " (when wet)" },

                    { new Regex(@"[\(（]再臨[\)）]"), " (Second Coming)" },
                    { new Regex(@"[\(（]解除不可[\)）]"), " (Cannot be released)" },

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
                        .Replace("：", ": ") // Replace the Full-Width Colon with a regular
                        .Replace(" :", ":")  // Remove any spaces before colon characters
                        .Replace("/", " / ") // Make sure slashes are surrounded by spaces
                        .Replace("  ", " ")  // In case we added any, replace double spaces with single
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
using LoTextExtractor.Lo.Generated;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace LoTextExtractor
{
    internal sealed class BinaryFormatterBinder : SerializationBinder
    {
        private readonly Assembly assembly;

        public BinaryFormatterBinder() : base()
        {
            assembly = Assembly.GetExecutingAssembly();
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Debug.WriteLine($"Missing type: typeName={typeName}; assemblyName={assemblyName}");

            var updatedTypeName = typeName;

            // Loop through the Generated classes and replace any type requests with the new location
            foreach (var type in assembly.DefinedTypes)
            {
                if (updatedTypeName.Contains(type.Name) && !updatedTypeName.Contains(type.FullName) && type.FullName.Contains("Generated"))
                {
                    if (updatedTypeName == "LO_JsonReader.LoTableManager")
                    {
                        updatedTypeName = type.FullName;
                        continue;
                    }

                    updatedTypeName = updatedTypeName.Replace(type.Name, type.FullName);
                }
            }

            // See if NET can resolve the type
            var typeToDeserialize = Type.GetType($"{updatedTypeName}, {assembly.FullName}");

            if (typeToDeserialize == null)
            {
                // TODO See if these can be created through reflection

                var kvpString = "System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Table_";
                if (typeName.StartsWith(kvpString, StringComparison.Ordinal))
                {
                    var tableName = $"Table_{typeName.Substring(kvpString.Length).Split(",").First()}";

                    return GetKeyValuePair($"{tableName}");
                }

                var dictString = "System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Table_";
                if (typeName.StartsWith(dictString, StringComparison.Ordinal))
                {
                    var tableName = $"Table_{typeName.Substring(dictString.Length).Split(",").First()}";

                    return GetDictionary($"{tableName}");
                }

                if (typeName == "System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[Table_PCStory, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                {
                    return KeyValuePair.Create(0, new List<Table_PCStory>()).GetType();
                }

                if (typeName == "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[Table_PCStory, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                {
                    return new Dictionary<int, List<Table_PCStory>>().GetType();
                }

                if (typeName == "System.Collections.Generic.List`1[[Table_PCStory, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]")
                {
                    return new List<Table_PCStory>().GetType();
                }
            }

            if (typeName != updatedTypeName)
            {
                Debug.WriteLine($"Updated type: typeName={updatedTypeName}; assemblyName={assemblyName}");
            }

            return typeToDeserialize;
        }

        private static dynamic GetKeyValuePair(string name)
        {
            if (name == "Table_BuffEffect_Client") { return KeyValuePair.Create("", new Table_BuffEffect_Client()).GetType(); }
            else if (name == "Table_DialogScript") { return KeyValuePair.Create("", new Table_DialogScript()).GetType(); }
            else if (name == "Table_StartPackage") { return KeyValuePair.Create("", new Table_StartPackage()).GetType(); }
            else if (name == "Table_GlobalValue") { return KeyValuePair.Create("", new Table_GlobalValue()).GetType(); }
            else if (name == "Table_ItemConsumable") { return KeyValuePair.Create("", new Table_ItemConsumable()).GetType(); }
            else if (name == "Table_ItemEquip") { return KeyValuePair.Create("", new Table_ItemEquip()).GetType(); }
            else if (name == "Table_ItemMaterial") { return KeyValuePair.Create("", new Table_ItemMaterial()).GetType(); }
            else if (name == "Table_Function") { return KeyValuePair.Create("", new Table_Function()).GetType(); }
            else if (name == "Table_Enchant") { return KeyValuePair.Create("", new Table_Enchant()).GetType(); }
            else if (name == "Table_EnchantCost") { return KeyValuePair.Create("", new Table_EnchantCost()).GetType(); }
            else if (name == "Table_MapChapter") { return KeyValuePair.Create("", new Table_MapChapter()).GetType(); }
            else if (name == "Table_MapStage") { return KeyValuePair.Create("", new Table_MapStage()).GetType(); }
            else if (name == "Table_CutScene") { return KeyValuePair.Create("", new Table_CutScene()).GetType(); }
            else if (name == "Table_Localization") { return KeyValuePair.Create("", new Table_Localization()).GetType(); }
            else if (name == "Table_Reward") { return KeyValuePair.Create("", new Table_Reward()).GetType(); }
            else if (name == "Table_MobGroup") { return KeyValuePair.Create("", new Table_MobGroup()).GetType(); }
            else if (name == "Table_PC") { return KeyValuePair.Create("", new Table_PC()).GetType(); }
            else if (name == "Table_PCAbility") { return KeyValuePair.Create("", new Table_PCAbility()).GetType(); }
            else if (name == "Table_Monster") { return KeyValuePair.Create("", new Table_Monster()).GetType(); }
            else if (name == "Table_MonsterAbility") { return KeyValuePair.Create("", new Table_MonsterAbility()).GetType(); }
            else if (name == "Table_SkillGroup") { return KeyValuePair.Create("", new Table_SkillGroup()).GetType(); }
            else if (name == "Table_Skill") { return KeyValuePair.Create("", new Table_Skill()).GetType(); }
            else if (name == "Table_SkillLevel") { return KeyValuePair.Create("", new Table_SkillLevel()).GetType(); }
            else if (name == "Table_Grid") { return KeyValuePair.Create("", new Table_Grid()).GetType(); }
            else if (name == "Table_BuffEffect") { return KeyValuePair.Create("", new Table_BuffEffect()).GetType(); }
            else if (name == "Table_AccountExp") { return KeyValuePair.Create("", new Table_AccountExp()).GetType(); }
            else if (name == "Table_PCExp") { return KeyValuePair.Create("", new Table_PCExp()).GetType(); }
            else if (name == "Table_PCMaking") { return KeyValuePair.Create("", new Table_PCMaking()).GetType(); }
            else if (name == "Table_EquipSlot") { return KeyValuePair.Create("", new Table_EquipSlot()).GetType(); }
            else if (name == "Table_UnlockCondition") { return KeyValuePair.Create("", new Table_UnlockCondition()).GetType(); }
            else if (name == "Table_LobbyScript") { return KeyValuePair.Create("", new Table_LobbyScript()).GetType(); }
            else if (name == "Table_StartPackage") { return KeyValuePair.Create("", new Table_StartPackage()).GetType(); }
            else if (name == "Table_GlobalValue") { return KeyValuePair.Create("", new Table_GlobalValue()).GetType(); }
            else if (name == "Table_ItemConsumable") { return KeyValuePair.Create("", new Table_ItemConsumable()).GetType(); }
            else if (name == "Table_ItemEquip") { return KeyValuePair.Create("", new Table_ItemEquip()).GetType(); }
            else if (name == "Table_ItemMaterial") { return KeyValuePair.Create("", new Table_ItemMaterial()).GetType(); }
            else if (name == "Table_Function") { return KeyValuePair.Create("", new Table_Function()).GetType(); }
            else if (name == "Table_MapChapter") { return KeyValuePair.Create("", new Table_MapChapter()).GetType(); }
            else if (name == "Table_MapStage") { return KeyValuePair.Create("", new Table_MapStage()).GetType(); }
            else if (name == "Table_CutScene") { return KeyValuePair.Create("", new Table_CutScene()).GetType(); }
            else if (name == "Table_Localization") { return KeyValuePair.Create("", new Table_Localization()).GetType(); }
            else if (name == "Table_Reward") { return KeyValuePair.Create("", new Table_Reward()).GetType(); }
            else if (name == "Table_MobGroup") { return KeyValuePair.Create("", new Table_MobGroup()).GetType(); }
            else if (name == "Table_PC") { return KeyValuePair.Create("", new Table_PC()).GetType(); }
            else if (name == "Table_PCAbility") { return KeyValuePair.Create("", new Table_PCAbility()).GetType(); }
            else if (name == "Table_Monster") { return KeyValuePair.Create("", new Table_Monster()).GetType(); }
            else if (name == "Table_MonsterAbility") { return KeyValuePair.Create("", new Table_MonsterAbility()).GetType(); }
            else if (name == "Table_SkillGroup") { return KeyValuePair.Create("", new Table_SkillGroup()).GetType(); }
            else if (name == "Table_Skill") { return KeyValuePair.Create("", new Table_Skill()).GetType(); }
            else if (name == "Table_SkillLevel") { return KeyValuePair.Create("", new Table_SkillLevel()).GetType(); }
            else if (name == "Table_Grid") { return KeyValuePair.Create("", new Table_Grid()).GetType(); }
            else if (name == "Table_BuffEffect") { return KeyValuePair.Create("", new Table_BuffEffect()).GetType(); }
            else if (name == "Table_AccountExp") { return KeyValuePair.Create("", new Table_AccountExp()).GetType(); }
            else if (name == "Table_PCExp") { return KeyValuePair.Create("", new Table_PCExp()).GetType(); }
            else if (name == "Table_EquipSlot") { return KeyValuePair.Create("", new Table_EquipSlot()).GetType(); }
            else if (name == "Table_UnlockCondition") { return KeyValuePair.Create("", new Table_UnlockCondition()).GetType(); }
            else if (name == "Table_LobbyScript") { return KeyValuePair.Create("", new Table_LobbyScript()).GetType(); }
            else if (name == "Table_DialogScript") { return KeyValuePair.Create("", new Table_DialogScript()).GetType(); }
            else if (name == "Table_PCMaking") { return KeyValuePair.Create("", new Table_PCMaking()).GetType(); }
            else if (name == "Table_Enchant") { return KeyValuePair.Create("", new Table_Enchant()).GetType(); }
            else if (name == "Table_EnchantCost") { return KeyValuePair.Create("", new Table_EnchantCost()).GetType(); }
            else if (name == "Table_EquipMaking") { return KeyValuePair.Create("", new Table_EquipMaking()).GetType(); }
            else if (name == "Table_PCEnchant_Proportion") { return KeyValuePair.Create("", new Table_PCEnchant_Proportion()).GetType(); }
            else if (name == "Table_PCEnchant_StatUpValue") { return KeyValuePair.Create("", new Table_PCEnchant_StatUpValue()).GetType(); }
            else if (name == "Table_PCEnchant_ReinforValue") { return KeyValuePair.Create("", new Table_PCEnchant_ReinforValue()).GetType(); }
            else if (name == "Table_PCEnchant_ReqReinforValue") { return KeyValuePair.Create("", new Table_PCEnchant_ReqReinforValue()).GetType(); }
            else if (name == "Table_SkillLevelExp") { return KeyValuePair.Create("", new Table_SkillLevelExp()).GetType(); }
            else if (name == "Table_ServerErrString") { return KeyValuePair.Create("", new Table_ServerErrString()).GetType(); }
            else if (name == "Table_PCRestore") { return KeyValuePair.Create("", new Table_PCRestore()).GetType(); }
            else if (name == "Table_Disassemble") { return KeyValuePair.Create("", new Table_Disassemble()).GetType(); }
            else if (name == "Table_PCStory") { return KeyValuePair.Create("", new Table_PCStory()).GetType(); }
            else if (name == "Table_MissionObject") { return KeyValuePair.Create("", new Table_MissionObject()).GetType(); }
            else if (name == "Table_RequireResource") { return KeyValuePair.Create("", new Table_RequireResource()).GetType(); }
            else if (name == "Table_Summon") { return KeyValuePair.Create("", new Table_Summon()).GetType(); }
            else if (name == "Table_SummonAbility") { return KeyValuePair.Create("", new Table_SummonAbility()).GetType(); }
            else if (name == "Table_CharCollection") { return KeyValuePair.Create("", new Table_CharCollection()).GetType(); }
            else if (name == "Table_TroopCategory") { return KeyValuePair.Create("", new Table_TroopCategory()).GetType(); }
            else if (name == "Table_FixedSquadMember") { return KeyValuePair.Create("", new Table_FixedSquadMember()).GetType(); }
            else if (name == "Table_FavorReact") { return KeyValuePair.Create("", new Table_FavorReact()).GetType(); }
            else if (name == "Table_Quest") { return KeyValuePair.Create("", new Table_Quest()).GetType(); }
            else if (name == "Table_DailyStage") { return KeyValuePair.Create("", new Table_DailyStage()).GetType(); }
            else if (name == "Table_EventChapter") { return KeyValuePair.Create("", new Table_EventChapter()).GetType(); }
            else if (name == "Table_RobotMaking") { return KeyValuePair.Create("", new Table_RobotMaking()).GetType(); }
            else if (name == "Table_Research") { return KeyValuePair.Create("", new Table_Research()).GetType(); }
            else if (name == "Table_WaveEvent") { return KeyValuePair.Create("", new Table_WaveEvent()).GetType(); }
            else if (name == "Table_CharSkin") { return KeyValuePair.Create("", new Table_CharSkin()).GetType(); }
            else if (name == "Table_LoginReward") { return KeyValuePair.Create("", new Table_LoginReward()).GetType(); }
            else if (name == "Table_Gacha") { return KeyValuePair.Create("", new Table_Gacha()).GetType(); }
            else if (name == "Table_ShopDialog") { return KeyValuePair.Create("", new Table_ShopDialog()).GetType(); }
            else if (name == "Table_EventLoginReward") { return KeyValuePair.Create("", new Table_EventLoginReward()).GetType(); }
            else if (name == "Table_EventDesc") { return KeyValuePair.Create("", new Table_EventDesc()).GetType(); }
            else if (name == "Table_Tutorial") { return KeyValuePair.Create("", new Table_Tutorial()).GetType(); }
            else if (name == "Table_TutorialDialog") { return KeyValuePair.Create("", new Table_TutorialDialog()).GetType(); }
            else if (name == "Table_BattleCutIn") { return KeyValuePair.Create("", new Table_BattleCutIn()).GetType(); }
            else if (name == "Table_ShopDesc") { return KeyValuePair.Create("", new Table_ShopDesc()).GetType(); }
            else if (name == "Table_Forbidden") { return KeyValuePair.Create("", new Table_Forbidden()).GetType(); }
            else if (name == "Table_StageRewardView") { return KeyValuePair.Create("", new Table_StageRewardView()).GetType(); }
            else if (name == "Table_Announce") { return KeyValuePair.Create("", new Table_Announce()).GetType(); }
            else if (name == "Table_PCPromotion") { return KeyValuePair.Create("", new Table_PCPromotion()).GetType(); }
            else if (name == "Table_PCPromotionBonus") { return KeyValuePair.Create("", new Table_PCPromotionBonus()).GetType(); }
            else if (name == "Table_CoreLink") { return KeyValuePair.Create("", new Table_CoreLink()).GetType(); }
            else if (name == "Table_CoreLinkBonus") { return KeyValuePair.Create("", new Table_CoreLinkBonus()).GetType(); }
            else if (name == "Table_MapStageEW") { return KeyValuePair.Create("", new Table_MapStageEW()).GetType(); }
            else if (name == "Table_EWRewardDay") { return KeyValuePair.Create("", new Table_EWRewardDay()).GetType(); }
            else if (name == "Table_BuffEffect_Client+BuffDesc") { return KeyValuePair.Create("", new Table_BuffEffect_Client.BuffDesc()).GetType(); }

            throw new InvalidOperationException($"Unknown table {name}");
        }

        private static dynamic GetDictionary(string name)
        {
            if (name == "Table_BuffEffect_Client") { return new Dictionary<string, Table_BuffEffect_Client>().GetType(); }
            else if (name == "Table_DialogScript") { return new Dictionary<string, Table_DialogScript>().GetType(); }
            else if (name == "Table_StartPackage") { return new Dictionary<string, Table_StartPackage>().GetType(); }
            else if (name == "Table_GlobalValue") { return new Dictionary<string, Table_GlobalValue>().GetType(); }
            else if (name == "Table_ItemConsumable") { return new Dictionary<string, Table_ItemConsumable>().GetType(); }
            else if (name == "Table_ItemEquip") { return new Dictionary<string, Table_ItemEquip>().GetType(); }
            else if (name == "Table_ItemMaterial") { return new Dictionary<string, Table_ItemMaterial>().GetType(); }
            else if (name == "Table_Function") { return new Dictionary<string, Table_Function>().GetType(); }
            else if (name == "Table_Enchant") { return new Dictionary<string, Table_Enchant>().GetType(); }
            else if (name == "Table_EnchantCost") { return new Dictionary<string, Table_EnchantCost>().GetType(); }
            else if (name == "Table_MapChapter") { return new Dictionary<string, Table_MapChapter>().GetType(); }
            else if (name == "Table_MapStage") { return new Dictionary<string, Table_MapStage>().GetType(); }
            else if (name == "Table_CutScene") { return new Dictionary<string, Table_CutScene>().GetType(); }
            else if (name == "Table_Localization") { return new Dictionary<string, Table_Localization>().GetType(); }
            else if (name == "Table_Reward") { return new Dictionary<string, Table_Reward>().GetType(); }
            else if (name == "Table_MobGroup") { return new Dictionary<string, Table_MobGroup>().GetType(); }
            else if (name == "Table_PC") { return new Dictionary<string, Table_PC>().GetType(); }
            else if (name == "Table_PCAbility") { return new Dictionary<string, Table_PCAbility>().GetType(); }
            else if (name == "Table_Monster") { return new Dictionary<string, Table_Monster>().GetType(); }
            else if (name == "Table_MonsterAbility") { return new Dictionary<string, Table_MonsterAbility>().GetType(); }
            else if (name == "Table_SkillGroup") { return new Dictionary<string, Table_SkillGroup>().GetType(); }
            else if (name == "Table_Skill") { return new Dictionary<string, Table_Skill>().GetType(); }
            else if (name == "Table_SkillLevel") { return new Dictionary<string, Table_SkillLevel>().GetType(); }
            else if (name == "Table_Grid") { return new Dictionary<string, Table_Grid>().GetType(); }
            else if (name == "Table_BuffEffect") { return new Dictionary<string, Table_BuffEffect>().GetType(); }
            else if (name == "Table_AccountExp") { return new Dictionary<string, Table_AccountExp>().GetType(); }
            else if (name == "Table_PCExp") { return new Dictionary<string, Table_PCExp>().GetType(); }
            else if (name == "Table_PCMaking") { return new Dictionary<string, Table_PCMaking>().GetType(); }
            else if (name == "Table_EquipSlot") { return new Dictionary<string, Table_EquipSlot>().GetType(); }
            else if (name == "Table_UnlockCondition") { return new Dictionary<string, Table_UnlockCondition>().GetType(); }
            else if (name == "Table_LobbyScript") { return new Dictionary<string, Table_LobbyScript>().GetType(); }
            else if (name == "Table_StartPackage") { return new Dictionary<string, Table_StartPackage>().GetType(); }
            else if (name == "Table_GlobalValue") { return new Dictionary<string, Table_GlobalValue>().GetType(); }
            else if (name == "Table_ItemConsumable") { return new Dictionary<string, Table_ItemConsumable>().GetType(); }
            else if (name == "Table_ItemEquip") { return new Dictionary<string, Table_ItemEquip>().GetType(); }
            else if (name == "Table_ItemMaterial") { return new Dictionary<string, Table_ItemMaterial>().GetType(); }
            else if (name == "Table_Function") { return new Dictionary<string, Table_Function>().GetType(); }
            else if (name == "Table_MapChapter") { return new Dictionary<string, Table_MapChapter>().GetType(); }
            else if (name == "Table_MapStage") { return new Dictionary<string, Table_MapStage>().GetType(); }
            else if (name == "Table_CutScene") { return new Dictionary<string, Table_CutScene>().GetType(); }
            else if (name == "Table_Localization") { return new Dictionary<string, Table_Localization>().GetType(); }
            else if (name == "Table_Reward") { return new Dictionary<string, Table_Reward>().GetType(); }
            else if (name == "Table_MobGroup") { return new Dictionary<string, Table_MobGroup>().GetType(); }
            else if (name == "Table_PC") { return new Dictionary<string, Table_PC>().GetType(); }
            else if (name == "Table_PCAbility") { return new Dictionary<string, Table_PCAbility>().GetType(); }
            else if (name == "Table_Monster") { return new Dictionary<string, Table_Monster>().GetType(); }
            else if (name == "Table_MonsterAbility") { return new Dictionary<string, Table_MonsterAbility>().GetType(); }
            else if (name == "Table_SkillGroup") { return new Dictionary<string, Table_SkillGroup>().GetType(); }
            else if (name == "Table_Skill") { return new Dictionary<string, Table_Skill>().GetType(); }
            else if (name == "Table_SkillLevel") { return new Dictionary<string, Table_SkillLevel>().GetType(); }
            else if (name == "Table_Grid") { return new Dictionary<string, Table_Grid>().GetType(); }
            else if (name == "Table_BuffEffect") { return new Dictionary<string, Table_BuffEffect>().GetType(); }
            else if (name == "Table_AccountExp") { return new Dictionary<string, Table_AccountExp>().GetType(); }
            else if (name == "Table_PCExp") { return new Dictionary<string, Table_PCExp>().GetType(); }
            else if (name == "Table_EquipSlot") { return new Dictionary<string, Table_EquipSlot>().GetType(); }
            else if (name == "Table_UnlockCondition") { return new Dictionary<string, Table_UnlockCondition>().GetType(); }
            else if (name == "Table_LobbyScript") { return new Dictionary<string, Table_LobbyScript>().GetType(); }
            else if (name == "Table_DialogScript") { return new Dictionary<string, Table_DialogScript>().GetType(); }
            else if (name == "Table_PCMaking") { return new Dictionary<string, Table_PCMaking>().GetType(); }
            else if (name == "Table_Enchant") { return new Dictionary<string, Table_Enchant>().GetType(); }
            else if (name == "Table_EnchantCost") { return new Dictionary<string, Table_EnchantCost>().GetType(); }
            else if (name == "Table_EquipMaking") { return new Dictionary<string, Table_EquipMaking>().GetType(); }
            else if (name == "Table_PCEnchant_Proportion") { return new Dictionary<string, Table_PCEnchant_Proportion>().GetType(); }
            else if (name == "Table_PCEnchant_StatUpValue") { return new Dictionary<string, Table_PCEnchant_StatUpValue>().GetType(); }
            else if (name == "Table_PCEnchant_ReinforValue") { return new Dictionary<string, Table_PCEnchant_ReinforValue>().GetType(); }
            else if (name == "Table_PCEnchant_ReqReinforValue") { return new Dictionary<string, Table_PCEnchant_ReqReinforValue>().GetType(); }
            else if (name == "Table_SkillLevelExp") { return new Dictionary<string, Table_SkillLevelExp>().GetType(); }
            else if (name == "Table_ServerErrString") { return new Dictionary<string, Table_ServerErrString>().GetType(); }
            else if (name == "Table_PCRestore") { return new Dictionary<string, Table_PCRestore>().GetType(); }
            else if (name == "Table_Disassemble") { return new Dictionary<string, Table_Disassemble>().GetType(); }
            else if (name == "Table_PCStory") { return new Dictionary<string, Table_PCStory>().GetType(); }
            else if (name == "Table_MissionObject") { return new Dictionary<string, Table_MissionObject>().GetType(); }
            else if (name == "Table_RequireResource") { return new Dictionary<string, Table_RequireResource>().GetType(); }
            else if (name == "Table_Summon") { return new Dictionary<string, Table_Summon>().GetType(); }
            else if (name == "Table_SummonAbility") { return new Dictionary<string, Table_SummonAbility>().GetType(); }
            else if (name == "Table_CharCollection") { return new Dictionary<string, Table_CharCollection>().GetType(); }
            else if (name == "Table_TroopCategory") { return new Dictionary<string, Table_TroopCategory>().GetType(); }
            else if (name == "Table_FixedSquadMember") { return new Dictionary<string, Table_FixedSquadMember>().GetType(); }
            else if (name == "Table_FavorReact") { return new Dictionary<string, Table_FavorReact>().GetType(); }
            else if (name == "Table_Quest") { return new Dictionary<string, Table_Quest>().GetType(); }
            else if (name == "Table_DailyStage") { return new Dictionary<string, Table_DailyStage>().GetType(); }
            else if (name == "Table_EventChapter") { return new Dictionary<string, Table_EventChapter>().GetType(); }
            else if (name == "Table_RobotMaking") { return new Dictionary<string, Table_RobotMaking>().GetType(); }
            else if (name == "Table_Research") { return new Dictionary<string, Table_Research>().GetType(); }
            else if (name == "Table_WaveEvent") { return new Dictionary<string, Table_WaveEvent>().GetType(); }
            else if (name == "Table_CharSkin") { return new Dictionary<string, Table_CharSkin>().GetType(); }
            else if (name == "Table_LoginReward") { return new Dictionary<string, Table_LoginReward>().GetType(); }
            else if (name == "Table_Gacha") { return new Dictionary<string, Table_Gacha>().GetType(); }
            else if (name == "Table_ShopDialog") { return new Dictionary<string, Table_ShopDialog>().GetType(); }
            else if (name == "Table_EventLoginReward") { return new Dictionary<string, Table_EventLoginReward>().GetType(); }
            else if (name == "Table_EventDesc") { return new Dictionary<string, Table_EventDesc>().GetType(); }
            else if (name == "Table_Tutorial") { return new Dictionary<string, Table_Tutorial>().GetType(); }
            else if (name == "Table_TutorialDialog") { return new Dictionary<string, Table_TutorialDialog>().GetType(); }
            else if (name == "Table_BattleCutIn") { return new Dictionary<string, Table_BattleCutIn>().GetType(); }
            else if (name == "Table_ShopDesc") { return new Dictionary<string, Table_ShopDesc>().GetType(); }
            else if (name == "Table_Forbidden") { return new Dictionary<string, Table_Forbidden>().GetType(); }
            else if (name == "Table_StageRewardView") { return new Dictionary<string, Table_StageRewardView>().GetType(); }
            else if (name == "Table_Announce") { return new Dictionary<string, Table_Announce>().GetType(); }
            else if (name == "Table_PCPromotion") { return new Dictionary<string, Table_PCPromotion>().GetType(); }
            else if (name == "Table_PCPromotionBonus") { return new Dictionary<string, Table_PCPromotionBonus>().GetType(); }
            else if (name == "Table_CoreLink") { return new Dictionary<string, Table_CoreLink>().GetType(); }
            else if (name == "Table_CoreLinkBonus") { return new Dictionary<string, Table_CoreLinkBonus>().GetType(); }
            else if (name == "Table_MapStageEW") { return new Dictionary<string, Table_MapStageEW>().GetType(); }
            else if (name == "Table_EWRewardDay") { return new Dictionary<string, Table_EWRewardDay>().GetType(); }
            else if (name == "Table_BuffEffect_Client+BuffDesc") { return new Dictionary<string, Table_BuffEffect_Client.BuffDesc>().GetType(); }

            throw new InvalidOperationException($"Unknown table {name}");
        }
    }
}
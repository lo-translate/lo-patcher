using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_SkillLevel
    {
        public string Key { get; set; }
        public string SkillIndex { get; set; }
        public Int32 SkillLevel { get; set; }
        public Int32 NeedActionPoint { get; set; }
        public Int32 TurnDelay { get; set; }
        public string SkillAttackRate { get; set; }
        public System.Collections.Generic.List<System.String> BuffEffectIndex { get; set; }
        public Int32 BuffEffectLV { get; set; }
        public Int32 BuffEffectUseAvailable { get; set; }
        public string BuffEffectRate { get; set; }
        public string GridIndex { get; set; }
        public string GridRate { get; set; }
        public Int32 Distance { get; set; }
        public Int32 MagicSkillAttrType { get; set; }
        public Int32 IsSummonSkill { get; set; }
        public string SummonKeyString { get; set; }
        public Int32 SummonLevel { get; set; }
        public Int32 Is_Massive_Summon { get; set; }
        public double Accuracy { get; set; }
        public Int32 GuardPierce { get; set; }
        public string Use_BuffStack { get; set; }
        public Int32 Stack_Consume { get; set; }
    }
}
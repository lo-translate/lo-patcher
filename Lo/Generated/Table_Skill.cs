using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_Skill
    {
        public string Key { get; set; }
        public string SkillName { get; set; }
        public Int32 IsDefaultSkill { get; set; }
        public Int32 MaxSkillLevel { get; set; }
        public Int32 TargetType { get; set; }
        public Int32 TargetSubType { get; set; }
        public Int32 SkillLevelType { get; set; }
        public Int32 SkillType { get; set; }
        public Int32 SkillSubType { get; set; }
        public Int32 SkillSlotType { get; set; }
        public string SkillIconIDGrid { get; set; }
        public string SkillIconIDCircle { get; set; }
        public Int32 SkillActionRange { get; set; }
        public string SkillDescription { get; set; }
        public Int32 AcquireSkillExp { get; set; }
        public double SkillExpRatio { get; set; }
        public Int32 MultiAttackType { get; set; }
        public Int32 IsDelaySkill { get; set; }
        public Int32 DelaySkillType { get; set; }
        public string DelaySkillKeyString { get; set; }
        public Int32 DelaySkillRound { get; set; }
        public string SkillDescriptionSimple { get; set; }
        public string IconChange_Wait { get; set; }
        public string SimpleDescChange_Wait { get; set; }
        public string DescChange_Wait { get; set; }
        public string Matching_Skill_FormChange { get; set; }
    }
}
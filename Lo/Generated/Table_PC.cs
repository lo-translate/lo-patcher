using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_PC
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public Int32 IsUse { get; set; }
        public string GroupType { get; set; }
        public Int32 IsHavingTurn { get; set; }
        public Int32 Gender { get; set; }
        public Int32 ActorBodyType { get; set; }
        public Int32 ActorClassType { get; set; }
        public Int32 RoleType { get; set; }
        public string SkillGroupIndex { get; set; }
        public string SkillGroupIndex_CH { get; set; }
        public Int32 DefaultLevel { get; set; }
        public Int32 StartGrade { get; set; }
        public Int32 MaxGrade { get; set; }
        public Int32 ResellCoin { get; set; }
        public string PCModelID { get; set; }
        public string PCModelID_CH { get; set; }
        public string PCFullbodyID { get; set; }
        public string PCLongIMGID { get; set; }
        public string PCInvenIconID { get; set; }
        public string PCTbarIconID { get; set; }
        public string PCTbarIconID_CH { get; set; }
        public string PCETCIconID { get; set; }
        public string PCETCIconID_CH { get; set; }
        public string PC2DModelID { get; set; }
        public string DialogJoin { get; set; }
        public System.Collections.Generic.List<System.String> DialogJoinVoice { get; set; }
        public Int32 Is21Squad { get; set; }
        public double BattleClearFavorRatio { get; set; }
        public double DeathFavorRatio { get; set; }
        public double AssistantLoginFavorRatio { get; set; }
        public double GivePresentFavorRatio { get; set; }
        public Int32 Enchant_Exclusive { get; set; }
        public string PC2DModelID_Damage { get; set; }
        public string Skin_LobbyScript { get; set; }
        public Int32 Is_CoreUnit { get; set; }
    }
}
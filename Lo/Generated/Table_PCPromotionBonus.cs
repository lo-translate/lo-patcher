using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_PCPromotionBonus
    {
        public string Key { get; set; }
        public string Promotion_PCKey { get; set; }
        public Int32 PC_Grade { get; set; }
        public double AttackDmg_Bonus { get; set; }
        public double DefenseDmg_Bonus { get; set; }
        public double DefaultHP_Bonus { get; set; }
        public string ApplyRate_Bonus { get; set; }
        public string EvadeRate_Bonus { get; set; }
        public string CriRate_Bonus { get; set; }
        public string FireResist_Bonus { get; set; }
        public string IceResist_Bonus { get; set; }
        public string EletronicResist_Bonus { get; set; }
        public double TurnSpeed_Bonus { get; set; }
     }
}

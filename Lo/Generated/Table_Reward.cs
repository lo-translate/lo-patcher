using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_Reward
    {
        public string Key { get; set; }
        public Int32 RewardPosType { get; set; }
        public Int32 AccountExpMin { get; set; }
        public Int32 AccountExpMax { get; set; }
        public string AccountExpRate { get; set; }
        public Int32 ExpMin { get; set; }
        public Int32 ExpMax { get; set; }
        public string ExpRate { get; set; }
        public Int32 SkillExp { get; set; }
        public Int32 CoinMin { get; set; }
        public Int32 CoinMax { get; set; }
        public string CoinRate { get; set; }
        public Int32 CashMin { get; set; }
        public Int32 CashMax { get; set; }
        public string CashRate { get; set; }
        public Int32 MetalMin { get; set; }
        public Int32 MetalMax { get; set; }
        public string MetalRate { get; set; }
        public Int32 NutrientMin { get; set; }
        public Int32 NutrientMax { get; set; }
        public string NutrientRate { get; set; }
        public Int32 PowerMin { get; set; }
        public Int32 PowerMax { get; set; }
        public string PowerRate { get; set; }
        public System.Collections.Generic.List<System.Int32> ItemCount { get; set; }
        public System.Collections.Generic.List<System.String> ItemIndexString { get; set; }
        public System.Collections.Generic.List<System.String> ItemRate { get; set; }
        public System.Collections.Generic.List<System.String> CharIndexString { get; set; }
        public System.Collections.Generic.List<System.String> CharRate { get; set; }
    }
}
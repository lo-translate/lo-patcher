using System;

namespace LoTextExtractor.Lo.Generated
{
    [Serializable]
    public class Table_Research
    {
        public string Key { get; set; }
        public Int32 Research_Category { get; set; }
        public string Research_Name { get; set; }
        public string Research_Desc { get; set; }
        public Int32 Research_Result { get; set; }
        public string Research_ResultTypeValue { get; set; }
        public string Research_ResultDesc { get; set; }
        public System.Collections.Generic.List<System.String> Research_ReqQuest { get; set; }
        public System.Collections.Generic.List<System.String> Unlock_Condition { get; set; }
        public Int32 Research_Pos_Line { get; set; }
        public Int32 Research_Pos_Row { get; set; }
        public string Research_Icon { get; set; }
        public Int32 MetalCost { get; set; }
        public Int32 NutrientCost { get; set; }
        public Int32 PowerCost { get; set; }
        public System.Collections.Generic.List<System.String> NeedItemKeyString { get; set; }
        public System.Collections.Generic.List<System.Int32> NeedItemCount { get; set; }
        public Int32 Research_Time { get; set; }
    }
}
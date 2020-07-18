using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_TroopCategory
    {
        public string Key { get; set; }
        public Int32 Squad_Index { get; set; }
        public string Squad_Icon { get; set; }
        public string Squad_Name { get; set; }
        public string Squad_Desc { get; set; }
        public System.Collections.Generic.List<System.String> Squad_Member { get; set; }
        public System.Collections.Generic.List<System.String> Squad_CompleteReward { get; set; }
        public Int32 Req_Squad_Member { get; set; }
    }
}
using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_RobotMaking
    {
        public string Key { get; set; }
        public string PCKeyString { get; set; }
        public Int32 PCMakingTime { get; set; }
        public Int32 BluePrint_Idx { get; set; }
        public Int32 MetalCost { get; set; }
        public Int32 NutrientCost { get; set; }
        public Int32 PowerCost { get; set; }
        public System.Collections.Generic.List<System.String> NeedItemKeyString { get; set; }
        public System.Collections.Generic.List<System.Int32> NeedItemCount { get; set; }
        public Int32 NeedAICore { get; set; }
        public System.Collections.Generic.List<System.String> ReqResearch { get; set; }
     }
}

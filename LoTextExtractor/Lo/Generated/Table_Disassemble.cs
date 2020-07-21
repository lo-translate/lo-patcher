using System;

namespace LoTextExtractor.Lo.Generated
{
    [Serializable]
    public class Table_Disassemble
    {
        public string Key { get; set; }
        public Int32 DisassembleType { get; set; }
        public Int32 Grade { get; set; }
        public Int32 MetalObtain { get; set; }
        public Int32 NutrientObtain { get; set; }
        public Int32 PowerObtain { get; set; }
        public System.Collections.Generic.List<System.String> GiveItemKeyString { get; set; }
        public System.Collections.Generic.List<System.Int32> GiveItemCount { get; set; }
    }
}
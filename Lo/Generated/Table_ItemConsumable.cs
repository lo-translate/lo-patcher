using System;

namespace LoExtractText.Lo.Generated
{
    [Serializable]
    public class Table_ItemConsumable
    {
        public string Key { get; set; }
        public string ItemName { get; set; }
        public string ItemDesc { get; set; }
        public Int32 ItemType { get; set; }
        public Int32 ItemSubType { get; set; }
        public Int32 ItemGrade { get; set; }
        public Int32 CountLimit { get; set; }
        public Int32 ItemUseType { get; set; }
        public Int32 IsExpire { get; set; }
        public Int32 Price { get; set; }
        public Int32 IsResell { get; set; }
        public Int32 ResellCoin { get; set; }
        public Int32 StackCount { get; set; }
        public string FunctionIndex { get; set; }
        public string ImgName { get; set; }
        public System.Collections.Generic.List<System.String> DropImg { get; set; }
        public string DropDesc { get; set; }
        public Int32 IsGiftItem { get; set; }
        public string Usable_PC { get; set; }
        public string Mail_Link_PC { get; set; }
        public Int32 ProductType { get; set; }
        public Int32 Is_SkinItem { get; set; }
        public Int32 Is_RBReward { get; set; }
    }
}
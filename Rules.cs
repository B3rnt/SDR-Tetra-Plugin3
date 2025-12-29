using System;

namespace SDRSharp.Tetra
{
    public struct Rules
    {
        public Rules(GlobalNames globalName, int length, RulesType type = RulesType.Direct, int ext1 = 0, int ext2 = 0, int ext3 = 0)
        {
            GlobalName = globalName;
            Length = length;
            Type = type;
            Ext1 = ext1;
            Ext2 = ext2;
            Ext3 = ext3;
        }

        public GlobalNames GlobalName;
        public int Length;
        public RulesType Type;
        public int Ext1;
        public int Ext2;
        public int Ext3;
    }
}

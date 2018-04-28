using System.Runtime.InteropServices;

namespace xqLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int Magic;
        public short T0;
        public short T0_Count;
        public short T1;
        public short T1_Count;
        public short T2;
        public short T2_Count;
        public short T3;
        public short T3_Count;
        public short T4;
        public short Unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T0Entry
    {
        public int nameCRC32;
        public int nameOffset;
        public short T1Count;
        public short T1From;
        public short T2From;
        public short T2To;
        public int Unk5;
        public int Unk6;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T1Entry
    {
        public int nameCRC32;
        public int nameOffset;
        public int T2EntryId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T2Entry
    {
        public short FuncId;
        public short T3ArgCount;
        public short T3EntryId;
        public short Unk1;
        public int Unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T3Entry
    {
        public int Cmd;
        public int Value;
    }
}
using System.Runtime.InteropServices;

namespace xqLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public int Magic;
        public short T0_Count;
        public short T0;
        public short T1;
        public short T1_Count;
        public short T2;
        public short T2_Count;
        public short T3;
        public short T3_Count;
        public short Unk1;
        public short T4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T0Entry
    {
        public int nameOffset;
        public int CRC32;
        public short T2From;
        public short T2To;
        public short T1From;
        public short T1Count;
        public int Unk5;
        public int Unk6;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T1Entry
    {
        public int nameOffset;
        public int CRC32;
        public int T2EntryId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T2Entry
    {
        public short T3EntryId;
        public short T3ArgCount;
        public short Unk1;
        public short FuncId;
        public int Unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T3Entry
    {
        public int Cmd;
        public int Value;
    }
}
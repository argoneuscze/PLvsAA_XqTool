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
        public int dbg;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T1Entry
    {
        public int dbg;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T2Entry
    {
        public short T3Offset; // Offset into T3 * 8
        public short T3ArgCount;
        public short Unk1;
        public short Unk2;
        public int Unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T3Entry
    {
        public int Cmd;
        public int Value;
    }
}
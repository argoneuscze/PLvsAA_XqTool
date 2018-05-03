using System.Runtime.InteropServices;

namespace xqLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
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
    public class T0Entry
    {
        public int nameOffset;
        public int nameCRC32;
        public short T2From;
        public short T2To;
        public short T1From;
        public short T1Count;
        public int Unk5;
        public int Unk6;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T1Entry
    {
        public int nameOffset;
        public int nameCRC32;
        public int T2EntryId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T2Entry
    {
        public short T3EntryId;
        public short T3ArgCount;
        public short Unk1;
        public short FuncId;
        public int Unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class T3Entry
    {
        public uint Cmd;
        public uint Value;

        public static T3Entry StringOffset(uint offset)
        {
            return new T3Entry
            {
                Cmd = 0x18,
                Value = offset
            };
        }
    }
}
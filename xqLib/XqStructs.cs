using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xqLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public int Magic;
        public int Unk1;
        public int HeaderSize;
        public short T1;
        public short Unk2;
        public short T2;
        public int Unk3;
        public short T3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T1Entry
    {
        public short T2Offset; // Offset into T2 * 8
        public short T2CmdCount;
        public short Unk1;
        public short Unk2;
        public int Unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct T2Entry
    {
        public int Cmd;
        public int Value;
    }
}

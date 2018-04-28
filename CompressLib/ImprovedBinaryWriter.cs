using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CompressLib
{
    public class ImprovedBinaryWriter : BinaryWriter
    {
        public ImprovedBinaryWriter(Stream output) : base(output)
        {
        }

        public unsafe void WriteStruct<T>(T structure)
        {
            var buffer = new byte[Marshal.SizeOf<T>()];

            fixed (byte* pBuf = buffer)
            {
                Marshal.StructureToPtr(structure, (IntPtr) pBuf, false);
            }

            Write(buffer);
        }

        public void WritePadding(int count, byte padding = 0x0)
        {
            for (var i = 0; i < count; ++i)
                Write(padding);
        }

        public void WriteAlignment(int alignment = 4, byte padding = 0x0)
        {
            var length = BaseStream.Length;
            var missing = length % alignment;
            for (var i = 0; i < alignment - missing; ++i) Write(padding);
        }
    }
}
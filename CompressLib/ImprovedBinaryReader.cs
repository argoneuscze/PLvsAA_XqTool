using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CompressLib
{
    public class ImprovedBinaryReader : BinaryReader
    {
        public ImprovedBinaryReader(Stream input, bool leaveOpen = false) : base(input, Encoding.Default, leaveOpen)
        {
        }

        public string ReadCStringSJIS()
        {
            var enc = Encoding.GetEncoding("Shift-JIS");
            byte nextByte;
            var bytes = new List<byte>();
            while ((nextByte = ReadByte()) != 0) bytes.Add(nextByte);
            return enc.GetString(bytes.ToArray());
        }

        public unsafe T ReadStruct<T>()
        {
            var bytes = ReadBytes(Marshal.SizeOf<T>());
            fixed (byte* ptr = bytes)
            {
                return Marshal.PtrToStructure<T>((IntPtr) ptr);
            }
        }
    }
}
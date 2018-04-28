using System;
using System.IO;

namespace CompressLib
{
    public class Level5
    {
        public static byte[] Decompress(Stream stream)
        {
            using (var reader = new ImprovedBinaryReader(stream, true))
            {
                var header = reader.ReadInt32();

                var method = (Method) (header & 0x07);
                var size = header >> 3;

                switch (method)
                {
                    case Method.NoCompression:
                        return reader.ReadBytes(size);
                    case Method.LZ10:
                        return LZ10.Decompress(reader.BaseStream, size);
                    default:
                        throw new NotImplementedException("Unsupported compression method.");
                }
            }
        }

        private enum Method
        {
            NoCompression = 0,
            LZ10 = 1,
            Huffman4Bit = 2,
            Huffman8Bit = 3,
            RLE = 4
        }
    }
}
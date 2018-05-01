using System;
using System.IO;
using CompressLib.Compression;

namespace CompressLib
{
    public class Level5
    {
        public enum Method
        {
            NoCompression = 0,
            LZ10 = 1,
            Huffman4Bit = 2,
            Huffman8Bit = 3,
            RLE = 4
        }

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
                    case Method.RLE:
                        return RLE.Decompress(reader.BaseStream, size);
                    case Method.Huffman4Bit:
                        return Huffman.Decompress(reader.BaseStream, 4, size);
                    case Method.Huffman8Bit:
                        return Huffman.Decompress(reader.BaseStream, 8, size);
                    default:
                        throw new NotImplementedException($"Unsupported compression method: {method}");
                }
            }
        }

        public static byte[] Compress(Stream stream, Method method)
        {
            var methodAndSize = (uint) stream.Length << 3;
            using (var ms = new MemoryStream())
            {
                switch (method)
                {
                    case Method.LZ10:
                        methodAndSize |= 0x1;
                        using (var bw = new ImprovedBinaryWriter(ms))
                        {
                            bw.Write(methodAndSize);
                            var data = LZ10.Compress(stream);
                            bw.Write(data);
                            return ms.ToArray();
                        }
                    default:
                        throw new InvalidDataException("Invalid method specified.");
                }
            }
        }
    }
}
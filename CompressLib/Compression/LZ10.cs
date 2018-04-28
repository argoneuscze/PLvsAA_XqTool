using System;
using System.IO;

namespace CompressLib
{
    public class LZ10
    {
        public static byte[] Decompress(Stream stream, int decompressedSize = 0)
        {
            /*  Data header (32bit)
                  Bit 0-3   Reserved
                  Bit 4-7   Compressed type (must be 1 for LZ77)
                  Bit 8-31  Size of decompressed data
                Repeat below. Each Flag Byte followed by eight Blocks.
                Flag data (8bit)
                  Bit 0-7   Type Flags for next 8 Blocks, MSB first
                Block Type 0 - Uncompressed - Copy 1 Byte from Source to Dest
                  Bit 0-7   One data byte to be copied to dest
                Block Type 1 - Compressed - Copy N+3 Bytes from Dest-Disp-1 to Dest
                  Bit 0-3   Disp MSBs
                  Bit 4-7   Number of bytes to copy (minus 3)
                  Bit 8-15  Disp LSBs
             */

            byte[] output;

            var bytesWritten = 0;

            // read header if needed
            if (decompressedSize > 0)
                output = new byte[decompressedSize];
            else
                throw new NotImplementedException("LZ10 header unsupported.");

            int flag = 0, mask = 1;
            while (bytesWritten < decompressedSize)
            {
                // get new flag data if done
                if (mask == 1)
                {
                    flag = stream.ReadByte();
                    mask = 0x80;
                }
                else
                {
                    mask >>= 1;
                }

                var type = flag & mask;

                if (type == 0)
                {
                    // uncompressed
                    stream.Read(output, bytesWritten, 1);
                    bytesWritten++;
                }
                else
                {
                    // compressed
                    var data1 = stream.ReadByte();
                    var data2 = stream.ReadByte();

                    var disp = ((data1 & 0xF) << 8) | data2;

                    if (disp > bytesWritten) throw new InvalidDataException("DISP is too large.");

                    var copyCount = ((data1 & 0xF0) >> 4) + 3;
                    var src = bytesWritten - disp - 1;

                    for (var i = 0; i < copyCount; ++i) output[bytesWritten++] = output[src + i];
                }
            }

            return output;
        }
    }
}
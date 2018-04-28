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

        // taken from Kuriimu (https://github.com/IcySon55/Kuriimu/blob/master/src/Kontract/Compression/LZ10.cs)
        public static unsafe byte[] Compress(Stream instream)
        {
            // make sure the decompressed size fits in 3 bytes.
            // There should be room for four bytes, however I'm not 100% sure if that can be used
            // in every game, as it may not be a built-in function.
            var inLength = instream.Length;
            var outstream = new MemoryStream();

            // save the input data in an array to prevent having to go back and forth in a file
            var indata = new byte[inLength];
            var numReadBytes = instream.Read(indata, 0, (int) inLength);
            if (numReadBytes != inLength)
                throw new Exception("Input too short!");

            var compressedLength = 0;

            fixed (byte* instart = &indata[0])
            {
                // we do need to buffer the output, as the first byte indicates which blocks are compressed.
                // this version does not use a look-ahead, so we do not need to buffer more than 8 blocks at a time.
                var outbuffer = new byte[8 * 2 + 1];
                outbuffer[0] = 0;
                int bufferlength = 1, bufferedBlocks = 0;
                var readBytes = 0;
                while (readBytes < inLength)
                {
                    #region If 8 blocks are bufferd, write them and reset the buffer

                    // we can only buffer 8 blocks at a time.
                    if (bufferedBlocks == 8)
                    {
                        outstream.Write(outbuffer, 0, bufferlength);
                        compressedLength += bufferlength;
                        // reset the buffer
                        outbuffer[0] = 0;
                        bufferlength = 1;
                        bufferedBlocks = 0;
                    }

                    #endregion

                    // determine if we're dealing with a compressed or raw block.
                    // it is a compressed block when the next 3 or more bytes can be copied from
                    // somewhere in the set of already compressed bytes.
                    int disp;
                    var oldLength = Math.Min(readBytes, 0x1000);
                    var length = GetOccurrenceLength(instart + readBytes, (int) Math.Min(inLength - readBytes, 0x12),
                        instart + readBytes - oldLength, oldLength, out disp);

                    // length not 3 or more? next byte is raw data
                    if (length < 3)
                    {
                        outbuffer[bufferlength++] = *(instart + readBytes++);
                    }
                    else
                    {
                        // 3 or more bytes can be copied? next (length) bytes will be compressed into 2 bytes
                        readBytes += length;

                        // mark the next block as compressed
                        outbuffer[0] |= (byte) (1 << (7 - bufferedBlocks));

                        outbuffer[bufferlength] = (byte) (((length - 3) << 4) & 0xF0);
                        outbuffer[bufferlength] |= (byte) (((disp - 1) >> 8) & 0x0F);
                        bufferlength++;
                        outbuffer[bufferlength] = (byte) ((disp - 1) & 0xFF);
                        bufferlength++;
                    }

                    bufferedBlocks++;
                }

                // copy the remaining blocks to the output
                if (bufferedBlocks > 0)
                {
                    outstream.Write(outbuffer, 0, bufferlength);
                    compressedLength += bufferlength;
                }
            }

            return outstream.ToArray();
        }

        public static unsafe int GetOccurrenceLength(byte* newPtr, int newLength, byte* oldPtr, int oldLength,
            out int disp, int minDisp = 1)
        {
            disp = 0;
            if (newLength == 0)
                return 0;
            var maxLength = 0;
            // try every possible 'disp' value (disp = oldLength - i)
            for (var i = 0; i < oldLength - minDisp; i++)
            {
                // work from the start of the old data to the end, to mimic the original implementation's behaviour
                // (and going from start to end or from end to start does not influence the compression ratio anyway)
                var currentOldStart = oldPtr + i;
                var currentLength = 0;
                // determine the length we can copy if we go back (oldLength - i) bytes
                // always check the next 'newLength' bytes, and not just the available 'old' bytes,
                // as the copied data can also originate from what we're currently trying to compress.
                for (var j = 0; j < newLength; j++)
                {
                    // stop when the bytes are no longer the same
                    if (*(currentOldStart + j) != *(newPtr + j))
                        break;
                    currentLength++;
                }

                // update the optimal value
                if (currentLength > maxLength)
                {
                    maxLength = currentLength;
                    disp = oldLength - i;

                    // if we cannot do better anyway, stop trying.
                    if (maxLength == newLength)
                        break;
                }
            }

            return maxLength;
        }
    }
}
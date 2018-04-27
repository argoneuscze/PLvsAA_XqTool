using System.Collections.Generic;
using System.IO;
using Kontract.Compression;
using Kontract.IO;

namespace xqLib
{
    public class Xq
    {
        private readonly List<string> commands;

        public readonly List<string> debug;
        private readonly List<T1Entry> t1_list;
        private readonly List<T2Entry> t2_list;
        private Header header;

        public Xq(Stream stream)
        {
            t1_list = new List<T1Entry>();
            t2_list = new List<T2Entry>();
            commands = new List<string>();

            debug = new List<string>();

            ReadStream(stream);
        }

        private void ReadStream(Stream stream)
        {
            using (var reader = new BinaryReaderX(stream))
            {
                // header
                header = reader.ReadStruct<Header>();

                // table 1
                reader.BaseStream.Position = header.T1 << 2;
                using (var table1 = new BinaryReaderX(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table1.BaseStream.Position < table1.BaseStream.Length)
                        t1_list.Add(table1.ReadStruct<T1Entry>());
                }

                // table 2
                reader.BaseStream.Position = header.T2 << 2;
                using (var table2 = new BinaryReaderX(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table2.BaseStream.Position < table2.BaseStream.Length)
                        t2_list.Add(table2.ReadStruct<T2Entry>());
                }

                debug.Add("Header");
                debug.Add(
                    $"Size: {header.HeaderSize << 2:X}, T1: {header.T1 << 2:X}, T2: {header.T2 << 2:X}, T3: {header.T3 << 2:X}");
                debug.Add($"Unk1: {header.Unk1:X}, Unk2: {header.Unk2:X}, Unk3: {header.Unk3:X}");

                debug.Add("T1");

                foreach (var item in t1_list)
                {
                    debug.Add($"T2Off: {item.T2Offset:X}, CmdCount: {item.T2CmdCount:X}");
                    debug.Add($"Unk1: {item.Unk1:X}, Unk2: {item.Unk2:X}, Unk3: {item.Unk3:X}");
                }

                debug.Add("T2");

                // table 3
                foreach (var entry in t2_list)
                {
                    if (entry.Cmd >> 1 != 0xC) debug.Add($"Cmd: {entry.Cmd >> 1}, Value: {entry.Value:X}");

                    if (entry.Cmd >> 1 != 0xC) continue;

                    reader.BaseStream.Position = header.T3 << 2;
                    using (var text = new BinaryReaderX(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                    {
                        text.BaseStream.Position = entry.Value;
                        var str = text.ReadCStringSJIS();

                        debug.Add("Cmd: " + str);

                        commands.Add(str);
                    }
                }
            }
        }

        private void Save()
        {
            // TODO
        }
    }
}
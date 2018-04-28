using System.Collections.Generic;
using System.IO;
using CompressLib;

namespace xqLib
{
    public class Xq
    {
        private readonly List<string> commands;

        public readonly List<string> debug;

        private readonly List<T0Entry> t0_list;
        private readonly List<T1Entry> t1_list;
        private readonly List<T2Entry> t2_list;
        private readonly List<T3Entry> t3_list;
        private Header header;

        public Xq(Stream stream)
        {
            t0_list = new List<T0Entry>();
            t1_list = new List<T1Entry>();
            t2_list = new List<T2Entry>();
            t3_list = new List<T3Entry>();
            commands = new List<string>();

            debug = new List<string>();

            ReadStream(stream);
        }

        private void ReadStream(Stream stream)
        {
            using (var reader = new ImprovedBinaryReader(stream))
            {
                // header
                header = reader.ReadStruct<Header>();

                // table 0
                reader.BaseStream.Position = header.T0 << 2;
                using (var table0 = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table0.BaseStream.Position < table0.BaseStream.Length)
                        t0_list.Add(table0.ReadStruct<T0Entry>());
                }

                // table 1
                reader.BaseStream.Position = header.T1 << 2;
                using (var table1 = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table1.BaseStream.Position < table1.BaseStream.Length)
                        t1_list.Add(table1.ReadStruct<T1Entry>());
                }

                // table 2
                reader.BaseStream.Position = header.T2 << 2;
                using (var table2 = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table2.BaseStream.Position < table2.BaseStream.Length)
                        t2_list.Add(table2.ReadStruct<T2Entry>());
                }

                // table 3
                reader.BaseStream.Position = header.T3 << 2;
                using (var table3 = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table3.BaseStream.Position < table3.BaseStream.Length)
                        t3_list.Add(table3.ReadStruct<T3Entry>());
                }

                debug.Add("Header");
                debug.Add(
                    $"T0: {header.T0 << 2:X}, T1: {header.T1 << 2:X}, T2: {header.T2 << 2:X}, T3: {header.T3 << 2:X}");

                debug.Add("T0");

                foreach (var item in t0_list) debug.Add($"Val: {item.dbg:X8}");

                debug.Add("T2");

                foreach (var item in t2_list)
                {
                    debug.Add($"T2Off: {item.T3Offset:X}, ArgCount: {item.T3ArgCount:X}");
                    debug.Add($"Unk1: {item.Unk1:X}, Unk2: {item.Unk2:X}, Unk3: {item.Unk3:X}");
                }

                debug.Add("T2");

                // table 4
                foreach (var entry in t3_list)
                {
                    if (entry.Cmd >> 1 != 0xC) debug.Add($"Cmd: {entry.Cmd >> 1}, Value: {entry.Value:X}");

                    if (entry.Cmd >> 1 != 0xC) continue;

                    reader.BaseStream.Position = header.T4 << 2;
                    using (var text = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                    {
                        text.BaseStream.Position = entry.Value;
                        var str = text.ReadCStringSJIS();

                        debug.Add("String: " + str);

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
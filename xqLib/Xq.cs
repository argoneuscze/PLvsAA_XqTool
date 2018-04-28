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

                debug.Add("T0");

                foreach (var item in t0_list)
                    debug.Add(
                        $"nameOffset: {item.nameOffset:X8}, CRC: {item.CRC32:X8}, T1From: {item.T1From:X4}, T1Count: {item.T1Count:X4}, T2From: {item.T2From:X4}, T2To: {item.T2To:X4}, Unk5: {item.Unk5:X8}, Unk6: {item.Unk6:X8}");

                debug.Add("T1");

                foreach (var item in t1_list)
                    debug.Add(
                        $"nameOffset: {item.nameOffset:X8}, CRC32: {item.CRC32:X8}, T2Entry: {item.T2EntryId:X8}");

                debug.Add("T2");

                var cur = 0;
                foreach (var item in t2_list)
                    debug.Add(
                        $"ID: {cur++:X}, T3Off: {item.T3EntryId:X}, ArgCount: {item.T3ArgCount:X}, Unk1: {item.Unk1:X}, FuncId: {item.FuncId:X}, Unk3: {item.Unk3:X}");

                debug.Add("T3");

                cur = 0;
                foreach (var item in t3_list)
                    debug.Add(
                        $"ID: {cur++:X}, Cmd: {item.Cmd:X}, Val: {item.Value:X}");

                /*debug.Add("T3");
                
                var cur = 0;
                // table 4
                foreach (var entry in t3_list)
                {
                    debug.Add($"ID: {cur++:X}, Cmd: {entry.Cmd >> 1}, Value: {entry.Value:X}");

                    if (entry.Cmd >> 1 != 0xC) continue;

                    reader.BaseStream.Position = header.T4 << 2;
                    using (var text = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                    {
                        text.BaseStream.Position = entry.Value;
                        var str = text.ReadCStringSJIS();

                        //debug.Add("String: " + str);

                        commands.Add(str);
                    }
                }*/

                // print names in table 0
                debug.Add("T0 Names");
                foreach (var entry in t0_list)
                {
                    reader.BaseStream.Position = header.T4 << 2;
                    using (var text = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                    {
                        text.BaseStream.Position = entry.nameOffset;
                        var str = text.ReadCStringSJIS();

                        debug.Add("Name: " + str);
                    }
                }

                // print names in table 1
                debug.Add("T1 Names");
                foreach (var entry in t1_list)
                {
                    reader.BaseStream.Position = header.T4 << 2;
                    using (var text = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                    {
                        text.BaseStream.Position = entry.nameOffset;
                        var str = text.ReadCStringSJIS();

                        debug.Add("Name: " + str);
                    }
                }

                // parse commands in table 2
                debug.Add("Commands");
                foreach (var t2Entry in t2_list)
                {
                    debug.Add($"New Command: {t2Entry.FuncId:X}");

                    for (var i = 0; i < t2Entry.T3ArgCount; ++i)
                    {
                        var cmdArgEntry = t3_list[t2Entry.T3EntryId + i];

                        if (cmdArgEntry.Cmd >> 1 == 0xC)
                        {
                            reader.BaseStream.Position = header.T4 << 2;
                            using (var text =
                                new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                            {
                                text.BaseStream.Position = cmdArgEntry.Value;
                                var str = text.ReadCStringSJIS();

                                debug.Add($"ArgString: {str}");

                                commands.Add(str);
                            }
                        }
                        else
                        {
                            debug.Add($"ArgCmd: {cmdArgEntry.Cmd >> 1}, ArgValue: {cmdArgEntry.Value:X}");
                        }
                    }
                }
            }
        }

        public void Save(Stream file)
        {
            using (var writer = new ImprovedBinaryWriter(file))
            {
                writer.WriteStruct(header);
                writer.WritePadding(8);

                // TODO change header pointers between each step, write into an intermediate outstream

                using (var table0 = new ImprovedBinaryWriter(new MemoryStream()))
                {
                    foreach (var entry in t0_list) table0.WriteStruct(entry);

                    table0.BaseStream.Position = 0;
                    writer.Write(Level5.Compress(table0.BaseStream, Level5.Method.LZ10));
                    writer.WriteAlignment();
                }

                using (var table1 = new ImprovedBinaryWriter(new MemoryStream()))
                {
                    foreach (var entry in t1_list) table1.WriteStruct(entry);

                    table1.BaseStream.Position = 0;
                    writer.Write(Level5.Compress(table1.BaseStream, Level5.Method.LZ10));
                    writer.WriteAlignment();
                }

                using (var table2 = new ImprovedBinaryWriter(new MemoryStream()))
                {
                    foreach (var entry in t2_list) table2.WriteStruct(entry);

                    table2.BaseStream.Position = 0;
                    writer.Write(Level5.Compress(table2.BaseStream, Level5.Method.LZ10));
                    writer.WriteAlignment();
                }

                using (var table3 = new ImprovedBinaryWriter(new MemoryStream()))
                {
                    foreach (var entry in t3_list) table3.WriteStruct(entry);

                    table3.BaseStream.Position = 0;
                    writer.Write(Level5.Compress(table3.BaseStream, Level5.Method.LZ10));
                    writer.WriteAlignment();
                }

                // TODO write name data at the end
            }
        }
    }
}
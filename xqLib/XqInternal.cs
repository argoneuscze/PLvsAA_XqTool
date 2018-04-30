using System.Collections.Generic;
using System.IO;
using System.Text;
using CompressLib;

namespace xqLib
{
    public class XqInternal
    {
        public readonly List<T0Entry> t0_list;
        public readonly List<T1Entry> t1_list;
        public readonly List<T2Entry> t2_list;
        public readonly List<T3Entry> t3_list;
        public readonly List<byte> t4_data;
        public Header header;

        public XqInternal(Stream stream)
        {
            t0_list = new List<T0Entry>();
            t1_list = new List<T1Entry>();
            t2_list = new List<T2Entry>();
            t3_list = new List<T3Entry>();
            t4_data = new List<byte>();

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

                // table 4
                reader.BaseStream.Position = header.T4 << 2;
                using (var table4 = new ImprovedBinaryReader(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                {
                    while (table4.BaseStream.Position < table4.BaseStream.Length)
                        t4_data.Add(table4.ReadByte());
                }
            }
        }

        public void Save(Stream file)
        {
            using (var fw = new ImprovedBinaryWriter(file))
            {
                var outstream = new MemoryStream();
                short header_size = 0x20;

                var new_header = new Header
                {
                    Magic = header.Magic,
                    Unk1 = header.Unk1
                };

                byte[] data;

                using (var bw = new ImprovedBinaryWriter(outstream))
                {
                    // table 0
                    using (var table0 = new ImprovedBinaryWriter(new MemoryStream()))
                    {
                        new_header.T0 = (short) (header_size >> 2);
                        new_header.T0_Count = (short) t0_list.Count;

                        foreach (var entry in t0_list) table0.WriteStruct(entry);

                        table0.BaseStream.Position = 0;
                        bw.Write(Level5.Compress(table0.BaseStream, Level5.Method.LZ10));
                        bw.WriteAlignment(4);
                    }

                    // table 1
                    using (var table1 = new ImprovedBinaryWriter(new MemoryStream()))
                    {
                        new_header.T1 = (short) ((bw.BaseStream.Position + header_size) >> 2);
                        new_header.T1_Count = (short) t1_list.Count;

                        foreach (var entry in t1_list) table1.WriteStruct(entry);

                        table1.BaseStream.Position = 0;
                        bw.Write(Level5.Compress(table1.BaseStream, Level5.Method.LZ10));
                        bw.WriteAlignment(4);
                    }

                    // table 2
                    using (var table2 = new ImprovedBinaryWriter(new MemoryStream()))
                    {
                        new_header.T2 = (short) ((bw.BaseStream.Position + header_size) >> 2);
                        new_header.T2_Count = (short) t2_list.Count;

                        foreach (var entry in t2_list) table2.WriteStruct(entry);

                        table2.BaseStream.Position = 0;
                        bw.Write(Level5.Compress(table2.BaseStream, Level5.Method.LZ10));
                        bw.WriteAlignment(4);
                    }

                    // table 3
                    using (var table3 = new ImprovedBinaryWriter(new MemoryStream()))
                    {
                        new_header.T3 = (short) ((bw.BaseStream.Position + header_size) >> 2);
                        new_header.T3_Count = (short) t3_list.Count;

                        foreach (var entry in t3_list) table3.WriteStruct(entry);

                        table3.BaseStream.Position = 0;
                        bw.Write(Level5.Compress(table3.BaseStream, Level5.Method.LZ10));
                        bw.WriteAlignment(4);
                    }

                    // table 4 (string data)
                    using (var table4 = new ImprovedBinaryWriter(new MemoryStream()))
                    {
                        new_header.T4 = (short) ((bw.BaseStream.Position + header_size) >> 2);

                        table4.Write(t4_data.ToArray());

                        table4.BaseStream.Position = 0;
                        bw.Write(Level5.Compress(table4.BaseStream, Level5.Method.LZ10));
                        bw.WriteAlignment(4);
                    }

                    data = outstream.ToArray();
                }

                fw.WriteStruct(new_header);
                fw.WritePadding(8);

                fw.Write(data);
            }
        }

        public void RealignTextOffsets(int offset, int count, bool replacing = false)
        {
            // table 0
            for (var i = 0; i < t0_list.Count; ++i)
            {
                var entry = t0_list[i];
                if (!replacing && entry.nameOffset >= offset || replacing && entry.nameOffset > offset)
                    entry.nameOffset += count;
            }

            // table 1
            for (var i = 0; i < t1_list.Count; ++i)
            {
                var entry = t1_list[i];
                if (!replacing && entry.nameOffset >= offset || replacing && entry.nameOffset > offset)
                    entry.nameOffset += count;
            }

            // table 3
            for (var i = 0; i < t3_list.Count; ++i)
            {
                var entry = t3_list[i];
                if (entry.Cmd != 0x18) continue;
                if (!replacing && entry.Value >= offset || replacing && entry.Value > offset)
                    entry.Value += count;
            }
        }

        public List<string> GetDebugData()
        {
            var debug = new List<string>();
            var debug_2 = new List<string>();

            debug.Add("T0");

            foreach (var item in t0_list)
                debug.Add(
                    $"nameOffset: {item.nameOffset:X8}, nameCRC32: {item.nameCRC32:X8}, T1From: {item.T1From:X4}, T1Count: {item.T1Count:X4}, T2From: {item.T2From:X4}, T2To: {item.T2To:X4}, Unk5: {item.Unk5:X8}, Unk6: {item.Unk6:X8}");

            debug.Add("T1");

            foreach (var item in t1_list)
                debug.Add(
                    $"nameOffset: {item.nameOffset:X8}, nameCRC32: {item.nameCRC32:X8}, T2Entry: {item.T2EntryId:X8}");

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

            using (var reader = new ImprovedBinaryReader(new MemoryStream(t4_data.ToArray())))
            {
                // print names in table 0
                debug.Add("T0 Names");
                foreach (var entry in t0_list)
                    using (var text = new ImprovedBinaryReader(reader.BaseStream, true))
                    {
                        text.BaseStream.Position = entry.nameOffset;
                        var str = text.ReadCStringSJIS();

                        debug.Add("Name: " + str);
                    }

                // print names in table 1
                debug.Add("T1 Names");
                foreach (var entry in t1_list)
                    using (var text = new ImprovedBinaryReader(reader.BaseStream, true))
                    {
                        text.BaseStream.Position = entry.nameOffset;
                        var str = text.ReadCStringSJIS();

                        debug.Add("Name: " + str);
                    }
                // parse commands in table 2
                debug.Add("Commands");
                var cnt = 0;
                foreach (var t2Entry in t2_list)
                {
                    if (t2Entry.FuncId != 0x1B59)
                        continue;

                    debug.Add($"[{cnt++}] New Command: {t2Entry.FuncId:X}");

                    for (var i = 0; i < t2Entry.T3ArgCount; ++i)
                    {
                        var cmdArgEntry = t3_list[t2Entry.T3EntryId + i];
                        
                        if (t2Entry.FuncId == 0x14 && i == 0)
                        {
                            string opcode;
                            opcode = XqOpcodes.OpCodes.TryGetValue((uint) cmdArgEntry.Value, out opcode)
                                ? opcode
                                : "N/A";

                            /*
                            if (opcode != "Event3DCharaInit" && opcode != "EvenCrtBuildChara" &&
                                opcode != "Event3DCharaPlace_Crt")
                                break;
                            else
                            {
                                var chara = t3_list[t2Entry.T3EntryId + 1];
                                using (var text = new ImprovedBinaryReader(reader.BaseStream, true))
                                {
                                    text.BaseStream.Position = chara.Value;
                                    var str = text.ReadCStringSJIS();

                                    debug_2.Add($"opcode: {opcode}, string: {str}");
                                }
                            }*/


                            debug.Add($"ArgCmd: {cmdArgEntry.Cmd:X}, ArgValue: {cmdArgEntry.Value:X} [{opcode}]");
                        }
                        else if (cmdArgEntry.Cmd == 0x18)
                            using (var text = new ImprovedBinaryReader(reader.BaseStream, true))
                            {
                                text.BaseStream.Position = cmdArgEntry.Value;
                                var str = text.ReadCStringSJIS();

                                debug_2.Add(
                                    $"ArgCmd: {cmdArgEntry.Cmd:X}, StrOffset: {cmdArgEntry.Value:X}, ArgString: {str}");
                            }
                        else
                            debug_2.Add($"ArgCmd: {cmdArgEntry.Cmd:X}, ArgValue: {cmdArgEntry.Value:X}");
                    }
                }
            }

            // print bytes in last table
            var hex = new StringBuilder();
            foreach (var b in t4_data) hex.AppendFormat("{0:X2} ", b);
            debug.Add(hex.ToString());

            return debug_2;
        }
    }
}
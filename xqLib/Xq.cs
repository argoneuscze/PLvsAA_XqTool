using System.Collections.Generic;
using System.IO;
using Kontract.Compression;
using Kontract.IO;

namespace xqLib
{
    public class Xq
    {
        public List<string> commands;
        private Header header;
        private readonly List<T1Entry> t1_list;
        private readonly List<T2Entry> t2_list;

        public Xq(Stream stream)
        {
            t1_list = new List<T1Entry>();
            t2_list = new List<T2Entry>();
            commands = new List<string>();

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

                // table 3
                foreach (var entry in t2_list)
                {
                    if (entry.Cmd >> 1 != 0xC) continue;

                    reader.BaseStream.Position = header.T3 << 2;
                    using (var text = new BinaryReaderX(new MemoryStream(Level5.Decompress(reader.BaseStream))))
                    {
                        text.BaseStream.Position = entry.Value;
                        commands.Add(text.ReadCStringSJIS());
                    }
                }
            }
        }
    }
}
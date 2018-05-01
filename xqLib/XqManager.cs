using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CompressLib.Checksum;

namespace xqLib
{
    public partial class XqManager
    {
        private readonly XqInternal _xq;

        public XqManager(XqInternal xq)
        {
            _xq = xq;
        }

        public static XqManager FromStream(Stream stream)
        {
            var xq = new XqInternal(stream);
            return new XqManager(xq);
        }

        public List<string> GetDebugData()
        {
            return _xq.GetDebugData();
        }

        public List<string> dumpStrings()
        {
            return _xq.dumpStrings();
        }

        public void Save(Stream file)
        {/*
            // TODO put this somewhere better
            var layton = AddTextData(0x1B, "レイトン");
            var barnham = AddTextData(0x1B, "ジーケン");
            var chelmey = 0;

            var ba_off = 0x1B;
            var la_off = ba_off + barnham;
            var ch_off = ba_off + barnham + layton;

            // delete movie and chapter animation
            RemoveFunctionCall(10);
            RemoveFunctionCall(10);

            // delete chelmey
            RemoveFunctionCall(7);

            RemoveFunctionCall(1);
            RemoveFunctionCall(1);

            // spawn char
            //AddFunc_Event3DCharaInit(5, layton, 0x12, 0x9, 0x2, 0x1);
            //AddFunc_Event3DCharaInit(5, layton, 0x0, 0x9, 0x1, 0x1);
            AddFunc_Event3DCharaInit(5, ba_off, 0x22, 0x1, 0x1, 0x1);

            // init layton
            AddFunc_1B59(1, ba_off, 2);
            AddFunc_3FAD(1, 0x4, ba_off);

            RemoveFunctionCall(17);
            RemoveFunctionCall(17);
            */
            
            RemoveFunctionCall(16);

            AddFunctionCall(16, 0x14, new T3Entry
            {
                Cmd = 0x02,
                Value = 0x2038A49E
            });

            _xq.Save(file);
        }

        public int AddTextData(int offset, string value)
        {
            var bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(value + '\0');
            _xq.t4_data.InsertRange(offset, bytes);

            _xq.RealignTextOffsets(offset, bytes.Length);

            return bytes.Length;
        }

        public int DeleteTextData(int offset, bool realign = true)
        {
            var count = 0;
            while (_xq.t4_data[offset] != '\0')
            {
                _xq.t4_data.RemoveAt(offset);
                count++;
            }

            _xq.t4_data.RemoveAt(offset);
            count++;

            if (realign)
                _xq.RealignTextOffsets(offset, -count);

            return count;
        }

        public void ReplaceTextData(int offset, string value)
        {
            var bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(value + '\0');
            var old_len = DeleteTextData(offset, false);
            var diff = bytes.Length - old_len;

            _xq.t4_data.InsertRange(offset, bytes);
            _xq.RealignTextOffsets(offset, diff, true);
        }

        public void AddFunctionCall(short offset, short function, params T3Entry[] args)
        {
            var call = new T2Entry()
            {
                FuncId = function,
                T3ArgCount = (short) args.Length,
                Unk1 = 0x3E9,
                Unk3 = 0
            };

            // find T3 argument location
            var oldID = _xq.t2_list[offset].T3EntryId;
            call.T3EntryId = (short) (oldID);

            // add to table
            _xq.t2_list.Insert(offset, call);

            // add arguments to T3
            for (var i = 0; i < args.Length; ++i)
            {
                _xq.t3_list.Insert(call.T3EntryId + i, args[i]);
            }

            // move T3 offset of every successive call by argument length
            for (var i = offset + 1; i < _xq.t2_list.Count; ++i)
            {
                _xq.t2_list[i].T3EntryId += (short) args.Length;
            }

            // increase the containing sequence's range
            for (var i = 0; i < _xq.t0_list.Count; ++i)
            {
                var seq = _xq.t0_list[i];
                if (seq.T2From > offset)
                    seq.T2From++;
                if (seq.T2To >= offset)
                    seq.T2To++;
            }
        }

        public void RemoveFunctionCall(short offset)
        {
            var call = _xq.t2_list[offset];

            // reduce containing sequence's range
            for (var i = 0; i < _xq.t0_list.Count; ++i)
            {
                var seq = _xq.t0_list[i];
                if (seq.T2From > offset)
                    seq.T2From--;
                if (seq.T2To >= offset)
                    seq.T2To--;
            }

            // move T3 offset of every successive call by negative argument length
            for (var i = offset + 1; i < _xq.t2_list.Count; ++i)
            {
                _xq.t2_list[i].T3EntryId -= call.T3ArgCount;
            }

            // remove arguments from T3
            for (var i = 0; i < call.T3ArgCount; ++i)
            {
                _xq.t3_list.RemoveAt(call.T3EntryId);
            }

            // remove from list
            _xq.t2_list.RemoveAt(offset);
        }
    }
}
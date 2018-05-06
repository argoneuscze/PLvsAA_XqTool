using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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

        public List<string> dumpBuiltinFunctions(params string[] funcNames)
        {
            return _xq.dumpBuiltinFunctions(funcNames);
        }

        public void Save(Stream file)
        {
            /*
            // initialize string builder
            var strLength = ReplaceTextData(0x1B, "マホーネ");
            var newOffset = 0x1B + strLength;

            var strings = new StringManager(this, newOffset);
            uint chara = 0x1B;

            // delete movie and chapter animation
            RemoveFunctionCall(10);
            RemoveFunctionCall(10);

            // delete chelmey
            RemoveFunctionCall(7);

            // spawn char
            AddFunc_Event3DCharaInit(7, chara, 0, 2, 2, 2);

            // EventMapFadeRGB([3] 3F800000, [1] 0, [1] 0)
            RemoveFunctionCall(13);
            RemoveFunctionCall(12);
            AddFunc_EventMapFadeRGB(12, 0x3F800000, 0, 0);

            RemoveFunctionCall(9);
            RemoveFunctionCall(8);
            AddFunc_WaitFrame(8, 100);
            AddFunc_EventMapFadeRGB3(8, 1, 0, 0);
            
            // add strings for book
            strings.AddString("book_name", "ラビリンシア２");
            strings.AddString("book_file", "chr/evt/rabirinsia.xc");
            strings.AddString("book_motion", "051_マホーネ用本を抱える（封筒なし）");
            strings.AddString("book_motion2", "055_マホーネ用本を差し出す");
            strings.AddString("book_limb", "AR03");
            strings.AddString("chara_motion", "マホーネ拡張モーション");
            strings.AddString("chara_motion_file", "chr/c105_ma_book.xc");
            strings.AddString("chara_emote", "本差し出す");

            // attach model
            AddFunc_35A2(8, chara, strings.GetOffset("chara_motion"));
            AddFunc_EventExtMotionBuild(8, strings.GetOffset("chara_motion"),
                strings.GetOffset("chara_motion_file"));
            AddFunc_Event3DCharaNullAttach(8, chara, strings.GetOffset("book_limb"),
                strings.GetOffset("book_name"));
            AddFunc_Event3DChrMdl_Visible(8, strings.GetOffset("book_name"));
            AddFunc_Event3DChrMdl_SetMdlMtn(8, strings.GetOffset("book_name"), strings.GetOffset("book_motion"), 0, 0);
            AddFunc_Event3DChrMdl_Build2(8, strings.GetOffset("book_name"), strings.GetOffset("book_file"));

            // play thing
            AddFunc_Event3DChrMdl_SetMdlMtn(22, strings.GetOffset("book_name"), strings.GetOffset("book_motion2"), 0, 0);
            AddFunc_EventSetMtnByMtnSet(22, chara, strings.GetOffset("chara_emote"), 0);
            AddFunc_WaitFrame(22, 100);
            */

            var funcs = GetBuiltinFuncOffsets("EventCameraShakeL", "EventCameraShake_Ex", "EventFlash");
            RemoveFunctionCalls(funcs.ToArray());

            _xq.Save(file);
        }

        public uint AddTextData(uint offset, string value)
        {
            var bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(value + '\0');
            _xq.t4_data.InsertRange((int) offset, bytes);

            _xq.RealignTextOffsets((int) offset, bytes.Length);

            return (uint) bytes.Length;
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

        public uint ReplaceTextData(int offset, string value)
        {
            var bytes = Encoding.GetEncoding("Shift-JIS").GetBytes(value + '\0');
            var old_len = DeleteTextData(offset, false);
            var diff = bytes.Length - old_len;

            _xq.t4_data.InsertRange(offset, bytes);
            _xq.RealignTextOffsets(offset, diff, true);

            return (uint) bytes.Length;
        }

        public void AddFunctionCall(short offset, short function, params T3Entry[] args)
        {
            var call = new T2Entry
            {
                FuncId = function,
                T3ArgCount = (short) args.Length,
                Unk1 = 0x3E9,
                Unk3 = 0
            };

            // find T3 argument location
            var oldID = _xq.t2_list[offset].T3EntryId;
            call.T3EntryId = oldID;

            // add to table
            _xq.t2_list.Insert(offset, call);

            // add arguments to T3
            for (var i = 0; i < args.Length; ++i) _xq.t3_list.Insert(call.T3EntryId + i, args[i]);

            // move T3 offset of every successive call by argument length
            for (var i = offset + 1; i < _xq.t2_list.Count; ++i) _xq.t2_list[i].T3EntryId += (short) args.Length;

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
            for (var i = offset + 1; i < _xq.t2_list.Count; ++i) _xq.t2_list[i].T3EntryId -= call.T3ArgCount;

            // remove arguments from T3
            for (var i = 0; i < call.T3ArgCount; ++i) _xq.t3_list.RemoveAt(call.T3EntryId);

            // remove from list
            _xq.t2_list.RemoveAt(offset);
        }

        public void RemoveFunctionCalls(params short[] offsets)
        {
            var sorted = new SortedSet<short>(offsets);
            foreach (var offset in sorted.Reverse())
                RemoveFunctionCall(offset);
        }

        public List<short> GetBuiltinFuncOffsets(params string[] funcNames)
        {
            var list = new List<short>();

            for (var i = 0; i < _xq.t2_list.Count; ++i)
            {
                var func = _xq.t2_list[i];

                if (func.FuncId != 0x14)
                    continue;

                var funcHash = _xq.t3_list[func.T3EntryId].Value;
                if (!XqOpcodes.OpCodes.TryGetValue(funcHash, out var funcName)) continue;

                if (funcNames.Any(str => str == funcName))
                    list.Add((short) i);
            }

            return list;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CompressLib.Checksum;

namespace xqLib
{
    public class XqManager
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

        public void Save(Stream file)
        {
            // TODO put this somewhere better
            var length = AddTextData(0x1B, "レイトン");

            RemoveFunctionCall(1);
            RemoveFunctionCall(1);

            AddFunctionCall(1, 0x1B59,
                new T3Entry
                {
                    Cmd = 0x18,
                    Value = 0x1B
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x01
                }, new T3Entry
                {
                    Cmd = 0x18,
                    Value = 0x1B + length
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x01
                });

            AddFunctionCall(1, 0x3FAD,
                new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x04
                }, new T3Entry
                {
                    Cmd = 0x18,
                    Value = 0x1B
                },
                new T3Entry
                {
                    Cmd = 0x18,
                    Value = 0x1B + length
                });

            RemoveFunctionCall(7);

            AddFunctionCall(7, 0x14,
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = 0xDAECCFD
                }, new T3Entry
                {
                    Cmd = 0x18,
                    Value = 0x1B
                },
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = 0x11
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x08
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x01
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x01
                });

            AddFunctionCall(8, 0x14,
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = 0xDAECCFD
                }, new T3Entry
                {
                    Cmd = 0x18,
                    Value = 0x1B + length
                },
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = 0x22
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0xB
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x7
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = 0x2
                });

            //RemoveFunctionCall(6);
            //RemoveFunctionCall(6);
            //RemoveFunctionCall(6);
            //RemoveFunctionCall(6);

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
                _xq.t2_list[i].T3EntryId -= (short) call.T3ArgCount;
            }

            // remove arguments from T3
            for (var i = 0; i < call.T3ArgCount; ++i)
            {
                _xq.t3_list.RemoveAt(call.T3EntryId + i);
            }

            // remove from list
            _xq.t2_list.RemoveAt(offset);
        }
    }
}
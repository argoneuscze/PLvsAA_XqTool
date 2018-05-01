using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        {
            /*
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

            /*
            RemoveFunctionCall(16);

            AddFunctionCall(16, 0x14, new T3Entry
            {
                Cmd = 0x02,
                Value = 0x2038A49E
            });
            */

            // delete movies
            RemoveFunctionCall(11);
            RemoveFunctionCall(10);

            // delete rgb?
            RemoveFunctionCall(13);
            RemoveFunctionCall(12);
            RemoveFunctionCall(9);
            RemoveFunctionCall(8);

            // use own fade
            RemoveFunctionCall(0);
            AddFunc_14(0, 0x3BD4B06A, new T3Entry
            {
                Cmd = 0x1,
                Value = 0x64
            });
            RemoveFunctionCall(15);

            /*
            //AddFunc_14(10, 0x11B76BD2, custom1, zero, zero);// ???, delay, ???
            AddFunc_14(10, 0xFD8D3202, custom1, zero, custom2);

            //AddFunc_14(16, 0x11B76BD2, custom2, zero, zero);
            AddFunc_14(16, 0xFD8D3202, one, zero, custom2);

            //AddFunc_14(14, 0x11B76BD2, custom3, zero, zero);
            AddFunc_14(14, 0xFD8D3202, zero, zero, custom2);
            */

            uint waitDelay = 5;
            /*
            AddFunc_EventMapFadeRGB4(10, 1, 0, 0, 0);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB4(10, 1, 100, 100, 100);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB4(10, 0, 0, 0, 0);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB4(10, 0, 0, 0, 100);
            AddFunc_WaitFrame(10, waitDelay);
            
            AddFunc_EventMapFadeRGB3(10, 2, 0, 0);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 15, 0, 0);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 100, 0, 0);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 200, 0, 0);
            AddFunc_WaitFrame(10, waitDelay);

            AddFunc_EventMapFadeRGB3(10, 2, 0, 1);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 15, 0, 2);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 100, 0, 16);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 200, 0, 100);
            AddFunc_WaitFrame(10, waitDelay);
            */

            /*
            for (uint i = 0; i <= 2; ++i)
            {
                for (uint b1 = 0; b1 <= 0xFF; b1 += 0x3F)
                {
                    for (uint b2 = 0; b2 <= 0xFF; b2 += 0x3F)
                    {
                        for (uint b3 = 0; b3 <= 0xFF; b3 += 0x3F)
                        {
                            for (uint b4 = 0; b4 <= 0xFF; b4 += 0x3F)
                            {
                                uint val = 0;
                                val |= b1 << 24;
                                val |= b2 << 16;
                                val |= b3 << 8;
                                val |= b4;

                                AddFunc_WaitFrame(10, waitDelay);
                                AddFunc_EventGmpFadeRGB(10, i, 0, val);
                                AddFunc_EventMapFadeRGB3(10, i, 0, val);
                            }
                        }
                    }
                }
            }
            */

            uint val1 = 0;
            uint val2 = 0;

            uint thr = 0x5;
            uint skip = 0x1;

            for (uint b1 = 0; b1 <= thr; b1 += skip)
            {
                for (uint b2 = 0; b2 <= thr; b2 += skip)
                {
                    for (uint b3 = 0; b3 <= thr; b3 += skip)
                    {
                     //   for (uint b4 = 0; b4 <= thr; b4 += skip)
                       // {
                            val1 |= b1 << 16;
                            val1 |= b2 << 8;
                            val1 |= b3;
                         //   val1 |= b4;

                            for (uint bb1 = 0; bb1 <= thr; bb1 += skip)
                            {
                                for (uint bb2 = 0; bb2 <= thr; bb2 += skip)
                                {
                                    for (uint bb3 = 0; bb3 <= thr; bb3 += skip)
                                    {
                                    //    for (uint bb4 = 0; bb4 <= thr; bb4 += skip)
                                      //  {
                                            val2 |= bb1 << 16;
                                            val2 |= bb2 << 8;
                                            val2 |= bb3;
                                         //   val2 |= bb4;

                                            AddFunc_WaitFrame(10, waitDelay);
                                            AddFunc_EventMapFadeRGB3(10, val1, 0, val2);
                                        //}
                                    }
                                }
                            }
                        //}
                    }
                }
            }

            /*
            AddFunc_EventMapFadeRGB3(10, 2, 0, 0xFF000000);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 2, 0, 0x00FF0000);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 2, 0, 0x0000FF00);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 2, 0, 0x000000FF);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 20, 0, 200);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 200, 0, 200);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 500, 0, 500);
            AddFunc_WaitFrame(10, waitDelay);
            AddFunc_EventMapFadeRGB3(10, 2000, 0, 2000);
            AddFunc_WaitFrame(10, waitDelay);*/

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
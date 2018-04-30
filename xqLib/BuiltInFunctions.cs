using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xqLib
{
    public partial class XqManager
    {
        public void AddFunc_3FAD(short insertIndex, int unk1, params int[] charNameOffsets)
        {
            /*
             * Seems to initialize characters
             * unk1 seems to be 0x04 by default
             */

            var entries = new List<T3Entry>
            {
                new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk1
                }
            };

            entries.AddRange(charNameOffsets.Select(t => new T3Entry
            {
                Cmd = 0x18,
                Value = t
            }));

            AddFunctionCall(insertIndex, 0x3FAD, entries.ToArray());
        }

        public void AddFunc_1B59(short insertIndex, int charNameOffset, int emoteId, bool isString = false)
        {
            /*
             * Seems to play character emotes
             */

            AddFunctionCall(insertIndex, 0x1B59,
                new T3Entry
                {
                    Cmd = 0x18,
                    Value = charNameOffset
                }, new T3Entry
                {
                    Cmd = isString ? 0x18 : 0x01,
                    Value = emoteId
                });
        }

        public void AddFunc_Event3DCharaInit(short insertIndex, int charNameOffset,
            int pos, int emote1, int emote2, int unk4)
        {
            /*
             * pos:
             *  0x00 - witness
             *  0x22 - right
             *  0x12 - left
             */

            AddFunctionCall(insertIndex, 0x14,
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = 0xDAECCFD
                }, new T3Entry
                {
                    Cmd = 0x18,
                    Value = charNameOffset
                },
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = pos
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = emote1
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = emote2
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk4
                });
        }
    }
}
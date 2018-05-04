using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xqLib
{
    public partial class XqManager
    {
        public void AddFunc_3FAD(short insertIndex, uint unk1, params int[] charNameOffsets)
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
                Value = (uint) t
            }));

            AddFunctionCall(insertIndex, 0x3FAD, entries.ToArray());
        }

        public void AddFunc_1B59(short insertIndex, uint charNameOffset, uint emoteId, bool isString = false)
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
                    Cmd = isString ? (uint) 0x18 : 0x01,
                    Value = emoteId
                });
        }

        public void AddFunc_14(short insertIndex, uint opCode, params T3Entry[] args)
        {
            /*
             * Adds a command that controls the scene, see opcodes
             */

            var allArgs = new List<T3Entry>
            {
                new T3Entry
                {
                    Cmd = 0x02,
                    Value = opCode
                }
            };
            allArgs.AddRange(args);

            AddFunctionCall(insertIndex, 0x14, allArgs.ToArray());
        }

        public void AddFunc_WaitFrame(short insertIndex, uint length)
        {
            AddFunc_14(insertIndex, 0xD0FD3A01, new T3Entry
            {
                Cmd = 0x01,
                Value = length
            });
        }

        public void AddFunc_EventSetMtnByMtnSet(short insertIndex, uint charNameOffset, uint emoteNameOffset, uint unk1)
        {
            AddFunc_14(insertIndex, 0x322EDC26, T3Entry.StringOffset(charNameOffset),
                T3Entry.StringOffset(emoteNameOffset), new T3Entry
                {
                    Cmd = 1,
                    Value = unk1
                });
        }

        public void AddFunc_EventMapFadeRGB(short insertIndex, uint unk1, uint unk2, uint unk3)
        {
            AddFunc_14(insertIndex, 0xE0A1080F,
                new T3Entry
                {
                    Cmd = 0x03,
                    Value = unk1
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk2
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk3
                });
        }

        public void AddFunc_EventMapFadeRGB3(short insertIndex, uint unk1, uint length, uint unk3)
        {
            AddFunc_14(insertIndex, 0xFD8D3202,
                new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk1
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = length
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk3
                });
        }

        public void AddFunc_EventGmpFadeRGB(short insertIndex, uint unk1, uint length, uint unk3)
        {
            AddFunc_14(insertIndex, 0x11B76BD2,
                new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk1
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = length
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk3
                });
        }

        public void AddFunc_EventMapFadeRGB4(short insertIndex, uint unk1, uint unk2, uint unk3, uint unk4)
        {
            AddFunc_14(insertIndex, 0x63E9A7A1,
                new T3Entry
                {
                    Cmd = 0x04,
                    Value = unk1
                }, new T3Entry
                {
                    Cmd = 0x04,
                    Value = unk2
                }, new T3Entry
                {
                    Cmd = 0x04,
                    Value = unk3
                }, new T3Entry
                {
                    Cmd = 0x01,
                    Value = unk4
                });
        }

        public void AddFunc_Event3DCharaInit(short insertIndex, uint charNameOffset,
            uint pos, uint emote1, uint emote2, uint unk4)
        {
            /*
             * pos:
             *  0x00 - witness
             *  0x22 - right
             *  0x12 - left
             */

            AddFunc_14(insertIndex, 0xDAECCFD,
                T3Entry.StringOffset(charNameOffset),
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

        public void AddFunc_Event3DChrMdl_Build2(short insertIndex, uint nameOffset, uint fileOffset)
        {
            AddFunc_14(insertIndex, 0x469FC6D7, T3Entry.StringOffset(nameOffset), T3Entry.StringOffset(fileOffset));
        }

        public void AddFunc_Event3DChrMdl_SetMdlMtn(short insertIndex, uint nameOffset, uint modelMotionOffset,
            uint unk1, uint unk2)
        {
            AddFunc_14(insertIndex, 0x35A7DED7, T3Entry.StringOffset(nameOffset),
                T3Entry.StringOffset(modelMotionOffset),
                new T3Entry
                {
                    Cmd = 1,
                    Value = unk1
                },
                new T3Entry
                {
                    Cmd = 1,
                    Value = unk2
                });
        }

        public void AddFunc_Event3DChrMdl_Visible(short insertIndex, uint nameOffset)
        {
            AddFunc_14(insertIndex, 0x2D95ABAF, T3Entry.StringOffset(nameOffset));
        }

        public void AddFunc_Event3DCharaNullAttach(short insertIndex, uint charNameOffset, uint limbOffset,
            uint nameOffset)
        {
            AddFunc_14(insertIndex, 0x297890D7, T3Entry.StringOffset(charNameOffset), T3Entry.StringOffset(limbOffset),
                T3Entry.StringOffset(nameOffset));
        }

        public void AddFunc_EventExtMotionBuild(short insertIndex, uint motionNameOffset, uint motionFileOffset)
        {
            AddFunc_14(insertIndex, 0xD40F2917, T3Entry.StringOffset(motionNameOffset),
                T3Entry.StringOffset(motionFileOffset));
        }

        public void AddFunc_35A2(short insertIndex, uint charNameOffset, uint motionNameOffset)
        {
            /*
             * Seems related to setting character model motions.
             */

            AddFunctionCall(insertIndex, 0x35A2, T3Entry.StringOffset(charNameOffset),
                T3Entry.StringOffset(motionNameOffset));
        }
    }
}
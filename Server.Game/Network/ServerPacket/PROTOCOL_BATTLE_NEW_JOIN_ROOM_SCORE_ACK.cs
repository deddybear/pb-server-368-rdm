using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_ACK : GameServerPacket
    {
        private readonly RoomModel Room;
        public PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_ACK(RoomModel Room)
        {
            this.Room = Room;
        }
        public override void Write()
        {
            WriteH(4239);
            WriteD(ProgressValue());
            WriteD((Room.GetTimeByMask() * 60) - Room.GetInBattleTime());
            WriteB(MapModeData(Room));
        }
        private byte[] MapModeData(RoomModel room)
        {
            using (SyncServerPacket S = new SyncServerPacket())
            {
                if (room.IsDinoMode("DE"))
                {
                    S.WriteD(room.FRDino);
                    S.WriteD(room.CTDino);
                }
                else if (room.RoomType == RoomCondition.DeathMatch && !room.IsBotMode())
                {
                    S.WriteD(room.FRKills);
                    S.WriteD(room.CTKills);
                }
                else if (room.RoomType == RoomCondition.FreeForAll)
                {
                    S.WriteD(GetSlotKill());
                    S.WriteD(0);
                }
                else if (room.IsBotMode())
                {
                    S.WriteD(room.IngameAiLevel);
                    S.WriteD(0);
                }
                else
                {
                    S.WriteD(room.FRRounds);
                    S.WriteD(room.CTRounds);
                }
                return S.ToArray();
            }
        }
        private int ProgressValue()
        {
            if (Room.IsBotMode())
            {
                return 3;
            }
            else if (Room.RoomType == RoomCondition.DeathMatch && !Room.IsBotMode())
            {
                return 1;
            }
            else if (Room.IsDinoMode())
            {
                return 4;
            }
            else if (Room.RoomType == RoomCondition.FreeForAll)
            {
                return 5;
            }
            else
            {
                return 2;
            }
        } 
        public int GetSlotKill()
        {
            int TotalKill = 0; // Inisialisasi total kill

            for (int i = 0; i < 16; i++) // Loop untuk setiap slot
            {
                SlotModel Slot = Room.GetSlot(i);

                TotalKill += Slot.AllKills;
            }

            return TotalKill; // Kembalikan total keseluruhan kill
        }

    }
}
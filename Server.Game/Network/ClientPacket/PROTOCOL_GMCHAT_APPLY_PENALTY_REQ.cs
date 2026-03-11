using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GMCHAT_APPLY_PENALTY_REQ : GameClientPacket
    {
        private long PlayerId;
        private int BanTime;
        private byte Type;
        public PROTOCOL_GMCHAT_APPLY_PENALTY_REQ(GameClient client, byte[] buff)
        {
            Makeme(client, buff);
        }
        public override void Read()
        {
            BanTime = ReadD();
            Type = ReadC();
            PlayerId = ReadQ();
            //Type 1 Banned Chat Perkalian Detik
            //BanTime = 300, 600, 1800, 3600, 0
            //Type 2 Banned Account 
            //BanTime 1 Kali = 0, 1 Hari = 1440 Menit, 3 Hari = 4320 Menit, 7 Hari = 10080 Menit, Permanent = -1
        }
        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null)
                {
                    return;
                }
                Account TargetUser = AccountManager.GetAccount(PlayerId, 31);
                if (TargetUser != null)
                {
                    Client.SendPacket(new PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(0, TargetUser, Type, BanTime));
                }
                else
                {
                    Client.SendPacket(new PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(0x80000000, null, 0, 0));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
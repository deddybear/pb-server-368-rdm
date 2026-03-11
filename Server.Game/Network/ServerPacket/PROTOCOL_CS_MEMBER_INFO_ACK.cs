using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_INFO_ACK : GameServerPacket
    {
        private readonly List<Account> players;
        public PROTOCOL_CS_MEMBER_INFO_ACK(List<Account> Players)
        {
            this.players = Players;
        }
        public override void Write()
        {
            WriteH(1869);
            WriteC((byte)players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                Account member = players[i];
                WriteC((byte)(member.Nickname.Length + 1));
                WriteN(member.Nickname, member.Nickname.Length + 2, "UTF-16LE");
                WriteQ(member.PlayerId);
                WriteQ(ComDiv.GetClanStatus(member.Status, member.IsOnline));
                WriteC((byte)member.Rank);
                WriteB(new byte[6]);
            }
        }
        
        //private void WriteData(Account member, StatClan stat, SyncServerPacket packetWriter)
        //{
        //    packetWriter.WriteQ(member.PlayerId);
        //    packetWriter.WriteU(member.Nickname, 66);
        //    packetWriter.WriteC((byte)member.Rank);
        //    packetWriter.WriteC((byte)member.ClanAccess);
        //    packetWriter.WriteQ(ComDiv.GetClanStatus(member.Status, member.IsOnline));
        //    packetWriter.WriteD(member.ClanDate);
        //    packetWriter.WriteC((byte)0);
        //    packetWriter.WriteD(1); //stat.Matches
        //    packetWriter.WriteD(1); //stat.MatchesWin
        //    packetWriter.WriteD(0); //namecard
        //    packetWriter.WriteC((byte)1);
        //    packetWriter.WriteD(10); //Medalhas da Semana
        //    packetWriter.WriteD(20); //Medalhas do mês
        //    packetWriter.WriteD(30); //Medalhas total

        //}
    }
}
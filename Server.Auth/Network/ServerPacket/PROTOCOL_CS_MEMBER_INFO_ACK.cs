using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Auth.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_INFO_ACK : AuthServerPacket
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
    }
}

using Plugin.Core.Network;
using Plugin.Core.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_NOTICE_ACK : GameServerPacket
    {
        public string _message;

        public int _ChatColor;

        public PROTOCOL_BASE_NOTICE_ACK(string msg, int ChatColor)
        {
            _message = msg;
            _ChatColor = ChatColor;
        }
        public override void Write()
        {

            ServerConfig CFG = GameXender.Client.Config;

            WriteH(663);
            WriteH(0);
            WriteD(_ChatColor);
            WriteD(CFG.ChannelAnnounceColor);
            WriteH(0); // > 1 = WriteN("", Length, "UTF-16LE");
            WriteH((ushort)_message.Length);
            WriteN(_message, _message.Length, "UTF-16LE");
            WriteH((ushort)CFG.ChannelAnnouce.Length);
            WriteN(CFG.ChannelAnnouce, CFG.ChannelAnnouce.Length, "UTF-16LE");
        }
    }
}
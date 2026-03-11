using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Data.Commands
{
    public class ChangeNickCommand : ICommand
    {
        public string Command => "changenick";
        public string Description => "modify value of player";
        public string Permission => "hostcommand";
        public string Args => "%value%";
        public string Execute(string Command, string[] Args, Account Player)
        {
            string Text = Args[0];
            string Result = "";

            Account PT = Player;
            if (PT == null)
            {
                return $"Player doesn't Exist!";
            }
            else if (DaoManagerSQL.IsPlayerNameExist(Text))
            {
                return "Nickname Already Exist";
            }
            else if (ComDiv.UpdateDB("accounts", "nickname", Text, "player_id", PT.PlayerId))
            {
                PT.Nickname = Text;
                PT.SendPacket(new PROTOCOL_AUTH_CHANGE_NICKNAME_ACK(PT.Nickname));
                if (PT.Room != null)
                {
                    using (PROTOCOL_ROOM_GET_NICKNAME_ACK packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(PT.SlotId, PT.Nickname))
                    {
                        PT.Room.SendPacketToPlayers(packet);
                    }
                    PT.Room.UpdateSlotsInfo();
                }
                if (PT.ClanId > 0)
                {
                    using (PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK packet = new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(PT))
                    {
                        ClanManager.SendPacket(packet, PT.ClanId, -1, true, true);
                    }
                }
                AllUtils.SyncPlayerToFriends(PT, true);
                DaoManagerSQL.CreatePlayerNickHistory(PT.PlayerId, PT.Nickname, Text, "Command GM");
                
            }

            return $"Success Change Name To {PT.Nickname}";
        }
    }
}

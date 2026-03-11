using System;
using System.Numerics;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using PointBlank.Game.Rcon;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_GMCHAT_APPLY_PENALTY_ACK : GameServerPacket
    {
        private readonly Account Player;
        private readonly uint Error;
        private readonly int Type;
        private readonly int BanTime;
        public PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(uint Error, Account Player, int Type, int BanTime)
        {
            this.Error = Error;
            this.Player = Player;
            this.Type = Type;
            this.BanTime = BanTime;
        }
        public override void Write()
        {
            WriteH(6664);
            WriteH(0);
            WriteD(Error);
            if (Error == 0)
            {
                if (Player != null)
                {
                    switch (Type)
                    {
                        case 1:
                            if (BanTime == 0)
                            {
                                ComDiv.UpdateDB("base_ban_history", "expire_date", DateTime.Now, "object_id", Player.BanObjectId);
                            }
                            else
                            {
                                int Days = BanTime / 60;
                                BanHistory Ban = DaoManagerSQL.SaveBanHistory(Player.PlayerId, "DURATION", $"{Player.PlayerId}", DateTimeUtil.Now().AddMinutes(Days));
                                if (Ban != null)
                                {
                                    string Message = $"Player '{Player.Nickname}' has been banned for {Days} Minutes!";
                                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Message))
                                    {
                                        GameXender.Client.SendPacketToAllClients(Packet);
                                    }
                                    ComDiv.UpdateDB("accounts", "ban_object_id", Ban.ObjectId, "player_id", Player.PlayerId);
                                    Player.BanObjectId = Ban.ObjectId;
                                    Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                    Player.Close(1000, true);
                                }
                            }
                            break;
                        case 2:
                            if (BanTime == -1)
                            {
                                if (ComDiv.UpdateDB("accounts", "access_level", -1, "player_id", Player.PlayerId))
                                {
                                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("PlayerBannedWarning", Player.Nickname)))
                                    {
                                        GameXender.Client.SendPacketToAllClients(packet);
                                    }
                                    Player.Access = AccessLevel.BANNED;
                                    Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                    Player.Close(1000, true);
                                }
                            }
                            else if (BanTime == 0)
                            {
                                Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                Player.Close(1000, true);
                            }
                            else
                            {
                                int Days = BanTime / 1440;
                                BanHistory Ban = DaoManagerSQL.SaveBanHistory(Player.PlayerId, "DURATION", $"{Player.PlayerId}", DateTimeUtil.Now().AddDays(Days));
                                if (Ban != null)
                                {
                                    string Message = $"Player '{Player.Nickname}' has been banned for {Days} Days!";
                                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Message))
                                    {
                                        GameXender.Client.SendPacketToAllClients(Packet);
                                    }
                                    ComDiv.UpdateDB("accounts", "ban_object_id", Ban.ObjectId, "player_id", Player.PlayerId);
                                    Player.BanObjectId = Ban.ObjectId;
                                    Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                    Player.Close(1000, true);
                                }
                            }
                            break;
                        default: break;
                    }
                }
            }
        }
    }
}

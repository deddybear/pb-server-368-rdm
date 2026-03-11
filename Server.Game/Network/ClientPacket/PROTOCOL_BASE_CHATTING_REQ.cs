using Npgsql;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.Managers;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Data.Managers;
using Server.Game.Network.ServerPacket;
using System;
using System.Data;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_CHATTING_REQ : GameClientPacket
    {
        private string Text;
        private ChattingType Type;
        public PROTOCOL_BASE_CHATTING_REQ(GameClient client, byte[] data)
        {
            Makeme(client, data);
        }
        public override void Read()
        {
            Type = (ChattingType)ReadH();
            Text = ReadU(ReadH() * 2);
        }
        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null || string.IsNullOrEmpty(Text) || Text.Length > 60 || Player.Nickname.Length == 0)
                {
                    return;
                }
                RoomModel Room = Player.Room;
                SlotModel Sender;
                switch (Type)
                {
                    case ChattingType.Team:
                    {
                        if (Room == null)
                        {
                            return;
                        }
                        Sender = Room.Slots[Player.SlotId];
                        int[] Array = Room.GetTeamArray(Sender.Team);
                        using (PROTOCOL_ROOM_CHATTING_ACK Packet = new PROTOCOL_ROOM_CHATTING_ACK((int)Type, Sender.Id, Player.UseChatGM(), Text))
                        {
                            byte[] Data = Packet.GetCompleteBytes("PROTOCOL_BASE_CHATTING_REQ-1");
                            lock (Room.Slots)
                            {
                                foreach (int SlotIdx in Array)
                                {
                                    SlotModel Receiver = Room.Slots[SlotIdx];
                                    if (Receiver != null)
                                    {
                                        Account PlayerRecv = Room.GetPlayerBySlot(Receiver);
                                        if (PlayerRecv != null && AllUtils.SlotValidMessage(Sender, Receiver))
                                        {
                                            PlayerRecv.SendCompletePacket(Data, Packet.GetType().Name);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                    case ChattingType type when (type == ChattingType.All || type == ChattingType.None || type == ChattingType.Lobby):
                        {
                        if (Room != null)
                        {
                            if (!ServerCommands_Old(Player, Room))
                            {
                                Sender = Room.Slots[Player.SlotId];
                                using (PROTOCOL_ROOM_CHATTING_ACK Packet = new PROTOCOL_ROOM_CHATTING_ACK((int)Type, Sender.Id, Player.UseChatGM(), Text))
                                {
                                    byte[] Data = Packet.GetCompleteBytes("PROTOCOL_BASE_CHATTING_REQ-2");
                                    lock (Room.Slots)
                                    {
                                        foreach (SlotModel Receiver in Room.Slots)
                                        {
                                            Account PlayerRecv = Room.GetPlayerBySlot(Receiver);
                                            if (PlayerRecv != null && AllUtils.SlotValidMessage(Sender, Receiver))
                                            {
                                                PlayerRecv.SendCompletePacket(Data, Packet.GetType().Name);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ChannelModel Channel = Player.GetChannel();
                            if (Channel == null)
                            {
                                return;
                            }
                            if (!ServerCommands_Old(Player, Room))
                            {
                                using (PROTOCOL_LOBBY_CHATTING_ACK packet = new PROTOCOL_LOBBY_CHATTING_ACK(Player, Text))
                                {
                                    Channel.SendPacketToWaitPlayers(packet);
                                }
                            }
                        }
                        break;
                    }
                    case ChattingType.Whisper:
                    case ChattingType.Reply:
                    case ChattingType.Clan:
                    case ChattingType.Match:
                    case ChattingType.ClanMemberPage:
                    {
                        break;
                    }
                }
            }
            catch (Exception Ex)
            { 
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        private bool ServerCommands_Old(Account player, RoomModel room)
        {
            try
            {
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.Parameters.AddWithValue("@username", player.Username);
                    command.CommandText = "SELECT access_level FROM accounts WHERE Username=@username";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader data = command.ExecuteReader();
                    while (data.Read())
                    {
                        player.Access = (AccessLevel)data.GetInt16(0);
                    }
                    command.Dispose();
                    data.Close();
                    connection.Dispose();
                    connection.Close();
                }

                string str = Text.Substring(1);

                // Command Admin
                if (!player.HaveGMLevel() || !(Text.StartsWith(":") || Text.StartsWith(@"\") || Text.StartsWith(".")))
                {
                    return false;
                }

                //Acces 3
                else if (str.StartsWith("fakerank ") && (int)player.Access >= 3)
                {
                    string Message = SetFakeRank(str, player, room);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("changenick ") && (int)player.Access >= 3)
                {
                    string Message = SetFakeNick(str, player, room);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }


                //Acces 6
                else if (str.StartsWith("banSE ") && (int)player.Access >= 6)
                {
                    string Message = BanForeverNick(str, player, true);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("banSE2 ") && (int)player.Access >= 6)
                {
                    string Message = BanForeverId(str, player, true);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("kp ") && (int)player.Access >= 6)
                {
                    string Message = KickByNick(str, player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("kp2 ") && (int)player.Access >= 6)
                {
                    string Message = KickById(str, player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("send ") && (int)player.Access >= 6)
                {
                    string Message = SendMessageToAll(str);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("spawn ") && (int)player.Access >= 6)
                {
                    string Message = CreateItemYourself(str, player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("cid ") && (int)player.Access >= 6)
                {
                    string Message = CreateItemById(str, player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("cia ") && (int)player.Access >= 6)
                {
                    string Message = CreateItemByNick(str, player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("ping ") && (int)player.Access >= 6)
                {
                    bool ChangeValue = int.Parse(str.Substring(5)).Equals(1);
                    if (ChangeValue.Equals(ConfigLoader.IsDebugPing))
                    {
                        string Message = "Ping Mode Already Changed To: " + ChangeValue;
                        player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                    }
                    switch (ChangeValue)
                    {
                        case true:
                            {
                                ConfigLoader.IsDebugPing = ChangeValue;
                                string Message = $"Mode '{(ChangeValue ? "Enabled" : "Disabled")}'";
                                player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                                break;
                            }
                        case false:
                            {
                                ConfigLoader.IsDebugPing = ChangeValue;
                                string Message = $"Mode '{(ChangeValue ? "Enabled" : "Disabled")}'";
                                player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                                break;
                            }
                        default:
                            {
                                string Message = $"Mode Unchanged Due To Wrong Value";
                                player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                                break;
                            }
                    }
                }   
                else if (str.StartsWith("endbattle") && (int)player.Access >= 6)
                {
                    if (room != null)
                    {
                        if (room.IsPreparing())
                        {
                            AllUtils.EndBattle(room);
                            string Message = "End Room Success!";
                            player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                        }
                        else
                        {
                            string Message = "End Room Failed!";
                            player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                        }
                    }
                    else
                    {
                        string Message = "Invalid Room!";
                        player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                    }
                }
                else if (str.StartsWith("refreshshop") && (int)player.Access >= 6)
                {
                    string Message = InstantRefill(player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("refreshredeem") && (int)player.Access >= 6)
                {
                    string Message = RefreshRedeemCode(player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else if (str.StartsWith("testopcode") && (int)player.Access >= 6)
                {
                    string Message = TestOpcode(str, player);
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, Message));
                }
                else
                {
                    string ErrorMessage = "Unknown Cmd";
                    player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, ErrorMessage));
                }
                return true;
            }
            catch (Exception Ex)
            {
                string ErrorMessage = "An unpredicted error while send command was found!";
                player.Connection.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, ErrorMessage));

                CLogger.Print(Ex.Message, LoggerType.Error, Ex);

                return true;
            }
        }

        public static string TestOpcode(string str, Account player)
        {
            if (player != null)
            {
                
                player.SendPacket(new PROTOCOL_TEST_ACK());

                return "Send Opcode Succes!";
            }

            return "Send Opcode Failed!";
        }

        public static string RefreshRedeemCode(Account player)
        {
            RedeemCodeXML.Load();

            return "Success Refresh Redeem Code List";
        }

        public static string InstantRefill(Account player)
        {
            ShopManager.Reset();
            ShopManager.Load(1);

            for (int i = 0; i < ShopManager.ShopDataItems.Count; i++)
            {
                ShopData data = ShopManager.ShopDataItems[i];
                player.SendPacket(new PROTOCOL_AUTH_SHOP_ITEMLIST_ACK(data, ShopManager.TotalItems));
            }
            for (int i = 0; i < ShopManager.ShopDataGoods.Count; i++)
            {
                ShopData data = ShopManager.ShopDataGoods[i];
                player.SendPacket(new PROTOCOL_AUTH_SHOP_GOODSLIST_ACK(data, ShopManager.TotalGoods));
            }
            for (int i = 0; i < ShopManager.ShopDataItemRepairs.Count; i++)
            {
                ShopData data = ShopManager.ShopDataItemRepairs[i];
                player.SendPacket(new PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK(data, ShopManager.TotalRepairs));
            }
            int cafe = (int)player.CafePC;
            if (cafe == 0)
            {
                for (int i = 0; i < ShopManager.ShopDataMt1.Count; i++)
                {
                    ShopData data = ShopManager.ShopDataMt1[i];
                    player.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(data, ShopManager.TotalMatching1));
                }
            }
            else
            {
                for (int i = 0; i < ShopManager.ShopDataMt2.Count; i++)
                {
                    ShopData data = ShopManager.ShopDataMt2[i];
                    player.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(data, ShopManager.TotalMatching2));
                }
            }
            player.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(true));
            return "Success Refresh shop";
        }

        public static string BanForeverNick(string str, Account player, bool warn)
        {
            Account victim = AccountManager.GetAccount(str.Substring(6), 1, 0);
            return BaseBanForever(player, victim, warn);
        }

        public static string BanForeverId(string str, Account player, bool warn)
        {
            Account victim = AccountManager.GetAccount(long.Parse(str.Substring(7)), 0);
            return BaseBanForever(player, victim, warn);
        }

        private static string BaseBanForever(Account player, Account victim, bool warn)
        {
            if (victim == null)
            {
                return $"User not found!!";
            }
            else if (victim.Access > player.Access)
            {
                return "GM can't be banned!!"; 
            }
            else if (player.PlayerId == victim.PlayerId)
            {
                return "can't ban yourself!!";
            }
            else if (ComDiv.UpdateDB("accounts", "access_level", -1, "player_id", victim.PlayerId))
            {
                if (warn)
                {
                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("player " + victim.Nickname + " has been banned."))
                    {
                        GameXender.Client.SendPacketToAllClients(packet);
                    }
                }
                victim.Access = AccessLevel.BANNED;
                victim.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                victim.Close(1000, true);

                return "account successfully banned!!";
            }
            else
            {
                return "failed to ban account!!";
            }
        }

        public static string CreateItemYourself(string str, Account player)
        {
            int id = int.Parse(str.Substring(6));
            if (id < 100000)
            {
                return "Wrong Item ID!!";
            }
            else if (player != null)
            {
                List<ItemsModel> Items = new List<ItemsModel>();

                ItemsModel Item = new ItemsModel(id)
                {
                    ObjectId = ComDiv.ValidateStockId(id),
                    Name = "Command Item",
                    Count = 1,
                    Equip = ItemEquipType.Permanent
                };

                if (Item == null)
                {
                    return $"Item Id: {id} was error!";
                }
                else
                {
                    Items.Add(Item);
                }
                if (Items.Count > 0)
                {
                    player.SendPacket(new PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Items));
                }
                player.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, player, Item));

                return "Spawn Item Succes!";
            }

            return "Spawn Item Failed!";
        }

        public static string CreateItemByNick(string str, Account player)
        {
            string txt = str.Substring(str.IndexOf(" ") + 1);
            string[] split = txt.Split(' ');
            string nick = split[0];
            int item_id = Convert.ToInt32(split[1]);
            if (item_id < 100000)
            {
                return "Wrong Item ID!!";
            }
            else
            {
                Account playerO = AccountManager.GetAccount(nick, 1, 0);
                if (playerO == null)
                {
                    return $"User not found!!";
                }
                if (playerO.PlayerId == player.PlayerId)
                {
                    return "you sent to yourself, try another CMD.";
                }
                else
                {
                    List<ItemsModel> Items = new List<ItemsModel>();

                    ItemsModel Item = new ItemsModel(item_id)
                    {
                        ObjectId = ComDiv.ValidateStockId(item_id),
                        Name = "Command Item",
                        Count = 1,
                        Equip = ItemEquipType.Permanent
                    };

                    if (Item == null)
                    {
                        return $"Item Id: {item_id} was error!";
                    }
                    else
                    {
                        Items.Add(Item);
                    }
                    if (Items.Count > 0)
                    {
                        playerO.SendPacket(new PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Items));
                    }
                    playerO.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, playerO, Item));


                    return "Succes Send Item To : " + playerO.Nickname;
                }
            }
        }

        public static string CreateItemById(string str, Account player)
        {
            string txt = str.Substring(str.IndexOf(" ") + 1);
            string[] split = txt.Split(' ');
            int item_id = Convert.ToInt32(split[1]);
            long player_id = Convert.ToInt64(split[0]);
            if (item_id < 100000)
            {
                return "Wrong Item ID!!";
            }
            else
            {
                Account playerO = AccountManager.GetAccount(player_id, 0);
                if (playerO != null)
                {
                    if (playerO.PlayerId == player.PlayerId)
                    {
                        return "you sent to yourself, try another CMD.";
                    }
                    else
                    {
                        List<ItemsModel> Items = new List<ItemsModel>();

                        ItemsModel Item = new ItemsModel(item_id)
                        {
                            ObjectId = ComDiv.ValidateStockId(item_id),
                            Name = "Command Item",
                            Count = 1,
                            Equip = ItemEquipType.Permanent
                        };

                        if (Item == null)
                        {
                            return $"Item Id: {item_id} was error!";
                        }
                        else
                        {
                            Items.Add(Item);
                        }
                        if (Items.Count > 0)
                        {
                            playerO.SendPacket(new PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Items));
                        }
                        playerO.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, playerO, Item));


                        return "Succes Send Item To : " + playerO.Nickname;
                    }
                }
                else
                {
                    return "Failed Send Item To : " + playerO.Nickname;
                }
            }
        }

        public static string KickByNick(string str, Account player)
        {
            string playerName = str.Substring(3);
            Account victim = AccountManager.GetAccount(playerName, 1, 0);
            return BaseKick(player, victim);
        }

        public static string KickById(string str, Account player)
        {
            Account victim = AccountManager.GetAccount(long.Parse(str.Substring(4)), 0);
            return BaseKick(player, victim);
        }

        private static string BaseKick(Account player, Account victim)
        {
            if (victim == null)
            {
                return "Player Not Found";
            }
            else if ((int)victim.Access > (int)player.Access)
            {
                return "This Player Have Acces , Cannot Kick";
            }
            else if (victim.PlayerId == player.PlayerId)
            {
                return "You Kick Yourself";
            }
            else if (victim.Connection != null)
            {
                victim.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                victim.Close(1000, true);
                return "Kick Player Succes : " + victim.Nickname;
            }
            else
            {
                return "This Player Offline : " + victim.Nickname;
            }
        }

        public static string SendMessageToAll(string str)
        {
            string msg = str.Substring(5);
            int count = 0;
            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(msg))
            {
                count = GameXender.Client.SendPacketToAllClients(packet);
            }
            return "Succes Send Message To All Player " + count;
        }

        public static string SetFakeRank(string str, Account player, RoomModel room)
        {
            int rank = int.Parse(str.Substring(9));
            if (rank > 111 || rank < 0)
            {
                return "Rank Wrong Value";
            }
            else if (player.Bonus.FakeRank == rank)
            {
                return "Rank Already Used";
            }
            else if (rank == 53 || rank == 54)
            {
                return "Can`t Use Rank GM/MOD";
            }
            else if (ComDiv.UpdateDB("player_bonus", "fake_rank", rank, "owner_id", player.PlayerId))
            {
                player.Bonus.FakeRank = rank;
                player.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, player));
                if (room != null)
                {
                    room.UpdateSlotsInfo();
                }
                if (rank == 55)
                {
                    return "Succes Change Fakerank To Default";
                }
                else
                {
                    return "Succes Change Fakerank";
                }
            }
            return "Failed Change Fakerank";
        }

        public static string SetFakeNick(string str, Account player, RoomModel room)
        {
            string name = str.Substring(11);
            if (name.Length > 20 || name.Length < 4)
            {
                return Translation.GetLabel("FakeNickWrongLength");
            }
            else if (DaoManagerSQL.IsPlayerNameExist(name))
            {
                return Translation.GetLabel("FakeNickAlreadyExist");
            }
            else if (ComDiv.UpdateDB("accounts", "nickname", name, "player_id", player.PlayerId))
            {
                player.Nickname = name;
                player.SendPacket(new PROTOCOL_AUTH_CHANGE_NICKNAME_ACK(name));
                if (room != null)
                {
                    using (PROTOCOL_ROOM_GET_NICKNAME_ACK packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(player.SlotId, player.Nickname))
                    {
                        room.SendPacketToPlayers(packet);
                    }
                }
                if (player.ClanId > 0)
                {
                    using (PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK packet = new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(player))
                    {
                        ClanManager.SendPacket(packet, player.ClanId, -1, true, true);
                    }
                }
                AllUtils.SyncPlayerToFriends(player, true);
                return "Succes Change Nick To : " + name;
            }
            return "Change Nick Failed";
        }
    }
}
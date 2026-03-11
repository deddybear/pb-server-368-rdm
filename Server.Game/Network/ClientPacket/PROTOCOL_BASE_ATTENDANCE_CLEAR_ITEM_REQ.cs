using Plugin.Core;
using Plugin.Core.Managers;
using Plugin.Core.Managers.Events;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Plugin.Core.SQL;
using Server.Game.Network.ServerPacket;
using System;
using Plugin.Core.Models;
using Server.Game.Data.Models;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ : GameClientPacket
    {
        private EventErrorEnum Error = EventErrorEnum.VISIT_EVENT_SUCCESS;
        private int EventId, Type, DayCheckedIdx;
        public PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ(GameClient client, byte[] data)
        {
            Makeme(client, data);
        }
        public override void Read()
        {
            EventId = ReadD();
            Type = ReadC();
            DayCheckedIdx = ReadC();
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
                if (Player.Nickname.Length == 0 || Type > 1)
                {
                    Error = EventErrorEnum.VISIT_EVENT_USERFAIL;
                }
                else if (Player.Event != null)
                {
                    if (Player.Event.LastVisitSequence1 == Player.Event.LastVisitSequence2)
                    {
                        Error = EventErrorEnum.VISIT_EVENT_ALREADYCHECK;
                    }
                    else
                    {
                        EventVisitModel Event = EventVisitSync.GetEvent(EventId);
                        if (Event == null)
                        {
                            Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(EventErrorEnum.VISIT_EVENT_UNKNOWN));
                            return;
                        }
                        if (Event.EventIsEnabled())
                        {
                            VisitItemModel Reward = Event.GetReward(Player.Event.LastVisitSequence2, Type);
                            if (Reward != null)
                            {
                                GoodsItem Good = ShopManager.GetGood(Reward.GoodId);
                                if (Good != null)
                                {
                                    if (Good.Item.Category == ItemCategory.Character || (Good.Item.Id >= 601000) && (Good.Item.Id <= 601999) || (Good.Item.Id >= 602000) && (Good.Item.Id <= 602999))
                                    {

                                        if (Player.Character.Characters.Count >= 60)
                                        {
                                            Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("{col:255.0.0.255}Tiene demasiados caracteres, el límite máximo es de 60 caracteres.{/col}")));
                                        }

                                        int Slots = Player.Character.Characters.Count;

                                        CharacterModel Chara = new CharacterModel()
                                        {
                                            Id = Good.Item.Id,
                                            Slot = Slots++,
                                            Name = Good.Item.Name,
                                            CreateDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
                                            PlayTime = 0
                                        };

                                        if (Player.Character.Characters.Find(x => x.Id == Chara.Id) == null)
                                        {
                                            DaoManagerSQL.CreatePlayerCharacter(Chara, Player.PlayerId);
                                            Player.Character.AddCharacter(Chara);
                                        }

                                        Player.Event.NextVisitDate = int.Parse(DateTimeUtil.Now().AddDays(1).ToString("yyMMdd"));

                                        ComDiv.UpdateDB("player_events", "owner_id", Player.PlayerId, new string[] { "next_visit_date", "last_visit_sequence2" }, Player.Event.NextVisitDate, ++Player.Event.LastVisitSequence2);

                                        Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, Player, new ItemsModel(Good.Item)));

                                        Client.SendPacket(new PROTOCOL_CHAR_CREATE_CHARA_ACK(0, 1, Chara, Player));

                                        Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("Succes Claim Chara!!")));
                                    }
                                    else
                                    {
                                        ItemsModel itemID = Player.Inventory.GetItem(Good.Item.Id);
                                        if (itemID != null && itemID.Equip == ItemEquipType.Permanent)
                                        {
                                            Player.Event.NextVisitDate = int.Parse(DateTimeUtil.Now().AddDays(1).ToString("yyMMdd"));
                                            ComDiv.UpdateDB("player_events", "owner_id", Player.PlayerId, new string[] { "next_visit_date", "last_visit_sequence2" }, Player.Event.NextVisitDate, ++Player.Event.LastVisitSequence2);

                                            Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("you already have this item permanently!!")));
                                        }
                                        else
                                        {
                                            Player.Event.NextVisitDate = int.Parse(DateTimeUtil.Now().AddDays(1).ToString("yyMMdd"));
                                            ComDiv.UpdateDB("player_events", "owner_id", Player.PlayerId, new string[] { "next_visit_date", "last_visit_sequence2" }, Player.Event.NextVisitDate, ++Player.Event.LastVisitSequence2);

                                            Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, Player, new ItemsModel(Good.Item)));
                                            Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("Succes Claim Item!!")));
                                        }
                                    }
                                }
                                else
                                {
                                    Error = EventErrorEnum.VISIT_EVENT_NOTENOUGH;
                                }
                            }
                            else
                            {
                                Error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                            }
                        }
                        else
                        {
                            Error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                        }
                    }
                }
                else
                {
                    Error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                }
                Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(Error));
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
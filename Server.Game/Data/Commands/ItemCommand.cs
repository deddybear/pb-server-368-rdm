using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Data.Commands
{
    public class ItemCommand : ICommand
    {
        public string Command => "spawn";
        public string Description => "modify value of player";
        public string Permission => "gamemastercommand";
        public string Args => "%value%";
        public string Execute(string Command, string[] Args, Account Player)
        {
            int Value = int.Parse(Args[0]);
            string Result = "";

            Account PT = Player;
            if (PT == null)
            {
                return $"Player doesn't Exist!";
            }

            List<ItemsModel> Items = new List<ItemsModel>();

            ItemsModel Item = new ItemsModel(Value)
            {
                ObjectId = ComDiv.ValidateStockId(Value),
                Name = "Command Item",
                Count = 1,
                Equip = ItemEquipType.Permanent
            };

            if (Item == null)
            {
                return $"Item Id: {Value} was error!";
            }
            else
            {
                Items.Add(Item);
            }
            if (Items.Count > 0)
            {
                PT.SendPacket(new PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Items));
            }
            PT.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, PT, Item));
            Result = $"{Item.Name} To UID: {PT.PlayerId} ({PT.Nickname})";

            return $"Spawn Item {Result}";
        }
    }
}

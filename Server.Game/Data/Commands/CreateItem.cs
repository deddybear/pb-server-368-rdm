using Plugin.Core;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Data.Commands
{
    public static class CreateItem
    {

        public static int GetItemCategory(int id)
        {
            int value = getIdStatics(id, 1);
            int type = value % 10000;
            if (value >= 1 && value <= 5)
            {
                return 1;
            }
            else if (value >= 6 && value <= 8)
            {
                return 2;
            }
            else if (value == 15)
            {
                return 2;
            }
            else if (value >= 26 && value <= 27)
            {
                return 2;
            }
            else if (type >= 7 && type <= 14)
            {
                return 2;
            }
            else if (value >= 16 && value <= 20)
            {
                return 3;
            }
            else
            {
                //Logger.warning("Invalid Category [" + value + "]: " + id);
            }
            return 0;
        }

        public static int getIdStatics(int id, int type)
        {
            if (type == 1)
            {
                return id / 100000; // Item Class
            }
            else if (type == 2)
            {
                return (id % 100000) / 1000; // Class Type
            }
            else if (type == 3)
            {
                return id % 1000; // Number
            }
            else if (type == 4)
            {
                return id % 10000000 / 100000;
            }
            return 0;
        }

        public static string CreateItemYourself(string str, Account player)
        {
            int id = int.Parse(str.Substring(3));
            if (id < 100000)
            {
                return Translation.GetLabel("CreateItemWrongID");
            }
            else if (player != null)
            {
                int category = GetItemCategory(id);

                ItemsModel Item = new ItemsModel(id)
                {
                    ObjectId = ComDiv.ValidateStockId(id),
                    Name = "Command Item",
                    Count = 100,
                    Equip = ItemEquipType.Permanent
                };

                player.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, new ItemsModel(Item)));
                return Translation.GetLabel("CreateItemSuccess");
            }
            else
            {
                return Translation.GetLabel("CreateItemFail");
            }
        }

    }
}

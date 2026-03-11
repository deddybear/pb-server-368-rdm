using Plugin.Core.Managers;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using Server.Game.Data.Commands;

namespace PointBlank.Game.Data.Command.Commands
{
    class UpdateCommand : ICommand
    {
        public string Command => "refreshshop";
        public string Description => "Send messages";
        public string Permission => "moderatorcommand";
        public string Args => "";
        public string Execute(string Command, string[] Args, Account Player)
        {
            ShopManager.Reset();
            ShopManager.Load(1);

            for (int i = 0; i < ShopManager.ShopDataItems.Count; i++)
            {
                ShopData data = ShopManager.ShopDataItems[i];
                Player.SendPacket(new PROTOCOL_AUTH_SHOP_ITEMLIST_ACK(data, ShopManager.TotalItems));
            }
            for (int i = 0; i < ShopManager.ShopDataGoods.Count; i++)
            {
                ShopData data = ShopManager.ShopDataGoods[i];
                Player.SendPacket(new PROTOCOL_AUTH_SHOP_GOODSLIST_ACK(data, ShopManager.TotalGoods));
            }
            for (int i = 0; i < ShopManager.ShopDataItemRepairs.Count; i++)
            {
                ShopData data = ShopManager.ShopDataItemRepairs[i];
                Player.SendPacket(new PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK(data, ShopManager.TotalRepairs));
            }
            int cafe = (int)Player.CafePC;
            if (cafe == 0)
            {
                for (int i = 0; i < ShopManager.ShopDataMt1.Count; i++)
                {
                    ShopData data = ShopManager.ShopDataMt1[i];
                    Player.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(data, ShopManager.TotalMatching1));
                }
            }
            else
            {
                for (int i = 0; i < ShopManager.ShopDataMt2.Count; i++)
                {
                    ShopData data = ShopManager.ShopDataMt2[i];
                    Player.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(data, ShopManager.TotalMatching2));
                }
            }
            Player.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(true));
            return "Success Refresh shop";
        }
    }
}

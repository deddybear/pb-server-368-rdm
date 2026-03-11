using Plugin.Core.SQL;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Data.Commands
{
    public class BattleEndCommand : ICommand
    {
        public string Command => "endbattle";
        public string Command2 => "buybattlepass";
        public string Description => "End work in progress battle";
        public string Description2 => "Only work in Lobby Chat and Room";
        public string Permission => "moderatorcommand";
        public string Args => "";
        public string Execute(string Command, string[] Args, Account Player)
        {
            RoomModel Room = Player.Room;
            if (Room == null)
            {
                return "A room is required to execute the command.";
            }
            if (!Room.IsPreparing() || !AllUtils.PlayerIsBattle(Player))
            {
                return $"Oops! the battle hasn't started.";
            }
            AllUtils.EndBattle(Room);
            return "Battle ended.";
        }
        /*public string Execute2(string Command2, string[] Args, Account Player)
        {
            RoomModel Room = Player.Room;
            var HargaBattlepassPremium = 99999;
            if (Player.Cash < HargaBattlepassPremium)
            {
                return $"Your cash is not enough to buy battlepass premium.";
            }
            else
            {
                Player.Cash -= HargaBattlepassPremium;
                DaoManagerSQL.UpdateBattlepassPremium(Player.PlayerId, 1);

                Player.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0, Player));
                Player.SendPacket(new PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(0));

                Player.SendPacket(new PROTOCOL_SEASON_CHALLENGE_INFO_ACK(Player));
                SendItemInfo.LoadGoldCash(Player);
            }
            return "Success buy premium battlepass.";
        }*/
    }
}

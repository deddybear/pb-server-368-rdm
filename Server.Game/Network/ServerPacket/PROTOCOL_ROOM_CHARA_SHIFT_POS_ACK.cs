using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_CHARA_SHIFT_POS_ACK : GameServerPacket
    {
        private readonly Account Player;
        public PROTOCOL_ROOM_CHARA_SHIFT_POS_ACK(Account Player)
        {
            this.Player = Player;
        }
        public override void Write()
        {
            WriteH(3850);
            WriteD(0);
            WriteH(0);
            WriteC((byte)Player.Character.GetCharacter(Player.Equipment.CharaRedId).Slot);
            WriteC((byte)Player.Character.GetCharacter(Player.Equipment.CharaBlueId).Slot);
        }
    }
}
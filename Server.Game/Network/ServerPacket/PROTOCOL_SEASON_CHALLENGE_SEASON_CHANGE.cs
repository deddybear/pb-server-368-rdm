using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SEASON_CHALLENGE_SEASON_CHANGE : GameServerPacket
    {
        private new Account Player;
        public PROTOCOL_SEASON_CHALLENGE_SEASON_CHANGE(Account Player)
        {
            this.Player = Player;
        }
        public override void Write()
        {
            WriteH(8450);
            WriteH(0);
            WriteC(2);
            WriteC(0);
        }
    }
}
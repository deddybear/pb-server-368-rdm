namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_TEST_ACK : GameServerPacket
    {
        public PROTOCOL_TEST_ACK()
        {
        }
        public override void Write()
        {
            WriteH(677);
            WriteC(1);
            WriteC(1);
            WriteC(1);
            WriteC(1);
            WriteC(1);
            WriteC(1);
            WriteC(1);
        }
    }
}
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_DETAIL_INFO_ACK : GameServerPacket
    {
        private readonly ClanModel Clan;
        private readonly int Error;
        private readonly Account Player;
        private readonly int Players;
        public PROTOCOL_CS_DETAIL_INFO_ACK(int Error, ClanModel Clan)
        {
            this.Error = Error;
            this.Clan = Clan;
            if (Clan != null)
            {
                Player = AccountManager.GetAccount(Clan.OwnerId, 31);
                Players = DaoManagerSQL.GetClanPlayers(Clan.Id);

            }
        }
        public override void Write()
        {
            WriteH(1825);
            WriteD(Error);
            WriteD(Clan.Id);
            WriteU(Clan.Name, 34);
            WriteC((byte)Clan.Rank);
            WriteC((byte)Players);
            WriteC((byte)Clan.MaxPlayers);
            WriteD(Clan.CreationDate);
            WriteD(Clan.Logo);
            WriteC((byte)Clan.NameColor);
            WriteC((byte)Clan.Effect);
            WriteC(0);
            WriteC((byte)Clan.GetClanUnit());
            WriteD(Clan.Exp);
            WriteQ(Clan.OwnerId);
            WriteC(0);
            WriteC(0);
            WriteC(0);
            WriteU(Player != null ? Player.Nickname : "", 66);
            WriteC((byte)(Player != null ? Player.NickColor : 0));
            WriteC((byte)(Player != null ? Player.Rank : 0));
            WriteU(Clan.Info, 510);
            WriteB(new byte[41]); //Unk
            WriteC(1); //Accept Type (request or free)
            WriteC((byte)Clan.RankLimit);
            WriteC((byte)Clan.MinAgeLimit);
            WriteC((byte)Clan.MaxAgeLimit);
            WriteC((byte)Clan.Authority);
            WriteU(Clan.News, 510);

            WriteD(Clan.Matches); //clan matches
            WriteD(Clan.MatchWins); //clan wins
            WriteD(Clan.MatchLoses); //clan loses
            WriteD(11111);
            WriteD(22222);
            WriteD(33333);
            WriteD(11111);
            WriteD(22222);
            WriteD(33333);
            WriteD(11111);
            WriteD(22222);
            WriteD(33333);
            WriteD(11111);
            WriteD(22222);
            WriteD(33333);
            WriteD(11111);
            WriteD(22222);
            WriteD(33333);
            WriteD(19);
            WriteD(20);
            WriteD(21);
            WriteD(22);
            WriteD(23);
            WriteD(24);

            //ordinary statistics
            WriteD(0); //clan matches
            WriteD(0); //clan wins
            WriteD(0); //clan loses
            WriteD(0); //unk
            WriteD(0); //unk
            WriteD(0); //clan kills
            WriteD(0); //clan assists
            WriteD(0); //clan deaths
            WriteD(0); //clan headshotes
            WriteD(0); //clan escapes

            //rating (current season)
            WriteD(0); //unk
            WriteD(0); //rating clan medals
            WriteD(0); //rating clan matches
            WriteD(0); //rating clan wins
            WriteD(0); //rating clan loses
            WriteD(0); //rating clan rank
            WriteD(0); //rating clan rank
            WriteD(0); //rating clan kills
            WriteD(0); //rating clan assists
            WriteD(0); //rating clan deathes
            WriteD(0); //rating clan headshots
            WriteD(0); //rating clan escapes

            //rating (previous season)
            WriteD(0); //unk
            WriteD(0); //rating clan medals
            WriteD(0); //rating clan matches
            WriteD(0); //rating clan wins
            WriteD(0); //rating clan loses
            WriteD(0); //rating clan rank
            WriteD(0); //rating clan rank
            WriteD(0); //rating clan kills
            WriteD(0); //rating clan assists
            WriteD(0); //rating clan deathes
            WriteD(0); //rating clan headshots
            WriteD(0); //rating clan escapes

            //stage medals
            //WriteD(33333); //current week
            //WriteD(20000); //previous week
            //WriteC(0);
            //WriteD(30); //Current stage and rank

            //WriteD(Clan.Matches);
            //WriteD(Clan.MatchWins);
            //WriteD(Clan.MatchLoses);
            //WriteD(Clan.Matches);
            //WriteD(Clan.MatchWins);
            //WriteD(Clan.MatchLoses);

            //WriteD(0); // ?

            //Old Ranking
            //WriteD(Clan.Matches); // Fights
            //WriteD(Clan.MatchWins); // Win
            //WriteD(Clan.MatchLoses); // Lose
            //WriteD(1);
            //WriteC(2);
            //WriteF(Clan.Points);
            //WriteC(3);
            //WriteH(4); // ?

            //Last Season
            //WriteD(Clan.Matches); // Fights
            //WriteD(Clan.MatchWins); // Win
            //WriteD(Clan.MatchLoses); // Lose
            //WriteD(0); // ?
            //WriteD(0); // ?
            //WriteC(0);
            //WriteF(Clan.Points);
            //WriteC(0);
            //WriteH(0); // ?

            //WriteQ(Clan.BestPlayers.Exp.PlayerId);
            //WriteQ(Clan.BestPlayers.Exp.PlayerId);
            //WriteQ(Clan.BestPlayers.Wins.PlayerId);
            //WriteQ(Clan.BestPlayers.Wins.PlayerId);
            //WriteQ(Clan.BestPlayers.Kills.PlayerId);
            //WriteQ(Clan.BestPlayers.Kills.PlayerId);
            //WriteQ(Clan.BestPlayers.Headshots.PlayerId);
            //WriteQ(Clan.BestPlayers.Headshots.PlayerId);
            //WriteQ(Clan.BestPlayers.Participation.PlayerId);
            //WriteQ(Clan.BestPlayers.Participation.PlayerId);

            //WriteD(1);
            //WriteD(2);
            //WriteD(3);
            //WriteD(4);
            //WriteD(5);
            //WriteD(6);
            //WriteD(7);
            //WriteD(8);
            //WriteD(8);
            //WriteD(10);
            //WriteD(11);
            //WriteD(12);
            //WriteD(13);
            //WriteD(14);
            //WriteD(15);
            //WriteD(16);
            //WriteD(17);
            //WriteD(18);
            //WriteD(19);
            //WriteD(20);

            //WriteB(new byte[34]);
        }
    }
}
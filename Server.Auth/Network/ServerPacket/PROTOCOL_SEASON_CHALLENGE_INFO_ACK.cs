using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Plugin.Core;
using Server.Auth.Data.Models;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_SEASON_CHALLENGE_INFO_ACK : AuthServerPacket
    {
        private readonly string SeasonName;
        private readonly short SeasonDays;
        private Account player;
        private int EnablePremium;
        private uint StartDate, EndDate;
        private List<CardData> cardDataList;
        public PROTOCOL_SEASON_CHALLENGE_INFO_ACK(Account player, List<CardData> cardDataList, uint StartDate, uint EndDate)
        {
            this.player = player;
            this.cardDataList = cardDataList;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            SeasonName = "Haze Battlepass";
            SeasonDays = 50; //max 99
        }
        public override void Write()
        {
            var currentLevel = DetermineCurrentLevel((int)player.SeasonExp, cardDataList);
            var LevelsComplete = (currentLevel > 0) ? (currentLevel - 1) : 0;
            int remainingBytes = 99 - cardDataList.Count;

            WriteH(8449);
            WriteH(0);
            WriteD(1); //unk (enable or disable season)
            WriteC((byte)currentLevel); //Current level = 5

            WriteD(player.SeasonExp); //Total earned points

            WriteC((byte)currentLevel); //Normal levels complete = 4
            WriteC((byte)currentLevel); //Premium levels complete = 4
            WriteC((byte)player.IsPremiumBattlepass); //Enable premium
            WriteC(0); //unk
            WriteD(currentLevel); //Current buy level
            WriteC(1); //Season (1-continuing, 2-fnished, 0 or 3 can't buy pass)
            WriteU(SeasonName, 42); //Name
            WriteH(SeasonDays); //Total Days
            WriteD(0); //unk

            foreach (var card in cardDataList)
            {
                WriteD(card.NormalCard);    // NORMAL CARD
                WriteD(card.PremiumCardA);  // PREMIUM CARD A
                WriteD(card.PremiumCardB);  // PREMIUM CARD B
                WriteD(card.RequiredExp);   // Total points for open card
            }

            for (int i = 0; i < remainingBytes; i++)
            {
                WriteB(new byte[16]);
            }

            WriteB(new byte[12]);

            WriteD(StartDate); //Start date
            WriteD(EndDate); //Snd date
        }

        private static int DetermineCurrentLevel(int playerExp, List<CardData> cardDataList)
        {
            int level = 0;
            foreach (var card in cardDataList)
            {
                if (playerExp >= card.RequiredExp)
                    level++;
                else
                    break;
            }
            return level;
        }
    }

    public class CardData
    {
        public int Number { get; set; }
        public int RequiredExp { get; set; }
        public int NormalCard { get; set; }
        public int PremiumCardA { get; set; }
        public int PremiumCardB { get; set; }

        public int EnablePremium;
        public uint StartDate, EndDate;
    }
}
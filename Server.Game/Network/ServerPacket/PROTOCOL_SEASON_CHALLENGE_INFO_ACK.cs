using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.Xml.Linq;
using Npgsql.Replication;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    internal class PROTOCOL_SEASON_CHALLENGE_INFO_ACK : GameServerPacket
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
            var LevelsComplete = currentLevel - 1;
            int remainingBytes = 99 - cardDataList.Count;

            WriteH(8449);
            WriteH(0);
            WriteD(1); //unk (enable or disable season)
            WriteC((byte)currentLevel); //Current level

            WriteD(player.SeasonExp); //Total earned points

            WriteC((byte)currentLevel); //Normal levels complete
            WriteC((byte)currentLevel); //Premium levels complete
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

        public uint StartDate, EndDate;
        public static List<CardData> LoadCardsWithXmlReader(string filePath)
        {
            List<CardData> cards = new List<CardData>();
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                CardData card = null;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Card")
                    {
                        card = new CardData
                        {
                            Number = int.Parse(reader.GetAttribute("Number"))
                        };
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "RequiredExp")
                    {
                        reader.Read();
                        card.RequiredExp = int.Parse(reader.Value);
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "NormalCard")
                    {
                        reader.Read();
                        card.NormalCard = int.Parse(reader.Value);
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "PremiumCardA")
                    {
                        reader.Read();
                        card.PremiumCardA = int.Parse(reader.Value);
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "PremiumCardB")
                    {
                        reader.Read();
                        card.PremiumCardB = int.Parse(reader.Value);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Card")
                    {
                        cards.Add(card);
                    }
                }
            }
            return cards;
        }
        public static uint LoadEndDate(string filePath)
        {
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "EndDate")
                    {
                        reader.Read();
                        var StartDate = uint.Parse(reader.Value);
                        return StartDate;
                    }
                }
            }
            return 0; // Default jika elemen tidak ditemukan
        }
        public static uint LoadStartDate(string filePath)
        {
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "StartDate")
                    {
                        reader.Read();
                        return uint.Parse(reader.Value);
                    }
                }
            }
            return 0; // Default jika elemen tidak ditemukan
        }
    }
}

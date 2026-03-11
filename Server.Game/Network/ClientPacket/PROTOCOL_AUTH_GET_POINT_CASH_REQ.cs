using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_GET_POINT_CASH_REQ : GameClientPacket
    {
        public PROTOCOL_AUTH_GET_POINT_CASH_REQ(GameClient client, byte[] data)
        {
            Makeme(client, data);
        }
        public override void Read()
        {
        }
        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null)
                {
                    return;
                }
                Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0, Player));
                List<CardData> cardDataList = LoadCardsWithXmlReader("Data/BattlepassInfo.xml");
                uint StartDate = LoadStartDate("Data/BattlepassInfo.xml");
                uint EndDate = LoadEndDate("Data/BattlepassInfo.xml");
                //Load season pass
                Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_SEASON_CHANGE(Player));
                Client.SendPacket(new PROTOCOL_SEASON_CHALLENGE_INFO_ACK(Player, cardDataList, StartDate, EndDate));

                ServerConfig CFG = GameXender.Client.Config;

                Client.SendPacket(new PROTOCOL_BASE_NOTICE_ACK(CFG.ChatAnnounce, CFG.ChatAnnounceColor));
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }
        private static List<CardData> LoadCardsWithXmlReader(string filePath)
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
        private static uint LoadEndDate(string filePath)
        {
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "EndDate")
                    {
                        reader.Read();
                        return uint.Parse(reader.Value);
                    }
                }
            }
            return 0; // Default jika elemen tidak ditemukan
        }
        private static uint LoadStartDate(string filePath)
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
        private static int LoadEnablePremiumWithXmlReader(string filePath)
        {
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "EnablePremium")
                    {
                        reader.Read();
                        return int.Parse(reader.Value);
                    }
                }
            }
            return 0; // Default jika elemen tidak ditemukan
        }
    }
}
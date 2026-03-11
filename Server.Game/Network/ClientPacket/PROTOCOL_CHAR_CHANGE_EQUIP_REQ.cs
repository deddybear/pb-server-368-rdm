using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CHAR_CHANGE_EQUIP_REQ : GameClientPacket
    {
        private readonly int[] EquipmentList = new int[14];
        private int AccessoryId, CharaPos, FR_SLOT, CT_SLOT;
        private bool AccesoryChange, EquipmentChange, ItemChange;

        private int EnableCouponChange, DisableCouponChange;

        private readonly SortedList<int, int> Items = new SortedList<int, int>();
        private readonly SortedList<int, int> EnableCoupons = new SortedList<int, int>();
        private readonly SortedList<int, int> DisableCoupons = new SortedList<int, int>();

        public PROTOCOL_CHAR_CHANGE_EQUIP_REQ(GameClient Client, byte[] Buffer)
        {
            //Console.WriteLine(HexDump(Buffer));
            Makeme(Client, Buffer);         
        }
        public override void Read()
        {

            AccessoryId = ReadD();

            ReadUD();
            AccesoryChange = (ReadC() == 1);

            //Disable Coupon
            int CouponCountDisable = ReadC();
            for (int i = 0; i < CouponCountDisable; i++)
            {
                int CouponId = ReadD();
                DisableCoupons.Add(i, CouponId);
            }
            DisableCouponChange = (ReadC());

            ReadC();// Tambah 1 Byte

            //Enable Coupon
            int CouponCount = ReadC();
            for (int i = 0; i < CouponCount; i++)
            {
                int CouponId = ReadD();
                EnableCoupons.Add(i, CouponId);
            }
            EnableCouponChange = (ReadC());

            ReadH();
            EquipmentList[0] = ReadD();
            ReadUD();
            EquipmentList[1] = ReadD();
            ReadUD();
            EquipmentList[2] = ReadD();
            ReadUD();
            EquipmentList[3] = ReadD();
            ReadUD();
            EquipmentList[4] = ReadD();
            ReadUD();
            CharaPos = ReadD();
            ReadUD();
            EquipmentList[5] = ReadD();
            ReadUD();
            EquipmentList[6] = ReadD();
            ReadUD();
            EquipmentList[7] = ReadD();
            ReadUD();
            EquipmentList[8] = ReadD();
            ReadUD();
            EquipmentList[9] = ReadD();
            ReadUD();
            EquipmentList[10] = ReadD();
            ReadUD();
            EquipmentList[11] = ReadD();
            ReadUD();
            EquipmentList[12] = ReadD();
            ReadUD();
            EquipmentList[13] = ReadD();
            ReadUD();
            EquipmentChange = (ReadC() == 1);
            int ItemCount = ReadC();
            for (int i = 0; i < ItemCount; i++)
            {
                int ItemId = ReadD();
                ReadUD();
                Items.Add(i, ItemId);
            }
            ItemChange = (ReadC() == 1);
            ReadC();
            FR_SLOT = ReadC();
            CT_SLOT = ReadC();
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
                int CharacterCount = Player.Character.Characters.Count;
                if (CharacterCount > 0)
                {
                    if (AccesoryChange)
                    {
                        AllUtils.ValidateAccesoryEquipment(Player, AccessoryId);
                    }
                    if (EquipmentChange)
                    {
                        AllUtils.ValidateCharacterEquipment(Player, Player.Equipment, EquipmentList, CharaPos, FR_SLOT, CT_SLOT);
                    }
                    if (EnableCouponChange > 0)
                    {
                        //Enable Coupon
                        AllUtils.ReadAndUpdateItemEnable(Player, EnableCoupons);
                    }
                    if (DisableCouponChange > 0)
                    {
                        //Enable Disable
                        AllUtils.ReadAndUpdateItemDisable(Player, DisableCoupons);
                    }
                    if (ItemChange)
                    {
                        AllUtils.ValidateItemEquipment(Player, Items);
                    }
                    AllUtils.ValidateCharacterSlot(Player, Player.Equipment, FR_SLOT, CT_SLOT);
                }
                RoomModel Room = Player.Room;
                if (Room != null)
                {
                    AllUtils.UpdateSlotEquips(Player, Room);
                }
                Client.SendPacket(new PROTOCOL_CHAR_CHANGE_EQUIP_ACK(0));


      
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
    }
}

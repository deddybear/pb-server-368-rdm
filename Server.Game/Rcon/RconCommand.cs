using Fleck;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Plugin.Core.Managers.Rcon;
using System;

namespace PointBlank.Game.Rcon
{
    public class RconCommand
    {
        private static RconCommand Manager;
        public static RconCommand Instance()
            => Manager != null ? Manager : (Manager = new RconCommand());

        private static IWebSocketConnection currentClient;  // Dijadikan static agar bisa diakses global
        private WebSocketServer ServerRcon;

        public RconCommand()
        {
            try
            {
                FleckLog.Level = Fleck.LogLevel.Error;

                ServerRcon = new WebSocketServer($"ws://{ConfigLoader.RconIp}:{ConfigLoader.RconPort}");
                ServerRcon.Start(Client =>
                {
                    Client.OnMessage = Message =>
                    {
                        currentClient = Client;  // Menyimpan client yang aktif
                        Receive(Decode(Message));  // Oper pesan ke Receive
                    };
                });
                CLogger.Print($"RconManager Address ws://{ConfigLoader.RconIp}:{ConfigLoader.RconPort}", LoggerType.Info);
            }
            catch (Exception Exc)
            {
                CLogger.Print("RconManager start: " + Exc.ToString(), LoggerType.Warning, Exc);
            }
        }

        private void Receive(string Message)
        {
            try
            {
                if (!ConfigLoader.RconValidIps.Contains(currentClient.ConnectionInfo.ClientIpAddress) && ConfigLoader.RconNotValidIpEnable)
                {
                    if (ConfigLoader.RconPrintNotValidIp)
                    {
                        RconLogger.Logs("Received message from unlisted ip: " + currentClient.ConnectionInfo.ClientIpAddress);
                        RconLogger.LogsPanel("Received message from unlisted ip: " + currentClient.ConnectionInfo.ClientIpAddress, 1);
                    }
                    return;
                }

                if (Message.Contains("|") && Message.Split('|').Length >= 3)
                {
                    string[] Pattern = Message.Split('|');

                    string Opcode = (Pattern[0]);
                    string Password = Pattern[1];

                    if (ConfigLoader.RconInfoCommand)
                    {
                        CLogger.Print($"[{currentClient.ConnectionInfo.ClientIpAddress}] Command : {Message}", LoggerType.Info);
                    }
                    if (Password != ConfigLoader.RconPassword)
                    {
                        RconLogger.Logs("RconManager received request with wrong password: " + Password);
                        RconLogger.LogsPanel("RconManager received request with wrong password: " + Password, 1);
                        return;
                    }

                    RconReceive Receive = null;
                    switch (Opcode)
                    {
                        /*//ADMIN PANEL
                        case "102": Receive = new RconSendPCCafePanels(); break;
                        case "103": Receive = new RconTopupPanel(); break;
                        case "104": Receive = new RconSendGiftPermanentPanel(); break;
                        case "105": Receive = new RconBannedPanel(); break;
                        case "106": Receive = new RconAnnouncerPanel(); break;
                        case "107": Receive = new RconSendRankPanel(); break;
                        case "108": Receive = new RconChangeNickPanel(); break;

                        //PLAYER AREA
                        case "WEBSHOP": Receive = new RconSendGiftOptionUser(); break;
                        case "REDEEMCODE": Receive = new RconRedeemUser(); break;

                        //DEVELOPER AREA
                        case "KICKPLAYER": Receive = new RconKickPlayerPanel(); break;
                        case "SETACCESS": Receive = new RconSetAccessPanel(); break;
                        case "BLOCKIP": Receive = new RconBlockIPPanel(); break;
                        case "RESTART": Receive = new RconRestartServerPanel(); break;
                        case "BALANCE": Receive = new RconSendBalancePanel(); break;*/
                        default:
                            RconLogger.Logs("RconManager received request with opcode: " + Opcode);
                            RconLogger.LogsPanel("RconManager received request with opcode: " + Opcode, 1); break;
                    }

                    try
                    {
                        // Inisialisasi dan jalankan perintah
                        Receive.Init(Pattern);
                        Receive.Run();
                        // Reset Token
                        Randomwinload();
                        ComDiv.UpdateDB("web_rcon_token", "token", finalString);
                        // Kirimkan respons jika perintah berhasil
                    }
                    catch (Exception ex)
                    {
                        // Jika terjadi pengecualian saat menjalankan perintah, kirimkan pesan gagal
                        RconLogger.Logs("Error executing command: " + ex.Message);
                        RconLogger.LogsPanel("Error executing command", 1);
                    }
                    finally
                    {
                        // Reset objek Receive jika diperlukan
                        Receive = null;
                    }
                }
                else
                {
                    RconLogger.LogsPanel("Wrong Command", 1);
                    RconLogger.Logs("RconManager received request with wrong message " + Message);
                }
            }
            catch (Exception Exc)
            {
                RconLogger.Logs("RconManager error receiving: " + Exc.Message);
                RconLogger.LogsPanel("RconManager error receiving", 1);
            }
        }

        public static void SendResponse(string pesan)
        {
            try
            {
                if (currentClient != null && currentClient.ConnectionInfo != null)
                {
                    currentClient.Send(pesan);
                }
            }
            catch (Exception ex)
            {
                RconLogger.LogsPanel("Error in SendResponse", 1);
                RconLogger.Logs("Error in SendResponse: " + ex.Message);
            }
        }
        public static bool CheckToken(string CheckToken)
        {
            foreach (string Token in RconManager.GetTokenRconList())
            {
                if (CheckToken.Length != 0 || Token.Length != 0 || Token != null || Token == "")
                {
                    if (CheckToken == Token)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static string finalString;
        public static void Randomwinload()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[32];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            finalString = new string(stringChars);
        }
        public static string Encode(string input)
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            string encoded = Convert.ToBase64String(inputBytes);
            return encoded;
        }
        public static string Decode(string encodedInput)
        {
            byte[] decodedBytes = Convert.FromBase64String(encodedInput);
            string decoded = System.Text.Encoding.UTF8.GetString(decodedBytes);
            return decoded;
        }
    }
}

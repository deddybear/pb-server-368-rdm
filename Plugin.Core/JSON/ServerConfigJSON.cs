using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Plugin.Core.JSON
{
    public class ServerConfigJSON
    {
        public static List<ServerConfig> Configs = new List<ServerConfig>();
        public static void Load()
        {
            string Path = "Data/ServerConfig.json";
            if (File.Exists(Path))
            {
                Parse(Path);
            }
            else
            {
                CLogger.Print($"File not found: {Path}", LoggerType.Warning);
            }
            CLogger.Print($"Plugin Loaded: {Configs.Count} Server Configs", LoggerType.Info);
        }
        public static ServerConfig GetConfig(int ConfigId)
        {
            lock (Configs)
            {
                foreach (ServerConfig Config in Configs)
                {
                    if (Config.ConfigId == ConfigId)
                    {
                        return Config;
                    }
                }
                return null;
            }
        }
        private static void Parse(string Path)
        {
            using (FileStream STR = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                if (STR.Length == 0)
                {
                    CLogger.Print($"File is empty: {Path}", LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        using (StreamReader Stream = new StreamReader(STR, Encoding.UTF8))
                        {
                            JsonDocument DataDeserialize = JsonDocument.Parse(Stream.ReadToEnd());
                            foreach (JsonElement Element in DataDeserialize.RootElement.GetProperty("Server").EnumerateArray())
                            {
                                if (!Element.TryGetProperty("ConfigId", out var configIdElement))
                                {
                                    CLogger.Print("ConfigId property is missing in JSON", LoggerType.Warning);
                                    continue;
                                }

                                int ConfigId = int.Parse(configIdElement.GetString());
                                if (ConfigId == 0)
                                {
                                    CLogger.Print($"Invalid Config Id: {ConfigId}", LoggerType.Warning);
                                    return;
                                }

                                ServerConfig Config = new ServerConfig()
                                {
                                    ConfigId = ConfigId,
                                    OnlyGM = Element.TryGetProperty("ChannelOnlyGM", out var onlyGMElement) ? bool.Parse(onlyGMElement.GetString()) : false,
                                    Missions = Element.TryGetProperty("EnableMissions", out var missionsElement) ? bool.Parse(missionsElement.GetString()) : false,
                                    AccessUFL = Element.TryGetProperty("AccessUFL", out var accessUFLElement) ? bool.Parse(accessUFLElement.GetString()) : false,
                                    UserFileList = Element.TryGetProperty("UserFileList", out var userFileListElement) ? userFileListElement.GetString() : string.Empty,
                                    ClientVersion = Element.TryGetProperty("ClientVersion", out var clientVersionElement) ? clientVersionElement.GetString() : string.Empty,
                                    GiftSystem = Element.TryGetProperty("EnableGiftSystem", out var giftSystemElement) ? bool.Parse(giftSystemElement.GetString()) : false,
                                    EnableClan = Element.TryGetProperty("EnableClan", out var enableClanElement) ? bool.Parse(enableClanElement.GetString()) : false,
                                    EnableTicket = Element.TryGetProperty("EnableTicket", out var enableTicketElement) ? bool.Parse(enableTicketElement.GetString()) : false,
                                    EnableTags = Element.TryGetProperty("EnableTags", out var enableTagsElement) ? bool.Parse(enableTagsElement.GetString()) : false,
                                    EnableBlood = Element.TryGetProperty("EnableBlood", out var enableBloodElement) ? bool.Parse(enableBloodElement.GetString()) : false,
                                    ExitURL = Element.TryGetProperty("ExitURL", out var exitURLElement) ? exitURLElement.GetString() : string.Empty,
                                    ShopURL = Element.TryGetProperty("ShopURL", out var shopURLElement) ? shopURLElement.GetString() : string.Empty,
                                    OfficialSteam = Element.TryGetProperty("OfficialSteam", out var officialSteamElement) ? officialSteamElement.GetString() : string.Empty,
                                    OfficialBannerEnabled = Element.TryGetProperty("OfficialBannerEnabled", out var officialBannerEnabledElement) ? bool.Parse(officialBannerEnabledElement.GetString()) : false,
                                    OfficialBanner = Element.TryGetProperty("OfficialBanner", out var officialBannerElement) ? officialBannerElement.GetString() : string.Empty,
                                    OfficialAddress = Element.TryGetProperty("OfficialAddress", out var officialAddressElement) ? officialAddressElement.GetString() : string.Empty,
                                    ChatAnnounceColor = Element.TryGetProperty("ChatAnnoucementColor", out var chatAnnounceColorElement) ? int.Parse(chatAnnounceColorElement.GetString()) : 0,
                                    ChannelAnnounceColor = Element.TryGetProperty("ChannelAnnoucementColor", out var channelAnnounceColorElement) ? int.Parse(channelAnnounceColorElement.GetString()) : 0,
                                    ChatAnnounce = Element.TryGetProperty("ChatAnnountcement", out var chatAnnounceElement) ? chatAnnounceElement.GetString() : string.Empty,
                                    ChannelAnnouce = Element.TryGetProperty("ChannelAnnouncement", out var channelAnnounceElement) ? channelAnnounceElement.GetString() : string.Empty,
                                    Showroom = Element.TryGetProperty("Showroom", out var showroomElement) ? ComDiv.ParseEnum<ShowroomView>(showroomElement.GetString()) : ShowroomView.S_Default
                                };
                                Configs.Add(Config);
                            }
                            Stream.Dispose();
                            Stream.Close();
                        }
                    }
                    catch (Exception Ex)
                    {
                        CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                    }
                }
                STR.Dispose();
                STR.Close();
            }
        }
        private static void Parse2(string Path)
        {
            using (FileStream STR = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                if (STR.Length == 0)
                {
                    CLogger.Print($"File is empty: {Path}", LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        using (StreamReader Stream = new StreamReader(STR, Encoding.UTF8))
                        {
                            JsonDocument DataDeserialize = JsonDocument.Parse(Stream.ReadToEnd());
                            foreach (JsonElement Element in DataDeserialize.RootElement.GetProperty("Server").EnumerateArray())
                            {
                                int ConfigId = int.Parse(Element.GetProperty("ConfigId").GetString());
                                if (ConfigId == 0)
                                {
                                    CLogger.Print($"Invalid Config Id: {ConfigId}", LoggerType.Warning);
                                    return;
                                }
                                ServerConfig Config = new ServerConfig()
                                {
                                    ConfigId = ConfigId,
                                    OnlyGM = bool.Parse(Element.GetProperty("ChannelOnlyGM").GetString()),
                                    Missions = bool.Parse(Element.GetProperty("EnableMissions").GetString()),
                                    AccessUFL = bool.Parse(Element.GetProperty("AccessUFL").GetString()),
                                    UserFileList = Element.GetProperty("UserFileList").GetString(),
                                    ClientVersion = Element.GetProperty("ClientVersion").GetString(),
                                    GiftSystem = bool.Parse(Element.GetProperty("EnableGiftSystem").GetString()),
                                    EnableClan = bool.Parse(Element.GetProperty("EnableClan").GetString()),
                                    EnableTicket = bool.Parse(Element.GetProperty("EnableTicket").GetString()),
                                    EnableTags = bool.Parse(Element.GetProperty("EnableTags").GetString()),
                                    EnableBlood = bool.Parse(Element.GetProperty("EnableBlood").GetString()),
                                    ExitURL = Element.GetProperty("ExitURL").GetString(),
                                    ShopURL = Element.GetProperty("ShopURL").GetString(),
                                    OfficialSteam = Element.GetProperty("OfficialSteam").GetString(),
                                    OfficialBannerEnabled = bool.Parse(Element.GetProperty("OfficialBannerEnabled").GetString()),
                                    OfficialBanner = Element.GetProperty("OfficialBanner").GetString(),
                                    OfficialAddress = Element.GetProperty("OfficialAddress").GetString(),
                                    ChatAnnounceColor = int.Parse(Element.GetProperty("ChatAnnoucementColor").GetString()),
                                    ChannelAnnounceColor = int.Parse(Element.GetProperty("ChannelAnnoucementColor").GetString()),
                                    ChatAnnounce = Element.GetProperty("ChatAnnountcement").GetString(),
                                    ChannelAnnouce = Element.GetProperty("ChannelAnnouncement").GetString(),
                                    Showroom = ComDiv.ParseEnum<ShowroomView>(Element.GetProperty("Showroom").GetString())
                                };
                                Configs.Add(Config);
                            }
                            Stream.Dispose();
                            Stream.Close();
                        }
                    }
                    catch (Exception Ex)
                    {
                        CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                    }
                }
                STR.Dispose();
                STR.Close();
            }
        }
    }
}

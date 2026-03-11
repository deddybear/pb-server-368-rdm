using System.Data;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Models.SubHead;

namespace Server.Match.Network.Actions.SubHead
{
    public class GrenadeSync
    {
        public static GrenadeInfo ReadInfo(SyncClientPacket C, bool GenLog, bool OnlyBytes = false)
        {
            GrenadeInfo Info = new GrenadeInfo()
            {
                Extensions = C.ReadC(),
                WeaponId = C.ReadD(),
                BoomInfo = C.ReadUH(),
                ObjPos_X = C.ReadUH(),
                ObjPos_Y = C.ReadUH(),
                ObjPos_Z = C.ReadUH(),
                Unk1 = C.ReadUH(),
                Unk2 = C.ReadUH(),
                Unk3 = C.ReadUH(),
                GrenadesCount = C.ReadUH(),
                Unk4 = C.ReadUH(),
                Unk5 = C.ReadUH(),
                Unk6 = C.ReadUH(),
                Unk7 = C.ReadC(),
            };
            if (!OnlyBytes)
            {
                Info.WeaponClass = (ClassType)ComDiv.GetIdStatics(Info.WeaponId, 2);
            }
            if (GenLog)
            {
                CLogger.Print($"Sub Head: GrenadeSync; Weapon Id: {Info.WeaponId}; Grenade Count: {Info.GrenadesCount}", LoggerType.Warning);
            }
            return Info;
        }
        public static void WriteInfo(SyncServerPacket S, GrenadeInfo Info)
        {

            //CLogger.Print($"WriteInfo GrenadeSync; Extensions Id: {Info.Extensions};", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; Grenade Count: {Info.GrenadesCount}", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; Unk7: {Info.Unk7}", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; WeaponId: {Info.WeaponId}", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; BoomInfo: {Info.BoomInfo}", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; ObjPos_X: {Info.ObjPos_X}", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; ObjPos_Y: {Info.ObjPos_Y}", LoggerType.Warning);
            //CLogger.Print($"WriteInfo GrenadeSync; ObjPos_Z: {Info.ObjPos_Z}", LoggerType.Warning);

            S.WriteC(Info.Extensions);    
            S.WriteD(Info.WeaponId);
            S.WriteH(Info.BoomInfo);
            S.WriteH(Info.ObjPos_X);
            S.WriteH(Info.ObjPos_Y);
            S.WriteH(Info.ObjPos_Z);
            S.WriteH(Info.Unk1); //Info.Unk1
            S.WriteH(Info.Unk2); //Info.Unk2
            S.WriteH(Info.Unk3); //Info.Unk3
            S.WriteH(Info.GrenadesCount);
            S.WriteH(Info.Unk4); //Info.Unk4
            S.WriteH(Info.Unk5); //Info.Unk5
            S.WriteH(Info.Unk6); //Info.Unk6
            S.WriteC(Info.Unk7); //Info.Unk6
        }
    }
}
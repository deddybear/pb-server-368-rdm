using Server.Match.Data.Enums;
using Server.Match.Data.Models.Event;
using System.Collections.Generic;
using Plugin.Core;
using Server.Match.Data.Utils;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.Enums;
using Server.Match.Data.Models;
using Microsoft.Extensions.Logging;
using System;

namespace Server.Match.Network.Actions.Event
{
    public class GrenadeHit
    {
        public static List<GrenadeHitInfo> ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog, bool OnlyBytes = false)
        {
            List<GrenadeHitInfo> Hits = new List<GrenadeHitInfo>();
            int Count = C.ReadC();
            for (int i = 0; i < Count; i++)
            {
                GrenadeHitInfo Hit = new GrenadeHitInfo()
                {
                    WeaponId = C.ReadD(),
                    Extensions = C.ReadC(),
                    Unk = C.ReadC(),
                    GrenadesCount = C.ReadUH(),
                    ObjectId = C.ReadUH(),
                    HitInfo = C.ReadUD(),
                    PlayerPos = C.ReadUHV(),
                    FirePos = C.ReadUHV(),
                    HitPos = C.ReadUHV(),
                    BoomInfo = C.ReadUH(),
                    DeathType = (CharaDeath)C.ReadC()
                };
                if (!OnlyBytes)
                {
                    Hit.HitEnum = (HitType)AllUtils.GetHitHelmet(Hit.HitInfo);
                    if (Hit.BoomInfo > 0)
                    {
                        Hit.BoomPlayers = new List<int>();
                        for (int x = 0; x < 16; x++)
                        {
                            int Flag = (1 << x);
                            if ((Hit.BoomInfo & Flag) == Flag)
                            {
                                Hit.BoomPlayers.Add(x);
                            }
                        }
                    }
                    Hit.WeaponClass = (ClassType)ComDiv.GetIdStatics(Hit.WeaponId, 2);
                }
                if (GenLog)
                {
                    CLogger.Print($"Slot: {Action.Slot}; GrenadeHit; [Player Postion] X: {Hit.FirePos.X}; Y: {Hit.FirePos.Y}; Z: {Hit.FirePos.Z}", LoggerType.Warning);
                    CLogger.Print($"Slot: {Action.Slot}; GrenadeHit; [Object Postion] X: {Hit.HitPos.X}; Y: {Hit.HitPos.Y}; Z: {Hit.HitPos.Z}", LoggerType.Warning);
                }
                Hits.Add(Hit);
            }
            return Hits;
        }
        public static void WriteInfo(SyncServerPacket S, List<GrenadeHitInfo> Hits)
        {
            S.WriteC((byte)Hits.Count);
            foreach (GrenadeHitInfo Hit in Hits)
            {
                S.WriteD(Hit.WeaponId);
                S.WriteC(Hit.Extensions);
                S.WriteC(Hit.Unk);
                S.WriteH(Hit.GrenadesCount);
                S.WriteH(Hit.ObjectId);
                S.WriteD(Hit.HitInfo);
                S.WriteHV(Hit.PlayerPos);
                S.WriteHV(Hit.FirePos);
                S.WriteHV(Hit.HitPos);
                S.WriteH(Hit.BoomInfo);
                S.WriteC((byte)Hit.DeathType);
            }
        }
    }
    public class GrenadeHit2
    {
        public static List<GrenadeHitInfo> ReadInfo(ActionModel Action, SyncClientPacket p, bool genLog, bool OnlyBytes = false)
        {
            return BaseReadInfo(Action, p, OnlyBytes, genLog);
        }

        public static void ReadInfo(SyncClientPacket p)
        {
            int objsCount = p.ReadC();
            p.Advance(32 * objsCount);
        }

        private static List<GrenadeHitInfo> BaseReadInfo(ActionModel Action, SyncClientPacket p, bool OnlyBytes, bool genLog)
        {
            List<GrenadeHitInfo> hits = new List<GrenadeHitInfo>();
            int objsCount = p.ReadC();
            for (int i = 0; i < objsCount; i++)
            {
                GrenadeHitInfo hit = new GrenadeHitInfo
                {
                    HitInfo = p.ReadUD(),
                    BoomInfo = p.ReadUH(),
                    PlayerPos = p.ReadUHV(),
                    Extensions = p.ReadC(),
                    WeaponId = p.ReadD(),
                    DeathType = (CharaDeath)p.ReadC(),
                    FirePos = p.ReadUHV(),
                    HitPos = p.ReadUHV(),
                    GrenadesCount = p.ReadUH()
                };
                if (!OnlyBytes)
                {
                    hit.HitEnum = (HitType)AllUtils.GetHitHelmet(hit.HitInfo);
                    if (hit.BoomInfo > 0)
                    {
                        hit.BoomPlayers = new List<int>();
                        for (int s = 0; s < 16; s++)
                        {
                            int flag = (1 << s);
                            if ((hit.BoomInfo & flag) == flag)
                            {
                                hit.BoomPlayers.Add(s);
                            }
                        }
                    }
                    hit.WeaponClass = (ClassType)ComDiv.GetIdStatics(hit.WeaponId, 2);
                }
                if (genLog)
                {
                    CLogger.Print($"Slot: {Action.Slot}; GrenadeHit; [Player Postion] X: {hit.FirePos.X}; Y: {hit.FirePos.Y}; Z: {hit.FirePos.Z}", LoggerType.Warning);
                    CLogger.Print($"Slot: {Action.Slot}; GrenadeHit; [Object Postion] X: {hit.HitPos.X}; Y: {hit.HitPos.Y}; Z: {hit.HitPos.Z}", LoggerType.Warning);
                }
                hits.Add(hit);
            }
            return hits;
        }
        public static void WriteInfo(SyncServerPacket s, List<GrenadeHitInfo> hits)
        {
            s.WriteC((byte)hits.Count);
            for (int i = 0; i < hits.Count; i++)
            {
                GrenadeHitInfo hit = hits[i];
                s.WriteD(hit.HitInfo);
                s.WriteH(hit.BoomInfo);
                s.WriteHV(hit.PlayerPos);
                s.WriteC(hit.Extensions);
                s.WriteD(hit.WeaponId);
                s.WriteC((byte)hit.DeathType);
                s.WriteHV(hit.FirePos);
                s.WriteHV(hit.HitPos);
                s.WriteH(hit.GrenadesCount);
            }
        }
    }
}
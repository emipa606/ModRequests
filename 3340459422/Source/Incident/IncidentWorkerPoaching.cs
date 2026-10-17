// ******************************************************************
//       /\ /|       @file       IncidentWorkerPoaching.cs
//       \ V/        @brief      事件 偷猎
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-17 13:45:19
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using SR.ModRimWorld.RaidExtension.Util;
using Verse;

namespace SR.ModRimWorld.RaidExtension
{
    [UsedImplicitly]
    public class IncidentWorkerPoaching : IncidentWorker_RaidEnemy
    {
        static IncidentWorkerPoaching()
        {
            HideRaidStrategy.RegisterRaidLikeEvent( typeof(IncidentWorkerPoaching));
        }

        /// <summary>
        /// 是否可以生成事件
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if( !base.CanFireNowSub( parms ))
                return false;

            if (!(parms.target is Map map))
            {
                Log.Error($"{MiscDef.LogTag}target must be a map.");
                return false;
            }
            if (HarmonyUtil.IsSOS2SpaceMap(map))
            {
                Log.Error($"{MiscDef.LogTag}target must not be an SOS2 space map.");
                return false;
            }

            var isAnimalTargetExist = map.IsAnimalTargetExist(MiscDef.MinTargetRequireHealthScale);

            //目标动物不存在 无法触发事件
            if (!isAnimalTargetExist)
            {
                Log.Warning($"{MiscDef.LogTag}can't find any animal.");
                return false;
            }

            //候选派系列表
            var candidateFactionList = CandidateFactions(parms).ToList();
            return Enumerable.Any(candidateFactionList, faction => faction.HostileTo(Faction.OfPlayer));
        }

        /// <summary>
        /// 派系能否成为资源组
        /// </summary>
        /// <param name="f"></param>
        /// <param name="map"></param>
        /// <param name="desperate"></param>
        /// <returns></returns>
        public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
        {
            return base.FactionCanBeGroupSource(f, parms, desperate) && f.def.humanlikeFaction && !f.Hidden;
        }

        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                Log.Error($"{MiscDef.LogTag}target must be a map.");
                return false;
            }

            var isAnimalTargetExist = map.IsAnimalTargetExist(MiscDef.MinTargetRequireHealthScale);
            //目标动物不存在 无法触发事件
            if (!isAnimalTargetExist)
            {
                Log.Warning($"{MiscDef.LogTag}can't find any animal.");
                return false;
            }
            return base.TryExecuteWorker(parms);
        }

        // The functionality of ResolveRaidPoints() is done in RaidStrategyWorkerLogging.

        /// <summary>
        /// 解决突袭策略
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="groupKind"></param>
        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.SrPoaching;
        }

        /// <summary>
        /// 获取信件定义
        /// </summary>
        /// <returns></returns>
        protected override LetterDef GetLetterDef()
        {
            if( HideRaidStrategy.Enabled )
                return base.GetLetterDef();
            return LetterDefOf.ThreatSmall;
        }

        /// <summary>
        /// 解决信件
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="pawnList"></param>
        protected virtual void ResolveLetter(IncidentParms parms, List<Pawn> pawnList)
        {
            var letterLabel = (TaggedString) GetLetterLabel(parms);
            var letterText = (TaggedString) GetLetterText(parms, pawnList);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawnList, ref letterLabel, ref letterText,
                GetRelatedPawnsInfoLetterText(parms), true);
            SendStandardLetter(letterLabel, letterText, GetLetterDef(), parms, pawnList,
                Array.Empty<NamedArgument>());
        }
    }

    public class IncidentWorkerPoachingSurprise : IncidentWorkerPoaching
    {
        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.SrPoachingSurprise;
        }
    }
}

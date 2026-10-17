// ******************************************************************
//       /\ /|       @file       IncidentWorkerHostileTraveler.cs
//       \ V/        @brief      事件 敌对旅行者
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-07-11 18:39:50
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;
using System.Collections.Generic;
using SR.ModRimWorld.RaidExtension.Util;

namespace SR.ModRimWorld.RaidExtension
{
    [UsedImplicitly]
    public class IncidentWorkerHostileTraveler : IncidentWorker_TravelerGroup
    {
        protected override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Combat;

        /// <summary>
        /// 派系可以成为组
        /// </summary>
        /// <param name="f"></param>
        /// <param name="map"></param>
        /// <param name="desperate"></param>
        /// <returns></returns>
        public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
        {
            Map map = (Map)parms.target;
            return !f.IsPlayer && !f.defeated && !f.temporary &&
                   (desperate || f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) &&
                       f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)) && !f.Hidden &&
                   f.HostileTo(Faction.OfPlayer) &&
                   f.def.pawnGroupMakers != null && f.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDef);
        }

        // Based on IncidentWorker_TravelerGroup.TryExecuteWorker().
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryResolveParms(parms))
                return false;

            bool isSurprise = this is IncidentWorkerHostileTravelerSurprise;
            if( isSurprise )
                parms.points *= 0.8f;

            // Using mere RCellFinder.TryFindTravelDestFrom() simply tries to find a reachable tile on the opposite
            // edge of the map, but it does not care about the colony, resulting in the hostile travellers sometimes
            // walking very close to the base (especially if it's not walled off), which looks silly.
            // First try to find a path that avoids the colony in a reasonable distance.
            IntVec3 travelDest = IntVec3.Invalid;
            if( !isSurprise )
                travelDest = PathAvoidsColonistsChecker.FindPathDestination( map, parms.spawnCenter );
            if( travelDest == IntVec3.Invalid ) // No luck? Simply try to find a path.
            {
                if (!RCellFinder.TryFindTravelDestFrom(parms.spawnCenter, map, out travelDest))
                {
                    Log.Warning(string.Concat("Failed to do traveler incident from ", parms.spawnCenter, ": Couldn't find anywhere for the traveler to go."));
                    return false;
                }
            }

            List<Pawn> list = SpawnPawns(parms);
            if (list.Count == 0)
                return false;

            LordJobHostileTravelAndExit lordJob = new LordJobHostileTravelAndExit(travelDest, isSurprise);
            LordMaker.MakeNewLord(parms.faction, lordJob, map, list);

            SendLetter(parms, list);

            return true;
        }

        private void SendLetter(IncidentParms parms, List<Pawn> pawns)
        {
            if( HideRaidStrategy.Enabled )
            {
                HideRaidStrategy.SendLetter(this, parms, pawns);
                return;
            }
            var letterLabel = "SrHostileTravellersPassing"
                .Translate((NamedArgument) parms.faction.Name).CapitalizeFirst();
            TaggedString letterText;
            if (pawns.Count == 1)
            {
                letterText = "SingleTravelerPassing".Translate(pawns[0].story.Title, parms.faction.Name, pawns[0].Name.ToStringFull, pawns[0].Named("PAWN"));
                letterText = letterText.AdjustedFor(pawns[0]);
            }
            else
                letterText = "GroupTravelersPassing".Translate(parms.faction.Name);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText,
                "LetterRelatedPawnsNeutralGroup".Translate((NamedArgument) Faction.OfPlayer.def.pawnsPlural), true);
            SendStandardLetter(letterLabel, letterText, LetterDefOf.ThreatSmall, parms, pawns);
        }
    }

    public class IncidentWorkerHostileTravelerSurprise : IncidentWorkerHostileTraveler
    {
    }
}

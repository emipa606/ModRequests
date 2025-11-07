using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BillDoorsFramework
{
    public class IncidentWorker_MedEvac : IncidentWorker_DropThings
    {
        Pawn intendedPawn;

        Pawn findMostWoundedPawn(List<Pawn> pawns)
        {
            bool anyoneWounded = false;
            Pawn pawn = null;
            if (pawns.Count == 1)
            {
                anyoneWounded = pawns[0].health.hediffSet.HasTendableHediff();
            }
            else if (pawns.Any())
            {
                pawns.SortBy((Pawn p) =>
                {
                    if (p.health.hediffSet.BleedRateTotal > 5) { anyoneWounded = true; return -9999; }
                    if (p.Downed) { anyoneWounded = true; return -6666; }
                    if (p.health.hediffSet.HasTendableHediff()) { anyoneWounded = true; return p.health.summaryHealth.SummaryHealthPercent; }
                    return 6666;
                });
            }
            if (anyoneWounded)
            {
                pawn = pawns[0];
            }
            return pawn;
        }

        Pawn FindMostWoundedPawn(List<Pawn> pawns)
        {
            List<Pawn> Pawns = pawns.Where(x => x.Spawned && !x.Map.areaManager.Home[x.Position]).ToList();
            return findMostWoundedPawn(Pawns);
        }

        Pawn FindMostWoundedPawn(List<Pawn> pawns, List<IntVec3> area)
        {
            List<Pawn> Pawns = pawns.Where(x => { return x.Spawned && area.Contains(x.Position); }).ToList();
            return findMostWoundedPawn(Pawns);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            intendedPawn = null;
            Map map = (Map)parms.target;
            List<Pawn> pawns;
            if (parms.faction == Faction.OfPlayer)
            {
                pawns = map.mapPawns.FreeColonistsAndPrisoners;
                pawns.Concat(map.mapPawns.SlavesOfColonySpawned);
            }
            else
            {
                pawns = map.mapPawns.PawnsInFaction(parms.faction);
            }

            List<IntVec3> cells = GenRadial.RadialCellsAround(parms.spawnCenter, 5f, true).ToList();

            Pawn pawn = FindMostWoundedPawn(pawns, new List<IntVec3> { parms.spawnCenter });
            if (pawn == null)
            {
                pawn = FindMostWoundedPawn(pawns, cells);
            }
            if (pawn == null)
            {
                pawn = FindMostWoundedPawn(pawns);
            }
            if (pawn != null)
            {
                intendedPawn = pawn;

                IntVec3 oneCellAbove = pawn.Position + new IntVec3(0, 0, 1);

                if (DropCellFinder.IsGoodDropSpot(oneCellAbove, map, false, true))
                {
                    SpawnSkyfaller(parms, oneCellAbove);
                }
                else
                {
                    SpawnSkyfaller(parms, pawn.Position);
                }
                Messages.Message(extension.messageKey.Translate(extension.skyfallerDef.label), new TargetInfo(parms.spawnCenter, map), MessageTypeDefOf.NeutralEvent);
                return true;
            }
            return false;
        }

        protected override void SpawnSkyfaller(IncidentParms parms, IntVec3 pos)
        {
            Skyfaller skyfaller = SkyfallerMaker.SpawnSkyfaller(extension.skyfallerDef, pos, (Map)parms.target);
            if (skyfaller is Skyfaller_MedEvac evac)
            {
                evac.intendedThing = intendedPawn;
            }
        }
    }
}

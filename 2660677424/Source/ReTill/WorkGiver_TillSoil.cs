using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReTill
{
    public class WorkGiver_TillSoil : WorkGiver_Scanner
    {
        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            if (ReTillDefOf.VCE_TilledSoil == null)
            {
                yield break;
            }

            var map = pawn.Map;
            var art = map.GetComponent<AutoReTillZone>();
            foreach (var zone in art.active)
            {
                foreach (var cell in zone.Cells)
                {
                    if (map.terrainGrid.TerrainAt(cell) == ReTillDefOf.VCE_TilledSoil)
                    {
                        continue;
                    }

                    if (GenConstruct.CanPlaceBlueprintAt(ReTillDefOf.VCE_TilledSoil, cell, Rot4.North, map))
                    {
                        yield return cell;
                    }
                }
            }
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            var map = pawn.Map;
            var blueprint = cell.GetFirstThing(map, ReTillDefOf.VCE_TilledSoil.blueprintDef);
            if (blueprint != null)
            {
                return JobOnThing(pawn, blueprint, forced);
            }

            if (cell.GetTerrain(map) == ReTillDefOf.VCE_TilledSoil)
            {
                return null;
            }

            if (!GenConstruct.CanPlaceBlueprintAt(ReTillDefOf.VCE_TilledSoil, cell, Rot4.North, map))
            {
                return null;
            }

            bool wantedThingGrowing = false;

            var toGrow = cell.GetPlantToGrowSettable(map)?.GetPlantDefToGrow();
            foreach (var thing in cell.GetThingList(map))
            {
                if (thing.def == toGrow)
                {
                    wantedThingGrowing = true;
                    break;
                }

                if (thing.def.BlocksPlanting())
                {
                    // avoid tilling soil under wild plants as it will get un-tilled when the plant is cut
                    return null;
                }
            }

            if (!wantedThingGrowing)
            {
                // don't till before planting; in many cases grass will grow before the workers get around to actually planting
                return null;
            }

            return JobMaker.MakeJob(ReTillDefOf.ReTill_PlaceAndBuildTilledSoil, cell);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var map = pawn.Map;
            foreach (var def in ReTill.TilledSoilDefs)
            {
                foreach (var blueprint in map.listerThings.ThingsOfDef(def.blueprintDef))
                {
                    yield return blueprint;
                }

                foreach (var frame in map.listerThings.ThingsOfDef(def.frameDef))
                {
                    yield return frame;
                }
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return null;
            }

            if (GenConstruct.FirstBlockingThing(t, pawn) != null)
            {
                return GenConstruct.HandleBlockingThingJob(t, pawn, forced);
            }

            if (!GenConstruct.CanConstruct(t, pawn, def.workType, forced))
            {
                return null;
            }

            if (t is Blueprint)
            {
                return JobMaker.MakeJob(JobDefOf.PlaceNoCostFrame, t);
            }

            if (t is Frame)
            {
                return JobMaker.MakeJob(ReTillDefOf.ReTill_FinishFrame, t);
            }

            return null;
        }
    }
}

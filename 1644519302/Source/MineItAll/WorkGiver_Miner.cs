using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace MineItAll
{
    class WorkGiver_Miner : RimWorld.WorkGiver_Miner
    {
        public DesignationDef MineAll => DefDatabase<DesignationDef>.GetNamed("MineAll");
        public string NoPathTrans => "NoPath".Translate();

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation designation in pawn.Map.designationManager.allDesignations)
			{
                if (designation.def != DesignationDefOf.Mine && designation.def != MineAll) continue;

				bool mayBeAccessible = false;
				for (int i = 0; i < 8; i++)
				{
					IntVec3 adjacentCell = designation.target.Cell + GenAdj.AdjacentCells[i];
					if (adjacentCell.InBounds(pawn.Map) && adjacentCell.Walkable(pawn.Map))
					{
						mayBeAccessible = true;
						break;
					}
				}

				if (mayBeAccessible)
				{
					Mineable mineable = designation.target.Cell.GetFirstMineable(pawn.Map);
					if (mineable != null)
					{
						yield return mineable;
					}
				}
			}
			yield break;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn, forced) && !pawn.Map.designationManager.AnySpawnedDesignationOfDef(MineAll);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!t.def.mineable)
                return null;

            DesignationManager designationManager = pawn.Map.designationManager;
            if (designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) == null && designationManager.DesignationAt(t.Position, MineAll) == null)
                return null;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return null;

            bool reachable = false;
            for (int i = 0; i < 8; i++)
            {
                IntVec3 adjacentCell = t.Position + GenAdj.AdjacentCells[i];
                if (adjacentCell.InBounds(pawn.Map) && adjacentCell.Standable(pawn.Map) && ReachabilityImmediate.CanReachImmediate(adjacentCell, t, pawn.Map, PathEndMode.Touch, pawn))
                {
                    reachable = true;
                    break;
                }
            }
            if (!reachable)
            {
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 adjacentCell = t.Position + GenAdj.AdjacentCells[i];
                    if (adjacentCell.InBounds(t.Map))
                    {
                        if (ReachabilityImmediate.CanReachImmediate(adjacentCell, t, pawn.Map, PathEndMode.Touch, pawn))
                        {
                            if (adjacentCell.Walkable(t.Map) && !adjacentCell.Standable(t.Map))
                            {
                                Thing haulableThing = null;
                                List<Thing> thingList = adjacentCell.GetThingList(t.Map);
                                for (int j = 0; j < thingList.Count; j++)
                                {
                                    if (thingList[j].def.designateHaulable && thingList[j].def.passability == Traversability.PassThroughOnly)
                                    {
                                        haulableThing = thingList[j];
                                        break;
                                    }
                                }
                                if (haulableThing != null)
                                {
                                    Job job = HaulAIUtility.HaulAsideJobFor(pawn, haulableThing);
                                    if (job != null)
                                    {
                                        return job;
                                    }
                                    JobFailReason.Is(NoPathTrans, null);
                                    return null;
                                }
                            }
                        }
                    }
                }
                JobFailReason.Is(NoPathTrans, null);
                return null;
            }
            return new Job(JobDefOf.Mine, t, 20000, true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Metro
{
    public class JobGiver_MutantBerserk : ThinkNode_JobGiver
    {
        private static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);

        private readonly float distanceToReact = 50f;
        public override Job TryGiveJob(Pawn pawn)
        {
            if (GridsUtility.Fogged(pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(JobDefOf.LayDown);
            }

            if (pawn.TryGetAttackVerb(null, false) == null)
            {
                return null;
            }
            if (pawn.IsSpider() || pawn.IsNosalis())
            {
                var activeThreat = pawn.FindClosestActiveThreats(distanceToReact);
                if (activeThreat != null)
                {
                    //Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                    return pawn.MeleeAttackJob(activeThreat, ExpiryInterval_Melee.RandomInRange);
                }

                var mapComp = pawn.Map.GetComponent<HiveAIManager>();
                foreach (var target in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn).OrderBy(x => x.Thing.Position.DistanceTo(pawn.Position)))
                {
                    if (target.Thing is Pawn victim && !victim.Downed) //target.Thing.Position.DistanceTo(pawn.Position) <= distanceToReact &&
                    {
                        var cells = pawn.GetHiveCells();
                        if (cells != null)
                        {
                            var closestHiveCell = cells.OrderBy(x => x.DistanceTo(target.Thing.Position)).First();
                            if (target.Thing.Position.DistanceTo(closestHiveCell) < distanceToReact && target.Thing.Position.DistanceTo(pawn.Position) < distanceToReact
                                && GenSight.LineOfSight(target.Thing.Position, pawn.Position, pawn.Map))
                            {
                                mapComp.Notify_PawnsInHiveAboutCloseThreat(pawn, target.Thing);
                                //Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                                return pawn.MeleeAttackJob(target.Thing, ExpiryInterval_Melee.RandomInRange);
                            }
                        }
                    }
                }

                if (!pawn.InSafeCondition()) return null;
                if (!MetroUtils.IsNightNow(pawn.Map)) return null;
                if (!pawn.GroupNeedsMoreFood()) return null;
            }

            Pawn pawn2 = pawn.FindPawnTarget(distanceToReact, (Thing p) => p is Pawn v && !v.Downed);
            if (pawn2 == null)
            {
                if (pawn.GetRoom() != null && !pawn.GetRoom().PsychologicallyOutdoors)
                {
                    Predicate<Thing> validator = delegate (Thing t)
                    {
                        return t.def.defName.ToLower().Contains("door");
                    };
                    var door = GenClosest.ClosestThingReachable(pawn.Position,
                    pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly,
                    TraverseMode.ByPawn, false)
                    , 5f, validator);
                    if (door != null)
                    {
                        return pawn.MeleeAttackJob(door, ExpiryInterval_Melee.RandomInRange);
                    }
                }
            }
            else if (!pawn2.Downed)
            {
                return pawn.MeleeAttackJob(pawn2, ExpiryInterval_Melee.RandomInRange);
            }
            else if (pawn2.Downed)
            {
                return null;
            }

            Building building = pawn.FindTurretTarget();
            if (building != null)
            {
                return pawn.MeleeAttackJob(building, ExpiryInterval_Melee.RandomInRange);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    IntVec3 loc;
                    if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out loc))
                    {
                        Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap, false);
                        return null;
                    }
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    if (randomCell == pawn.Position)
                    {
                        return JobMaker.MakeJob(JobDefOf.Wait, 30, false);
                    }

                    return JobMaker.MakeJob(JobDefOf.Goto, randomCell);
                }
            }
            return null;
        }
    }
}
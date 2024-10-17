using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
	public static class MetroUtils
	{
        public static Job MeleeAttackJob(this Pawn pawn, Thing enemyTarget, int expiryInterval)
        {
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, enemyTarget);
            job.expiryInterval = expiryInterval;
            job.checkOverrideOnExpire = true;
            job.expireRequiresEnemiesNearby = true;
            return job;
        }

        public static Building FindTurretTarget(this Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn,
                TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat |
                TargetScanFlags.NeedAutoTargetable, (Thing t) => t is Building, 0f, 70f,
                default(IntVec3), float.MaxValue, false, true);
        }

        public static void ReCheckActiveThreats(ref List<Thing> threats)
        {
            for (int num = threats.Count - 1; num >= 0; num--)
            {
                var threat = threats[num];
                if (threat is Pawn victim)
                {
                    if (victim.Dead)
                    {
                        threats.RemoveAt(num);
                    }
                }
                else if (threat is null)
                {
                    threats.RemoveAt(num);
                }
                else if (threat.Map is null)
                {
                    threats.RemoveAt(num);
                }
            }
        }

        public static Thing FindClosestActiveThreats(this Pawn pawn, float distance)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            Thing threat = null;
            Predicate<Thing> validator = delegate (Thing t)
            {
                if (t.Position.DistanceTo(pawn.Position) > distance) return false;
                if (t is Pawn victim && victim.Downed) return false;
                if (t.Map == null) return false;
                return true;
            };

            if (pawn.IsSpider())
            {
                ReCheckActiveThreats(ref mapComp.activeThreatsForSpiders);
                threat = mapComp.activeThreatsForSpiders.Where(x => validator(x)).OrderBy(x => x.Position.DistanceTo(pawn.Position)).FirstOrDefault();
            }
            else if (pawn.IsNosalis())
            {
                ReCheckActiveThreats(ref mapComp.activeThreatsForNosalises);
                threat = mapComp.activeThreatsForNosalises.Where(x => validator(x)).OrderBy(x => x.Position.DistanceTo(pawn.Position)).FirstOrDefault();
            }
            return threat;
        }

        public static Pawn FindPawnTarget(this Pawn pawn, float distance, Predicate<Thing> customValidator = null)
        {
            Pawn victim = null;
            if (customValidator == null)
            {
                customValidator = new Predicate<Thing>(x => true);
            }
            bool Predicate(Thing p) => p != null && p != pawn && p.def != pawn.def && p is Pawn prey && pawn.CanReserve(p) && FoodUtility.IsAcceptablePreyFor(pawn, prey);
            victim = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), distance, Predicate);
            return victim;
        }
        public static Corpse FindCorpseTarget(this Pawn pawn, float distance, Predicate<Thing> customValidator = null)
        {
            Corpse victim = null;
            if (customValidator == null)
            {
                customValidator = new Predicate<Thing>(x => true);
            }
            bool Predicate(Thing p) => p != null && p is Corpse prey && p != pawn && prey.InnerPawn.def != pawn.def && pawn.CanReserve(p) && customValidator(p);
            victim = (Corpse)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), distance, Predicate);
            return victim;
        }

        public static bool TryFindClosestMountainRoof(this Pawn pawn, out IntVec3 spot)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            HashSet<IntVec3> cells = mapComp.GetHiveCellsFor(pawn);
            foreach (var cell in cells)
            {
                var roof = cell.GetRoof(pawn.Map);
                if (roof == RoofDefOf.RoofRockThick || roof == RoofDefOf.RoofRockThin)
                {
                    if (cell.GetEdifice(pawn.Map) == null && pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
                    {
                        spot = cell;
                        return true;
                    }
                }
            }
            spot = IntVec3.Invalid;
            return false;
        }

        public static bool TryFindClosestMountainToMine(this Pawn pawn, out Mineable rock)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            HashSet<IntVec3> cells = mapComp.GetHiveCellsFor(pawn);
            var hiveCenter = mapComp.GetHiveCenterFor(pawn);
            foreach (var cell in cells.OrderBy(x => IntVec3Utility.DistanceTo(x, hiveCenter)))
            {
                var roof = cell.GetRoof(pawn.Map);
                if (roof == RoofDefOf.RoofRockThick || roof == RoofDefOf.RoofRockThin)
                {
                    var firstMineable = cell.GetFirstMineable(pawn.Map);
                    if (firstMineable != null && pawn.CanReserveAndReach(firstMineable, PathEndMode.Touch, Danger.Deadly))
                    {
                        rock = firstMineable;
                        return true;
                    }
                }
            }
            rock = null;
            return false;
        }

        public static bool TryFindClosestMountainToExpand(this Pawn pawn, out Mineable rock)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            HashSet<IntVec3> cells = mapComp.GetHiveCellsFor(pawn);
            var hiveCenter = mapComp.GetHiveCenterFor(pawn);
            foreach (var cell in cells.OrderBy(x => IntVec3Utility.DistanceTo(x, hiveCenter)))
            {
                if (CellHasNoAdjastmentOutdoorCells(cell, cells, pawn.Map))
                {
                    var roof = cell.GetRoof(pawn.Map);
                    if (roof == RoofDefOf.RoofRockThick || roof == RoofDefOf.RoofRockThin)
                    {
                        var firstMineable = cell.GetFirstMineable(pawn.Map);
                        if (firstMineable != null && mapComp.ThingIsNotReservedByPawns(firstMineable, pawn) 
                            && pawn.CanReserveAndReach(firstMineable, PathEndMode.Touch, Danger.Deadly))
                        {
                            rock = firstMineable;
                            return true;
                        }
                    }
                }
            }
            rock = null;
            return false;
        }

        public static bool CellHasNoAdjastmentOutdoorCells(IntVec3 cell, HashSet<IntVec3> rockGroup, Map map)
        {
            foreach (var c in CellRect.SingleCell(cell).ExpandedBy(2))
            {
                if (!rockGroup.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool GroupNeedsExpansion(this Pawn pawn)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            return mapComp.GroupNeedsExpansion(pawn);
        }

        public static bool GroupNeedsMoreFood(this Pawn pawn)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            return mapComp.GroupNeedsMoreFood(pawn);
        }

        public static HashSet<IntVec3> GetHiveCells(this Pawn pawn)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            return mapComp.GetHiveCellsFor(pawn);
        }

        public static HashSet<IntVec3> GetMinedOutHiveCells(this Pawn pawn)
        {
            var mapComp = pawn.Map.GetComponent<HiveAIManager>();
            return mapComp.GetMinedOutCellsFor(pawn);
        }

        public static bool IsNosalis(this Pawn pawn)
        {
            if (pawn.def == MetroDefOf.Metro_Nosalis)
            {
                return true;
            }
            return false;
        }

        public static bool IsSpider(this Pawn pawn)
        {
            if (pawn.def == MetroDefOf.Metro_Spider)
            {
                return true;
            }
            return false;
        }

        public static bool IsNightNow(Map map)
        {
            return GenLocalDate.HourInteger(map) >= 20 || GenLocalDate.HourInteger(map) <= 5;
        }

        public static bool IsDayOrClose(Map map)
        {
            return GenLocalDate.HourInteger(map) >= 4 && GenLocalDate.HourInteger(map) < 20;
        }
        public static bool HarmedRecently(this Pawn pawn)
        {
            return Find.TickManager.TicksGame - pawn.mindState.lastHarmTick < 2500;
        }
        public static bool UrgentlyNeedsToRunAwayFromSunLight(this Pawn pawn)
        {
            if (pawn.IsNosalis() || pawn.IsSpider())
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MetroDefOf.Metro_SunLightDamage);
                if (hediff?.severityInt >= 0.70)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool InSafeCondition(this Pawn pawn)
        {
            if (pawn.HarmedRecently()) return false;
            if (!pawn.Position.Roofed(pawn.Map))
            {
                if (!IsNightNow(pawn.Map)) return false;
                if (IsDayOrClose(pawn.Map)) return false;
                if (pawn.UrgentlyNeedsToRunAwayFromSunLight()) return false;
            }
            return true;
        }
    }
}


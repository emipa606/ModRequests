using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
    public class JobGiver_HiveDefense : JobGiver_AIFightEnemies
    {
        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            Hive hive = pawn.mindState.duty.focus.Thing as Hive;
            if (hive != null && hive.Spawned)
            {
                return hive.Position;
            }
            return pawn.Position;
        }
        protected override float GetFlagRadius(Pawn pawn)
        {
            return pawn.mindState.duty.radius;
        }
        protected override Job MeleeAttackJob(Thing enemyTarget)
        {
            Job job = base.MeleeAttackJob(enemyTarget);
            job.attackDoorIfTargetLost = true;
            return job;
        }
    }
    public class JobGiver_MineRandom : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null) return null;

            Hive hive = HiveUtility.GetHive(pawn);
            if (hive == null) return null;

            for (int i = 0; i < 40; i++)
            {
                IntVec3 randomCell = region.RandomCell;
                for (int j = 0; j < 4; j++)
                {
                    IntVec3 c = randomCell + GenAdj.CardinalDirections[j];
                    int dist = IntVec3Utility.ManhattanDistanceFlat(c, hive.Position);
                    if (dist > 3)
                    {
                        continue;
                    }
                    if (!c.InBounds(pawn.Map))
                    {
                        continue;
                    }
                    Building edifice = c.GetEdifice(pawn.Map);
                    if (edifice != null && (edifice.def.passability == Traversability.Impassable || edifice.def.IsDoor) && edifice.def.size == IntVec2.One && edifice.def != RimWorld.ThingDefOf.CollapsedRocks && pawn.CanReserve(edifice, 1, -1, null, false))
                    {
                        return new Job(RimWorld.JobDefOf.Mine, edifice)
                        {
                            ignoreDesignations = true
                        };
                    }
                }
            }
            return null;
        }
    }
    public class JobGiver_MaintainHives : JobGiver_AIFightEnemies
    {
        private bool onlyIfDamagingState;
        private static readonly float CellsInScanRadius = GenRadial.NumCellsInRadius(7.9f);

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_MaintainHives job = (JobGiver_MaintainHives)base.DeepCopy(resolve);
            job.onlyIfDamagingState = onlyIfDamagingState;
            return job;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            Room room = pawn.GetRoom();
            for (int i = 0; i < CellsInScanRadius; i++)
            {
                IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
                if (!intVec.InBounds(pawn.Map) || intVec.GetRoom(pawn.Map) != room)
                {
                    continue;
                }
                Hive hive = (Hive)pawn.Map.thingGrid.ThingAt(intVec, ThingDefOf.BI_Hive);
                if (hive != null && pawn.CanReserve(hive))
                {
                    CompMaintainable compMaintainable = hive.TryGetComp<CompMaintainable>();
                    if (compMaintainable.CurStage != 0 && (!onlyIfDamagingState || compMaintainable.CurStage == MaintainableStage.Damaging))
                    {
                        return JobMaker.MakeJob(JobDefOf.BI_Maintain, hive);
                    }
                }
            }
            return null;
        }
    }
    public class JobGiver_WanderHive : JobGiver_Wander
    {
        public JobGiver_WanderHive()
        {
            wanderRadius = 8f;
            ticksBetweenWandersRange = new IntRange(125, 200);
        }
        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            Hive hive = pawn.mindState.duty.focus.Thing as Hive;
            if (hive == null || !hive.Spawned)
            {
                return pawn.Position;
            }
            return hive.Position;
        }
    }
    public class JobGiver_GetRest : ThinkNode_JobGiver
    {
        private RestCategory minCategory;
        private float maxLevelPercentage = 1f;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetRest job = (JobGiver_GetRest)base.DeepCopy(resolve);
            job.minCategory = minCategory;
            job.maxLevelPercentage = maxLevelPercentage;
            return job;
        }
        public override float GetPriority(Pawn pawn)
        {
            Need_Rest rest = pawn.needs.rest;
            if (rest == null)
            {
                return 0f;
            }
            if ((int)rest.CurCategory < (int)minCategory)
            {
                return 0f;
            }
            if (rest.CurLevelPercentage > maxLevelPercentage)
            {
                return 0f;
            }
            if (Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
            {
                return 0f;
            }
            Lord lord = pawn.GetLord();
            if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
            {
                return 0f;
            }
            int num = GenLocalDate.HourOfDay(pawn);
            float curLevel = rest.CurLevel;
            if (num >= 5 && num <= 13)
            {
                if (curLevel < 0.3f)
                {
                    return 8f;
                }
            }
            return 0f;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Rest rest = pawn.needs.rest;
            if (rest == null || (int)rest.CurCategory < (int)minCategory || rest.CurLevelPercentage > maxLevelPercentage) return null;
            if (RestUtility.DisturbancePreventsLyingDown(pawn)) return null;

            return new Job(RimWorld.JobDefOf.LayDown, FindGroundSleepSpotFor(pawn));
        }
        private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
        {
            Map map = pawn.Map;
            Hive hive = HiveUtility.GetHive(pawn);
            if (hive != null)
            {
                if (pawn.CanReach(hive.Position, PathEndMode.OnCell, Danger.Deadly, true, true, TraverseMode.PassDoors))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int radius = (i == 0) ? 4 : 8;
                        if (CellFinder.TryRandomClosewalkCellNear(hive.Position, map, radius, out IntVec3 result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
                        {
                            return result;
                        }
                    }
                }
            }
            return CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4);
        }
    }
    public class JobGiver_FightFire : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn != null && pawn.Downed) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "BeatFire")) return null;

            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn2 = ((AttachableThing)t).parent as Pawn;
                return pawn2 == null && pawn.CanReserve(t);
            };
            if (HiveUtility.WithinHive(pawn, pawn as Thing, false))
            {
                Hive hive = HiveUtility.GetHive(pawn);
                if (hive != null)
                {
                    Thing thingOnFire = GenClosest.ClosestThingReachable(hive.Position, pawn.Map, ThingRequest.ForDef(RimWorld.ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn), 12f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                    if (thingOnFire != null && thingOnFire != pawn)
                    {
                        return new Job(RimWorld.JobDefOf.BeatFire, thingOnFire);
                    }
                }
            }
            return null;
        }
    }
    public class JobGiver_Butcher : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn != null && pawn.Downed) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "BI_Butcher")) return null;

            Thing thing = FindTarget(pawn);
            if (thing == null) return null;

            Job job = new Job(JobDefOf.BI_Butcher, thing);
            job.ignoreForbidden = true;
            job.count = 1;
            return job;
        }
        public static Thing FindTarget(Pawn pawn)
        {
            Thing result = null;
            Predicate<Thing> validator = delegate (Thing t)
            {
                Corpse c = t as Corpse;
                if (c != null && c.InnerPawn != null && c.InnerPawn.RaceProps.IsFlesh && c.GetRotStage() != RotStage.Dessicated && !c.IsBurning() && !c.Fogged())
                {
                    if (HiveUtility.WithinHive(pawn, c as Thing, false) && pawn.CanReserve(c))
                    {
                        return true;
                    }
                }
                return false;
            };
            result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);

            if (result != null) return result;

            validator = delegate (Thing t)
            {
                if (t != null && t.def.category == ThingCategory.Item && !t.def.IsCorpse && t.IngestibleNow && !t.IsBurning() && !t.Fogged())
                {
                    if (HiveUtility.WithinHive(pawn, t, false) && t.def.defName != RimWorld.ThingDefOf.InsectJelly.defName && pawn.CanReserve(t))
                    {
                        return true;
                    }
                }
                return false;
            };
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
        }
    }
    public class JobGiver_Gather : ThinkNode_JobGiver
    {
        public static Job ForceJob(Pawn pawn)
        {
            if (pawn != null && pawn.Downed) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "HaulToCell")) return null;

            Hive hive = HiveUtility.GetHive(pawn);
            if (hive == null) return null;

            IntVec3 pos = CellFinder.RandomClosewalkCellNear(hive.Position, pawn.Map, 5);
            if (pos == IntVec3.Invalid || !pawn.CanReserve(pos))
            {
                return null;
            }

            Thing target = FindTarget(pawn);
            if (target == null) return null;

            if (!GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map, true)) return null;

            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.HaulToCell, target, pos);
            job.canBashDoors = true;
            job.canBashFences = true;
            job.attackDoorIfTargetLost = true;
            job.count = 12;
            return job;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            return ForceJob(pawn);
        }
        public static Thing FindTarget(Pawn pawn)
        {
            Thing result = null;
            Predicate<Thing> validator = delegate (Thing t)
            {
                Corpse c = t as Corpse;
                if (c != null && c.InnerPawn != null && c.InnerPawn.RaceProps.IsFlesh && c.GetRotStage() != RotStage.Dessicated && !c.IsBurning() && !c.Fogged())
                {
                    if (!HiveUtility.WithinHive(pawn, c, true))
                    {
                        if ((c.InnerPawn.Faction == null || (c.InnerPawn.Faction != null && c.InnerPawn.Faction != pawn.Faction && c.InnerPawn.Faction.def.defName != "VFEI_Insect")) && pawn.CanReserve(c))
                        {
                            return true;
                        }
                    }
                }
                return false;
            };
            result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
            if (result != null) return result;

            validator = delegate (Thing t)
            {
                if (t != null && t.def.category == ThingCategory.Item && !t.def.IsCorpse && t.IngestibleNow && !t.IsBurning() && !t.Fogged())
                {
                    if (!HiveUtility.WithinHive(pawn, t, true) && pawn.CanReserve(t))
                    {
                        return true;
                    }
                }
                return false;
            };
            result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
            if (result != null) return result;

            validator = delegate (Thing t)
            {
                Pawn p = t as Pawn;
                if (p != null && p.Downed && p.RaceProps.IsFlesh && !p.RaceProps.DeathActionWorker.DangerousInMelee && !p.IsBurning() && !p.Fogged())
                {
                    if (!HiveUtility.WithinHive(pawn, p, true) && pawn.CanReserve(p))
                    {
                        return true;
                    }
                }
                return false;
            };
            return result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
        }
    }
    public class JobGiver_Patrol : ThinkNode_JobGiver
    {
        private LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;
        private Danger maxDanger = Danger.Deadly;
        private int jobMaxDuration = 9999;
        private IntRange WaitTicks = new IntRange(30, 80);

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Patrol job = (JobGiver_Patrol)base.DeepCopy(resolve);
            job.locomotionUrgency = locomotionUrgency;
            job.maxDanger = maxDanger;
            job.jobMaxDuration = jobMaxDuration;
            return job;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn != null && pawn.Downed) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "BI_GotoPatrol")) return null;

            Hive hive = HiveUtility.GetHive(pawn);
            if (hive == null) return null;

            IntVec3 cell = HiveUtility.GetPatrolSpot(pawn, hive, out LocomotionUrgency locomotionUrgency);
            if (!cell.IsValid)
            {
                IntVec3 pos = HiveUtility.FindPathToPrey(pawn);
                if (pos != IntVec3.Invalid)
                {
                    cell = pos;
                    HiveUtility.SetPatrolSpot(pawn, hive, cell, locomotionUrgency);
                }
            }
            if (!HiveUtility.JobsGivenRecentTick(pawn, "Mine"))
            {
                if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly, true, true, TraverseMode.PassDoors))
                {
                    return JobGiver_Sapper.ForceJob(pawn, cell);
                }
            }

            Thing target = null;
            if (!HiveUtility.JobsGivenRecentTick(pawn, "AttackMelee"))
            {
                target = JobGiver_Hunt.FindTarget(pawn);
                if (target != null)
                {
                    return JobGiver_Hunt.ForceJob(pawn);
                }
            }
            if (!HiveUtility.JobsGivenRecentTick(pawn, "BI_HaulToCell"))
            {
                target = JobGiver_Gather.FindTarget(pawn);
                if (target != null)
                {
                    return JobGiver_Gather.ForceJob(pawn);
                }
            }

            pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
            if (pawn.mindState.nextMoveOrderIsWait)
            {
                Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Wait_Wander);
                job.expiryInterval = WaitTicks.RandomInRange;
                return job;
            }

            Predicate<IntVec3> validator = delegate (IntVec3 c)
            {
                if (!c.ContainsStaticFire(pawn.Map))
                {
                        return true;
                }
                return false;
            };
            IntVec3 gotoCell = CellFinder.RandomClosewalkCellNear(cell, pawn.Map, 3, validator);
            Job job2 = JobMaker.MakeJob(JobDefOf.BI_GotoPatrol, gotoCell);
            job2.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
            job2.expiryInterval = jobMaxDuration;
            job2.canBashDoors = true;
            job2.canBashFences = true;
            job2.attackDoorIfTargetLost = true;
            return job2;
        }
    }
    public class JobGiver_Hunt : ThinkNode_JobGiver
    {
        public static Job ForceJob(Pawn pawn)
        {
            if (pawn != null && pawn.Downed) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "AttackMelee")) return null;

            Hive hive = HiveUtility.GetHive(pawn);
            if (hive == null) return null;

            Thing target = HiveUtility.GetAttackTarget(pawn, hive);
            Pawn p = target as Pawn;
            if (target.DestroyedOrNull() || HiveUtility.AttackTargetTooFarAway(pawn, target) || (p != null && p.Downed))
            {
                target = FindTarget(pawn);
                HiveUtility.SetAttackTarget(pawn, hive, target);
            }
            if (target.DestroyedOrNull()) return null;
            if (!GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map, true)) return null;

            if (!HiveUtility.JobsGivenRecentTick(pawn, "BI_ChewConduits") && target.def == RimWorld.ThingDefOf.PowerConduit)
            {
                return new Job(JobDefOf.BI_ChewConduits, target);
            }

            Job job = new Job(RimWorld.JobDefOf.AttackMelee, target);
            job.canBashDoors = true;
            job.canBashFences = true;
            job.attackDoorIfTargetLost = true;
            return job;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            return ForceJob(pawn);
        }
        public static Thing FindTarget(Pawn pawn)
        {
            Thing result = null;
            Predicate<Thing> validator = delegate (Thing t)
            {
                Building_Turret b = t as Building_Turret;
                if (!b.DestroyedOrNull() && !b.IsBurning() && b.LastAttackedTarget != null && b.LastAttackedTarget.Pawn != null && b.LastAttackedTarget.Pawn.Faction != null && b.LastAttackedTarget.Pawn.Faction == pawn.Faction && !b.Fogged())
                {
                    return true;
                }
                return false;
            };
            result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
            if (result != null) return result;

            validator = delegate (Thing t)
            {
                Building_Door b = t as Building_Door;
                if (!b.DestroyedOrNull() && !b.IsBurning() && !b.Fogged())
                {
                    return true;
                }
                return false;
            };
            result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
            if (result != null) return result;

            validator = delegate (Thing t)
            {
                Pawn p = t as Pawn;
                if (!p.DestroyedOrNull() && !p.IsBurning() && ((p.Faction != null && p.Faction != pawn.Faction && p.Faction.def.defName != "VFEI_Insect") || p.Faction == null) && !p.Downed && pawn.CanReserve(p) && !p.Fogged())
                {
                    return true;
                }
                return false;
            };
            result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
            if (result != null) return result;

            if (pawn.mindState != null && pawn.mindState.duty.def == DutyDefOf.BI_HiveHunters)
            {
                List<Thing> targetList = new List<Thing>();
                foreach (Thing t in pawn.Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.PowerConduit))
                {
                    Building b = t as Building;
                    if (!b.DestroyedOrNull() && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, b.Position) <= 8)
                    {
                        if (!b.IsBurning() && b.def == RimWorld.ThingDefOf.PowerConduit && b.PowerComp != null && b.PowerComp.PowerNet != null && b.PowerComp.PowerNet.batteryComps != null)
                        {
                            if (b.PowerComp.PowerNet.HasActivePowerSource && b.PowerComp.PowerNet.batteryComps.Count > 0 && PowerSourceNearby(b) && !b.Fogged())
                            {
                                if (pawn.CanReserve(t) && pawn.CanReach(pawn, PathEndMode.OnCell, Danger.None, true, true, TraverseMode.NoPassClosedDoors))
                                {
                                    targetList.Add(t);
                                }
                            }
                        }
                    }
                }
                if (!targetList.NullOrEmpty())
                {
                    if (Rand.Range(1, 100) <= 33) return targetList.RandomElement();
                }
            }
            return null;
        }
        public static bool PowerSourceNearby(Building b)
        {
            if (b != null && b.PowerComp != null && b.PowerComp.PowerNet != null)
            {
                List<CompPowerTrader> compList = b.PowerComp.PowerNet.powerComps;
                if (!compList.NullOrEmpty())
                {
                    foreach (CompPowerTrader current in compList)
                    {
                        Building powerSource = current.parent as Building;
                        if (powerSource != null)
                        {
                            if (IntVec3Utility.ManhattanDistanceFlat(powerSource.Position, b.Position) <= 12) return true;
                        }
                    }
                }
            }
            return false;
        }
    }
    public class JobGiver_Sapper : ThinkNode_JobGiver
    {
        public static Job ForceJob(Pawn pawn, IntVec3 c)
        {
            if (pawn != null && pawn.Downed) return null;
            if (!c.IsValid) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "Mine")) return null;

            Hive hive = HiveUtility.GetHive(pawn);
            if (hive == null) return null;

            if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings))
            {
                HiveUtility.SetPatrolSpot(pawn, hive, hive.Position, LocomotionUrgency.Walk);
                return null;
            }

            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, c, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
            {
                List<IntVec3> cells = pawnPath.NodesReversed;
                if (!cells.NullOrEmpty())
                {
                    foreach (IntVec3 cell in cells)
                    {
                        Building b = cell.GetEdifice(pawn.Map);
                        if (b != null && b.def != null)
                        {
                            if (b.def.passability != Traversability.Impassable) return null;
                            if (b.def.size != IntVec2.One) return null;
                            if (b.Fogged()) return null;
                        }
                    }
                }
                IntVec3 cellBeforeBlocker;
                Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                if (thing != null && pawn.CanReserve(thing) && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly, true, true, TraverseMode.PassDoors))
                {
                    Job job = new Job(RimWorld.JobDefOf.Mine, thing);
                    job.canBashDoors = true;
                    job.canBashFences = true;
                    job.ignoreDesignations = true;
                    return job;
                }
            }
            return null;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            return null;
        }
    }
    public class JobGiver_SpawnHive : ThinkNode_JobGiver
    {
        private LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;
        private Danger maxDanger = Danger.Some;
        private int jobMaxDuration = 999999;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_SpawnHive job = (JobGiver_SpawnHive)base.DeepCopy(resolve);
            job.locomotionUrgency = locomotionUrgency;
            job.maxDanger = maxDanger;
            job.jobMaxDuration = jobMaxDuration;
            return job;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null) return null;
            if (BetterInfestationsMod.settings == null) return null;
            if (HiveUtility.TotalSpawnedHivesCount(pawn.Map) >= BetterInfestationsMod.settings.maxHivesPerMap) return null;
            if (pawn.Downed) return null;
            if (HiveUtility.JobsGivenRecentTick(pawn, "BI_GotoSpawnHive")) return null;

            Queen queen = pawn as Queen;
            if (queen == null) return null;

            if (Find.TickManager.TicksGame < queen.nextHiveSpawnTick) return null;

            IntVec3 cell = IntVec3.Invalid;
            Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out cell, pawn.Map);
            if (!cell.IsValid) return null;

            cell = CellFinder.RandomClosewalkCellNear(cell, pawn.Map, 3);

            if (!HiveUtility.JobsGivenRecentTick(pawn, "Mine"))
            {
                if (!pawn.CanReach(cell, PathEndMode.Touch, Danger.Deadly, true, true, TraverseMode.PassDoors))
                {
                    return JobGiver_Sapper.ForceJob(pawn, cell);
                }
            }

            Job job = JobMaker.MakeJob(JobDefOf.BI_GotoSpawnHive, cell);
            job.canBashDoors = true;
            job.canBashFences = true;
            job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
            job.expiryInterval = jobMaxDuration;
            return job;
        }
    }
    public class JobGiver_GetFood : ThinkNode_JobGiver
    {
        private HungerCategory minCategory;
        private float maxLevelPercentage = 1f;
        public bool forceScanWholeMap;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetFood obj = (JobGiver_GetFood)base.DeepCopy(resolve);
            obj.minCategory = minCategory;
            obj.maxLevelPercentage = maxLevelPercentage;
            obj.forceScanWholeMap = forceScanWholeMap;
            return obj;
        }

        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null) return 0f;
            if ((int)pawn.needs.food.CurCategory < 3 && FoodUtility.ShouldBeFedBySomeone(pawn)) return 0f;
            if ((int)food.CurCategory < (int)minCategory) return 0f;
            if (food.CurLevelPercentage > maxLevelPercentage) return 0f;
            if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat) return 9.5f;
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null || (int)food.CurCategory < (int)minCategory || food.CurLevelPercentage > maxLevelPercentage) return null;

            Thing foodHive = HiveUtility.GetFoodAtHive(pawn);
            if (foodHive != null)
            {
                return ForceJob(pawn, foodHive, foodHive.def);
            }
            bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out Thing foodSource, out ThingDef foodDef, canRefillDispenser: true, canUseInventory: true, canUsePackAnimalInventory: false, allowForbidden: false, true, allowSociallyImproper: false, false, forceScanWholeMap))
            {
                return null;
            }
            Pawn pawn2 = foodSource as Pawn;
            if (pawn2 != null)
            {
                Job job = JobMaker.MakeJob(RimWorld.JobDefOf.PredatorHunt, pawn2);
                job.killIncappedTarget = true;
                return job;
            }
            return ForceJob(pawn, foodSource, foodDef);
        }
        public static Job ForceJob(Pawn pawn, Thing foodSource, ThingDef foodDef)
        {
            if (pawn == null || foodSource == null || foodDef == null) return null;
            float nutrition = FoodUtility.GetNutrition(pawn, foodSource, foodDef);
            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Ingest, foodSource);
            job.count = FoodUtility.WillIngestStackCountOf(pawn, foodDef, nutrition);
            return job;
        }
    }
}
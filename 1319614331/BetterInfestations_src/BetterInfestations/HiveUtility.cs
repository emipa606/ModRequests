using System;
using System.Reflection;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BetterInfestations
{
    public static class HiveUtility
    {
        public static int TotalSpawnedHivesCount(Map map)
        {
            return map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive).Count;
        }
        public static bool AnyHivePreventsClaiming(Thing thing)
        {
            if (!thing.Spawned) return false;

            int num = GenRadial.NumCellsInRadius(2f);
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = thing.Position + GenRadial.RadialPattern[i];
                if (c.InBounds(thing.Map) && c.GetFirstThing(thing.Map, thing.def) != null) return true;
            }
            return false;
        }
        public static void NotifyAttackedNearbyHives(Hive hive, Lord lord, string memo)
        {
            if (hive != null && hive.Spawned)
            {
                if (lord != null)
                {
                    lord.ReceiveMemo(memo);
                }
                foreach (Thing thing in hive.Map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
                {
                    Hive otherHive = thing as Hive;
                    if (otherHive != null && hive != otherHive)
                    {
                        if (IntVec3Utility.ManhattanDistanceFlat(hive.Position, otherHive.Position) <= 24)
                        {
                            if (otherHive.CompSpawnerPawns != null)
                            {
                                Lord lord2 = otherHive.CompSpawnerPawns.Lord[0];
                                if (lord2 != null)
                                {
                                    lord.ReceiveMemo(memo);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static Hive GetHive(Pawn pawn)
        {
            if (!pawn.DestroyedOrNull() && pawn.Spawned)
            {
                foreach (Thing thing in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
                {
                    Hive hive = thing as Hive;
                    if (hive != null)
                    {
                        if (PawnFromHive(hive, pawn))
                        {
                            return hive;
                        }
                    }
                }
            }
            return null;
        }
        public static Hive GetLordHive(Lord lord)
        {
            if (lord == null) return null;

            foreach (Thing thing in lord.Map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
            {
                Hive hive = thing as Hive;
                if (hive != null)
                {
                    CompSpawnerPawns comp = hive.CompSpawnerPawns;
                    if (comp == null) return null;

                    for (int i = 0; i < 4; i++)
                    {
                        if (comp.Lord[i] == lord)
                        {
                            return hive;
                        }
                    }
                }
            }
            return null;
        }
        public static bool PawnFromHive(Hive hive, Pawn pawn)
        {
            if (!hive.DestroyedOrNull() && !pawn.DestroyedOrNull())
            {
                foreach (Pawn p in AllHivePawns(hive))
                {
                    if (p == pawn)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool WithinHive(Pawn pawn, Thing thing, bool NearOtherHive)
        {
            if (pawn == null || thing == null) return false;

            foreach (Thing t in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
            {
                Hive hive = t as Hive;
                if (hive != null)
                {
                    if (PawnFromHive(hive, pawn) || NearOtherHive)
                    {
                        int dist = IntVec3Utility.ManhattanDistanceFlat(hive.Position, thing.Position);
                        if (dist <= 8) return true;
                    }
                }
            }
            return false;
        }
        public static List<Pawn> AllHivePawns(Hive hive)
        {
            if (hive != null)
            {
                List<Pawn> pawns = new List<Pawn>();
                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    pawns.AddRange(hive.CompSpawnerPawns.spawnedPawns[i]);
                }
                return pawns;
            }
            return null;
        }
        public static void CallReinforcements(Pawn pawn, Thing thing)
        {
            foreach (Pawn p in pawn.Map.mapPawns.PawnsInFaction(Faction.OfInsects))
            {
                if (!p.Downed && p.CurJob != null && p.CurJob.def != RimWorld.JobDefOf.AttackMelee && !JobsGivenRecentTick(p, "AttackMelee"))
                {
                    if (IntVec3Utility.ManhattanDistanceFlat(p.Position, pawn.Position) <= 8)
                    {
                        Job job = new Job(RimWorld.JobDefOf.AttackMelee, thing);
                        job.canBashDoors = true;
                        job.canBashFences = true;
                        job.attackDoorIfTargetLost = true;
                        p.jobs.StartJob(job, JobCondition.InterruptForced);
                        Hive hive = GetHive(p);
                        if (hive != null)
                        {
                            SetPatrolSpot(p, hive, thing.Position, LocomotionUrgency.Sprint);
                        }
                    }
                }
            }
        }
        public static IntVec3 GetPatrolSpot(Pawn pawn, Hive hive, out LocomotionUrgency locomotionUrgency)
        {
            if (pawn != null && hive != null)
            {
                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[i])
                    {
                        if (p == pawn)
                        {
                            locomotionUrgency = hive.CompSpawnerPawns.patrolLocomotion[i];
                            return hive.CompSpawnerPawns.patrolLoc[i];
                        }
                    }
                }
            }
            locomotionUrgency = LocomotionUrgency.Walk;
            return IntVec3.Invalid;
        }
        public static void SetPatrolSpot(Pawn pawn, Hive hive, IntVec3 cell, LocomotionUrgency locomotionUrgency)
        {
            if (pawn != null && hive != null)
            {
                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[i])
                    {
                        if (p == pawn)
                        {
                            hive.CompSpawnerPawns.patrolLoc[i] = cell;
                            hive.CompSpawnerPawns.patrolLocomotion[i] = locomotionUrgency;
                            return;
                        }
                    }
                }
            }
        }
        public static Thing GetAttackTarget(Pawn pawn, Hive hive)
        {
            if (pawn != null && hive != null)
            {
                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[i])
                    {
                        if (p == pawn) return hive.CompSpawnerPawns.attackTarget[i];
                    }
                }
            }
            return null;
        }
        public static void SetAttackTarget(Pawn pawn, Hive hive, Thing thing)
        {
            if (pawn != null && hive != null)
            {
                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[i])
                    {
                        if (p == pawn)
                        {
                            hive.CompSpawnerPawns.attackTarget[i] = thing;
                            return;
                        }
                    }
                }
            }
        }
        public static float GetGroupStrength(Pawn pawn, Hive hive)
        {
            if (pawn != null && hive != null)
            {
                float points = 0;
                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[i])
                    {
                        if (p.jobs.posture != PawnPosture.LayingOnGroundNormal && p.mindState != null && p.mindState.duty != null)
                        {
                            points += p.kindDef.combatPower;
                        }
                    }
                }
                return points;
            }
            return 0f;
        }
        public static IntVec3 GetColonyStockpileSpot(Map map)
        {
            List<Zone> zones = map.zoneManager.AllZones;
            if (zones != null && zones.Any())
            {
                foreach (Zone zone in zones)
                {
                    Zone_Stockpile stockpile = zone as Zone_Stockpile;
                    if (stockpile != null)
                    {
                        List<IntVec3> cells = zone.cells as List<IntVec3>;
                        if (cells == null || !cells.Any())
                        {
                            continue;
                        }
                        int foodAmount = 0;
                        foreach (IntVec3 cell in cells)
                        {
                            Thing thing = map.thingGrid.ThingAt(cell, ThingCategory.Item);
                            if (thing != null && !thing.def.IsCorpse && thing.IngestibleNow && !thing.IsBurning())
                            {
                                foodAmount += thing.stackCount;
                                if (foodAmount >= 300)
                                {
                                    return cells.RandomElement();
                                }
                            }
                        }
                    }
                }
            }
            return IntVec3.Invalid;
        }
        public static IntVec3 FindPathToPrey(Pawn pawn)
        {
            if (pawn != null && pawn.Downed) return IntVec3.Invalid;
            List<Thing> targetList = new List<Thing>();

            foreach (Thing t in pawn.Map.listerThings.AllThings)
            {
                if (t != null && t.def.category == ThingCategory.Item && !t.def.IsCorpse && t.IngestibleNow && !t.IsBurning() && !t.Fogged())
                {
                    targetList.Add(t);
                }
                Pawn p = t as Pawn;
                if (p != null && (p.Faction == null || (p.Faction != null && p.Faction != pawn.Faction && p.Faction.def.defName != "VFEI_Insect")) && !p.IsBurning() && !p.Fogged())
                {
                    targetList.Add(t);
                }
                Corpse c = t as Corpse;
                if (c != null && c.InnerPawn != null && c.InnerPawn.RaceProps.IsFlesh && c.GetRotStage() != RotStage.Dessicated && !c.IsBurning() && !c.Fogged())
                {
                    if (c.InnerPawn.Faction == null || (c.InnerPawn.Faction != null && c.InnerPawn.Faction != pawn.Faction && c.InnerPawn.Faction.def.defName != "VFEI_Insect"))
                    {
                        targetList.Add(t);
                    }
                }
            }

            if (targetList.NullOrEmpty()) return IntVec3.Invalid;

            Thing result = null;
            Predicate<Thing> validator = delegate (Thing t)
            {
                if (!WithinHive(pawn, t, true) && pawn.CanReserve(t))
                {
                    return true;
                }
                return false;
            };
            result = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, targetList, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassAllDestroyableThings, Danger.Deadly, false), 9999, validator);
            if (result == null) return IntVec3.Invalid;

            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, result.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), PathEndMode.OnCell))
            {
                List<IntVec3> cells = pawnPath.NodesReversed;
                if (!cells.NullOrEmpty())
                {
                    foreach (IntVec3 cell in cells)
                    {
                        if (IntVec3Utility.ManhattanDistanceFlat(pawn.Position, cell) <= 24)
                        {
                            return cell;
                        }
                    }
                }
            }

            return IntVec3.Invalid;
        }
        public static bool JobsGivenRecentTick(Pawn pawn, string JobName)
        {
            FieldInfo FI_jobsGivenRecentTicksTextual = typeof(Pawn_JobTracker).GetField("jobsGivenRecentTicksTextual", Patches.allFlags);
            List<string> jobs = (List<string>)FI_jobsGivenRecentTicksTextual.GetValue(pawn.jobs);
            if (!jobs.NullOrEmpty())
            {
                foreach (string job in jobs)
                {
                    if (job.Contains(JobName)) return true;
                }
            }
            FieldInfo FI_jobsGivenThisTickTextual = typeof(Pawn_JobTracker).GetField("jobsGivenThisTickTextual", Patches.allFlags);
            string job2 = (string)FI_jobsGivenThisTickTextual.GetValue(pawn.jobs);
            if (!job2.NullOrEmpty())
            {
                if (job2.Contains(JobName)) return true;
            }
            return false;
        }
        public static bool AttackTargetTooFarAway(Pawn pawn, Thing thing)
        {
            if (pawn != null && thing != null)
            {
                Hive hive = GetHive(pawn);
                if (hive == null) return false;

                for (int i = 0; i < hive.CompSpawnerPawns.spawnedPawns.Count; i++)
                {
                    foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[i])
                    {
                        if (p == pawn)
                        {
                            bool closeToTarget = false;
                            foreach (Pawn pawn2 in hive.CompSpawnerPawns.spawnedPawns[i])
                            {
                                if (IntVec3Utility.ManhattanDistanceFlat(pawn2.Position, thing.Position) <= 12) closeToTarget = true;
                            }
                            if (!closeToTarget) return true;
                        }
                    }
                }
            }
            return false;
        }
        public static Thing GetFoodAtHive(Pawn pawn)
        {
            Hive hive = GetHive(pawn);
            if (hive == null) return null;

            foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(hive.Position, hive.Rotation, new IntVec2(8, 8)))
            {
                IEnumerable<Thing> things = hive.Map.thingGrid.ThingsAt(cell);
                foreach (Thing thing in things)
                {
                    if (thing.def == RimWorld.ThingDefOf.InsectJelly && !thing.IsBurning() && !thing.Fogged() && pawn.CanReserve(thing, 1, 75) && pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors)) return thing;
                }
            }
            return null;
        }
        public static bool QueenActive(Hive hive)
        {
            if (hive == null) return false;
            foreach (Pawn p in hive.CompSpawnerPawns.spawnedPawns[0])
            {
                Queen q = p as Queen;
                if (q != null && q.Spawned) return true;
            }
            return false;
        }
        public static void SpawnRandomCorpses(Hive hive)
        {
            if (hive == null) return;

            List<Faction> factions = Find.FactionManager.AllFactions as List<Faction>;
            Faction pirateFaction = null;
            Faction mechanoidFaction = null;
            Faction tribeSavageFaction = null;
            Faction tribeRoughFaction = null;
            Faction tribeCivilFaction = null;
            Faction outlanderRoughFaction = null;
            Faction outlanderCivilFaction = null;
            foreach (Faction faction in factions)
            {
                if (faction.def.defName == "Pirate")
                    pirateFaction = faction;
                else if (faction.def.defName == "Mechanoid")
                    mechanoidFaction = faction;
                else if (faction.def.defName == "TribeSavage")
                    tribeSavageFaction = faction;
                else if (faction.def.defName == "TribeRough")
                    tribeRoughFaction = faction;
                else if (faction.def.defName == "TribeCivil")
                    tribeCivilFaction = faction;
                else if (faction.def.defName == "OutlanderRough")
                    outlanderRoughFaction = faction;
                else if (faction.def.defName == "OutlanderCivil")
                    outlanderCivilFaction = faction;
            }
            List<PawnKindDef> allPawnKinds = DefDatabase<PawnKindDef>.AllDefsListForReading;
            PawnKindDef scavenger = null;
            PawnKindDef thrasher = null;
            PawnKindDef pirate = null;
            PawnKindDef pirateBoss = null;
            PawnKindDef grenadier_destructive = null;
            PawnKindDef grenadier_emp = null;
            PawnKindDef grenadier_smoke = null;
            PawnKindDef mercenary_gunner = null;
            PawnKindDef mercenary_sniper = null;
            PawnKindDef mercenary_sniper_acidifier = null;
            PawnKindDef mercenary_slasher = null;
            PawnKindDef mercenary_slasher_acidifier = null;
            PawnKindDef mercenary_heavy = null;
            PawnKindDef mercenary_elite = null;
            PawnKindDef mercenary_elite_acidifier = null;
            PawnKindDef tribal_penitent = null;
            PawnKindDef tribal_archer = null;
            PawnKindDef tribal_warrior = null;
            PawnKindDef tribal_hunter = null;
            PawnKindDef tribal_trader = null;
            PawnKindDef tribal_berserker = null;
            PawnKindDef tribal_heavyarcher = null;
            PawnKindDef tribal_chiefMelee = null;
            PawnKindDef tribal_chiefRanged = null;
            PawnKindDef villager = null;
            PawnKindDef town_guard = null;
            PawnKindDef town_trader = null;
            PawnKindDef town_councilman = null;
            PawnKindDef mech_centipede = null;
            PawnKindDef mech_lancer = null;
            PawnKindDef mech_scyther = null;
            PawnKindDef mech_pikeman = null;

            List<PawnKindDef> piratePawnKinds = new List<PawnKindDef>();
            List<PawnKindDef> mercenaryPawnKinds = new List<PawnKindDef>();
            List<PawnKindDef> tribeCivilPawnKinds = new List<PawnKindDef>();
            List<PawnKindDef> tribeRoughPawnKinds = new List<PawnKindDef>();
            List<PawnKindDef> tribeSavagePawnKinds = new List<PawnKindDef>();
            List<PawnKindDef> outlanderPawnKinds = new List<PawnKindDef>();
            List<PawnKindDef> mechanoidPawnKinds = new List<PawnKindDef>();

            foreach (PawnKindDef pawnKindDef in allPawnKinds)
            {
                if (pawnKindDef.defName == "Scavenger")
                {
                    scavenger = pawnKindDef;
                    piratePawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Thrasher")
                {
                    thrasher = pawnKindDef;
                    piratePawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Pirate")
                {
                    pirate = pawnKindDef;
                    piratePawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "PirateBoss")
                {
                    pirateBoss = pawnKindDef;
                }
                else if (pawnKindDef.defName == "Grenadier_Destructive")
                {
                    grenadier_destructive = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Grenadier_EMP")
                {
                    grenadier_emp = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Grenadier_Smoke")
                {
                    grenadier_smoke = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Gunner")
                {
                    mercenary_gunner = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Sniper")
                {
                    mercenary_sniper = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Sniper_Acidifier")
                {
                    mercenary_sniper_acidifier = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Slasher")
                {
                    mercenary_slasher = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Slasher_Acidifier")
                {
                    mercenary_slasher_acidifier = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Heavy")
                {
                    mercenary_heavy = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Elite")
                {
                    mercenary_elite = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mercenary_Elite_Acidifier")
                {
                    mercenary_elite_acidifier = pawnKindDef;
                    mercenaryPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_Penitent")
                {
                    tribal_penitent = pawnKindDef;
                    tribeRoughPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_Archer")
                {
                    tribal_archer = pawnKindDef;
                    tribeRoughPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_Warrior")
                {
                    tribal_warrior = pawnKindDef;
                    tribeRoughPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_Hunter")
                {
                    tribal_hunter = pawnKindDef;
                    tribeCivilPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_Trader")
                {
                    tribal_trader = pawnKindDef;
                    tribeCivilPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_Berserker")
                {
                    tribal_berserker = pawnKindDef;
                    tribeSavagePawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_HeavyArcher")
                {
                    tribal_heavyarcher = pawnKindDef;
                    tribeSavagePawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Tribal_ChiefMelee")
                {
                    tribal_chiefMelee = pawnKindDef;
                }
                else if (pawnKindDef.defName == "Tribal_ChiefRanged")
                {
                    tribal_chiefRanged = pawnKindDef;
                }
                else if (pawnKindDef.defName == "Villager")
                {
                    villager = pawnKindDef;
                    outlanderPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Town_Guard")
                {
                    town_guard = pawnKindDef;
                    outlanderPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Town_Trader")
                {
                    town_trader = pawnKindDef;
                    outlanderPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Town_Councilman")
                {
                    town_councilman = pawnKindDef;
                    outlanderPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mech_Centipede")
                {
                    mech_centipede = pawnKindDef;
                    mechanoidPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mech_Lancer")
                {
                    mech_lancer = pawnKindDef;
                    mechanoidPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mech_Scyther")
                {
                    mech_scyther = pawnKindDef;
                    mechanoidPawnKinds.Add(pawnKindDef);
                }
                else if (pawnKindDef.defName == "Mech_Pikeman")
                {
                    mech_pikeman = pawnKindDef;
                    mechanoidPawnKinds.Add(pawnKindDef);
                }
            }

            List<Faction> factionsList = new List<Faction> { null, null, Faction.OfAncients, Faction.OfAncientsHostile, mechanoidFaction, pirateFaction, pirateFaction, tribeRoughFaction, tribeSavageFaction, tribeCivilFaction, outlanderRoughFaction, outlanderCivilFaction };
            Faction faction1 = factionsList.RandomElement();
            factionsList.RemoveAll(x => x == faction1);
            Faction faction2 = factionsList.RandomElement();

            if (Rand.Range(1, 100) <= 15 && (faction1 == Faction.OfAncients || faction2 == Faction.OfAncients))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.AncientSoldier, Faction.OfAncients, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.AncientSoldier, Faction.OfAncients, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.AncientSoldier, Faction.OfAncients, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 15 && (faction1 == Faction.OfAncientsHostile || faction2 == Faction.OfAncientsHostile))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 30 && (faction1 == mechanoidFaction || faction2 == mechanoidFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(mechanoidPawnKinds.RandomElement(), mechanoidFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(mechanoidPawnKinds.RandomElement(), mechanoidFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(mechanoidPawnKinds.RandomElement(), mechanoidFaction, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 40 && (faction1 == pirateFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(piratePawnKinds.RandomElement(), pirateFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(piratePawnKinds.RandomElement(), pirateFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(piratePawnKinds.RandomElement(), pirateFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 15) SpawnCorpsesNearHive(pirateBoss, pirateFaction, 1, hive);
            }
            if (Rand.Range(1, 100) <= 40 && (faction2 == pirateFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(mercenaryPawnKinds.RandomElement(), pirateFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(mercenaryPawnKinds.RandomElement(), pirateFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(mercenaryPawnKinds.RandomElement(), pirateFaction, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 25 && (faction1 == tribeRoughFaction || faction2 == tribeRoughFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeRoughPawnKinds.RandomElement(), tribeRoughFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeRoughPawnKinds.RandomElement(), tribeRoughFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeRoughPawnKinds.RandomElement(), tribeRoughFaction, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 25 && (faction1 == tribeSavageFaction || faction2 == tribeSavageFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeSavagePawnKinds.RandomElement(), tribeSavageFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeSavagePawnKinds.RandomElement(), tribeSavageFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeSavagePawnKinds.RandomElement(), tribeSavageFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 15) SpawnCorpsesNearHive(tribal_chiefMelee, tribeSavageFaction, 1, hive);
                if (Rand.Range(1, 100) <= 15) SpawnCorpsesNearHive(tribal_chiefRanged, tribeSavageFaction, 1, hive);
            }
            if (Rand.Range(1, 100) <= 25 && (faction1 == tribeCivilFaction || faction2 == tribeCivilFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeCivilPawnKinds.RandomElement(), tribeCivilFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeCivilPawnKinds.RandomElement(), tribeCivilFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(tribeCivilPawnKinds.RandomElement(), tribeCivilFaction, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 25 && (faction1 == outlanderRoughFaction || faction2 == outlanderRoughFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(outlanderPawnKinds.RandomElement(), outlanderRoughFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(outlanderPawnKinds.RandomElement(), outlanderRoughFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(outlanderPawnKinds.RandomElement(), outlanderRoughFaction, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 25 && (faction1 == outlanderCivilFaction || faction2 == outlanderCivilFaction))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(outlanderPawnKinds.RandomElement(), outlanderCivilFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(outlanderPawnKinds.RandomElement(), outlanderCivilFaction, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(outlanderPawnKinds.RandomElement(), outlanderCivilFaction, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 30 && (faction1 == null))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.Drifter, null, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.Drifter, null, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.Drifter, null, Rand.Range(1, 6), hive);
            }
            if (Rand.Range(1, 100) <= 30 && (faction2 == null))
            {
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.SpaceRefugee, null, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.SpaceRefugee, null, Rand.Range(1, 6), hive);
                if (Rand.Range(1, 100) <= 50) SpawnCorpsesNearHive(RimWorld.PawnKindDefOf.SpaceRefugee, null, Rand.Range(1, 6), hive);
            }
        }
        public static void SpawnRandomItems(Hive hive, List<ThingDef> thingDefs)
        {
            if (hive == null) return;

            ThingDef componentIndustrial = null;
            ThingDef componentSpacer = null;
            ThingDef gun_MachinePistol = null;
            ThingDef gun_IncendiaryLauncher = null;
            ThingDef gun_SmokeLauncher = null;
            ThingDef gun_EMPLauncher = null;
            ThingDef gun_BoltActionRifle = null;
            ThingDef gun_PumpShotgun = null;
            ThingDef gun_ChainShotgun = null;
            ThingDef gun_HeavySMG = null;
            ThingDef gun_LMG = null;
            ThingDef gun_AssaultRifle = null;
            ThingDef gun_SniperRifle = null;
            ThingDef gun_Minigun = null;
            ThingDef mechSerumHealer = null;
            ThingDef mechSerumResurrector = null;
            ThingDef techprofSubpersonaCore = null;
            ThingDef thrumboHorn = null;
            ThingDef elephantTusk = null;

            ThingDef apparel_SmokepopBelt = null;
            ThingDef apparel_SimpleHelmet = null;
            ThingDef apparel_AdvancedHelmet = null;
            ThingDef apparel_PowerArmorHelmet = null;
            ThingDef apparel_PsychicFoilHelmet = null;
            ThingDef apparel_FlakVest = null;
            ThingDef apparel_FlakPants = null;
            ThingDef apparel_FlakJacket = null;
            ThingDef apparel_PowerArmor = null;
            ThingDef apparel_ArmorRecon = null;

            foreach (ThingDef thingDef in thingDefs)
            {
                if (thingDef.defName == "ComponentIndustrial")
                    componentIndustrial = thingDef;
                else if (thingDef.defName == "ComponentSpacer")
                    componentSpacer = thingDef;
                else if (thingDef.defName == "Gun_MachinePistol")
                    gun_MachinePistol = thingDef;
                else if (thingDef.defName == "Gun_IncendiaryLauncher")
                    gun_IncendiaryLauncher = thingDef;
                else if (thingDef.defName == "Gun_SmokeLauncher")
                    gun_SmokeLauncher = thingDef;
                else if (thingDef.defName == "Gun_EMPLauncher")
                    gun_EMPLauncher = thingDef;
                else if (thingDef.defName == "Gun_BoltActionRifle")
                    gun_BoltActionRifle = thingDef;
                else if (thingDef.defName == "Gun_PumpShotgun")
                    gun_PumpShotgun = thingDef;
                else if (thingDef.defName == "Gun_ChainShotgun")
                    gun_ChainShotgun = thingDef;
                else if (thingDef.defName == "Gun_HeavySMG")
                    gun_HeavySMG = thingDef;
                else if (thingDef.defName == "Gun_LMG")
                    gun_LMG = thingDef;
                else if (thingDef.defName == "Gun_AssaultRifle")
                    gun_AssaultRifle = thingDef;
                else if (thingDef.defName == "Gun_SniperRifle")
                    gun_SniperRifle = thingDef;
                else if (thingDef.defName == "Gun_Minigun")
                    gun_Minigun = thingDef;
                else if (thingDef.defName == "MechSerumHealer")
                    mechSerumHealer = thingDef;
                else if (thingDef.defName == "MechSerumResurrector")
                    mechSerumResurrector = thingDef;
                else if (thingDef.defName == "TechprofSubpersonaCore")
                    techprofSubpersonaCore = thingDef;
                else if (thingDef.defName == "ThrumboHorn")
                    thrumboHorn = thingDef;
                else if (thingDef.defName == "ElephantTusk")
                    elephantTusk = thingDef;
                else if (thingDef.defName == "Apparel_SmokepopBelt")
                    apparel_SmokepopBelt = thingDef;
                else if (thingDef.defName == "Apparel_SimpleHelmet")
                    apparel_SimpleHelmet = thingDef;
                else if (thingDef.defName == "Apparel_AdvancedHelmet")
                    apparel_AdvancedHelmet = thingDef;
                else if (thingDef.defName == "Apparel_PowerArmorHelmet")
                    apparel_PowerArmorHelmet = thingDef;
                else if (thingDef.defName == "Apparel_PsychicFoilHelmet")
                    apparel_PsychicFoilHelmet = thingDef;
                else if (thingDef.defName == "Apparel_FlakVest")
                    apparel_FlakVest = thingDef;
                else if (thingDef.defName == "Apparel_FlakPants")
                    apparel_FlakPants = thingDef;
                else if (thingDef.defName == "Apparel_FlakJacket")
                    apparel_FlakJacket = thingDef;
                else if (thingDef.defName == "Apparel_PowerArmor")
                    apparel_PowerArmor = thingDef;
                else if (thingDef.defName == "Apparel_ArmorRecon")
                    apparel_ArmorRecon = thingDef;
            }

            if (Rand.Range(1, 100) <= 40)
            {
                if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(elephantTusk, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(thrumboHorn, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(RimWorld.ThingDefOf.AIPersonaCore, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(techprofSubpersonaCore, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(mechSerumResurrector, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(mechSerumHealer, 1, hive);
            }
            if (Rand.Range(1, 100) <= 40)
            {
                if (Rand.Range(1, 100) <= 12) SpawnItemsNearHive(gun_MachinePistol, 1, hive);
                else if (Rand.Range(1, 100) <= 10) SpawnItemsNearHive(gun_PumpShotgun, 1, hive);
                else if (Rand.Range(1, 100) <= 10) SpawnItemsNearHive(gun_BoltActionRifle, 1, hive);
                else if (Rand.Range(1, 100) <= 10) SpawnItemsNearHive(gun_EMPLauncher, 1, hive);
                else if (Rand.Range(1, 100) <= 10) SpawnItemsNearHive(gun_SmokeLauncher, 1, hive);
                else if (Rand.Range(1, 100) <= 10) SpawnItemsNearHive(gun_IncendiaryLauncher, 1, hive);
                else if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(gun_LMG, 1, hive);
                else if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(gun_HeavySMG, 1, hive);
                else if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(gun_ChainShotgun, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(gun_SniperRifle, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(gun_AssaultRifle, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(gun_Minigun, 1, hive);
            }
            if (Rand.Range(1, 100) <= 40)
            {
                if (Rand.Range(1, 100) <= 15) SpawnItemsNearHive(RimWorld.ThingDefOf.Silver, Rand.Range(4, 8), hive);
                else if (Rand.Range(1, 100) <= 15) SpawnItemsNearHive(RimWorld.ThingDefOf.Chemfuel, Rand.Range(4, 8), hive);
                else if (Rand.Range(1, 100) <= 12) SpawnItemsNearHive(RimWorld.ThingDefOf.Plasteel, Rand.Range(2, 4), hive);
                else if (Rand.Range(1, 100) <= 12) SpawnItemsNearHive(RimWorld.ThingDefOf.Uranium, Rand.Range(2, 4), hive);
                else if (Rand.Range(1, 100) <= 12) SpawnItemsNearHive(RimWorld.ThingDefOf.Gold, Rand.Range(2, 4), hive);
            }
            if (Rand.Range(1, 100) <= 40)
            {
                if (Rand.Range(1, 100) <= 12) SpawnItemsNearHive(componentIndustrial, Rand.Range(2, 4), hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(componentSpacer, Rand.Range(1, 3), hive);
            }
            if (Rand.Range(1, 100) <= 40)
            {
                if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(RimWorld.ThingDefOf.Apparel_ShieldBelt, 1, hive);
                else if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(apparel_SmokepopBelt, 1, hive);
                else if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(apparel_SimpleHelmet, 1, hive);
                else if (Rand.Range(1, 100) <= 8) SpawnItemsNearHive(apparel_PsychicFoilHelmet, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(apparel_AdvancedHelmet, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(apparel_ArmorRecon, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(apparel_FlakJacket, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(apparel_FlakPants, 1, hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(apparel_FlakVest, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(apparel_PowerArmor, 1, hive);
                else if (Rand.Range(1, 100) <= 3) SpawnItemsNearHive(apparel_PowerArmorHelmet, 1, hive);
            }
            if (Rand.Range(1, 100) <= 40)
            {
                if (Rand.Range(1, 100) <= 10) SpawnItemsNearHive(RimWorld.ThingDefOf.MedicineIndustrial, Rand.Range(2, 5), hive);
                else if (Rand.Range(1, 100) <= 5) SpawnItemsNearHive(RimWorld.ThingDefOf.MedicineUltratech, Rand.Range(1, 3), hive);
            }
        }
        public static void SpawnCorpsesNearHive(PawnKindDef pawnKindDef, Faction faction, int num, Hive hive)
        {
            if (pawnKindDef == null || hive == null) return;

            for (int i = 0; i < num; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, faction, PawnGenerationContext.NonPlayer, -1, true));
                if (pawn != null)
                {
                    IntVec3 c = CellFinder.RandomClosewalkCellNear(hive.Position, hive.Map, 12);
                    GenSpawn.Spawn(pawn, c, hive.Map);
                    pawn.Kill(null);
                    foreach (Thing thing in hive.Map.thingGrid.ThingsAt(c))
                    {
                        Corpse corpse = thing as Corpse;
                        if (corpse != null)
                        {
                            CompRottable compRottable = corpse.GetComp<CompRottable>();
                            if (compRottable != null)
                            {
                                compRottable.RotProgress = 999999;
                            }
                        }
                    }
                }
            }
        }
        public static void SpawnItemsNearHive(ThingDef thingDef, int numOfStacks, Hive hive)
        {
            if (thingDef == null || hive == null) return;

            IntVec3 c = CellFinder.RandomClosewalkCellNear(hive.Position, hive.Map, 12);
            for (int i = 0; i < numOfStacks; i++)
            {
                ThingDef stuff = null;
                if (thingDef.MadeFromStuff)
                {
                    if (Rand.Range(1, 100) <= 30) stuff = RimWorld.ThingDefOf.Plasteel;
                    else stuff = RimWorld.ThingDefOf.Steel;
                }
                Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, stuff), c, hive.Map);
                if (thing != null)
                {
                    thing.stackCount = Rand.Range(1, thing.def.stackLimit);
                }
            }
        }
        public static bool SwarmingHordeCheck(Map map)
        {
            if (map != null)
            {
                int count = 0;
                foreach (Thing t in map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
                {
                    foreach (Pawn p in AllHivePawns(t as Hive))
                    {
                        Lord lord = p.GetLord();
                        if (!map.IsPlayerHome || (lord != null && lord.LordJob != null && lord.LordJob.GetType() == typeof(LordJob_AssaultColony)))
                        {
                            return false;
                        }
                        count++;
                    }
                    if (count >= 150) return true;
                }
            }
            return false;
        }
        public static Thing ExecuteSwarmingHorde(Map map)
        {
            if (map != null)
            {
                Thing result = null;
                Lord lord = LordMaker.MakeNewLord(Faction.OfInsects, Activator.CreateInstance(typeof(LordJob_AssaultColony), null) as LordJob, map);
                foreach (Thing t in map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
                {
                    foreach (Pawn p in AllHivePawns(t as Hive))
                    {
                        Queen q = p as Queen;
                        if ((q != null && Rand.Range(1,100) <= 25) || (q == null && Rand.Range(1,100) <= 75))
                        {
                            Lord lord2 = p.GetLord();
                            if (lord != null && lord2 != null)
                            {
                                lord2.Notify_PawnLost(p, PawnLostCondition.ForcedToJoinOtherLord);
                                lord.AddPawn(p);
                                result = p as Thing;
                            }
                        }
                    }
                }
                return result;
            }
            return null;
        }
    }
}
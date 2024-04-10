using RimWorld;
using Verse;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace BetterInfestations
{
    public class CompProperties_Maintainable : CompProperties
    {
        public int ticksHealthy = 1000;
        public int ticksNeedsMaintenance = 1000;
        public int damagePerTickRare = 10;

        public CompProperties_Maintainable()
        {
            compClass = typeof(CompMaintainable);
        }
    }
    public class CompMaintainable : ThingComp
    {
        public int ticksSinceMaintain;
        public CompProperties_Maintainable Props => (CompProperties_Maintainable)props;
        public MaintainableStage CurStage
        {
            get
            {
                if (ticksSinceMaintain < Props.ticksHealthy)
                {
                    return MaintainableStage.Healthy;
                }
                if (ticksSinceMaintain < Props.ticksHealthy + Props.ticksNeedsMaintenance)
                {
                    return MaintainableStage.NeedsMaintenance;
                }
                return MaintainableStage.Damaging;
            }
        }
        private bool Active => (parent as Hive)?.CompDormant.Awake ?? true;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksSinceMaintain, "ticksSinceMaintain", 0);
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Active)
            {
                ticksSinceMaintain++;
                if (Find.TickManager.TicksGame % 250 == 0)
                {
                    CheckTakeDamage();
                }
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            if (Active)
            {
                ticksSinceMaintain += 250;
                CheckTakeDamage();
            }
        }
        private void CheckTakeDamage()
        {
            if (CurStage == MaintainableStage.Damaging)
            {
                parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, Props.damagePerTickRare));
            }
        }
        public void Maintained()
        {
            ticksSinceMaintain = 0;
        }
        public override string CompInspectStringExtra()
        {
            switch (CurStage)
            {
                case MaintainableStage.NeedsMaintenance:
                    return "DueForMaintenance".Translate();
                case MaintainableStage.Damaging:
                    return "DeterioratingDueToLackOfMaintenance".Translate();
                default:
                    return null;
            }
        }
    }
    public class CompProperties_SpawnerJelly : CompProperties
    {
        public ThingDef thingToSpawn;
        public int spawnCount = 1;
        public IntRange spawnIntervalRange = new IntRange(100, 100);
        public bool writeTimeLeftToSpawn;
        public bool showMessageIfOwned;
        public string saveKeysPrefix;

        public CompProperties_SpawnerJelly()
        {
            compClass = typeof(CompSpawnerJelly);
        }
    }
    public class CompSpawnerJelly : ThingComp
    {
        private int ticksUntilSpawn;
        public CompProperties_SpawnerJelly PropsSpawner => (CompProperties_SpawnerJelly)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                ResetCountdown();
            }
        }
        public override void CompTick()
        {
            TickInterval(1);
        }
        public override void CompTickRare()
        {
            TickInterval(250);
        }
        private void TickInterval(int interval)
        {
            if (!parent.Spawned)
            {
                return;
            }
            CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
            if (comp != null)
            {
                if (!comp.Awake)
                {
                    return;
                }
            }
            else if (parent.Position.Fogged(parent.Map))
            {
                return;
            }
            ticksUntilSpawn -= interval;
            CheckShouldSpawn();
        }
        private void CheckShouldSpawn()
        {
            if (ticksUntilSpawn <= 0)
            {
                TryDoSpawn();
                ResetCountdown();
            }
        }
        public bool TryDoSpawn()
        {
            if (!parent.Spawned)
            {
                return false;
            }
            Hive hive = parent as Hive;
            if (hive == null) return false;

            int spawnCount = PropsSpawner.spawnCount + HiveUtility.AllHivePawns(hive).Count;
            if (TryFindSpawnCell(parent, PropsSpawner.thingToSpawn, spawnCount, out IntVec3 result))
            {
                Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
                thing.stackCount = spawnCount;
                if (thing == null)
                {
                    Log.Error("Could not spawn anything for " + parent);
                }
                GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Near, out Thing lastResultingThing);
                lastResultingThing.SetForbidden(value: true);
                if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
                }
                return true;
            }
            return false;
        }
        public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
            {
                if (item.Walkable(parent.Map))
                {
                    Building edifice = item.GetEdifice(parent.Map);
                    if (edifice == null || !thingToSpawn.IsEdifice())
                    {
                        Building_Door building_Door = edifice as Building_Door;
                        if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, item, parent.Map)))
                        {
                            bool flag = false;
                            List<Thing> thingList = item.GetThingList(parent.Map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                Thing thing = thingList[i];
                                if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                result = item;
                                return true;
                            }
                        }
                    }
                }
            }
            result = IntVec3.Invalid;
            return false;
        }
        private void ResetCountdown()
        {
            ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
        }
        public override void PostExposeData()
        {
            string str = PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_");
            Scribe_Values.Look(ref ticksUntilSpawn, str + "ticksUntilSpawn", 0);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEBUG: Spawn " + PropsSpawner.thingToSpawn.label;
                command_Action.icon = TexCommand.DesirePower;
                command_Action.action = delegate
                {
                    TryDoSpawn();
                    ResetCountdown();
                };
                yield return command_Action;
            }
        }
        public override string CompInspectStringExtra()
        {
            if (PropsSpawner.writeTimeLeftToSpawn)
            {
                return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)) + ": " + ticksUntilSpawn.ToStringTicksToPeriod();
            }
            return null;
        }
    }
    public class CompProperties_SpawnerFilth : CompProperties
    {
        public ThingDef filthDef;
        public int spawnCountOnSpawn = 5;
        public float spawnMtbHours = 12f;
        public float spawnRadius = 3f;
        public float spawnEveryDays = -1f;
        public RotStage? requiredRotStage;

        public CompProperties_SpawnerFilth()
        {
            compClass = typeof(CompSpawnerFilth);
        }
    }
    public class CompSpawnerFilth : ThingComp
    {
        private int nextSpawnTimestamp = -1;
        private CompProperties_SpawnerFilth Props => (CompProperties_SpawnerFilth)props;

        private bool CanSpawnFilth
        {
            get
            {
                Hive hive = parent as Hive;
                if (hive != null && !hive.CompDormant.Awake)
                {
                    return false;
                }
                if (Props.requiredRotStage.HasValue && parent.GetRotStage() != Props.requiredRotStage)
                {
                    return false;
                }
                return true;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextSpawnTimestamp, "nextSpawnTimestamp", -1);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                for (int i = 0; i < Props.spawnCountOnSpawn; i++)
                {
                    TrySpawnFilth();
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            TickInterval(1);
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            TickInterval(250);
        }
        private void TickInterval(int interval)
        {
            if (!CanSpawnFilth)
            {
                return;
            }
            if (Props.spawnMtbHours > 0f && Rand.MTBEventOccurs(Props.spawnMtbHours, 2500f, interval))
            {
                TrySpawnFilth();
            }
            if (Props.spawnEveryDays >= 0f && Find.TickManager.TicksGame >= nextSpawnTimestamp)
            {
                if (nextSpawnTimestamp != -1)
                {
                    TrySpawnFilth();
                }
                nextSpawnTimestamp = Find.TickManager.TicksGame + (int)(Props.spawnEveryDays * 60000f);
            }
        }
        public void TrySpawnFilth()
        {
            if (parent.Map != null && CellFinder.TryFindRandomReachableCellNear(parent.Position, parent.Map, Props.spawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), (IntVec3 x) => x.Standable(parent.Map), (Region x) => true, out IntVec3 result))
            {
                FilthMaker.TryMakeFilth(result, parent.Map, Props.filthDef);
            }
        }
    }
    public class CompProperties_SpawnerHives : CompProperties
    {
        public float HiveSpawnPreferredMinDist = 3.5f;
        public float HiveSpawnRadius = 10f;

        public CompProperties_SpawnerHives()
        {
            compClass = typeof(CompSpawnerHives);
        }
    }
    public class CompSpawnerHives : ThingComp
    {
        private int nextHiveSpawnTick = -1;
        public bool canSpawnHives = true;
        private bool wasActivated;
        private CompProperties_SpawnerHives Props => (CompProperties_SpawnerHives)props;

        private bool CanSpawnChildHive
        {
            get
            {
                if (canSpawnHives && BetterInfestationsMod.settings != null)
                {
                    return HiveUtility.TotalSpawnedHivesCount(parent.Map) < BetterInfestationsMod.settings.maxHivesPerMap;
                }
                return false;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                CalculateNextHiveSpawnTick();
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
            if ((comp?.Awake ?? true) && !wasActivated)
            {
                CalculateNextHiveSpawnTick();
                wasActivated = true;
            }
            if ((comp == null || comp.Awake) && Find.TickManager.TicksGame >= nextHiveSpawnTick)
            {
                if (TrySpawnChildHive(out Hive newHive))
                {
                    Messages.Message("MessageHiveReproduced".Translate(), newHive, MessageTypeDefOf.NegativeEvent);
                }
                else
                {
                    CalculateNextHiveSpawnTick();
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            if (!canSpawnHives)
            {
                return "DormantHiveNotReproducing".Translate();
            }
            if (CanSpawnChildHive)
            {
                return "HiveReproducesIn".Translate() + ": " + (nextHiveSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod();
            }
            return null;
        }
        public void CalculateNextHiveSpawnTick()
        {
            if (BetterInfestationsMod.settings == null) return;

            Room room = parent.GetRoom();
            int num = 0;
            int num2 = GenRadial.NumCellsInRadius(9f);
            for (int i = 0; i < num2; i++)
            {
                IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(parent.Map) && intVec.GetRoom(parent.Map) == room && intVec.GetThingList(parent.Map).Any((Thing t) => t is Hive))
                {
                    num++;
                }
            }
            float days = Rand.Range(BetterInfestationsMod.settings.hiveReproductionMinSpawnInDays, BetterInfestationsMod.settings.hiveReproductionMaxSpawnInDays);
            float reproduceRateFactorFromNearbyHiveCount = days / (1f + (num * 0.01f));
            int ticks = (int)(reproduceRateFactorFromNearbyHiveCount * 60000);
            nextHiveSpawnTick = Find.TickManager.TicksGame + ticks;
        }
        public bool TrySpawnChildHive(out Hive newHive)
        {
            if (!CanSpawnChildHive)
            {
                newHive = null;
                return false;
            }
            IntVec3 loc = FindChildHiveLocation(parent.Position, parent.Map, parent.def, Props, allowUnreachable: false);
            if (!loc.IsValid)
            {
                newHive = null;
                return false;
            }
            newHive = (Hive)ThingMaker.MakeThing(parent.def);
            if (newHive.Faction != parent.Faction)
            {
                newHive.SetFaction(parent.Faction);
            }
            Hive hive = parent as Hive;
            if (hive != null)
            {
                if (hive.CompSpawnerPawns.maxSpawnedPawnsPoints[2] < 0)
                {
                    hive.CompSpawnerPawns.maxSpawnedPawnsPoints[2] = 1000f;
                }
                else if (hive.CompSpawnerPawns.maxSpawnedPawnsPoints[2] > 0)
                {
                    hive.CompSpawnerPawns.maxSpawnedPawnsPoints[3] = 1000f;
                }
                if (hive.CompDormant.Awake)
                {
                    newHive.CompDormant.WakeUp();
                }
                newHive.questTags = hive.questTags;
            }
            GenSpawn.Spawn(newHive, loc, parent.Map, WipeMode.FullRefund);
            CalculateNextHiveSpawnTick();
            return true;
        }
        public static IntVec3 FindChildHiveLocation(IntVec3 pos, Map map, ThingDef parentDef, CompProperties_SpawnerHives props, bool allowUnreachable)
        {
            IntVec3 result = IntVec3.Invalid;
            for (int i = 0; i < 3; i++)
            {
                float minDist = props.HiveSpawnPreferredMinDist;
                bool flag;
                if (i >= 2)
                {
                    flag = allowUnreachable && CellFinder.TryFindRandomCellNear(pos, map, (int)props.HiveSpawnRadius, (IntVec3 c) => CanSpawnHiveAt(c, map, pos, parentDef, minDist), out result);
                }
                else
                {
                    if (i == 1)
                    {
                        minDist = 0f;
                    }
                    flag = CellFinder.TryFindRandomReachableCellNear(pos, map, props.HiveSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), (IntVec3 c) => CanSpawnHiveAt(c, map, pos, parentDef, minDist), null, out result);
                }
                if (flag)
                {
                    result = CellFinder.FindNoWipeSpawnLocNear(result, map, parentDef, Rot4.North, 2, (IntVec3 c) => CanSpawnHiveAt(c, map, pos, parentDef, minDist));
                    break;
                }
            }
            return result;
        }
        private static bool CanSpawnHiveAt(IntVec3 c, Map map, IntVec3 parentPos, ThingDef parentDef, float minDist)
        {
            if (!c.Walkable(map) || c.Fogged(map) || (minDist != 0f && !(c.DistanceToSquared(parentPos) >= minDist * minDist)) || c.GetFirstThing(map, RimWorld.ThingDefOf.InsectJelly) != null || c.GetFirstThing(map, RimWorld.ThingDefOf.GlowPod) != null)
            {
                return false;
            }
            for (int i = 0; i < 9; i++)
            {
                IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
                if (!c2.InBounds(map))
                {
                    continue;
                }
                List<Thing> thingList = c2.GetThingList(map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    if (thingList[j] is Hive || thingList[j] is TunnelHiveSpawner)
                    {
                        return false;
                    }
                }
            }
            List<Thing> thingList2 = c.GetThingList(map);
            for (int k = 0; k < thingList2.Count; k++)
            {
                Thing thing = thingList2[k];
                if (thing.def.category == ThingCategory.Building && thing.def.passability == Traversability.Impassable && GenSpawn.SpawningWipes(parentDef, thing.def))
                {
                    return true;
                }
            }
            return true;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Dev: Reproduce";
                command_Action.icon = TexCommand.GatherSpotActive;
                command_Action.action = delegate
                {
                    TrySpawnChildHive(out Hive _);
                };
                yield return command_Action;
            }
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref nextHiveSpawnTick, "nextHiveSpawnTick", 0);
            Scribe_Values.Look(ref canSpawnHives, "canSpawnHives", defaultValue: true);
            Scribe_Values.Look(ref wasActivated, "wasActivated", defaultValue: true);
        }
    }
    public class CompProperties_SpawnerPawns : CompProperties
    {
        public CompProperties_SpawnerPawns()
        {
            compClass = typeof(CompSpawnerPawns);
        }
    }
    public class CompSpawnerPawns : ThingComp
    {
        public List<float> maxSpawnedPawnsPoints = new List<float> { 1000f, 1000f, -1f, -1f };
        public List<int> nextPawnSpawnTick = new List<int> { -1, -1, -1, -1 };
        public List<List<Pawn>> spawnedPawns = new List<List<Pawn>> { new List<Pawn>(), new List<Pawn>(), new List<Pawn>(), new List<Pawn>() };
        public List<Lord> Lord = new List<Lord> { null, null, null, null };
        public bool canSpawnPawns = true;
        public bool queenSpawned = false;
        public List<Thing> attackTarget = new List<Thing> { null, null, null, null };
        public List<IntVec3> patrolLoc = new List<IntVec3> { IntVec3.Invalid, IntVec3.Invalid, IntVec3.Invalid, IntVec3.Invalid };
        public List<LocomotionUrgency> patrolLocomotion = new List<LocomotionUrgency> { LocomotionUrgency.Walk, LocomotionUrgency.Walk, LocomotionUrgency.Walk, LocomotionUrgency.Walk };
        public List<bool> waitForOrders = new List<bool> { true, true, true, true };
        public List<int> waitTicks = new List<int> { 0, 0, 0, 0 };
        public int reassignPawnTick = -1;

        public CompProperties_SpawnerPawns Props => (CompProperties_SpawnerPawns)props;

        public float SpawnedPawnsPoints(int index)
        {
            FilterOutUnspawnedPawns(index);
            return spawnedPawns[index].Sum(x => Convert.ToSingle(x.kindDef.combatPower));
        }
        public int GroupStrength(int index)
        {
            FilterOutUnspawnedPawns(index);
            int num = 0;
            for (int i = 0; i < spawnedPawns[index].Count; i++)
            {
                if (spawnedPawns[index][i].jobs.posture != PawnPosture.LayingOnGroundNormal && spawnedPawns[index][i].mindState != null && spawnedPawns[index][i].mindState.duty != null)
                {
                    num += (int)spawnedPawns[index][i].kindDef.combatPower;
                }
            }
            return num;
        }
        public bool Active => parent.GetComp<CompCanBeDormant>()?.Awake ?? true;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public static Lord CreateNewLord(Thing byThing, Type lordJobType)
        {
            if (!CellFinder.TryFindRandomCellNear(byThing.Position, byThing.Map, 5, (IntVec3 c) => c.Standable(byThing.Map) && byThing.Map.reachability.CanReach(c, byThing, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)), out IntVec3 result))
            {
                result = IntVec3.Invalid;
            }
            return LordMaker.MakeNewLord(byThing.Faction, Activator.CreateInstance(lordJobType, null) as LordJob, byThing.Map);
        }
        public void SpawnInitialPawns()
        {
            if (BetterInfestationsMod.settings == null) return;

            if (BetterInfestationsMod.settings.hiveLevel >= 2)
            {
                maxSpawnedPawnsPoints[2] = 1000f;
                if (BetterInfestationsMod.settings.hiveLevel == 3)
                {
                    maxSpawnedPawnsPoints[3] = 1000f;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (maxSpawnedPawnsPoints[i] == -1) continue;

                if (i == 0 || (i > 0 && BetterInfestationsMod.settings.initialHunterSpawnsAllowed))
                {
                    while (SpawnedPawnsPoints(i) < BetterInfestationsMod.settings.initialPawnsPoints)
                    {
                        if (!TrySpawnPawn(i, out Pawn _, RandomPawnKindDef(), BetterInfestationsMod.settings.newbornInsects)) break;
                    }
                }
            }
            CalculateNextPawnSpawnTick(0);
        }
        private void CalculateNextPawnSpawnTick(int index)
        {
            if (BetterInfestationsMod.settings == null) return;

            float days = Rand.Range(BetterInfestationsMod.settings.minSpawnInDays[index], BetterInfestationsMod.settings.maxSpawnInDays[index]);
            float dayTicks = 60000;
            if (HiveUtility.QueenActive(parent as Hive)) dayTicks = 42000;
            int ticks = (int)(days * dayTicks);
            nextPawnSpawnTick[index] = Find.TickManager.TicksGame + ticks;
        }
        private void FilterOutUnspawnedPawns(int index)
        {
            spawnedPawns[index].RemoveAll(x => !x.Spawned);
        }
        private void ReassignNullDutyPawns()
        {
            foreach (Pawn p in parent.Map.mapPawns.AllPawns)
            {
                if (p != null && p.Spawned && (p.kindDef == RimWorld.PawnKindDefOf.Megascarab || p.kindDef == RimWorld.PawnKindDefOf.Spelopede || p.kindDef == RimWorld.PawnKindDefOf.Megaspider || p.kindDef == PawnKindDefOf.BI_Queen))
                {
                    if (!p.Downed && p.mindState.duty == null && p.Faction == Faction.OfInsects)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Lord lord = Lord[i];
                            if (lord == null) continue;

                            if (lord.ownedPawns != null && !lord.ownedPawns.Contains(p) && spawnedPawns[i].Contains(p))
                            {
                                FieldInfo FI_curLordToil = typeof(Lord).GetField("curLordToil", Patches.allFlags);
                                LordToil lordToil = (LordToil)FI_curLordToil.GetValue(lord);
                                lord.AddPawn(p);
                                parent.Map.attackTargetsCache.UpdateTarget(p);
                                lordToil.UpdateAllDuties();
                            }
                        }
                    }
                }
            }
            reassignPawnTick = Find.TickManager.TicksGame + 600;
        }
        public PawnKindDef RandomPawnKindDef()
        {
            IEnumerable<PawnKindDef> source = new List<PawnKindDef> { RimWorld.PawnKindDefOf.Megascarab, RimWorld.PawnKindDefOf.Spelopede, RimWorld.PawnKindDefOf.Megaspider };
            if (source.TryRandomElement(out PawnKindDef result))
            {
                return result;
            }
            return null;
        }
        public bool TrySpawnPawn(int index, out Pawn pawn, PawnKindDef chosenKind, bool newbornPawn)
        {
            if (chosenKind == null || BetterInfestationsMod.settings == null)
            {
                pawn = null;
                return false;
            }
            if (!canSpawnPawns) newbornPawn = false;
            Hive hive = parent as Hive;
            PawnGenerationRequest request = new PawnGenerationRequest(chosenKind, parent.Faction, PawnGenerationContext.NonPlayer, -1, true, newbornPawn);
            pawn = PawnGenerator.GeneratePawn(request);
            if (chosenKind == PawnKindDefOf.BI_Queen)
            {
                pawn.gender = Gender.Female;
            }
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 2), parent.Map);
            spawnedPawns[index].Add(pawn);
            Lord lord = Lord[index];
            if (lord == null)
            {
                Type lordJobType = null;
                if (index == 0) lordJobType = typeof(LordJob_DefendAndExpandHive);
                else lordJobType = typeof(LordJob_HiveHunters);
                lord = CreateNewLord(parent, lordJobType);
                Lord[index] = lord;
            }
            lord.AddPawn(pawn);
            SoundDef soundDef = SoundDefOf.Hive_Spawn;
            if (soundDef != null)
            {
                soundDef.PlayOneShot(parent);
            }
            return true;
        }
        public override void CompTick()
        {
            if (BetterInfestationsMod.settings == null) return;

            if (Find.TickManager.TicksGame >= reassignPawnTick)
            {
                ReassignNullDutyPawns();
            }

            for (int i = 0; i < 4; i++)
            {
                if (!parent.Spawned || !Active)
                {
                    return;
                }
                if (i == 0 && nextPawnSpawnTick[i] == -1)
                {
                    SpawnInitialPawns();
                }
                else if (i != 0 && nextPawnSpawnTick[i] == -1)
                {
                    CalculateNextPawnSpawnTick(i);
                }
                FilterOutUnspawnedPawns(i);
                if (Find.TickManager.TicksGame >= nextPawnSpawnTick[i])
                {
                    if (canSpawnPawns && SpawnedPawnsPoints(i) < maxSpawnedPawnsPoints[i] && TrySpawnPawn(i, out Pawn pawn, RandomPawnKindDef(), BetterInfestationsMod.settings.newbornInsects) && pawn.caller != null)
                    {
                        pawn.caller.DoCall();
                    }
                    if (i == 0 && canSpawnPawns && SpawnedPawnsPoints(i) >= 500 && !queenSpawned && BetterInfestationsMod.settings.queensAllowed && Rand.Range(1, 100) <= 30)
                    {
                        if (TrySpawnPawn(i, out Pawn q, PawnKindDefOf.BI_Queen, BetterInfestationsMod.settings.newbornInsects) && q.caller != null)
                        {
                            Messages.Message("A bug queen has spawned.", q, MessageTypeDefOf.NegativeEvent);
                            q.caller.DoCall();
                            queenSpawned = true;
                        }
                    }
                    CalculateNextPawnSpawnTick(i);
                }
                GroupController(i);
            }
        }
        public void GroupController(int index)
        {
            if (index == 0) return;

            if (Find.TickManager.TicksGame > waitTicks[index])
            {
                if (GroupStrength(index) >= 750)
                {
                    Pawn pawn = null;
                    foreach (Pawn p in spawnedPawns[index])
                    {
                        int dist = IntVec3Utility.ManhattanDistanceFlat(p.Position, patrolLoc[index]);
                        if (dist > 8)
                        {
                            bool alseep = p.jobs.posture == PawnPosture.LayingOnGroundNormal;
                            bool atWork = p.jobs.curJob != null && p.jobs.curJob.targetA != null && p.jobs.curJob.targetA != patrolLoc[index];
                            if (!alseep && !atWork)
                            {
                                waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                return;
                            }
                        }
                        pawn = p;
                    }
                    if (pawn != null)
                    {
                        if (waitForOrders[index])
                        {
                            if (Rand.Range(1, 100) <= 40)
                            {
                                waitForOrders[index] = false;
                                waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                return;
                            }
                        }
                        else
                        {
                            IntVec3 pos = IntVec3.Invalid;
                            if (Rand.Range(1, 100) <= 5)
                            {
                                pos = HiveUtility.GetColonyStockpileSpot(pawn.Map);
                                if (pos != IntVec3.Invalid && pawn.CanReserve(pos))
                                {
                                    patrolLoc[index] = pos;
                                    patrolLocomotion[index] = LocomotionUrgency.Jog;
                                    waitForOrders[index] = true;
                                    waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                    return;
                                }
                            }
                            else
                            {
                                pos = HiveUtility.FindPathToPrey(pawn);
                                if (pos != IntVec3.Invalid && pawn.CanReserve(pos))
                                {
                                    patrolLoc[index] = pos;
                                    patrolLocomotion[index] = LocomotionUrgency.Walk;
                                    waitForOrders[index] = true;
                                    waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                    return;
                                }
                            }
                        }
                    }
                }
                else if (GroupStrength(index) >= 350)
                {
                    Pawn pawn = null;
                    foreach (Pawn p in spawnedPawns[index])
                    {
                        int dist = IntVec3Utility.ManhattanDistanceFlat(p.Position, patrolLoc[index]);
                        if (dist > 8)
                        {
                            bool alseep = p.jobs.posture == PawnPosture.LayingOnGroundNormal;
                            bool atWork = p.jobs.curJob != null && p.jobs.curJob.targetA != null && p.jobs.curJob.targetA != patrolLoc[index];
                            if (!alseep && !atWork)
                            {
                                waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                return;
                            }
                        }
                        pawn = p;
                    }
                    if (pawn != null)
                    {
                        if (waitForOrders[index])
                        {
                            if (Rand.Range(1, 100) <= 40)
                            {
                                waitForOrders[index] = false;
                                waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                return;
                            }
                        }
                        else
                        {
                            IntVec3 pos = IntVec3.Invalid;
                            if (Rand.Range(1, 100) <= 5)
                            {
                                pos = HiveUtility.GetHive(pawn).Position;
                                if (pos != IntVec3.Invalid && pawn.CanReserve(pos))
                                {
                                    patrolLoc[index] = pos;
                                    patrolLocomotion[index] = LocomotionUrgency.Walk;
                                    waitForOrders[index] = true;
                                    waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                    return;
                                }
                            }
                            else
                            {
                                pos = HiveUtility.FindPathToPrey(pawn);
                                if (pos != IntVec3.Invalid && pawn.CanReserve(pos))
                                {
                                    patrolLoc[index] = pos;
                                    patrolLocomotion[index] = LocomotionUrgency.Walk;
                                    waitForOrders[index] = true;
                                    waitTicks[index] = Find.TickManager.TicksGame + 1000;
                                    return;
                                }
                            }
                        }
                    }
                }
                else if (Find.TickManager.TicksGame > waitTicks[index])
                {
                    waitTicks[index] = Find.TickManager.TicksGame + 7500;
                    patrolLoc[index] = parent.Position;
                    patrolLocomotion[index] = LocomotionUrgency.Walk;
                }
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                if (BetterInfestationsMod.settings == null)
                {
                    yield return null;
                }
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEBUG: Spawn bug for defense group";
                command_Action.icon = TexCommand.ReleaseAnimals;
                command_Action.action = delegate
                {
                    TrySpawnPawn(0, out Pawn _, RandomPawnKindDef(), BetterInfestationsMod.settings.newbornInsects);
                };
                yield return command_Action;

                command_Action = new Command_Action();
                command_Action.defaultLabel = "DEBUG: Spawn bug for hunting party 1";
                command_Action.icon = TexCommand.ReleaseAnimals;
                command_Action.action = delegate
                {
                    TrySpawnPawn(1, out Pawn _, RandomPawnKindDef(), BetterInfestationsMod.settings.newbornInsects);
                };
                yield return command_Action;

                command_Action = new Command_Action();
                command_Action.defaultLabel = "DEBUG: Spawn bug for hunting party 2";
                command_Action.icon = TexCommand.ReleaseAnimals;
                command_Action.action = delegate
                {
                    TrySpawnPawn(2, out Pawn _, RandomPawnKindDef(), BetterInfestationsMod.settings.newbornInsects);
                };
                yield return command_Action;

                command_Action = new Command_Action();
                command_Action.defaultLabel = "DEBUG: Spawn bug for hunting party 3";
                command_Action.icon = TexCommand.ReleaseAnimals;
                command_Action.action = delegate
                {
                    TrySpawnPawn(3, out Pawn _, RandomPawnKindDef(), BetterInfestationsMod.settings.newbornInsects);
                };
                yield return command_Action;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            List<Pawn> defenseGroup = spawnedPawns[0];
            List<Pawn> huntingGroup1 = spawnedPawns[1];
            List<Pawn> huntingGroup2 = spawnedPawns[2];
            List<Pawn> huntingGroup3 = spawnedPawns[3];
            int defenseGroupSpawnTick = nextPawnSpawnTick[0];
            int huntingGroup1SpawnTick = nextPawnSpawnTick[1];
            int huntingGroup2SpawnTick = nextPawnSpawnTick[2];
            int huntingGroup3SpawnTick = nextPawnSpawnTick[3];
            Thing defenseGroupAttackTarget = attackTarget[0];
            Thing huntingGroup1AttackTarget = attackTarget[1];
            Thing huntingGroup2AttackTarget = attackTarget[2];
            Thing huntingGroup3AttackTarget = attackTarget[3];
            IntVec3 defenseGroupPatrolLoc = patrolLoc[0];
            IntVec3 huntingGroup1PatrolLoc = patrolLoc[1];
            IntVec3 huntingGroup2PatrolLoc = patrolLoc[2];
            IntVec3 huntingGroup3PatrolLoc = patrolLoc[3];
            LocomotionUrgency defenseGroupPatrolLocomotion = patrolLocomotion[0];
            LocomotionUrgency huntingGroup1PatrolLocomotion = patrolLocomotion[1];
            LocomotionUrgency huntingGroup2PatrolLocomotion = patrolLocomotion[2];
            LocomotionUrgency huntingGroup3PatrolLocomotion = patrolLocomotion[3];
            bool defenseGroupWaitForOrders = waitForOrders[0];
            bool huntingGroup1WaitForOrders = waitForOrders[1];
            bool huntingGroup2WaitForOrders = waitForOrders[2];
            bool huntingGroup3WaitForOrders = waitForOrders[3];
            int defenseGroupWaitTicks = waitTicks[0];
            int huntingGroup1WaitTicks = waitTicks[1];
            int huntingGroup2WaitTicks = waitTicks[2];
            int huntingGroup3WaitTicks = waitTicks[3];
            float defenseGroupMaxSpawnedPawnsPoints = maxSpawnedPawnsPoints[0];
            float huntingGroup1MaxSpawnedPawnsPoints = maxSpawnedPawnsPoints[1];
            float huntingGroup2MaxSpawnedPawnsPoints = maxSpawnedPawnsPoints[2];
            float huntingGroup3MaxSpawnedPawnsPoints = maxSpawnedPawnsPoints[3];
            Lord defenseGroupLord = Lord[0];
            Lord huntingGroup1Lord = Lord[1];
            Lord huntingGroup2Lord = Lord[2];
            Lord huntingGroup3Lord = Lord[3];

            Scribe_Values.Look(ref defenseGroupSpawnTick, "defenseGroupSpawnTick", 0);
            Scribe_References.Look(ref defenseGroupAttackTarget, "defenseGroupAttackTarget");
            Scribe_Values.Look(ref defenseGroupPatrolLoc, "defenseGroupPatrolLoc", IntVec3.Invalid);
            Scribe_Values.Look(ref defenseGroupPatrolLocomotion, "defenseGroupPatrolLocomotion", LocomotionUrgency.Walk);
            Scribe_Values.Look(ref defenseGroupWaitForOrders, "defenseGroupWaitForOrders", true);
            Scribe_Values.Look(ref defenseGroupWaitTicks, "defenseGroupWaitTicks", 0);
            Scribe_Values.Look(ref defenseGroupMaxSpawnedPawnsPoints, "defenseGroupMaxSpawnedPawnsPoints", 1000f);
            Scribe_Collections.Look(ref defenseGroup, "defenseGroup", LookMode.Reference);
            Scribe_References.Look(ref defenseGroupLord, "defenseGroupLord", false);

            Scribe_Values.Look(ref huntingGroup1SpawnTick, "huntingGroup1SpawnTick", 0);
            Scribe_References.Look(ref huntingGroup1AttackTarget, "huntingGroup1AttackTarget");
            Scribe_Values.Look(ref huntingGroup1PatrolLoc, "huntingGroup1PatrolLoc", IntVec3.Invalid);
            Scribe_Values.Look(ref huntingGroup1PatrolLocomotion, "huntingGroup1PatrolLocomotion", LocomotionUrgency.Walk);
            Scribe_Values.Look(ref huntingGroup1WaitForOrders, "huntingGroup1WaitForOrders", true);
            Scribe_Values.Look(ref huntingGroup1WaitTicks, "huntingGroup1WaitTicks", 0);
            Scribe_Values.Look(ref huntingGroup1MaxSpawnedPawnsPoints, "huntingGroup1MaxSpawnedPawnsPoints", 1000f);
            Scribe_Collections.Look(ref huntingGroup1, "huntingGroup1", LookMode.Reference);
            Scribe_References.Look(ref huntingGroup1Lord, "huntingGroup1Lord", false);

            Scribe_Values.Look(ref huntingGroup2SpawnTick, "huntingGroup2SpawnTick", 0);
            Scribe_References.Look(ref huntingGroup2AttackTarget, "huntingGroup2AttackTarget");
            Scribe_Values.Look(ref huntingGroup2PatrolLoc, "huntingGroup2PatrolLoc", IntVec3.Invalid);
            Scribe_Values.Look(ref huntingGroup2PatrolLocomotion, "huntingGroup2PatrolLocomotion", LocomotionUrgency.Walk);
            Scribe_Values.Look(ref huntingGroup2WaitForOrders, "huntingGroup2WaitForOrders", true);
            Scribe_Values.Look(ref huntingGroup2WaitTicks, "huntingGroup2WaitTicks", 0);
            Scribe_Values.Look(ref huntingGroup2MaxSpawnedPawnsPoints, "huntingGroup2MaxSpawnedPawnsPoints", -1f);
            Scribe_Collections.Look(ref huntingGroup2, "huntingGroup2", LookMode.Reference);
            Scribe_References.Look(ref huntingGroup2Lord, "huntingGroup2Lord", false);

            Scribe_Values.Look(ref huntingGroup3SpawnTick, "huntingGroup3SpawnTick", 0);
            Scribe_References.Look(ref huntingGroup3AttackTarget, "huntingGroup3AttackTarget");
            Scribe_Values.Look(ref huntingGroup3PatrolLoc, "huntingGroup3PatrolLoc", IntVec3.Invalid);
            Scribe_Values.Look(ref huntingGroup3PatrolLocomotion, "huntingGroup3PatrolLocomotion", LocomotionUrgency.Walk);
            Scribe_Values.Look(ref huntingGroup3WaitForOrders, "huntingGroup3WaitForOrders", true);
            Scribe_Values.Look(ref huntingGroup3WaitTicks, "huntingGroup3WaitTicks", 0);
            Scribe_Values.Look(ref huntingGroup3MaxSpawnedPawnsPoints, "huntingGroup3MaxSpawnedPawnsPoints", -1f);
            Scribe_Collections.Look(ref huntingGroup3, "huntingGroup3", LookMode.Reference);
            Scribe_References.Look(ref huntingGroup3Lord, "huntingGroup3Lord", false);

            Scribe_Values.Look(ref canSpawnPawns, "canSpawnPawns", defaultValue: true);
            Scribe_Values.Look(ref queenSpawned, "queenSpawned", defaultValue: false);

            spawnedPawns[0] = defenseGroup;
            spawnedPawns[1] = huntingGroup1;
            spawnedPawns[2] = huntingGroup2;
            spawnedPawns[3] = huntingGroup3;
            nextPawnSpawnTick[0] = defenseGroupSpawnTick;
            nextPawnSpawnTick[1] = huntingGroup1SpawnTick;
            nextPawnSpawnTick[2] = huntingGroup2SpawnTick;
            nextPawnSpawnTick[3] = huntingGroup3SpawnTick;
            attackTarget[0] = defenseGroupAttackTarget;
            attackTarget[1] = huntingGroup1AttackTarget;
            attackTarget[2] = huntingGroup2AttackTarget;
            attackTarget[3] = huntingGroup3AttackTarget;
            patrolLoc[0] = defenseGroupPatrolLoc;
            patrolLoc[1] = huntingGroup1PatrolLoc;
            patrolLoc[2] = huntingGroup2PatrolLoc;
            patrolLoc[3] = huntingGroup3PatrolLoc;
            patrolLocomotion[0] = defenseGroupPatrolLocomotion;
            patrolLocomotion[1] = huntingGroup1PatrolLocomotion;
            patrolLocomotion[2] = huntingGroup2PatrolLocomotion;
            patrolLocomotion[3] = huntingGroup3PatrolLocomotion;
            waitForOrders[0] = defenseGroupWaitForOrders;
            waitForOrders[1] = huntingGroup1WaitForOrders;
            waitForOrders[2] = huntingGroup2WaitForOrders;
            waitForOrders[3] = huntingGroup3WaitForOrders;
            waitTicks[0] = defenseGroupWaitTicks;
            waitTicks[1] = huntingGroup1WaitTicks;
            waitTicks[2] = huntingGroup2WaitTicks;
            waitTicks[3] = huntingGroup3WaitTicks;
            maxSpawnedPawnsPoints[0] = defenseGroupMaxSpawnedPawnsPoints;
            maxSpawnedPawnsPoints[1] = huntingGroup1MaxSpawnedPawnsPoints;
            maxSpawnedPawnsPoints[2] = huntingGroup2MaxSpawnedPawnsPoints;
            maxSpawnedPawnsPoints[3] = huntingGroup3MaxSpawnedPawnsPoints;
            Lord[0] = defenseGroupLord;
            Lord[1] = huntingGroup1Lord;
            Lord[2] = huntingGroup2Lord;
            Lord[3] = huntingGroup3Lord;

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                for (int i = 0; i < 4; i++)
                {
                    spawnedPawns[i].RemoveAll((Pawn x) => x == null);
                }
            }
        }
    }
}
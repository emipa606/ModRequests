using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace BetterInfestations
{
    [StaticConstructorOnStartup]
    static class Patches
    {
        public const BindingFlags allFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
        static Patches()
        {
            var harmony = new Harmony("betterinfestations.harmony");
            harmony.PatchAll();
        }
        [HarmonyPatch(typeof(InfestationCellFinder))]
        [HarmonyPatch("GetScoreAt")]
        static class Patch_InfestationCellFinder_GetScoreAt
        {
            static bool CellHasBlockingThings(IntVec3 cell, Map map)
            {
                List<Thing> thingList = cell.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Pawn || thingList[i] is Hive || thingList[i] is TunnelHiveSpawner)
                    {
                        return true;
                    }
                    if (thingList[i].def.category == ThingCategory.Building && thingList[i].def.passability == Traversability.Impassable && GenSpawn.SpawningWipes(ThingDefOf.BI_Hive, thingList[i].def))
                    {
                        return true;
                    }
                }
                return false;
            }
            public static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
            {
                float num = 0f;
                int num2 = 0;
                for (int i = 0; i < 700; i += 10)
                {
                    IntVec3 c = cell + GenRadial.RadialPattern[i];
                    if (c.InBounds(map))
                    {
                        Building edifice = c.GetEdifice(map);
                        if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
                        {
                            num += 1f;
                        }
                        num2++;
                    }
                }
                return num / num2;
            }
            public static int DistanceToColonyBuilding(IntVec3 cell, Map map)
            {
                int dist = 0;
                int nearest = int.MaxValue;
                List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
                for (int i = 0; i < allBuildingsColonist.Count; i++)
                {
                    dist = IntVec3Utility.ManhattanDistanceFlat(allBuildingsColonist[i].Position, cell);
                    if (dist <= nearest)
                    {
                        nearest = dist;
                    }
                }
                return nearest;
            }
            public static int DistanceToHive(IntVec3 cell, Map map)
            {
                int dist = 0;
                int nearest = int.MaxValue;
                List<Thing> hives = map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive);
                for (int i = 0; i < hives.Count; i++)
                {
                    dist = IntVec3Utility.ManhattanDistanceFlat(hives[i].Position, cell);
                    if (dist <= nearest)
                    {
                        nearest = dist;
                    }
                }
                return nearest;
            }
            public static float GetScoreAt(IntVec3 cell, Map map)
            {
                float score = 0f;
                TerrainDef terrainDef = cell.GetTerrain(map);
                if (terrainDef != null)
                {
                    if (!terrainDef.label.Contains("rough") && !terrainDef.label.Contains("hewn") && !terrainDef.label.Contains("smooth") && !terrainDef.label.Contains("stony"))
                    {
                        return 0f;
                    }
                }
                if (!cell.Walkable(map))
                {
                    return 0f;
                }
                if (cell.Fogged(map))
                {
                    return 0f;
                }
                if (CellHasBlockingThings(cell, map))
                {
                    return 0f;
                }
                Region region = cell.GetRegion(map);
                if (region == null)
                {
                    return 0f;
                }
                if (!cell.GetRoom(map).TouchesMapEdge)
                {
                    return 0f;
                }
                if (cell.GetRoom(map).CellCount < 300)
                {
                    return 0f;
                }
                float temperature = cell.GetTemperature(map);
                if (temperature < -17f)
                {
                    return 0f;
                }
                if (DistanceToColonyBuilding(cell, map) < 50)
                {
                    score -= 22f;
                }
                if (DistanceToHive(cell, map) < 40)
                {
                    score -= 22f;
                }
                if (cell.Roofed(map))
                {
                    score += 7.5f;
                }
                if (cell.GetRoof(map) == RoofDefOf.RoofRockThick)
                {
                    score += 7.5f;
                }
                float mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
                if (mountainousnessScoreAt >= 0.17f)
                {
                    score += 7.5f;
                }
                if (map.glowGrid.GameGlowAt(cell, true) <= 0.3f)
                {
                    score += 7.5f;
                }
                if (score < 0)
                {
                    score = 0f;
                }
                score += 7.5f;
                return score;
            }
            static bool Prefix(ref float __result, IntVec3 cell, Map map)
            {
                __result = GetScoreAt(cell, map);
                return false;
            }
        }
        [HarmonyPatch(typeof(InfestationCellFinder))]
        [HarmonyPatch("TryFindCell")]
        public static class Patch_InfestationCellFinder_TryFindCell
        {
            public struct LocationCandidate
            {
                public IntVec3 cell;
                public float score;
                public LocationCandidate(IntVec3 cell, float score)
                {
                    this.cell = cell;
                    this.score = score;
                }
            }
            public static List<LocationCandidate> locationCandidates = new List<LocationCandidate>();
            public static bool TryFindCell(out IntVec3 cell, Map map)
            {
                locationCandidates.Clear();
                for (int i = 0; i < map.Size.z; i++)
                {
                    for (int j = 0; j < map.Size.x; j++)
                    {
                        IntVec3 c = new IntVec3(j, 0, i);
                        float scoreAt = Patch_InfestationCellFinder_GetScoreAt.GetScoreAt(c, map);
                        if (scoreAt > 0f)
                        {
                            locationCandidates.Add(new LocationCandidate(c, scoreAt));
                        }
                    }
                }
                if (!TryGetHiveSpawnLocation(out LocationCandidate result))
                {
                    cell = IntVec3.Invalid;
                    return false;
                }
                cell = CellFinder.FindNoWipeSpawnLocNear(result.cell, map, ThingDefOf.BI_Hive, Rot4.North, 2, (IntVec3 x) => Patch_InfestationCellFinder_GetScoreAt.GetScoreAt(x, map) > 0f && x.GetFirstThing(map, ThingDefOf.BI_Hive) == null && x.GetFirstThing(map, ThingDefOf.BI_TunnelHiveSpawner) == null);
                return true;
            }
            public static bool TryGetHiveSpawnLocation(out LocationCandidate result)
            {
                float highestScore = 0f;
                if (locationCandidates.NullOrEmpty())
                {
                    result = default;
                    return false;
                }

                foreach (LocationCandidate x in locationCandidates)
                {
                    if (x.score > highestScore)
                    {
                        highestScore = x.score;
                    }
                }
                locationCandidates.RemoveAll(x => x.score != highestScore);
                if (locationCandidates.NullOrEmpty())
                {
                    result = default;
                    return false;
                }
                result = locationCandidates.RandomElement();
                return true;
            }
            static bool Prefix(ref bool __result, out IntVec3 cell, Map map)
            {
                __result = TryFindCell(out cell, map);
                return false;
            }
        }
        [HarmonyPatch(typeof(InfestationCellFinder))]
        [HarmonyPatch("DebugDraw")]
        public static class Patch_InfestationCellFinder_DebugDraw
        {
            private static int timeStamp = 0;
            private static List<Pair<IntVec3, float>> tmpCachedInfestationChanceCellColors;
            static void DebugDraw()
            {
                if (DebugViewSettings.drawInfestationChance)
                {
                    if (tmpCachedInfestationChanceCellColors == null)
                    {
                        tmpCachedInfestationChanceCellColors = new List<Pair<IntVec3, float>>();
                        timeStamp = 0;
                    }
                    if (Find.TickManager.TicksGame >= timeStamp)
                    {
                        timeStamp = Find.TickManager.TicksGame + 180;
                        tmpCachedInfestationChanceCellColors.Clear();
                        Map currentMap = Find.CurrentMap;
                        float num = 0.001f;
                        for (int i = 0; i < currentMap.Size.z; i++)
                        {
                            for (int j = 0; j < currentMap.Size.x; j++)
                            {
                                float scoreAt = Patch_InfestationCellFinder_GetScoreAt.GetScoreAt(new IntVec3(j, 0, i), currentMap);
                                if (scoreAt > num)
                                {
                                    num = scoreAt;
                                }
                            }
                        }
                        for (int k = 0; k < currentMap.Size.z; k++)
                        {
                            for (int l = 0; l < currentMap.Size.x; l++)
                            {
                                IntVec3 intVec = new IntVec3(l, 0, k);
                                float scoreAt2 = Patch_InfestationCellFinder_GetScoreAt.GetScoreAt(intVec, currentMap);
                                if (scoreAt2 >= 7.5f)
                                {
                                    float second = GenMath.LerpDouble(7.5f, num, 0.25f, 1f, scoreAt2);
                                    tmpCachedInfestationChanceCellColors.Add(new Pair<IntVec3, float>(intVec, second));
                                }
                            }
                        }
                    }
                    for (int m = 0; m < tmpCachedInfestationChanceCellColors.Count; m++)
                    {
                        IntVec3 first = tmpCachedInfestationChanceCellColors[m].First;
                        float second2 = tmpCachedInfestationChanceCellColors[m].Second;
                        CellRenderer.RenderCell(first, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, second2)));
                    }
                }
                else
                {
                    tmpCachedInfestationChanceCellColors = null;
                }
            }

            static bool Prefix()
            {
                DebugDraw();
                return false;
            }
        }
        [HarmonyPatch(typeof(PathFinder))]
        [HarmonyPatch("GetBuildingCost")]
        public static class Patch_PathFinder_GetBuildingCost
        {
            static bool Prefix(ref int __result, Building b, TraverseParms traverseParms, Pawn pawn)
            {
                if (pawn != null)
                {
                    if (pawn.kindDef == RimWorld.PawnKindDefOf.Megascarab || pawn.kindDef == RimWorld.PawnKindDefOf.Spelopede || pawn.kindDef == RimWorld.PawnKindDefOf.Megaspider || pawn.kindDef == PawnKindDefOf.BI_Queen)
                    {
                        if (traverseParms != null && traverseParms.mode == TraverseMode.PassAllDestroyableThings)
                        {
                            if (b != null && b.def != null && b.def.passability == Traversability.Impassable && b.def.size == IntVec2.One && b.Faction != null && b.Faction.IsPlayer)
                            {
                                __result = 0;
                            }
                            else
                            {
                                __result = 500;
                            }
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_MindState))]
        [HarmonyPatch("StartFleeingBecauseOfPawnAction")]
        public static class Patch_Pawn_MindState_StartFleeingBecauseOfPawnAction
        {
            static FieldInfo FI_pawn = typeof(Pawn_MindState).GetField("pawn", allFlags);
            static Pawn pawn(Pawn_MindState instance)
            {
                return (Pawn)FI_pawn.GetValue(instance);
            }
            static bool Prefix(Pawn_MindState __instance)
            {
                if (pawn(__instance) != null)
                {
                    if (pawn(__instance).kindDef == RimWorld.PawnKindDefOf.Megascarab || pawn(__instance).kindDef == RimWorld.PawnKindDefOf.Spelopede || pawn(__instance).kindDef == RimWorld.PawnKindDefOf.Megaspider || pawn(__instance).kindDef == PawnKindDefOf.BI_Queen)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(RimWorld.HiveUtility))]
        [HarmonyPatch("AnyHivePreventsClaiming")]
        public static class Patch_HiveUtility_AnyHivePreventsClaiming
        {
            static bool Prefix(ref bool __result, Thing thing)
            {
                __result = HiveUtility.AnyHivePreventsClaiming(thing);
                return false;
            }
        }
        [HarmonyPatch(typeof(InspectPaneFiller))]
        [HarmonyPatch("DrawInspectStringFor")]
        public static class Patch_InspectPaneFiller_DrawInspectStringFor
        {
            public static MethodInfo MI_InspectStringPartsFromComps = typeof(ThingWithComps).GetMethod("InspectStringPartsFromComps", allFlags);
            public static MethodInfo MI_DrawInspectString = typeof(InspectPaneFiller).GetMethod("DrawInspectString", allFlags);
            static string InspectStringPartsFromComps(ThingWithComps instance)
            {
                return (string)MI_InspectStringPartsFromComps.Invoke(instance, null);
            }
            static void DrawInspectString(InspectPaneFiller instance, string str, Rect rect)
            {
                MI_DrawInspectString.Invoke(instance, new object[] { str, rect });
            }
            public static string GetInspectString(Pawn pawn)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(pawn.MainDesc(true));
                string text = null;
                Lord lord = pawn.GetLord();
                if (lord != null && lord.LordJob != null)
                {
                    text = lord.LordJob.GetReport(pawn);
                }
                if (pawn.jobs.curJob != null)
                {
                    try
                    {
                        string text2 = pawn.jobs.curDriver.GetReport().CapitalizeFirst();
                        if (!text.NullOrEmpty())
                        {
                            text = text + ": " + text2;
                        }
                        else
                        {
                            text = text2;
                        }
                    }
                    catch (Exception arg)
                    {
                        Log.Error("JobDriver.GetReport() exception: " + arg);
                    }
                }
                if (!text.NullOrEmpty())
                {
                    stringBuilder.AppendLine(text);
                }
                if (pawn is ThingWithComps thingWithComps)
                {
                    stringBuilder.Append(InspectStringPartsFromComps(thingWithComps));
                }
                return stringBuilder.ToString().TrimEndNewlines();
            }
            static bool Prefix(InspectPaneFiller __instance, ISelectable sel, Rect rect)
            {
                Pawn pawn = sel as Pawn;
                if (pawn != null)
                {
                    if (pawn.kindDef == RimWorld.PawnKindDefOf.Megascarab || pawn.kindDef == RimWorld.PawnKindDefOf.Spelopede || pawn.kindDef == RimWorld.PawnKindDefOf.Megaspider || pawn.kindDef == PawnKindDefOf.BI_Queen)
                    {
                        string text;
                        try
                        {
                            text = GetInspectString(pawn);
                            Thing thing = sel as Thing;
                            if (thing != null)
                            {
                                string inspectStringLowPriority = thing.GetInspectStringLowPriority();
                                if (!inspectStringLowPriority.NullOrEmpty())
                                {
                                    if (!text.NullOrEmpty())
                                    {
                                        text = text.TrimEndNewlines() + "\n";
                                    }
                                    text += inspectStringLowPriority;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            text = "GetInspectString exception on " + sel.ToString() + ":\n" + ex.ToString();
                            Log.Error(text);
                        }
                        if (!text.NullOrEmpty() && GenText.ContainsEmptyLines(text))
                        {
                            Log.ErrorOnce($"Inspect string for {sel} contains empty lines.\n\nSTART\n{text}\nEND", 837163521);
                        }
                        DrawInspectString(__instance, text, rect);
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Pawn_HealthTracker))]
        [HarmonyPatch("PostApplyDamage")]
        public static class Patch_Pawn_HealthTracker_PreApplyDamage
        {
            static FieldInfo FI_pawn = typeof(Pawn_HealthTracker).GetField("pawn", allFlags);
            static Pawn pawn(Pawn_HealthTracker instance)
            {
                return (Pawn)FI_pawn.GetValue(instance);
            }
            static void Postfix(Pawn_HealthTracker __instance, DamageInfo dinfo)
            {
                Pawn p = pawn(__instance) as Pawn;
                if (p != null && p.Spawned)
                {
                    if (p.kindDef == RimWorld.PawnKindDefOf.Megascarab || p.kindDef == RimWorld.PawnKindDefOf.Spelopede || p.kindDef == RimWorld.PawnKindDefOf.Megaspider || p.kindDef == PawnKindDefOf.BI_Queen)
                    {
                        if (!p.Downed && !p.IsBurning() && p.jobs != null && p.jobs.curJob != null && p.jobs.curJob.def != RimWorld.JobDefOf.AttackMelee && !HiveUtility.JobsGivenRecentTick(p, "AttackMelee"))
                        {
                            Thing thing = dinfo.Instigator;
                            if (thing != null && thing.Position != null)
                            {
                                Job job = new Job(RimWorld.JobDefOf.AttackMelee, thing);
                                job.canBashDoors = true;
                                job.canBashFences = true;
                                job.attackDoorIfTargetLost = true;
                                p.jobs.StartJob(job, JobCondition.InterruptForced);
                                HiveUtility.CallReinforcements(p, thing);
                                Hive hive = HiveUtility.GetHive(p);
                                if (hive != null)
                                {
                                    HiveUtility.SetPatrolSpot(p, hive, thing.Position, LocomotionUrgency.Sprint);
                                }
                            }
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Site))]
        [HarmonyPatch("ShouldRemoveMapNow")]
        public static class Patch_Site_ShouldRemoveMapNow
        {
            static bool Prefix(Site __instance, ref bool __result)
            {
                if (__instance.def.defName == "BI_InfestationWorldObject" && __instance.Faction != null && __instance.Map != null && __instance.Faction == Faction.OfInsects && !ExterminateBugsComp.IsDefeated(__instance.Map, Faction.OfInsects))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Site))]
        [HarmonyPatch("CheckAllEnemiesDefeated")]
        public static class Patch_Site_CheckAllEnemiesDefeated
        {
            static bool Prefix(Site __instance)
            {
                if (__instance.def.defName == "BI_InfestationWorldObject" && __instance.Faction != null && __instance.Map != null && __instance.Faction == Faction.OfInsects && !ExterminateBugsComp.IsDefeated(__instance.Map, Faction.OfInsects))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(FactionGenerator))]
        [HarmonyPatch("GenerateFactionsIntoWorld")]
        public static class Patch_FactionGenerator_GenerateFactionsIntoWorld
        {
            public static bool RandomInfestationTile(out int index, Predicate<int> extraValidator = null)
            {
                for (int i = 0; i < 500; i++)
                {
                    if ((from _ in Enumerable.Range(0, 100)
                         select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                         {
                             Tile tile = Find.WorldGrid[x];
                             if (!tile.biome.canBuildBase || !tile.biome.implemented || tile.hilliness == Hilliness.Impassable || GenTemperature.MinTemperatureAtTile(x) < -17f)
                             {
                                 return 0f;
                             }
                             return (extraValidator != null && !extraValidator(x)) ? 0f : tile.biome.settlementSelectionWeight;
                         }, out int tileIndex) && TileFinder.IsValidTileForNewSettlement(tileIndex))
                    {
                        index = tileIndex;
                        return true;
                    }
                }
                index = 0;
                return false;
            }
            static void Postfix()
            {
                for (int i = 0; i < Rand.Range(8, 12); i++)
                {
                    int index;
                    if (RandomInfestationTile(out index, (int x) => Find.World.HasCaves(x)))
                    {
                        Site site = (Site)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("BI_InfestationWorldObject"));
                        site.Tile = index;
                        site.parts.Add(new SitePart(site, DefDatabase<SitePartDef>.GetNamed("BI_Infestation_SitePart"), DefDatabase<SitePartDef>.GetNamed("BI_Infestation_SitePart").Worker.GenerateDefaultParams(0f, site.Tile, Faction.OfInsects)));
                        site.SetFaction(Faction.OfInsects);
                        Find.WorldObjects.Add(site);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Lord))]
        [HarmonyPatch("CanExistWithoutPawns", MethodType.Getter)]
        public static class Patch_Lord_CanExistWithoutPawns
        {
            static bool Prefix(ref bool __result, Lord __instance)
            {
                Lord lord = __instance;
                if (lord.LordJob != null)
                {
                    if ((lord.LordJob.GetType() == typeof(LordJob_DefendAndExpandHive)) || (lord.LordJob.GetType() == typeof(LordJob_HiveHunters)))
                    {
                        Hive hive = HiveUtility.GetLordHive(lord);
                        if (hive != null)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Site))]
        [HarmonyPatch("PostMapGenerate")]
        public static class Patch_Site_PostMapGenerate
        {
            static FieldInfo FI_allEnemiesDefeatedSignalSent = typeof(Site).GetField("allEnemiesDefeatedSignalSent", allFlags);
            static bool Prefix(Site __instance)
            {
                Site site = __instance;
                if (site != null && site.Map != null)
                {
                    if (site.Map.ParentFaction == Faction.OfInsects)
                    {
                        MapParent mapParent = site.Map.Parent;
                        if (mapParent != null)
                        {
                            List<WorldObjectComp> allComps = mapParent.AllComps;
                            for (int i = 0; i < allComps.Count; i++)
                            {
                                allComps[i].PostMapGenerate();
                            }
                            Map map = mapParent.Map;
                            for (int i = 0; i < __instance.parts.Count; i++)
                            {
                                __instance.parts[i].def.Worker.PostMapGenerate(map);
                            }
                            FI_allEnemiesDefeatedSignalSent.SetValue(__instance, false);
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}
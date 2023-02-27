using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using FactionBaseGeneration;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RadWorld
{
    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor))]
    public static class RandomSettlementTileFor_Patch
    {
        public static Faction curFaction;
        public static void Prefix(Faction faction, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
        {
            curFaction = faction;
        }
        public static void Postfix(Faction faction, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
        {
            curFaction = null;
        }
    }
    
    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.IsValidTileForNewSettlement))]
    public static class IsValidTileForNewSettlement_Patch
    {
        public static bool mustBeOnCavern;
        public static void Postfix(ref bool __result, int tile, StringBuilder reason = null)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (mustBeOnCavern)
            {
                __result = RW_Utils.IsCavernBiome(tile2.biome);
                return;
            }
            if (RandomSettlementTileFor_Patch.curFaction != null)
            {
                if (RandomSettlementTileFor_Patch.curFaction.def != RW_DefOf.RW_MutantRough)
                {
                    if (!RW_Utils.IsCavernBiome(tile2.biome))
                    {
                        __result = false;
                    }
                }
                else
                {
                    if (RW_Utils.IsCavernBiome(tile2.biome))
                    {
                        __result = false;
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(World), nameof(World.HasCaves))]
    public static class HasCaves_Patch
    {
        public static void Postfix(ref bool __result, int tile)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (RW_Utils.IsCavernBiome(tile2.biome))
            {
                __result = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(WITab_Terrain), "FillTab")]
    public static class FillTab_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var list = instr.ToList();
            bool found = false;
            for (var i = 0; i < list.Count; i++)
            {
                var inst = list[i];
                yield return inst;
                if (!found && inst.opcode == OpCodes.Callvirt && list[i - 10].operand is String strOperand && strOperand == "Rainfall")
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_3, null);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FillTab_Patch), "AddNuclearFalloutInfo", null, null));
    
                }
            }
        }
    
        private static void AddNuclearFalloutInfo(Listing_Standard listing_Standard, Tile tile)
        {
            var options = tile.biome.GetModExtension<BiomeOptions>();
            if (options != null)
            {
                listing_Standard.LabelDouble("RW.NuclearFalloutModifier".Translate(), options.nuclearFalloutModifier.ToString(), null);
            }
        }
    }
    
    
    
    [HarmonyPatch(typeof(Caravan), nameof(Caravan.Tick))]
    public static class MapParent_CaravanTick_Patch
    {
        public static void Postfix(Caravan __instance)
        {
            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                var pawns = __instance.PawnsListForReading;
                for (int num = pawns.Count - 1; num >= 0; num--)
                {
                    DoPawnToxicDamage(__instance, pawns[num]);
                }
            }
        }
        public static void DoPawnToxicDamage(Caravan caravan, Pawn p)
        {
            if (p.RaceProps.IsFlesh)
            {
                float num = 0.028758334f;
                num *= caravan.Biome.GetNuclearModifier();
                var resistance = p.GetRadiationImpactMultiplier();
                num *= resistance;
                if (num != 0f)
                {
                    float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 0x46EDC5D));
                    num *= num2;
                    if (RadWorldMod.settings.disableCustomRadiationSystem)
                    {
                        HealthUtility.AdjustSeverity(p, HediffDefOf.ToxicBuildup, num);
                    }
                    else
                    {
                        HealthUtility.AdjustSeverity(p, RW_DefOf.RW_Radiation, num);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Map), "FinalizeInit")]
    public static class MapGeneratedPatch
    {
        public static void Postfix(Map __instance)
        {
            if (__instance.Biome.GetNuclearModifier() != 0f && !__instance.gameConditionManager.ConditionIsActive(RW_DefOf.RW_ToxicFallout))
            {
                GameCondition_ToxicFalloutRadWorld gameCondition = (GameCondition_ToxicFalloutRadWorld)GameConditionMaker.MakeConditionPermanent(RW_DefOf.RW_ToxicFallout);
                __instance.gameConditionManager.RegisterCondition(gameCondition);
            }
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), "FindNearEdgeCell")]
    public static class FindNearEdgeCellPatch
    {
        public static bool Prefix(ref IntVec3 __result, Map map, Predicate<IntVec3> extraCellValidator)
        {
            if (map.Biome.IsCavernBiome())
            {
                Predicate<IntVec3> baseValidator = (IntVec3 x) => x.Standable(map);// && !x.Fogged(map);
                Faction hostFaction = map.ParentFaction;
                if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => baseValidator(x) && (extraCellValidator == null || extraCellValidator(x))
                && ((hostFaction != null && map.reachability.CanReachFactionBase(x, hostFaction)) || (hostFaction == null && map.reachability.CanReachBiggestMapEdgeDistrict(x))), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 result))
                {
                    __result = CellFinder.RandomClosewalkCellNear(result, map, 5);
                    map.GetComponent<MapComponentGeneration>().ReFog = true;
                    return false;
                }
                if (extraCellValidator != null && CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => baseValidator(x) && extraCellValidator(x), map, CellFinder.EdgeRoadChance_Neutral, out result))
                {
                    __result = CellFinder.RandomClosewalkCellNear(result, map, 5);
                    map.GetComponent<MapComponentGeneration>().ReFog = true;
                    return false;

                }
                if (CellFinder.TryFindRandomEdgeCellWith(baseValidator, map, CellFinder.EdgeRoadChance_Neutral, out result))
                {
                    __result = CellFinder.RandomClosewalkCellNear(result, map, 5);
                    map.GetComponent<MapComponentGeneration>().ReFog = true;
                    return false;

                }
                Log.Warning("Could not find any valid edge cell.");
                __result = CellFinder.RandomCell(map);
                map.GetComponent<MapComponentGeneration>().ReFog = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BiomeDef), "IsPackAnimalAllowed")]
    public static class Patch_IsPackAnimalAllowed
    {
        public static void Postfix(BiomeDef __instance, ref bool __result, ThingDef pawn)
        {
            if (!__instance.IsCavernBiome() && pawn != RW_DefOf.RW_IrradiatedBear)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(BiomeDef), "AllWildAnimals", MethodType.Getter)]
    public static class AnimalRemoval
    {
        public static void Postfix(BiomeDef __instance, ref IEnumerable<PawnKindDef> __result)
        {
            if (!__instance.IsCavernBiome() && !RadWorldMod.settings.disableNonRadiatedAnimalRemoval)
            {
                __result = __result.ToList().Where(x => x.race?.modContentPack?.PackageId == RW_Utils.ModPackageId);
            }
        }
    }

    [HarmonyPatch(typeof(MapParent), nameof(MapParent.MapGeneratorDef), MethodType.Getter)]
    public static class MapParent_MapGeneratorDef_Patch
    {
        public static void Postfix(MapParent __instance, ref MapGeneratorDef __result)
        {
            Tile tile2 = Find.WorldGrid[__instance.Tile];
            if (RW_Utils.IsCavernBiome(tile2.biome))
            {
                if (__instance.def.mapGenerator == null)
                {
                    __result = RW_DefOf.RW_CavernEncounter;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.MapGeneratorDef), MethodType.Getter)]
    public static class Settlement_MapGeneratorDef_Patch
    {
        public static void Postfix(Settlement __instance, ref MapGeneratorDef __result)
        {
            Tile tile2 = Find.WorldGrid[__instance.Tile];
            if (RW_Utils.IsCavernBiome(tile2.biome))
            {
                if (__instance.Faction == Faction.OfPlayer)
                {
                    __result = RW_DefOf.RW_CavernBase_Player;
                }
                else
                {
                    __result = RW_DefOf.RW_CavernBase_Faction;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(GenStep_Settlement), "CanScatterAt")]
    public static class Settlement_CanScatterAt_Patch
    {
        private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);
        public static void Postfix(ref bool __result, IntVec3 c, Map map)
        {
            if (!__result)
            {
                __result = CanScatterAt(c, map);
            }
        }
    
        private static bool CanScatterAt(IntVec3 c, Map map)
        {
            //if (!base.CanScatterAt(c, map))
            //{
            //    return false;
            //}
            if (!c.Standable(map))
            {
                return false;
            }
            //if (c.Roofed(map))
            //{
            //    return false;
            //}
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
            {
                return false;
            }
            int min = SettlementSizeRange.min;
            if (!new CellRect(c.x - min / 2, c.z - min / 2, min, min).FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RoyalTitlePermitWorker_CallShuttle), "GetReportFromCell")]
    public static class GetReportFromCellPatch
    {
        public static bool Prefix(ref string __result, IntVec3 cell, Map map)
        {
            if (map.Biome.IsCavernBiome())
            {
                __result = GetReportFromCell(cell, map);
                return false;
            }
            return true;
        }

        private static string GetReportFromCell(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map))
            {
                return "OutOfBounds".Translate().CapitalizeFirst();
            }
            if (cell.Fogged(map))
            {
                return "ShuttleCannotLand_Fogged".Translate().CapitalizeFirst();
            }
            if (!cell.Walkable(map))
            {
                return "ShuttleCannotLand_Unwalkable".Translate().CapitalizeFirst();
            }
            List<Thing> thingList = cell.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing is IActiveDropPod || thing is Skyfaller || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building)
                {
                    return "BlockedBy".Translate(thing).CapitalizeFirst();
                }
                PlantProperties plant = thing.def.plant;
                if (plant != null && plant.IsTree)
                {
                    return "BlockedBy".Translate(thing).CapitalizeFirst();
                }
            }
            return null;
        }
    }
}

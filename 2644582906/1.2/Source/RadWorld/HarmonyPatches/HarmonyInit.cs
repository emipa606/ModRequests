using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RadWorld
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("Declinedkilr.RadWorld");
            harmony.PatchAll();
            var prefix = AccessTools.Method(typeof(HarmonyPatches), "Prefix");
            var postfix = AccessTools.Method(typeof(HarmonyPatches), "Postfix");

            foreach (var type in typeof(PawnGroupKindWorker).AllSubclassesNonAbstract())
            {
                var methodToPatch = AccessTools.Method(type, "GeneratePawns", new Type[] { typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(List<Pawn>), typeof(bool) });
                if (methodToPatch != null)
                {
                    harmony.Patch(methodToPatch, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                }
                else
                {
                    Log.Error("Can't patch " + type);
                }
            }
        }
        private static void Prefix()
        {
            Patch_GenerateStartingApparelFor.generateRadProtectiveGear = true;
        }

        private static void Postfix()
        {
            Patch_GenerateStartingApparelFor.generateRadProtectiveGear = false;
        }
    }

    [HarmonyPatch(typeof(RCellFinder), "TryFindRandomPawnEntryCell")]
    internal static class Patch_TryFindRandomPawnEntryCell
    {
        private static bool Prefix(ref bool __result, ref IntVec3 result, Map map, float roadChance, bool allowFogged = false, Predicate<IntVec3> extraValidator = null)
        {
            if (map.Biome.IsCavernBiome())
            {
                __result = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && map.reachability.CanReachColony(c)
                    && c.GetRoom(map).TouchesMapEdge && (allowFogged || !c.Fogged(map)) && (extraValidator == null || extraValidator(c)), map, roadChance, out result);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DropCellFinder), "RandomDropSpot")]
    internal static class Patch_RandomDropSpot
    {
        private static bool Prefix(ref IntVec3 __result, Map map)
        {
            if (map.Biome.IsCavernBiome())
            {
                __result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && !c.Fogged(map), map);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StartingPawnUtility), "NewGeneratedStartingPawn")]
    internal static class Patch_NewGeneratedStartingPawn
    {
        public static bool generationIsActive;
        private static void Prefix()
        {
            generationIsActive = true;
        }
        private static void Postfix()
        {
            generationIsActive = false;
        }
    }

    [HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
    internal static class Patch_GenerateStartingApparelFor
    {
        public static bool generateRadProtectiveGear;
        private static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (request.Tile != -1 || Patch_NewGeneratedStartingPawn.generationIsActive)
            {
                if (request.Tile != -1)
                {
                    Tile tile = Find.WorldGrid[request.Tile];
                    if (tile.biome != null && tile.biome.GetNuclearModifier() <= 0 || pawn.Faction.def != RW_DefOf.RW_VaultRough)
                    {
                        return;
                    }
                }
                if (pawn.Faction != null && pawn.Faction.def != RW_DefOf.RW_MutantRough && pawn.apparel != null 
                    && (generateRadProtectiveGear || Patch_NewGeneratedStartingPawn.generationIsActive && Rand.Chance(0.3f)))
                {
                    var allApparelPairs = ThingStuffPair.AllWith((ThingDef td) => td.IsApparel && (int)pawn.Faction.def.techLevel >= (int)td.techLevel && td.techLevel < TechLevel.Archotech 
                        && td.GetStatValueAbstract(RW_DefOf.RW_RadiationResistanceOffset) > 0);
                    var hats = allApparelPairs.Where(x => IsHeadgear(x.thing) && CanUseStuff(pawn, x) && x.thing.apparel.CorrectGenderForWearing(pawn.gender));

                    if (hats.TryRandomElementByWeight(pa => pa.Commonality, out var hatPair))
                    {
                        var hat = ThingMaker.MakeThing(hatPair.thing, hatPair.stuff) as Apparel;
                        pawn.apparel.Wear(hat, false);
                        var comp = hat.TryGetComp<CompGasMaskReloadable>();
                        if (comp != null)
                        {
                            var randomFilterCount = Rand.RangeInclusive(1, comp.Props.maxCharges);
                            for (var i = 0; i < randomFilterCount; i++)
                            {
                                var fuel = ThingMaker.MakeThing(comp.Props.ammoDef);
                                comp.ReloadFrom(fuel);
                            }
                        }
                    }

                    var apparels = allApparelPairs.Where(x => !hats.Any(y => x.thing == y.thing) && CanUseStuff(pawn, x) && x.thing.apparel.CorrectGenderForWearing(pawn.gender));
                    if (apparels.TryRandomElementByWeight(pa => pa.Commonality, out var apparelPair))
                    {
                        var apparel = ThingMaker.MakeThing(apparelPair.thing, apparelPair.stuff) as Apparel;
                        pawn.apparel.Wear(apparel, false);
                    }
                }
            }

        }
        public static bool IsHeadgear(ThingDef td)
        {
            if (!td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
            {
                return td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead);
            }
            return true;
        }
        private static bool CanUseStuff(Pawn pawn, ThingStuffPair pair)
        {
            if (pair.stuff != null && pawn.Faction != null && !pawn.Faction.def.CanUseStuffForApparel(pair.stuff))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan), "TryFormAndSendCaravan")]
    internal static class Patch_TryFormAndSendCaravan
    {
        public static bool considerOutdoor;
		private static void Prefix()
        {
            considerOutdoor = true;
        }
        private static void Postfix()
        {
            considerOutdoor = false;
        }
    }

    [HarmonyPatch(typeof(Room), "PsychologicallyOutdoors", MethodType.Getter)]
    internal static class Patch_PsychologicallyOutdoors
    {
        private static void Postfix(ref bool __result)
        {
            if (Patch_TryFormAndSendCaravan.considerOutdoor)
            {
                __result = true;
            }
        }
    }
}

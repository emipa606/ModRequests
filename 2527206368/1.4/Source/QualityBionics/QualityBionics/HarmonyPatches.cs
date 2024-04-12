using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Verse.PawnCapacityUtility;

//There was an issue where mods that had multiple Hediffs with the same Implant could get into an infinite loop that added Efficiency until it
//reached the Overflow. I prevent this by changing all (Efficiency *= QualityMultiplier) calculations to (Efficiency = baseEfficiency * QualityMultiplier).
//The base efficiency is added in the HediffCompQualityBionics.cs
//If you have any questions, you can reach me on Discord under StatistNo1#2157
//
// - StatistNo1

namespace QualityBionics
{
    [StaticConstructorOnStartup]
    internal static class HarmonyContainer
    {
        public static Harmony harmony;

        private static HashSet<string> customHediffDefs = new HashSet<string>
        {
            "synthetic",
            "cybernetic",
        };
        static HarmonyContainer()
        {
            harmony = new Harmony("QualityBionics.Mod");
            harmony.PatchAll();
            var prefix = typeof(HarmonyContainer).GetMethod("ApplyOnPawnPrefix");
            var postfix = typeof(HarmonyContainer).GetMethod("ApplyOnPawnPostfix");
            var baseType = typeof(RecipeWorker);
            var types = baseType.AllSubclassesNonAbstract();
            foreach (Type cur in types)
            {
                var method = cur.GetMethod("ApplyOnPawn");
                try
                {
                    harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                }
                catch (Exception ex)
                {
                    //Log.Error("Error patching " + cur + " - " + method + " - " + ex);
                }
            }
            AddQualityToImplants();
        }
        public static void AddQualityToImplants()
        {
            foreach (var hediff in DefDatabase<HediffDef>.AllDefs)
            {
                //if (hediff.spawnThingOnRemoved != null && hediff.spawnThingOnRemoved.isTechHediff)
                if (hediff.spawnThingOnRemoved != null && hediff.spawnThingOnRemoved.isTechHediff && hediff.addedPartProps != null) //new - Changed so only Hediffs with Effifiencies get the quality added.
                {
                    var defName = hediff.defName.ToLower();
                    if (defName.Contains("bionic") || defName.Contains("archotech") || customHediffDefs.Contains(hediff.defName) 
                            || hediff.spawnThingOnRemoved.techLevel >= QualityBionicsMod.settings.minTechLevelForQuality)
                    {
                        if (hediff.comps is null)
                        {
                            hediff.comps = new List<HediffCompProperties>();
                        }
                        //hediff.comps.Add(new HediffCompProperties_QualityBionics());
                        hediff.comps.Add(new HediffCompProperties_QualityBionics() {baseEfficiency = hediff.addedPartProps.partEfficiency}); //new - Added the base Efficiency to the Property to calculate from.
                        if (hediff.spawnThingOnRemoved.comps is null)
                        {
                            hediff.spawnThingOnRemoved.comps = new List<CompProperties>();
                        }
                        if (!hediff.spawnThingOnRemoved.comps.Any(x => x.compClass == typeof(CompQuality)))
                        {
                            hediff.spawnThingOnRemoved.comps.Add(new CompProperties { compClass = typeof(CompQuality) });
                        }
                    }
                }
            }
        }

        public static Pair<ThingDef, QualityCategory>? thingWithQuality;
        public static void ApplyOnPawnPrefix(RecipeWorker __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (__instance.recipe?.removesHediff != null)
            {
                if (!pawn.health?.hediffSet?.GetNotMissingParts().Contains(part) ?? false)
                {
                    return;
                }
                Hediff hediff = pawn.health?.hediffSet?.hediffs?.FirstOrDefault((Hediff x) => x.def == __instance.recipe.removesHediff);
                if (hediff != null)
                {
                    if (hediff.def.spawnThingOnRemoved != null)
                    {
                        var comp = hediff.TryGetComp<HediffCompQualityBionics>();
                        if (comp != null)
                        {
                            thingWithQuality = new Pair<ThingDef, QualityCategory>(hediff.def.spawnThingOnRemoved, comp.quality);
                        }
                    }
                }
            }

        }
        public static void ApplyOnPawnPostfix(RecipeWorker __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            thingWithQuality = null;
            if (__instance.recipe?.addsHediff != null && ingredients != null)
            {
                var hediff = pawn.health?.hediffSet?.hediffs?.FindLast(x => x.def == __instance.recipe.addsHediff);
                if (hediff != null)
                {
                    var comp = hediff.TryGetComp<HediffCompQualityBionics>();
                    if (comp != null)
                    {
                        foreach (var ingredient in ingredients)
                        {
                            if (ingredient != null && hediff.def.spawnThingOnRemoved == ingredient.def && ingredient.TryGetQuality(out var qualityCategory))
                            {
                                comp.quality = qualityCategory;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MedicalRecipesUtility), "SpawnThingsFromHediffs")]
    public class SpawnThingsFromHediffs_Patch
    {
        public static List<Pair<ThingDef, QualityCategory>?> thingsWithQualities = new List<Pair<ThingDef, QualityCategory>?>();
        private static void Prefix(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
        {
            if (pawn.health.hediffSet.GetNotMissingParts().Contains(part))
            {
                foreach (Hediff item in pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part))
                {
                    if (item.def.spawnThingOnRemoved != null)
                    {
                        var comp = item.TryGetComp<HediffCompQualityBionics>();
                        if (comp != null)
                        {
                            if (thingsWithQualities is null)
                            {
                                thingsWithQualities = new List<Pair<ThingDef, QualityCategory>?>();
                            }
                            thingsWithQualities.Add(new Pair<ThingDef, QualityCategory>(item.def.spawnThingOnRemoved, comp.quality));
                        }
                    }
                }
            }
        }
        private static void Postfix(Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
        {
            thingsWithQualities = null;
        }
    }

    [HarmonyPatch(typeof(ThingWithComps), "SpawnSetup")]
    public class SpawnSetup_Patch
    {
        private static void Postfix(ThingWithComps __instance)
        {
            if (HarmonyContainer.thingWithQuality.HasValue && __instance.def == HarmonyContainer.thingWithQuality.Value.First)
            {
                var comp = __instance.TryGetComp<CompQuality>();
                if (comp != null)
                {
                    comp.SetQuality(HarmonyContainer.thingWithQuality.Value.Second, ArtGenerationContext.Colony);
                    HarmonyContainer.thingWithQuality = null;
                }
            }
            if (SpawnThingsFromHediffs_Patch.thingsWithQualities != null)
            {
                var pair = SpawnThingsFromHediffs_Patch.thingsWithQualities.FirstOrDefault(x => x.Value.First == __instance.def);
                if (pair.HasValue)
                {
                    var comp = __instance.TryGetComp<CompQuality>();
                    if (comp != null)
                    {
                        comp.SetQuality(pair.Value.Second, ArtGenerationContext.Colony);
                        SpawnThingsFromHediffs_Patch.thingsWithQualities.Remove(pair);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Hediff_AddedPart), "TipStringExtra", MethodType.Getter)]
    public class TipStringExtra_Patch
    {
        private static void Prefix(Hediff_AddedPart __instance, out float? __state)
        {
            __state = null;
            if (__instance.def.addedPartProps != null)
            {
                var comp = __instance.TryGetComp<HediffCompQualityBionics>();
                if (comp != null)
                {
                    __state = __instance.def.addedPartProps.partEfficiency;
                    //__instance.def.addedPartProps.partEfficiency *= QualityBionicsMod.settings.GetQualityMultipliers(comp.quality);
                    __instance.def.addedPartProps.partEfficiency = comp.Props.baseEfficiency * QualityBionicsMod.settings.GetQualityMultipliers(comp.quality); //new - Changed the calculation to prevent infinite loops.
                }
            }
        }

        private static void Postfix(Hediff_AddedPart __instance, float? __state)
        {
            if (__state.HasValue)
            {
                __instance.def.addedPartProps.partEfficiency = __state.Value;
            }
        }
    }
    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    public class GetMaxHealth_Patch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.Part?.def == __instance)
                {
                    var comp = hediff.TryGetComp<HediffCompQualityBionics>();
                    if (comp != null)
                    {
                        __result *= QualityBionicsMod.settings.GetQualityMultipliersForHP(comp.quality);
                        __result = (int)__result;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculatePartEfficiency")]
    public class CalculatePartEfficiency_Patch
    {
        private static void Prefix(out Tuple<Hediff, float, float> __state, HediffSet diffSet, BodyPartRecord part, bool ignoreAddedParts = false, List<CapacityImpactor> impactors = null)
        {
            __state = null;
            BodyPartRecord rec;
            for (rec = part.parent; rec != null; rec = rec.parent)
            {
                if (diffSet.HasDirectlyAddedPartFor(rec))
                {
                    List<Hediff_AddedPart> a = new List<Hediff_AddedPart>();
                    diffSet.GetHediffs<Hediff_AddedPart>(ref a);

                    Hediff_AddedPart hediff_AddedPart = (from x in a where x.Part == rec select x).First();

                    if (hediff_AddedPart != null)
                    {
                        if (hediff_AddedPart.def.addedPartProps != null)
                        {
                            var comp = hediff_AddedPart.TryGetComp<HediffCompQualityBionics>();
                            // Instead traverse to first non-normal parent
                            if (comp != null && comp.quality != QualityCategory.Normal)
                            {
                                __state = new Tuple<Hediff, float, float>(hediff_AddedPart, hediff_AddedPart.def.addedPartProps.partEfficiency, QualityBionicsMod.settings.GetQualityMultipliers(comp.quality));
                                //hediff_AddedPart.def.addedPartProps.partEfficiency *= QualityBionicsMod.settings.GetQualityMultipliers(comp.quality);
                                hediff_AddedPart.def.addedPartProps.partEfficiency = comp.Props.baseEfficiency * QualityBionicsMod.settings.GetQualityMultipliers(comp.quality); //new - Changed the calculation to prevent infinite loops.
                                return;
                            }
                        }
                    }
                }
            }
            if (part.parent != null && diffSet.PartIsMissing(part.parent))
            {
                return;
            }
            if (!ignoreAddedParts)
            {
                for (int i = 0; i < diffSet.hediffs.Count; i++)
                {
                    Hediff_AddedPart hediff_AddedPart2 = diffSet.hediffs[i] as Hediff_AddedPart;
                    if (hediff_AddedPart2 != null && hediff_AddedPart2.Part == part)
                    {
                        if (hediff_AddedPart2 != null)
                        {
                            if (hediff_AddedPart2.def.addedPartProps != null)
                            {
                                var comp = hediff_AddedPart2.TryGetComp<HediffCompQualityBionics>();
                                if (comp != null)
                                {
                                    __state = new Tuple<Hediff, float, float>(hediff_AddedPart2, hediff_AddedPart2.def.addedPartProps.partEfficiency, 1);
                                    //hediff_AddedPart2.def.addedPartProps.partEfficiency *= QualityBionicsMod.settings.GetQualityMultipliers(comp.quality);
                                    hediff_AddedPart2.def.addedPartProps.partEfficiency = comp.Props.baseEfficiency * QualityBionicsMod.settings.GetQualityMultipliers(comp.quality); //new - Changed the calculation to prevent infinite loops.
                                    return;
                                }
                            }
                        }
                        return;
                    }
                }
            }
        }

        private static void Postfix(Tuple<Hediff, float, float> __state, HediffSet diffSet, BodyPartRecord part, ref float __result, bool ignoreAddedParts = false, List<CapacityImpactor> impactors = null)
        {
            if (__state != null)
            {
                __state.Item1.def.addedPartProps.partEfficiency = __state.Item2;
                __result *= __state.Item3;
            }
        }
    }


    [HarmonyPatch(typeof(StatsReportUtility), "DrawStatsReport", new Type[] { typeof(Rect), typeof(Thing) })]
    public static class DrawStatsReport_Patch
    {
        private static void Prefix(Rect rect, Thing thing, out Pair<HediffDef, float>? __state)
        {
            __state = null;
            if (thing.def.isTechHediff)
            {
                if (thing.TryGetQuality(out var qc))
                {
                    IEnumerable<RecipeDef> enumerable = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.addsHediff != null && x.IsIngredient(thing.def));
                    foreach (RecipeDef item6 in enumerable)
                    {
                        HediffDef diff = item6.addsHediff;

                        if ((diff.comps?.Any(x => x?.GetType() == typeof(HediffCompProperties_QualityBionics)) ?? false) && diff.addedPartProps != null)
                        {
                            __state = new Pair<HediffDef, float>(diff, diff.addedPartProps.partEfficiency);
                            //diff.addedPartProps.partEfficiency *= QualityBionicsMod.settings.GetQualityMultipliers(qc);
                            diff.addedPartProps.partEfficiency = diff.comps.OfType<HediffCompProperties_QualityBionics>().First().baseEfficiency * QualityBionicsMod.settings.GetQualityMultipliers(qc); //new - Changed the calculation to prevent infinite loops.
                        }
                    }
                }

            }
        }

        private static void Postfix(Rect rect, Thing thing, Pair<HediffDef, float>? __state)
        {
            if (__state.HasValue)
            {
                __state.Value.First.addedPartProps.partEfficiency = __state.Value.Second;
            }
        }
    }
}

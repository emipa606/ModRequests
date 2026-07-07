using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GeneExtractorWeights;

[HarmonyPatch]
public static class GeneExtractorPatch
{
    private static readonly SimpleCurve GeneCountChanceCurve = new()
    {
        new CurvePoint(1f, 0.7f),
        new CurvePoint(2f, 0.2f),
        new CurvePoint(3f, 0.08f),
        new CurvePoint(4f, 0.02f)
    };

    private static GeneExtractorWeightsSettings GetSettings()
    {
        return LoadedModManager.GetMod<GeneExtractorWeights>().GetSettings<GeneExtractorWeightsSettings>();
    }

    private static bool Includes(this IntRange range, int value)
    {
        return range.min <= value && value <= range.max;
    }

    private static Pawn GetContainedPawn(Building_GeneExtractor extractor)
    {
        var info = typeof(Building_GeneExtractor).GetProperty("ContainedPawn",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (info is null) throw new InvalidOperationException("Could not find ContainedPawn property");
        return (Pawn)info.GetValue(extractor);
    }

    [HarmonyPatch(typeof(Building_GeneExtractor), "Finish")]
    [HarmonyPrefix]
    public static bool Prefix(Building_GeneExtractor __instance)
    {
        
        var containedPawn = GetContainedPawn(__instance);
        //typeof(Building_GeneExtractor).GetMethod("Cancel")?.Invoke(__instance, null);

        typeof(Building_GeneExtractor).GetField("startTick", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, -1);
        //__instance.startTick = -1;

        typeof(Building_GeneExtractor).GetField("selectedPawn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
        //__instance.selectedPawn = (Pawn)null;

        typeof(Building_GeneExtractor).GetField("sustainerWorking", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
        //__instance.sustainerWorking = (Sustainer)null;
        if (containedPawn == null)
            return false;

        var settings = GetSettings();
        settings.InitializeGenes();

        var genesToAdd = new List<GeneDef>();
        var genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
        var num = Mathf.Min((int)GeneCountChanceCurve.RandomElementByWeight(p => p.y).x,
            containedPawn.genes.GenesListForReading.Count(x => SelectionWeight(x) > 0));
        for (var index = 0; index < num && containedPawn.genes.GenesListForReading.TryRandomElementByWeight(SelectionWeight, out var result); ++index)
        {
            if (genesToAdd.Any(g => result.def.defName.Equals(g.defName))) //Pulled the same gene. Try again.
            {
                index--;
                continue;
            }

            genesToAdd.Add(result.def);
        }

        genepack.Initialize(genesToAdd);
        GeneUtility.ExtractXenogerm(containedPawn, Mathf.RoundToInt(60000f * settings.RegrowingDays.RandomInRange));
        var intVec3 = __instance.def.hasInteractionCell ? __instance.InteractionCell : __instance.Position;
        __instance.innerContainer.TryDropAll(intVec3, __instance.Map, ThingPlaceMode.Near);
        if (!containedPawn.Dead && (containedPawn.IsPrisonerOfColony || containedPawn.IsSlaveOfColony))
            containedPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.XenogermHarvested_Prisoner);
        GenPlace.TryPlaceThing(genepack, intVec3, __instance.Map, ThingPlaceMode.Near);
        Messages.Message(
            "GeneExtractionComplete".Translate(containedPawn.Named("PAWN")) + ": " +
            genesToAdd.Select(x => x.label).ToCommaList().CapitalizeFirst(),
            new LookTargets((TargetInfo)(Thing)containedPawn, (TargetInfo)(Thing)genepack),
            MessageTypeDefOf.PositiveEvent);

        float SelectionWeight(Gene g)
        {
            if (!settings.IgnoreMetabolismLimit &&
                !GeneTuning.BiostatRange.Includes(g.def.biostatMet + genesToAdd.Sum(x => x.biostatMet)))
                return 0.0f;
            var result = 0f;

            try
            {
                result = settings.GenesDictionary[g.def.defName].Weight;
            }
            catch (Exception e)
            {
                Log.Warning("Could not find gene " + g.def.defName + " in settings. Using default weight.");
            }

            return result;
        }

        return false;
    }

    [HarmonyPatch(typeof(Building_GeneExtractor), nameof(Building_GeneExtractor.CanAcceptPawn))]
    [HarmonyPrefix]
    public static bool CanAcceptPawn(ref AcceptanceReport __result, Building_GeneExtractor __instance, Pawn pawn, Pawn ___selectedPawn)
    {
        var settings = GetSettings();
        if(!settings._isInitialized)
            settings.InitializeGenes();
        
        //var selectedPawn = typeof(Building_GeneExtractor).GetField("selectedPawn", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(__instance);

        if (!pawn.IsColonist && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
        {
            __result = (AcceptanceReport)false;
            return false;
        }

        if (___selectedPawn != null && ___selectedPawn != pawn)
        {
            __result = (AcceptanceReport)false;
            return false;
        }

        if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
        {
            __result = (AcceptanceReport)false;
            return false;
        }

        if (!__instance.PowerOn)
        {
            __result = (AcceptanceReport)"NoPower".Translate().CapitalizeFirst();
            return false;
        }

        if (__instance.innerContainer.Count > 0)
        {
            __result = (AcceptanceReport)"Occupied".Translate();
            return false;
        }

        if (pawn.genes == null || !pawn.genes.GenesListForReading.Any<Gene>())
        {
            __result = (AcceptanceReport)"PawnHasNoGenes".Translate(pawn.Named("PAWN"));
            return false;
        }

        if (pawn.genes.GenesListForReading.Any<Gene>((Predicate<Gene>)(x =>
                settings.GenesDictionary[x.def.defName].Weight > 0)) != true)
        {
            __result = (AcceptanceReport)"PawnHasNoNonArchiteGenes".Translate(pawn.Named("PAWN"));
            return false;
        }
        __result = pawn.health.hediffSet.HasHediff(HediffDefOf.XenogerminationComa) ? (AcceptanceReport)"InXenogerminationComa".Translate() : (AcceptanceReport)true;
        return false;
    }

}
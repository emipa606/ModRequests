using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RadWorld
{
    [HarmonyPatch(typeof(Need_Food), nameof(Need_Food.MaxLevel), MethodType.Getter)]
    public static class MaxLevel_Patch
    {
        public static void Postfix(Pawn ___pawn, ref float __result)
        {
            var hediff = ___pawn.health.hediffSet.GetFirstHediffOfDef(RW_DefOf.RW_EnlargedStomach);
            if (hediff != null)
            {
                __result *= 1.3f;
            }
            else
            {
                hediff = ___pawn.health.hediffSet.GetFirstHediffOfDef(RW_DefOf.RW_ReducedStomach);
                if (hediff != null)
                {
                    __result /= 1.3f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Need_Outdoors), "Disabled", MethodType.Getter)]
    public static class DisabledPatch
    {
        public static void Postfix(ref bool __result, Need_Outdoors __instance, Pawn ___pawn)
        {
            if (___pawn.Map?.Biome.IsCavernBiome() ?? false)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateNewPawnInternal")]
    internal static class Patch_GenerateNewPawnInternal
    {
        private static void Postfix(ref Pawn __result)
        {
            if (__result.IsMutant())
            {
                if (!__result.story?.traits?.HasTrait(RW_DefOf.RW_MutantBlood) ?? false)
                {
                    __result.story.traits.GainTrait(new Trait(RW_DefOf.RW_MutantBlood));
                }
                if (__result.health.hediffSet.GetFirstHediffOfDef(RW_DefOf.RW_MutantBrain) is null)
                {
                    __result.health.AddHediff(RW_DefOf.RW_MutantBrain, __result.health.hediffSet.GetBrain());
                }
                if (__result.health.hediffSet.GetFirstHediffOfDef(RW_DefOf.RW_ScalySkin) is null)
                {
                    __result.health.AddHediff(RW_DefOf.RW_ScalySkin);
                }
                var randomMutationCount = Rand.RangeInclusive(2, 4);
                var hediffsPerBodyParts = RW_Utils.hediffsPerBodyParts.Where(x => x.Key != RW_DefOf.RW_ScalySkin).ToDictionary(x => x.Key, x => x.Value);
                while (randomMutationCount > 0)
                {
                    var hediffCandidates = RW_Utils.GetFreeHediffCandidatesFor(__result, hediffsPerBodyParts);
                    if (hediffCandidates.Any())
                    {
                        var randomHediff = hediffCandidates.RandomElement();
                        var part = randomHediff.Value;
                        if (randomHediff.Key == RW_DefOf.RW_RadiationBurnScar)
                        {
                            part = __result.RaceProps.body.AllParts.Where(x => x.depth == BodyPartDepth.Outside && x.coverageAbs > 0).RandomElement();
                        }
                        var hediff = HediffMaker.MakeHediff(randomHediff.Key, __result, part);
                        __result.health.AddHediff(hediff, part);
                        randomMutationCount--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                var trait = __result.story?.traits?.GetTrait(RW_DefOf.RW_MutantBlood);
                if (trait != null)
                {
                    __result.story.traits.allTraits.Remove(trait);
                }
            }
        }
    }
}

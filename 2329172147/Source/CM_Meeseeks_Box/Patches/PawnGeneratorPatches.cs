using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class PawnGeneratorPatches
    {
        [HarmonyPatch(typeof(PawnGenerator))]
        [HarmonyPatch("GenerateTraits", MethodType.Normal)]
        public static class PawnGenerator_GenerateTraits
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn)
            {
                if (pawn.kindDef == MeeseeksDefOf.MeeseeksKind)
                {
                    TraitDef meeseeksTrait = DefDatabase<TraitDef>.GetNamedSilentFail("CM_Meeseeks_Box_Trait_Meeseeks");
                    if (meeseeksTrait != null)
                        pawn.story.traits.GainTrait(new Trait(meeseeksTrait, 0, true));
                    TraitDef annoyingVoice = DefDatabase<TraitDef>.GetNamedSilentFail("AnnoyingVoice");
                    if (annoyingVoice != null)
                        pawn.story.traits.GainTrait(new Trait(annoyingVoice, 0, true));
                    return false;
                }

                return true;
            }

            [HarmonyPostfix]
            public static void Postfix(Pawn pawn)
            {
                if (pawn.kindDef != MeeseeksDefOf.MeeseeksKind && pawn.story.traits.HasTrait(MeeseeksDefOf.CM_Meeseeks_Box_Trait_Meeseeks))
                {
                    Logger.MessageFormat(pawn, "PawnGenerator_GenerateTraits patch - removing Meeseeks trait from a non-meeseks: " + pawn.Label);
                    pawn.story.traits.allTraits = pawn.story.traits.allTraits.Where(trait => trait.def != MeeseeksDefOf.CM_Meeseeks_Box_Trait_Meeseeks).ToList();
                }
            }
        }
    }
}

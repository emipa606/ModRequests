using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using TorannMagic;
using Verse;
using Verse.AI.Group;

namespace ARoM_SpaceTimeMage
{
    [StaticConstructorOnStartup]
    static partial class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        public static TraitDef TM_TimeFlayer = TraitDef.Named("TM_TimeFlayer");

        private static Pawn currentInnerPawn = null;

        static HarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony("rimworld.NetzachSloth.ARoM_SpaceTimeMage");

            harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Projectile_Resurrection), name: "ReduceSkillsOfPawn"),
                    prefix: new HarmonyMethod(patchType, nameof(prefix_ReduceSkillsOfPawn_ARoM_SpaceTimeMage)));

            harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Verb_ReverseTime), name: "AgeCorpse"),
                    prefix: new HarmonyMethod(patchType, nameof(prefix_AgeCorpse_ARoM_SpaceTimeMage)));

            harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Verb_ReverseTime), name: "AgeCorpse"),
                    postfix: new HarmonyMethod(patchType, nameof(postfix_AgeCorpse_ARoM_SpaceTimeMage)));
        }

        static bool prefix_ReduceSkillsOfPawn_ARoM_SpaceTimeMage(Verb_ReverseTime __instance, Pawn p)
        {
            if (p.story.traits.HasTrait(TM_TimeFlayer))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        static void prefix_AgeCorpse_ARoM_SpaceTimeMage(Verb_ReverseTime __instance, Thing thing)
        {
            currentInnerPawn = (thing as Corpse).InnerPawn;
        }

        static void postfix_AgeCorpse_ARoM_SpaceTimeMage(Verb_ReverseTime __instance)
        {
            Pawn CasterPawn = __instance.CasterPawn;

            if (CasterPawn.story.traits.HasTrait(TM_TimeFlayer))
            {
                if (CasterPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DeathReversalHD))
                {
                    CasterPawn.health.RemoveHediff(CasterPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_DeathReversalHD));
                }
            }

            if (currentInnerPawn.story.traits.HasTrait(TM_TimeFlayer))
            {
                if (currentInnerPawn.health.hediffSet.HasHediff(TorannMagicDefOf.TM_DeathReversalHD))
                {
                    currentInnerPawn.health.RemoveHediff(currentInnerPawn.health.hediffSet.GetFirstHediffOfDef(TorannMagicDefOf.TM_DeathReversalHD));
                }
            }

            currentInnerPawn = null;
        }

    }
}

using RimWorld;
using Verse;
using TorannMagic;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace A_RimWorld_of_Magic___Psionic_Lich
{
    [StaticConstructorOnStartup]
    public static class MyMod
    {
        static MyMod()
        {
            var harmonyInstance = new Harmony("rimworld.netzachsloth.psioniclich");

            harmonyInstance.PatchAll();
        }
    }

    [HarmonyPatch(typeof(CompAbilityUserMagic), "AssignAbilities")]
    public class AssignAbilitiesMagic_AddExtraClassTrait_Patch
    {
        static void Postfix(CompAbilityUserMagic __instance)
        {
            if (__instance.customClass != null
                && __instance.customClass.classTrait != null)
            {
                if (__instance.Pawn.story.traits.HasTrait(PsionicLichDefOf.TM_PsionicLich))
                {
                    List<Hediff> allHediffs = __instance.Pawn.health.hediffSet.GetHediffs<Hediff>().ToList();
                    bool isLich = false;
                    for (int i = 0; i < allHediffs.Count(); i++)
                    {
                        Hediff hediff = allHediffs[i];
                        if (hediff.def.defName.Equals(PsionicLichDefOf.TM_PsionicLichHediff.ToString()))
                        {
                            isLich = true;
                            break;
                        }
                    }
                    if (!isLich)
                    {
                        HealthUtility.AdjustSeverity(__instance.Pawn, PsionicLichDefOf.TM_PsionicLichHediff, 1f);
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(CompAbilityUserMight), "AssignAbilities")]
    public class AssignAbilitiesMight_AddExtraClassTrait_Patch
    {
        static void Postfix(CompAbilityUserMagic __instance)
        {
            if (__instance.customClass != null
                && __instance.customClass.classTrait != null)
            {
                if (__instance.Pawn.story.traits.HasTrait(PsionicLichDefOf.TM_PsionicLich))
                {
                    List<Hediff> allHediffs = __instance.Pawn.health.hediffSet.GetHediffs<Hediff>().ToList();
                    bool isLich = false;
                    for (int i = 0; i < allHediffs.Count(); i++)
                    {
                        Hediff hediff = allHediffs[i];
                        if (hediff.def.defName.Equals(PsionicLichDefOf.TM_PsionicLichHediff.ToString()))
                        {
                            isLich = true;
                            break;
                        }
                    }
                    if (!isLich)
                    {
                        HealthUtility.AdjustSeverity(__instance.Pawn, PsionicLichDefOf.TM_PsionicLichHediff, 1f);
                    }
                }
            }
        }
    }

    [DefOf]
    public static class PsionicLichDefOf
    {
        public static TraitDef TM_PsionicLich;
        public static HediffDef TM_PsionicLichHediff;
    }
}
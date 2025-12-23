
using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using HarmonyLib;
using VFECore.Abilities;
using VPEArchocaster;
using AbilityDef = VFECore.Abilities.AbilityDef;
using System.ComponentModel;

namespace VPEArchocaster
{

    [Category("Base")]
    [HarmonyPatch(typeof(AbilityExtension_Psycast))]
    [HarmonyPatch(nameof(AbilityExtension_Psycast.GetPsyfocusUsedByPawn))]

    public static class AbilityExtension_Psycast_GetStaticPsyfocusUsedByPawn_Patch
    {
        public static void Postfix(AbilityExtension_Psycast __instance, ref float __result)
        {

            AbilityExtension_TargetValidator extra_settings = __instance.abilityDef.GetModExtension<AbilityExtension_TargetValidator>();

            if (extra_settings == null)
            {
                return;
            }
            if (extra_settings.staticPsycost <= 0f)
            {
                return;
            }

            __result = extra_settings.staticPsycost;
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ShardMods
{
    [HarmonyPatch(typeof(Verb), "EffectiveRange", MethodType.Getter)]
    class RangePatch_EffectiveRange
    { 
        static void Postfix(Verb __instance, ref float __result)
        {
            //Verse.Log.Message($"postfix effectiverange i:{__instance} r:{__result} cp:{__instance.CasterPawn} t:{__instance.GetType().FullName}");
            if (!(__instance is Verb_CastPsycast || __instance.GetType().FullName == "CombatPsycasts.Verbs.Verb_PsychicShoot"))
                return;
            if (__instance.CasterPawn != null)
                __result *= __instance.CasterPawn.GetStatValue(StatDefOf.PsychicSensitivity);
        }
    }
}

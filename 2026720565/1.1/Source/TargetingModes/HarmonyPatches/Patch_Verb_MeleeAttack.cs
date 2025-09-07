using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TargetingModes
{

    public static class Patch_Verb_MeleeAttack
    {

        [HarmonyPatch(typeof(Verb_MeleeAttack))]
        [HarmonyPatch("GetNonMissChance")]
        public static class Patch_GetNonMissChance
        {

            public static void Postfix(Verb_MeleeAttack __instance, ref float __result)
            {
                if (typeof(Thing).Equals(__instance.caster))
                {
                    Thing caster = __instance.caster;
                    if (caster != null)
                    {
                        if (typeof(CompTargetingMode).Equals(caster.TryGetComp<CompTargetingMode>()))
                        {
                            CompTargetingMode targetingComp = caster.TryGetComp<CompTargetingMode>();
                            if (targetingComp != null && __result == caster.GetStatValue(StatDefOf.MeleeHitChance))
                            {
                                __result *= __result * targetingComp.GetTargetingMode().HitChanceFactor;
                            }
                        }
                    }
                }

            }

        }
    }
}

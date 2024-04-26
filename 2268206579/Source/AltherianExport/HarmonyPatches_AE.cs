using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace AltherianExport
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches_AE 
    {
        static HarmonyPatches_AE()
        {
            Harmony harmony = new Harmony("zomuro.AltherianExport");

            //StunApplied_AE
            harmony.Patch(AccessTools.Method(typeof(StunHandler), "Notify_DamageApplied",
                new[] { typeof(DamageInfo)}),
                new HarmonyMethod(typeof(HarmonyPatches_AE), nameof(StunApplied_AE)));
        }

        // PREFIX: Modifies StunHandler's Notify_DamageApplied so that it stuns based on additional dmg types
        public static void StunApplied_AE(StunHandler __instance, DamageInfo __0)
        {
            Pawn pawn = __instance.parent as Pawn;
            if (pawn != null && (pawn.Downed || pawn.Dead))
            {
                return;
            }

            if (__0.Def == DamageDefOf_AE.Electroshock_AE)
            {
                Traverse traverse = Traverse.Create(__instance);
                __instance.StunFor(Mathf.RoundToInt(__0.Amount * 30f), __0.Instigator, true, true);
                traverse.Field("stunFromEMP").SetValue(true);
                return;
            }

            if (__0.Def == DamageDefOf_AE.Sonic_AE)
            {
                Traverse traverse = Traverse.Create(__instance);
                __instance.StunFor(Mathf.RoundToInt(2*30f), __0.Instigator, true, true);
                traverse.Field("stunFromEMP").SetValue(false);
                return;
            }
        }

    }

}


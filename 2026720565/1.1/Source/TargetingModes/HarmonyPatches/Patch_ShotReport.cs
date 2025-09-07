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

    public static class Patch_ShotReport
    {

        [HarmonyPatch(typeof(ShotReport))]
        [HarmonyPatch(nameof(ShotReport.HitFactorFromShooter))]
        [HarmonyPatch(new Type[] { typeof(Thing), typeof(float) })]
        public static class Patch_HitFactorFromShooter
        {

            public static void Postfix(ref float __result, Thing caster)
            {
                if (typeof(CompTargetingMode).Equals(caster.TryGetComp<CompTargetingMode>()))
                    __result = Mathf.Max(__result * caster.TryGetComp<CompTargetingMode>().GetTargetingMode().HitChanceFactor, ShootTuning.MinAccuracyFactorFromShooterAndDistance);
            }

        }

    }

}

﻿using System;
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

    public static class Patch_DamageWorker_Bite
    {

        [HarmonyPatch(typeof(DamageWorker_Bite))]
        [HarmonyPatch("ChooseHitPart")]
        public static class Patch_ChooseHitPart
        {

            public static void Postfix(ref BodyPartRecord __result, DamageInfo dinfo, Pawn pawn)
            {
                __result = TargetingModesUtility.ResolvePrioritizedPart_External(__result, dinfo, pawn);
            }

        }

    }

}

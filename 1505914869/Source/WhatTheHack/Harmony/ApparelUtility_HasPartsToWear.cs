﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

//Can wear belts d
[HarmonyPatch(typeof(ApparelUtility), "HasPartsToWear")]
internal class ApparelUtility_HasPartsToWear
{
    private static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
    {
        if (!__result && Utilities.IsBelt(apparel.apparel) && p.health != null &&
            p.health.hediffSet.HasHediff(WTH_DefOf.WTH_BeltModule)) //TODO: make more general
        {
            __result = true;
        }
    }
}

//Can never wear two belts together. 
/*
[HarmonyPatch(typeof(ApparelUtility), "CanWearTogether")]
class ApparelUtility_CanWearTogether
{
    static bool Prefix(ThingDef A, ThingDef B, BodyDef body, ref bool __result)
    {
        if(Utilities.IsBelt(A.apparel) && Utilities.IsBelt(B.apparel))
        {
            return false;
        }
        return true;
    }
}
*/
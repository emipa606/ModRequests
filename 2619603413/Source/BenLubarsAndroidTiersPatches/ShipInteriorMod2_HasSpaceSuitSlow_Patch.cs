using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch]
    static class ShipInteriorMod2_HasSpaceSuitSlow_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var shipInteriorMod2 = Type.GetType("SaveOurShip2.ShipInteriorMod2,ShipsHaveInsides", false);
            if (shipInteriorMod2 != null)
            {
                yield return shipInteriorMod2.GetMethod("HasSpaceSuitSlow", BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (BenLubarsAndroidTiersPatches.Instance.patchSpaceSuit && pawn.IsAndroid())
            {
                __result = true;
            }
        }
    }
}

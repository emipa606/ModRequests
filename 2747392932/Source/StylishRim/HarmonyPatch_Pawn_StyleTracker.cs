using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace StylishRim
{
    internal class HarmonyPatch_Pawn_StyleTracker
    {
        public static bool HasUnwantedHairStyle_Prefix(ref bool __result, Pawn_StyleTracker __instance)
        {
            if (PawnStyleSet.Get(__instance.pawn)?.ShowHair ?? false)
            {
                __result = false;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

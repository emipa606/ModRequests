using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace RealisticMeatScaling
{
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class Patch_StatExtension_GetStatValue
    {
        private const float MeatUnitWeight = 0.18f;
        private const float MeatYieldFraction = 0.50f;
        private static bool gettingMeatAmount = false;
        static void Postfix(ref float __result, Thing thing, StatDef stat)
        {
            if (gettingMeatAmount || stat != StatDefOf.MeatAmount || thing?.def?.race == null || thing?.def?.race.IsMechanoid == true)
                return;
            if (thing is Pawn pawn)
            {
                try
                {
                    gettingMeatAmount = true;
                    float mass = pawn.GetStatValue(StatDefOf.Mass);
                    float meatWeight = mass * MeatYieldFraction;
                    float meatUnits = meatWeight / MeatUnitWeight;
                    __result = Mathf.Floor(meatUnits);
                }
                finally
                {
                    gettingMeatAmount = false;
                }
            }
        }
    }
}
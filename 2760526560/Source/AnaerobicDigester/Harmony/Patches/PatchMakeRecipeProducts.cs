using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnaerobicDigester.Harmony.Patches
{
    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("MakeRecipeProducts")]
    public class PatchMakeRecipeProducts
    {
        [HarmonyPostfix]
        public static void Postfix(IBillGiver billGiver)
        {
            if (billGiver is Building_WorkTable_GasPerRecipe building && building.gasComp != null)
            {
                building.gasComp.Network?.Draw(building.gasComp.Props.gasConsumptionWhenUsed);
            }
        }
    }
}

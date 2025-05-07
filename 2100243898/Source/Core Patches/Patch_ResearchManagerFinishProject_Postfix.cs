using System;
using HarmonyLib;
using RimWorld;

namespace DTechprinting
{
    [HarmonyPatch(typeof(ResearchManager), "FinishProject")]
    class Patch_ResearchManagerFinishProject_Postfix
    {

        private static void Postfix()
        {
            Base.UpdateTechshardRecipes();
        }
    }
}

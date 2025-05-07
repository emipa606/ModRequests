using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;

namespace DTechprinting
{
    [HarmonyPatch(typeof(ResearchManager))]
    [HarmonyPatch("AddTechprints")]
    class Patch_AddTechprints_PrefixPostfix
    {
        private static bool Prefix(ResearchProjectDef proj, ref int amount)
        {
            if (!proj.IsFinished)
                amount *= 100;
            return true;
        }

        private static void Postfix()
        {
            Base.UpdateTechshardRecipes();
        }
    }
}

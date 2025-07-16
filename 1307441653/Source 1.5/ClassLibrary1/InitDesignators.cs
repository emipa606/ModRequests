using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Shoo
{
    [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
    class InitDesignators
    {
        public static void Postfix(ref ReverseDesignatorDatabase __instance)
        {
            List<Designator> desList = AccessTools.Field(typeof(ReverseDesignatorDatabase), "desList").GetValue(__instance) as List<Designator>;
            desList.Add(new Designator_Shoo());
        }
    }
}

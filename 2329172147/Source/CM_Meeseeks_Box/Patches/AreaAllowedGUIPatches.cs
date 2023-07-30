using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class AreaAllowedGUIPatches
    {
        [HarmonyPatch(typeof(AreaAllowedGUI))]
        [HarmonyPatch("DoAreaSelector", MethodType.Normal)]
        public class AreaAllowedGUI_DoAreaSelector
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn p, Area area)
            {
                if (p != null && p.GetComp<CompMeeseeksMemory>() != null && area != null)
                {
                    return false;
                }

                return true;
            }
        }
    }
}

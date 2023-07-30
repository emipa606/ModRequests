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
    public static class Pawn_PlayerSettingsPatches
    {
        [HarmonyPatch(typeof(Pawn_PlayerSettings))]
        [HarmonyPatch("AreaRestriction", MethodType.Setter)]
        public class Pawn_PlayerSettingsPatches_AreaRestriction
        {
            [HarmonyPrefix]
            public static void Prefix(ref Area value, Pawn ___pawn)
            {
                if (___pawn != null && ___pawn.GetComp<CompMeeseeksMemory>() != null)
                {
                    value = null;
                }
            }
        }
    }
}

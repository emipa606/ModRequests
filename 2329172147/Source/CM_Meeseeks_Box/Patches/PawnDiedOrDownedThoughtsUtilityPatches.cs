using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class PawnDiedOrDownedThoughtsUtilityPatches
    {
        [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility))]
        [HarmonyPatch("TryGiveThoughts", new Type[] { typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind) })]
        public class PawnDiedOrDownedThoughtsUtility_TryGiveThoughts
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind)
            {
                if (victim != null && victim.kindDef == MeeseeksDefOf.MeeseeksKind)
                    return false;

                return true;
            }
        }
    }
}

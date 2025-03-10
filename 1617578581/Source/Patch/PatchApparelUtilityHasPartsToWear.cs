﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Radiology.Patch
{

    /// <summary>
    /// Prevent pawns from equipping something if they have mutated body part apparel
    /// </summary>
    [HarmonyPatch(typeof(ApparelUtility), "HasPartsToWear", new Type[] { typeof(Pawn), typeof(ThingDef) })]
    public static class PatchApparelUtilityHasPartsToWear
    {
        static bool Prefix(Pawn p, ThingDef apparel, ref bool __result)
        {
            if (apparel.thingClass.IsAssignableFrom(typeof(ApparelBodyPart))) return true;

            foreach (ApparelBodyPart apparelMutation in p.apparel.WornApparel.OfType<ApparelBodyPart>())
            {
                if (apparel.apparel.bodyPartGroups.Intersect(apparelMutation.def.apparel.bodyPartGroups).Any())
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}

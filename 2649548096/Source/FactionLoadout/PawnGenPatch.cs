﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace FactionLoadout
{
    [HarmonyPatch(typeof(FactionUtility), "HostileTo")]
    public static class PawnGenPatch
    {
        public static bool Active = false;

        [HarmonyPriority(Priority.First)]
        static bool Prefix(ref bool __result)
        {
            if (Active)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}

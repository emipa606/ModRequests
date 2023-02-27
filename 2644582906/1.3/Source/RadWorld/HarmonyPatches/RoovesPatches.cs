using System;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RadWorld
{
    [HarmonyPatch(typeof(RoofGrid), "SetRoof")]
    internal static class Patch_SetRoof
    {
        private static void Postfix(RoofGrid __instance, ref IntVec3 c, ref RoofDef def, Map ___map)
        {
            if (Current.ProgramState == ProgramState.Playing && Scribe.mode == LoadSaveMode.Inactive && ___map.Biome.IsCavernBiome() && def is null)
            {
                ___map.roofGrid.SetRoof(c, RoofDefOf.RoofRockThin);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace SR
{
    [HarmonyPatch(typeof(TerrainGrid), "SetTerrain")]
    public class TerrainGrid_SetTerrain
    {
        internal static void Prefix(IntVec3 c, TerrainDef newTerr, Map ___map, ref (TerrainDef, bool) __state)
        {
            __state = (___map.terrainGrid.TerrainAt(c), ___map.terrainGrid.UnderTerrainAt(c) == null);
        }

        internal static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map, ref (TerrainDef, bool) __state)
        {
            if (__state.Item1 == newTerr || __state.Item1 == null) //If we're not actually changing anything or it had no terrain previously..
                return; //Who cares?
            if (__state.Item2 //If there was no underTerrain before..
             && newTerr.HasSoilPlaceWorker() //And the new terrain is one of the soils we care about..
             && !__state.Item1.IsWater //And the old terrain wasn't water (not water fill)..
             && !__state.Item1.IsWetBridgeable()) //And the old terrain wasn't wet bridgeable (not "water" fill)..
                ___map.terrainGrid.SetUnderTerrain(c, __state.Item1); //Store the old terrain as underTerrain.
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Noise;

namespace CMS
{
    [HarmonyPatch(typeof(RockNoises), "Init")]
    class HarmonyPatches
    {
        static bool Prefix(Map map)
		{
			RockNoises.rockNoises = new List<RockNoises.RockNoise>();
			foreach (ThingDef rockDef in Find.World.NaturalRockTypesIn(map.Tile))
			{
				RockNoises.RockNoise rockNoise = new RockNoises.RockNoise();
				rockNoise.rockDef = rockDef;
				//Use a consistent seed to make it reproducible, but add hashcode per rock type or else they all have the same noise in the end. The division by two is to make it less likely that we end up hitting the integer MaxValue.
				rockNoise.noise = new Perlin(0.004999999888241291, 2.0, 0.5, 6, (map.ConstantRandSeed + rockDef.GetHashCode() / 2), QualityMode.Medium);
				RockNoises.rockNoises.Add(rockNoise);
				NoiseDebugUI.StoreNoiseRender(rockNoise.noise, rockNoise.rockDef + " score", map.Size.ToIntVec2);
			}
			return false;
        }
    }
}

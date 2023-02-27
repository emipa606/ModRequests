using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	[StaticConstructorOnStartup]
	public static class CavernPerlin
	{
		[TweakValue("0RadWorld", 0, 10)] public static double persistence = 0.40000000596046448;
		[TweakValue("0RadWorld", 0, 1f)] public static float frequency = 0.09f;
		[TweakValue("0RadWorld", 0, 10)] public static double lacunarity = 2.0;
		[TweakValue("0RadWorld", 0, 10)] public static int octaves = 6;

		[TweakValue("0RadWorld", 0, 1f)] public static float frequency2 = 0.025f;
		[TweakValue("0RadWorld", 0, 10)] public static double lacunarity2 = 2.0;
		[TweakValue("0RadWorld", 0, 10)] public static int octaves2 = 6;
		static CavernPerlin()
        {
		}

		private static Dictionary<BiomeDef, Multiply> noisesPerBiomes = new Dictionary<BiomeDef, Multiply>();

		public static Multiply GetNoiseFor(BiomeDef biomeDef)
        {
			if (!noisesPerBiomes.TryGetValue(biomeDef, out var value))
            {
				value = SetupRadiationNoise(biomeDef);
				noisesPerBiomes[biomeDef] = value;
			}
			return value;
        }
		private static float FreqMultiplier
		{
			get
			{
				return 1f;
			}
		}

		public static Multiply SetupRadiationNoise(BiomeDef biomeDef)
		{
			float freqMultiplier = FreqMultiplier;
			Rand.PushState();
			Rand.Seed = biomeDef.GetHashCode();
			var randValue = Rand.Range(0.09f, 0.25f);
			ModuleBase moduleBase = new Perlin((double)(randValue * freqMultiplier), 2.0, 0.40000000596046448, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
			ModuleBase moduleBase2 = new RidgedMultifractal((double)(0.005f * freqMultiplier), 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			moduleBase2 = new ScaleBias(0.5, 0.5, moduleBase2);
			var noiseElevation = new Multiply(moduleBase, moduleBase2);
			NoiseDebugUI.StorePlanetNoise(noiseElevation, "noiseRadiation" + biomeDef.defName);
			Rand.PopState();
			return noiseElevation;
		}


		private static FloatRange ElevationRange = new FloatRange(650f, 750f);
	}
}

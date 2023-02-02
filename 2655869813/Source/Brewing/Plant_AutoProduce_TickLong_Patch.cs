using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using VanillaBrewingExpanded;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Brewing
{
    [HarmonyPatch(typeof(Plant_AutoProduce), nameof(Plant_AutoProduce.TickLong))]
    static class Plant_AutoProduce_TickLong_Patch
    {
        [HarmonyPatch(typeof(Plant), nameof(Plant.TickLong))]
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void BaseTickLong(Plant __instance)
		{
			throw new NotImplementedException("stub method should not have been called");
		}

		// destructive patch; original code that was removed is commented out
		public static bool Prefix(Plant_AutoProduce __instance)
		{
			BaseTickLong(__instance);

			__instance.counter++;
			if (__instance.counter <= 60 || __instance.Growth <= 0.7f)
			{
				return false;
			}

			__instance.counter = 0;

			//var random = new Random();
			//if (random.NextDouble() > 0.4)
			if (Rand.Chance(0.6f))
			{
				var fruit = ThingMaker.MakeThing(__instance.def.plant.harvestedThingDef);
				//fruit.SetForbidden(true);
				//if (random.NextDouble() < 0.4)
				if (Rand.Chance(0.4f))
				{
					fruit.stackCount = 2;
				}
				else
				{
					fruit.stackCount = 4;
				}

				GenPlace.TryPlaceThing(fruit, __instance.Position + IntVec3.South, __instance.Map, ThingPlaceMode.Near);

				switch (BenLubarsVanillaExpandedPatches.autoAllowDroppedFruit.Value)
                {
					case HomeAreaOrAlways.Never:
						fruit.SetForbidden(true);
						break;
					case HomeAreaOrAlways.HomeArea:
						fruit.SetForbiddenIfOutsideHomeArea();
						break;
					case HomeAreaOrAlways.Always:
						break;
                }
			}

			return false;
		}
    }
}

using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace RemoveGradualRelationsDecay
{
	[StaticConstructorOnStartup]
    public class HarmonyPatches
	{
		private static readonly Type patchType = typeof(HarmonyPatches);

		static HarmonyPatches()
		{
			var harmony = new Harmony(id: "rimworld.gerads.removegradualrelationsdecay");

			harmony.Patch(AccessTools.Method(typeof(Faction), "CheckReachNaturalGoodwill"), prefix: new HarmonyMethod(patchType, nameof(CheckReachNaturalGoodwillPrefix)));
		}

		public static bool CheckReachNaturalGoodwillPrefix()
		{
			return false; // TODO: set up settings
		}
	}
}

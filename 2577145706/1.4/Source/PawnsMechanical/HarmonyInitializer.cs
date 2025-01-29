using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VexedThings.HarmonyPatches
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			var Harmony = new Harmony("vexedtrees.RimRobots");
			Harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}

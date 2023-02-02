using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Reflection;
using Verse;

namespace MoreIdeoColors
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		private static readonly Type patchType = typeof(HarmonyPatches);
		// this static constructor runs to create a HarmonyInstance and install a patch.
		static HarmonyPatches()
		{
			var harmony = new Harmony("rimworld.martinbaste.MoreIdeoColors");

			harmony.PatchAll();

			// find the FillTab method of the class RimWorld.ITab_Pawn_Character
			/*MethodInfo targetmethod = AccessTools.Method(typeof(RimWorld.PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike");

            // find the static method to call before (i.e. Prefix) the targetmethod
            HarmonyMethod suffixmethod = new HarmonyMethod(typeof(XenoPrecept.HarmonyPatches).GetMethod("AppendThoughts_Relations_Suffix"));

            // patch the targetmethod, by calling prefixmethod before it runs, with no postfixmethod (i.e. null)
            harmony.Patch(targetmethod, postfix: suffixmethod);*/
		}
		
	}
}
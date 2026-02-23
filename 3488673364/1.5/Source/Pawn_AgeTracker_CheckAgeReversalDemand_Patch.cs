using HarmonyLib;
using RimWorld;
using Verse;

namespace YoungAndOldHair
{
	[HarmonyPatch(typeof(Pawn_AgeTracker), "CheckAgeReversalDemand")]
	public static class Pawn_AgeTracker_CheckAgeReversalDemand_Patch
	{
		public static void Postfix(Pawn_AgeTracker __instance)
		{
			if (__instance.pawn.story?.hairDef != null && __instance.pawn.HairIsAllowed(__instance.pawn.story.hairDef) is false)
			{
				__instance.pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(__instance.pawn);
				__instance.pawn.Drawer?.renderer?.SetAllGraphicsDirty();
			}
		}
	}
}
using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(WorkGiver_Researcher), "ShouldSkip")]
internal class Patch
{
	private static void Postfix(ref bool __result, Pawn pawn, bool forced)
	{
		if (pawn.IsColonyMech)
		{
			__result = true;
		}
	}
}

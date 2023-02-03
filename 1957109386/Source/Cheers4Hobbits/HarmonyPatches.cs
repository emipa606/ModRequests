using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;

namespace Cheers4Hobbits
{
	using static MinHumanlikeBodySizePatcher;

	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		const bool DEBUG = false;

		static HarmonyPatches()
		{
			HarmonyInstance.DEBUG = DEBUG;
			try
			{
				HarmonyInstance.Create("Cheers4Hobbits").PatchAll();
			}
			finally
			{
				HarmonyInstance.DEBUG = false;
			}
		}
	}

	[HarmonyPatch(typeof(AddictionUtility), nameof(AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize))]
	static class AddictionUtility_ModifyChemicalEffectForToleranceAndBodySize_Patch
	{
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			MinHumanlikeBodySizeTranspiler(instructions);
	}

	[HarmonyPatch(typeof(IngestionOutcomeDoer_GiveHediff), "DoIngestionOutcomeSpecial")]
	static class IngestionOutcomeDoer_GiveHediff_DoIngestionOutcomeSpecial_Patch
	{
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			MinHumanlikeBodySizeTranspiler(instructions);
	}
}

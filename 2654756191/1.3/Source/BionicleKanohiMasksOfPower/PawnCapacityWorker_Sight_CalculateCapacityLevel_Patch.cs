using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(PawnCapacityWorker_Sight), "CalculateCapacityLevel")]
	public static class PawnCapacityWorker_Sight_CalculateCapacityLevel_Patch
	{
		private static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if ((diffSet.pawn.Wears(BionicleDefOf.BKMOP_Akaku, out var apparel) && apparel.IsMasterworkOrLegendary()) || (diffSet.pawn.Wears(BionicleDefOf.BKMOP_Ruru, out var apparel2) && apparel2.IsMasterworkOrLegendary()))//double sight with akaku and ruru
			{
				__result *= 2f;
			}
		}
	}

	[HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
	public static class ThoughtWorker_Dark_CurrentStateInternal_Patch//nullifies darkness debuff with ruru
	{
		private static void Postfix(ref ThoughtState __result, Pawn p)
		{
			if (p.Wears(BionicleDefOf.BKMOP_Ruru, out var apparel) && apparel.IsMasterworkOrLegendary())
			{
				__result = false;
			}
		}
	}
}
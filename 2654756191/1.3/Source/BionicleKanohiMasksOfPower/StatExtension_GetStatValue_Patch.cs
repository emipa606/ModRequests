using HarmonyLib;
using RimWorld;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]//need to use different patch (statworker) to get explanation working?
	public static class StatExtension_GetStatValue_Patch
	{
		private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
		{
			if (thing is Pawn pawn)
			{
				if (stat == StatDefOf.MoveSpeed && pawn.Wears(BionicleDefOf.BKMOP_Kakama, out var apparel) && apparel.IsMasterworkOrLegendary())//doubles move speed with kakama
				{
					__result *= 2f;
					//explanation?.Append($"{apparel.Label.CapitalizeFirst()}: x{2f.ToStringPercent()}");
				}
				else if (stat == StatDefOf.CarryingCapacity && pawn.Wears(BionicleDefOf.BKMOP_Pakari, out var apparel2) && apparel2.IsMasterworkOrLegendary())//doubles carry capacity with pakari
                {
					__result *= 2f;
					//explanation?.Append($"{apparel2.Label.CapitalizeFirst()}: x{2f.ToStringPercent()}");
				}
				else if (stat == StatDefOf.AimingDelayFactor && pawn.Wears(BionicleDefOf.BKMOP_Kakama, out var apparel3) && apparel3.IsMasterworkOrLegendary())//halves aiming time with kakama
                {
					__result *= 0.5f;
					//explanation?.Append($"{apparel3.Label.CapitalizeFirst()}: x{0.5f.ToStringPercent()}");
				}
				else if (stat == StatDefOf.ResearchSpeed && pawn.Wears(BionicleDefOf.BKMOP_Rau, out var apparel4) && apparel4.IsMasterworkOrLegendary())//doubles research speed with rau
                {
					__result *= 2f;
					//explanation?.Append($"{apparel4.Label.CapitalizeFirst()}: x{2f.ToStringPercent()}");
				}
				else if (stat == StatDefOf.IncomingDamageFactor && pawn.IsDuplicate())//duplicates take no damage
                {
					__result *= 0f;
					//explanation?.Append($"{pawn.health.hediffSet.GetFirstHediffOfDef(BionicleDefOf.BKMOP_PawnDuplicate).Label.CapitalizeFirst()}: x{0f.ToStringPercent()}");
				}
			}
		}
	}
}
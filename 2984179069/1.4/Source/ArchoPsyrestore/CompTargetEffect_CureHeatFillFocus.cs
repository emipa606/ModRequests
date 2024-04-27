using RimWorld;
using Verse;

namespace ArchoPsyrestore
{
	public class CompTargetEffect_CureHeatFillFocus : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Faction != Faction.OfPlayer || pawn.AnimalOrWildMan() || pawn.Dead)
			{
				return;
			}
			Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
			if (psychicEntropy != null)
			{
				psychicEntropy.OffsetPsyfocusDirectly(1f);
				if (pawn.psychicEntropy != null)
				{
					pawn.psychicEntropy.TryAddEntropy(-1000f);
				}
			}
		}
	}
}

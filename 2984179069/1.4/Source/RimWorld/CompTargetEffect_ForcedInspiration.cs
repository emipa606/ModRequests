using Verse;

namespace RimWorld
{
	public class CompTargetEffect_ForcedInspiration : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (!pawn.AnimalOrWildMan() && pawn.Faction == Faction.OfPlayer && !pawn.Dead)
			{
				InspirationDef randomAvailableInspirationDef = pawn.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
				pawn.mindState.inspirationHandler.TryStartInspiration(randomAvailableInspirationDef);
			}
		}
	}
}

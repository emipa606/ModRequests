using RimWorld;
using Verse;

namespace ArchoStasis
{
	public class CompTargetEffect_ArchotechStasis : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Faction != Faction.OfMechanoids && !pawn.Dead)
			{
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ArchoStasis, pawn);
				BodyPartRecord result = null;
				pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out result);
				BattleLogEntry_ItemUsed battleLogEntry_ItemUsed = new BattleLogEntry_ItemUsed(user, target, parent.def, RulePackDefOf.Event_ItemUsed);
				hediff.combatLogEntry = new WeakReference<LogEntry>(battleLogEntry_ItemUsed);
				hediff.combatLogText = battleLogEntry_ItemUsed.ToGameStringFromPOV(null);
				pawn.health.AddHediff(hediff, result);
				Find.BattleLog.Add(battleLogEntry_ItemUsed);
			}
		}
	}
}

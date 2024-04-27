using System.Linq;
using RimWorld;
using Verse;

namespace ArchoPsyHealer
{
	public class CompTargetEffect_ArchoHeal : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Faction != Faction.OfPlayer || pawn.Dead)
			{
				return;
			}
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(hediff.def);
				if (immunityRecord != null)
				{
					immunityRecord.immunity = 1f;
				}
				for (int i = 0; i < 5; i++)
				{
					if ((from x in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
						where x.CanHealNaturally() || x.CanHealFromTending()
						select x).TryRandomElement(out var result))
					{
						result.Heal(1E+10f);
					}
				}
			}
		}
	}
}

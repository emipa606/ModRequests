using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
    public class DamageWorker_Sonic_AE : DamageWorker_AddInjury
    {
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}

			DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);

			System.Random rand = new System.Random();
			if (pawn != null)
            {
				if (pawn.RaceProps.IsMechanoid == false && rand.NextDouble() <= 0.025f)
                {
					List<BodyPartDef> parts = new List<BodyPartDef>(); parts.Append(BodyPartDefOf_AE.Ear);
					HediffGiverUtility.TryApply(pawn, HediffDefOf_AE.HearingLoss, parts, true, 1, null);
				}	
			}

			return damageResult;
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			bool flag = !pawn.RaceProps.IsMechanoid;
			if (flag)
			{
				base.FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
			}
		}

	}
}

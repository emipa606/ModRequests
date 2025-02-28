using RimWorld;
using Verse.AI;

namespace Verse
{
	public class HediffComp_EVOR_ReactOnDamage : HediffComp
	{
		public HediffCompProperties_EVOR_ReactOnDamage Props
		{
			get
			{
				return (HediffCompProperties_EVOR_ReactOnDamage)this.props;
			}
		}
		public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			this.React();
		}
		private void React()
		{
			if (this.Props.createHediff != null)
			{
				BodyPartRecord part = this.parent.Part;
				if (this.Props.createHediffOn != null)
				{
					part = this.parent.pawn.RaceProps.body.AllParts.FirstOrFallback((BodyPartRecord p) => p.def == this.Props.createHediffOn, null);
				}
				this.parent.pawn.health.AddHediff(this.Props.createHediff, part, null, null);
			}
			if (this.Props.vomit)
			{
				this.parent.pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, true, true, null, null, false, false);
			}
		}
	}

	public class HediffCompProperties_EVOR_ReactOnDamage : HediffCompProperties
	{
		public HediffCompProperties_EVOR_ReactOnDamage()
		{
			this.compClass = typeof(HediffComp_EVOR_ReactOnDamage);
		}
		public BodyPartDef createHediffOn;
		public HediffDef createHediff;
		public bool vomit;
	}
}

using System.Text;
using Verse;

namespace Diabetes
{
	public class HediffComp_SeverityPerDayToHediff : HediffComp_SeverityPerDay
	{
		public HediffCompProperties_SeverityPerDayToHediff Props
		{
			get
			{
				return this.props as HediffCompProperties_SeverityPerDayToHediff;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff);
			if (hediff == null)
			{
				return;
			}
			base.CompPostTick(ref severityAdjustment);
			hediff.Severity += severityAdjustment;
			severityAdjustment *= -1;
			base.CompPostTick(ref severityAdjustment);
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompDebugString());
			if (!this.Pawn.Dead)
			{
				stringBuilder.AppendLine("hediff: " + this.Props.hediff.ToString());
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}
}

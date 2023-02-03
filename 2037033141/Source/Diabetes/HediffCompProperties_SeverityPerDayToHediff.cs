using Verse;

namespace Diabetes
{
	public class HediffCompProperties_SeverityPerDayToHediff : HediffCompProperties_SeverityPerDay
	{
		public HediffCompProperties_SeverityPerDayToHediff()
		{
			this.compClass = typeof(HediffComp_SeverityPerDayToHediff);
		}

		public HediffDef hediff;
	}
}

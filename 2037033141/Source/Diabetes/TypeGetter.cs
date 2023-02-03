using Verse;

namespace Diabetes
{
	public static class TypeGetter
	{
		public static HediffDef HediffDef(EHediffDef e)
		{
			return DefDatabase<HediffDef>.GetNamed(e.ToString());
		}

		public static ThingDef ThingDef(EThingDef e)
		{
			return DefDatabase<ThingDef>.GetNamed(e.ToString());
		}

		public static JobDef JobDef(EJobDef e)
		{
			return DefDatabase<JobDef>.GetNamed(e.ToString());
		}
	}
}

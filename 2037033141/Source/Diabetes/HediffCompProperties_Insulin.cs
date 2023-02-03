using Verse;

namespace Diabetes
{
	public class HediffCompProperties_Insulin : HediffCompProperties_SeverityPerDay
	{
		public HediffCompProperties_Insulin()
		{
			this.compClass = typeof(HediffComp_Insulin);
		}
	}
}

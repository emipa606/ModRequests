using Verse;

namespace RimWorld
{
	[DefOf]
	public static class MAG_JobDefOf
	{
		public static JobDef BetterResurrect;

		static MAG_JobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}
	}
}

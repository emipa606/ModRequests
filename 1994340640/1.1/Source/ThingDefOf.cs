using RimWorld;
using Verse;

namespace WallStuff
{
	[DefOf]	public static class ThingDefOf
	{ 
		public static ThingDef Wall_Hifi;

		static ThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WallStuff.ThingDefOf));
		}
	}
}

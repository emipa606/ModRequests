using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
	[DefOf]
	public static class HDThingDefOf
	{
		public static ThingDef Mote_Dream;

		static HDThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HDThingDefOf));
		}
	}
}

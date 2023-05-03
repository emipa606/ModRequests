using System;
using Verse;
using RimWorld;

namespace Mashed_BlackPlague
{
	[DefOf]
	public static class ThingDefOf
	{

		static ThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
		}

		public static ThingDef BlackPlague_TuurngaitRace;
		public static ThingDef BlackPlague_Vessel;
		//public static ThingDef BlackPlague_FinisSpecialis;
	}
}

using System;
using Verse;
using RimWorld;

namespace Mashed_BlackPlague
{
	[DefOf]
	public static class ThoughtDefOf
	{

		static ThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
		}

		public static ThoughtDef BlackPlague_TouchedVessel;
		public static ThoughtDef BlackPlague_TouchedVessel_Infected;
		public static ThoughtDef BlackPlague_TuurngaitConnectionLost;
		public static ThoughtDef BlackPlague_TuurngaitHivemindThought;
	}
}

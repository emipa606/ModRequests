using System;
using Verse;
using RimWorld;

namespace Mashed_BlackPlague
{
	[DefOf]
	public static class HediffDefOf
	{

		static HediffDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
		}

		public static HediffDef BlackPlague_TuurngaitVirus;
	}
}

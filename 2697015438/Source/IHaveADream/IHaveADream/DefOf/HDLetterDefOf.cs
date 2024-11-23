using RimWorld;
using Verse;

namespace HDream
{
	[DefOf]
	public static class HDLetterDefOf
	{
		public static LetterDef Wish;

		static HDLetterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HDLetterDefOf));
		}
	}
}

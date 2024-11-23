using RimWorld;

namespace HDream
{
	[DefOf]
	public static class HDTraitDefOf
	{
		public static TraitDef SimpleMind;

		static HDTraitDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HDTraitDefOf));
		}
	}
}

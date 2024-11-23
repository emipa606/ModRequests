using RimWorld;

namespace HDream
{
	[DefOf]
	public static class HDJoyKindDefOf
	{
		public static JoyKindDef Chemical;

		static HDJoyKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HDJoyKindDefOf));
		}
	}
}
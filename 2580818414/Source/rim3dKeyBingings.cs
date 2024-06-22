using RimWorld;
using Verse;

namespace rim3d
{
	[DefOf]
	public static class rim3dKeyBingings
	{
		public static KeyBindingDef Toggle3D;

		static rim3dKeyBingings()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingDefOf));
		}
	}
}

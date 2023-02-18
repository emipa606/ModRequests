using System;
using RimWorld;
using Verse;

namespace Crystosentient.Dictionary
{
	// Token: 0x0200001D RID: 29
	[DefOf]
	public static class DefOfThing
	{
		// Token: 0x0600007D RID: 125 RVA: 0x00002557 File Offset: 0x00000757
		static DefOfThing()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DefOfThing));
		}

		// Token: 0x04000044 RID: 68
		public static ThingDef GDCRYST_BUILDABLE_WallAmethystRough;

		public static ThingDef GDCRYST_BUILDABLE_WallAmethystSmooth;

		public static ThingDef GDCRYST_FILTH_AmethystGemFilth;
	}
}

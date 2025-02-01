using System;
using RimWorld;

namespace LostTechnology.Def
{
	[DefOf]
	public static class SitePartDefOf
	{
		static SitePartDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
		}

		// Token: 0x04004D19 RID: 19737
		public static SitePartDef MechCluster;
		public static SitePartDef MechClusterForceNoConditionCauser;
		public static SitePartDef ItemStash;
		public static SitePartDef RaidSource;
		public static SitePartDef Techprint;



	}
}

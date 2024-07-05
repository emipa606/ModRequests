using System;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x0200000D RID: 13
	public class CompProperties_AffectVision : CompProperties
	{
		// Token: 0x06000042 RID: 66 RVA: 0x00005A06 File Offset: 0x00003C06
		public CompProperties_AffectVision()
		{
			this.compClass = typeof(CompAffectVision);
		}

		// Token: 0x04000032 RID: 50
		public float fovMultiplier;

		// Token: 0x04000033 RID: 51
		public bool denyDarkness = false;

		// Token: 0x04000034 RID: 52
		public bool denyWeather = false;

		// Token: 0x04000035 RID: 53
		public bool applyImmediately;
	}
}

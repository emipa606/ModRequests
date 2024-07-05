using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x0200000C RID: 12
	public class CompAffectVision : ThingComp
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600003F RID: 63 RVA: 0x000059D8 File Offset: 0x00003BD8
		public CompProperties_AffectVision Props
		{
			get
			{
				return (CompProperties_AffectVision)this.props;
			}
		}

		// Token: 0x04000031 RID: 49
		public static readonly Type COMP_CLASS = typeof(CompAffectVision);
	}
}

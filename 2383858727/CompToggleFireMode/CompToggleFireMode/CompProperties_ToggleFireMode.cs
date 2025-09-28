using System;
using Verse;

namespace CompToggleFireMode
{
	// Token: 0x02000002 RID: 2
	public class CompProperties_ToggleFireMode : CompProperties
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public CompProperties_ToggleFireMode()
		{
			this.compClass = typeof(CompToggleFireMode);
		}

		// Token: 0x04000001 RID: 1
		public ResearchProjectDef requiredResearch;
	}
}

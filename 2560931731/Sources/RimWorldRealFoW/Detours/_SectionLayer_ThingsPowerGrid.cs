using System;
using RimWorldRealFoW;
using Verse;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000022 RID: 34
	public static class _SectionLayer_ThingsPowerGrid
	{
		// Token: 0x06000083 RID: 131 RVA: 0x00009070 File Offset: 0x00007270
		public static bool TakePrintFrom_Prefix(Thing t)
		{
			return t.fowIsVisible(true);
		}
	}
}

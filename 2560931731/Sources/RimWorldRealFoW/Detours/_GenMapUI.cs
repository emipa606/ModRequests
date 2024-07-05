using System;
using RimWorldRealFoW;
using Verse;
using RimWorldRealFoW.Utils;
namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000027 RID: 39
	public static class _GenMapUI
	{
		// Token: 0x06000089 RID: 137 RVA: 0x00009214 File Offset: 0x00007414
		public static bool DrawThingLabel_Prefix(Thing thing)
		{
			return thing.fowIsVisible(false);
		}
	}
}

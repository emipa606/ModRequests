using System;
using RimWorld.Planet;
using RimWorldRealFoW;
using Verse;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000029 RID: 41
	public static class _Selector
	{
		// Token: 0x0600008C RID: 140 RVA: 0x00009390 File Offset: 0x00007590
		public static bool Select_Prefix(object obj)
		{
			Thing thing = obj as Thing;
			Pawn pawn = obj as Pawn;
			bool flag = thing != null && !thing.Destroyed && (pawn == null || !pawn.IsWorldPawn()) && !thing.fowIsVisible(false);
			return !flag;
		}
	}
}

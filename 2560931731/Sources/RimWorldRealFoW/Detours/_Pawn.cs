using System;
using RimWorldRealFoW;
using Verse;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000024 RID: 36
	internal static class _Pawn
	{
		// Token: 0x06000085 RID: 133 RVA: 0x000090A8 File Offset: 0x000072A8
		public static bool DrawGUIOverlay_Prefix(Pawn __instance)
		{
			return !(__instance.Spawned && !__instance.fowIsVisible(false));
		}
	}
}

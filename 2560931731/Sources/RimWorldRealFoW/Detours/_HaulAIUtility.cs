using System;
using RimWorldRealFoW;
using Verse;
using Verse.AI;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000021 RID: 33
	public static class _HaulAIUtility
	{
		// Token: 0x06000082 RID: 130 RVA: 0x0000902C File Offset: 0x0000722C
		public static bool HaulToStorageJob_Prefix(Pawn p, Thing t, Job __result)
		{
			return !(p.Faction != null && p.Faction.IsPlayer && !t.fowIsVisible(false));
		}
	}
}

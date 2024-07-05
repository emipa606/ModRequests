using System;
using RimWorldRealFoW;
using Verse;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000025 RID: 37
	public static class _ReservationUtility
	{
		public static void CanReserve_Postfix(this Pawn p, ref bool __result, LocalTargetInfo target)
		{
			if (__result && p.Faction != null && p.Faction.IsPlayer && target.HasThing && target.Thing.def.category != ThingCategory.Pawn)
			{
				__result = target.Thing.fowIsVisible(false);
			}
		}

		public static void CanReserveAndReach_Postfix(this Pawn p, bool __result, LocalTargetInfo target)
		{
			if (__result && p.Faction != null && p.Faction.IsPlayer && target.HasThing && target.Thing.def.category != ThingCategory.Pawn)
			{
				__result = target.Thing.fowIsVisible(false);
			}
		}
	}
}

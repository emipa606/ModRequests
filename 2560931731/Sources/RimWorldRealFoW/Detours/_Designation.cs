using System;
using RimWorld;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000019 RID: 25
	public static class _Designation
	{
		// Token: 0x06000078 RID: 120 RVA: 0x00008BE4 File Offset: 0x00006DE4
		public static void Notify_Added_Postfix(ref Designation __instance)
		{
			if (__instance.def == DesignationDefOf.Mine && !__instance.target.HasThing)
			{
				MapComponentSeenFog mapComponentSeenFog = __instance.designationManager.map.getMapComponentSeenFog();
				if (mapComponentSeenFog != null)
				{
					mapComponentSeenFog.RegisterMineDesignation(__instance);
				}
			}
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00008C40 File Offset: 0x00006E40
		public static void Notify_Removing_Postfix(ref Designation __instance)
		{
			if (__instance.def == DesignationDefOf.Mine && !__instance.target.HasThing)
			{
				MapComponentSeenFog mapComponentSeenFog = __instance.designationManager.map.getMapComponentSeenFog();
				if (mapComponentSeenFog != null)
				{
					mapComponentSeenFog.DeregisterMineDesignation(__instance);
				}
			}
		}
	}
}

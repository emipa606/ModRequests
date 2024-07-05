using System;
using RimWorld;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000015 RID: 21
	public static class _GenView
	{
		// Token: 0x06000074 RID: 116 RVA: 0x00008AA8 File Offset: 0x00006CA8
		public static void ShouldSpawnMotesAt_Postfix(IntVec3 loc, Map map, bool drawOffscreen, ref bool __result)
		{
			if (__result)
			{
				MapComponentSeenFog mapComponentSeenFog = _GenView.lastUsedMapComponent;
				if (map != _GenView.lastUsedMap)
				{
					_GenView.lastUsedMap = map;
					mapComponentSeenFog = (_GenView.lastUsedMapComponent = map.GetComponent<MapComponentSeenFog>());
				}
				__result = (mapComponentSeenFog == null || mapComponentSeenFog.isShown(Faction.OfPlayer, loc.x, loc.z));
			}
		}

		// Token: 0x04000087 RID: 135
		private static MapComponentSeenFog lastUsedMapComponent;

		// Token: 0x04000088 RID: 136
		private static Map lastUsedMap;
	}
}

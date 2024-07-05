using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorldRealFoW.Utils;
namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200001A RID: 26
	public static class _MainTabWindow_Wildlife
	{
		// Token: 0x0600007A RID: 122 RVA: 0x00008C9C File Offset: 0x00006E9C
		public static bool get_Pawns_Prefix(ref IEnumerable<Pawn> __result)
		{
			__result = Find.CurrentMap.mapPawns.AllPawns.Where(delegate (Pawn p)
			{
				
				return 
				p.Spawned && (p.Faction == null 
				|| p.Faction == Faction.OfInsects) 
				&& p.AnimalOrWildMan() 
				&& !p.Position.Fogged(p.Map)
				&& (p.fowIsVisible()||RFOWSettings.wildLifeTabVisible)
				;

			
			});
			return false;
		}
	}
}

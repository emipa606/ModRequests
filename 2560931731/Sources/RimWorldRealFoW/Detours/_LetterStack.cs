using System;
using RimWorld.Planet;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000026 RID: 38
	public static class _LetterStack
	{
		// Token: 0x06000088 RID: 136 RVA: 0x0000919C File Offset: 0x0000739C
		public static void ReceiveLetter_Prefix(ref LookTargets lookTargets)
		{
			if (lookTargets != null && lookTargets.PrimaryTarget.HasThing)
			{
				Thing thing = lookTargets.PrimaryTarget.Thing;
				if ((thing != null && thing.Faction == null) || !thing.Faction.IsPlayer)
				{
					lookTargets = new GlobalTargetInfo(thing.Position, thing.Map, false);
				}
			}
		}
	}
}

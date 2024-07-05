using System;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200002B RID: 43
	public static class _Messages
	{
		// Token: 0x06000090 RID: 144 RVA: 0x00009684 File Offset: 0x00007884
		public static void Message_Prefix(string text, ref LookTargets lookTargets)
		{
			bool value = Traverse.Create(typeof(Messages)).Method("AcceptsMessage", new object[]
			{
				text,
				lookTargets
			}).GetValue<bool>();
			if (value)
			{
				bool hasThing = lookTargets.PrimaryTarget.HasThing;
				if (hasThing)
				{
					Thing thing = lookTargets.PrimaryTarget.Thing;
					if (thing.Faction == null || !thing.Faction.IsPlayer)
					{
						lookTargets = new GlobalTargetInfo(thing.Position, thing.Map, false);
					}
				}
			}
		}
	}
}

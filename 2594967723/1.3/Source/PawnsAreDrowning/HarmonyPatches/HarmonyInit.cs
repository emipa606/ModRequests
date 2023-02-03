using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnsAreDrowning
{
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			new Harmony("123nick.PawnsAreDrowning").PatchAll();
		}
	}

	[HarmonyPatch(typeof(Pawn), "Tick")]
	public static class Pawn_Tick
	{
		public static void Postfix(Pawn __instance)
		{
			if (__instance?.Map != null && Find.TickManager.TicksGame % 60 == 0)
            {
				if (IsDownedOrIncapable(__instance) && InWaterTerrain(__instance) && CanDrown(__instance))
                {
					if (DrowningMod.settings.drowningStates.TryGetValue(__instance.Position.GetTerrain(__instance.Map).defName, out float severity))
                    {
						var level = __instance.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
						if (level == 0) level = 0.01f;
						HealthUtility.AdjustSeverity(__instance, PDDefOf.PD_Drowning, severity / level);
						return;
                    }
                }
				var hediff = __instance.health?.hediffSet?.GetFirstHediffOfDef(PDDefOf.PD_Drowning);
				if (hediff != null)
                {
					__instance.health.RemoveHediff(hediff);
                }
            }
		}

		public static bool CanDrown(Pawn pawn)
        {
			if (DrowningMod.settings.raceStates.TryGetValue(pawn.def.defName, out bool canDrown) && !canDrown)
            {
				return false;
            }
			if (pawn.apparel?.WornApparel != null) 
			{ 
				foreach (var apparel in pawn.apparel.WornApparel)
                {
					if (DrowningMod.settings.apparelStates.TryGetValue(apparel.def.defName, out bool apparelState) && apparelState)
                    {
						return false;
                    }
                }
			}
			return true;
        }
		public static bool InWaterTerrain(Pawn pawn)
        {
			var terrain = pawn.Position.GetTerrain(pawn.Map);
			if (terrain != null && DrowningMod.settings.drowningStates.ContainsKey(terrain.defName))
            {
				return true;
            }
			return false;
        }

		public static bool IsDownedOrIncapable(Pawn pawn)
        {
			if (pawn.Downed)
            {
				return true;
            }
			PawnCapacitiesHandler pawnCapacitiesHandler;
			if (pawn == null)
			{
				pawnCapacitiesHandler = null;
			}
			else
			{
				Pawn_HealthTracker health = pawn.health;
				pawnCapacitiesHandler = ((health != null) ? health.capacities : null);
			}
			PawnCapacitiesHandler pawnCapacitiesHandler2 = pawnCapacitiesHandler;
			if (pawnCapacitiesHandler2 == null)
			{
				return true;
			}
			if (pawnCapacitiesHandler2.GetLevel(PawnCapacityDefOf.Consciousness) < 0.1f)
			{
				return true;
			}
			if (pawnCapacitiesHandler2.GetLevel(PawnCapacityDefOf.Moving) < 0.1f)
			{
				return true;
			}
			return false;
		}
	}
}

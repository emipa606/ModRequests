using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Umbrellas {
	[HarmonyPatch(typeof(ThoughtWorker_InSunlight), "CurrentStateInternal")]
	class RBSunlightAffecterPrefix
	{
		static void Postfix(Pawn p, ref ThoughtState __result)
		{
			if (UmbrellaDefMethods.HasUmbrella(p))
			{
                __result = ThoughtState.Inactive;
			}
		}
	}

    [HarmonyPatch(typeof(Pawn_MindState), "MindStateTick")]
    class RBMindStatePrefix {

        static bool Prefix(Pawn_MindState __instance) {
            if (!__instance.pawn.NonHumanlikeOrWildMan() && (UmbrellaDefMethods.HasUmbrella(__instance.pawn) || (RimbrellasMod.settings.cowboyHatsPreventSoakingWet && UmbrellaDefMethods.HasCowboyHat(__instance.pawn)))) {
				if (__instance.wantsToTradeWithColony) {
					TradeUtility.CheckInteractWithTradersTeachOpportunity(__instance.pawn);
				}
				if (__instance.meleeThreat != null && !__instance.MeleeThreatStillThreat) {
					__instance.meleeThreat = null;
				}
				__instance.mentalStateHandler.MentalStateHandlerTick();
				__instance.mentalBreaker.MentalBreakerTick();
				__instance.inspirationHandler.InspirationHandlerTick();
				if (!__instance.pawn.GetPosture().Laying()) {
					__instance.applyBedThoughtsTick = 0;
				}
				if (__instance.pawn.IsHashIntervalTick(100)) {
					if (__instance.pawn.Spawned) {
						int regionsToScan = __instance.anyCloseHostilesRecently ? 24 : 18;
						__instance.anyCloseHostilesRecently = PawnUtility.EnemiesAreNearby(__instance.pawn, regionsToScan, passDoors: true);
					}
					else {
						__instance.anyCloseHostilesRecently = false;
					}
				}
				if (__instance.WillJoinColonyIfRescued && __instance.AnythingPreventsJoiningColonyIfRescued) {
					__instance.WillJoinColonyIfRescued = false;
				}
				if (__instance.pawn.Spawned && __instance.pawn.IsWildMan() && !__instance.WildManEverReachedOutside && __instance.pawn.GetRoom() != null && __instance.pawn.GetRoom().TouchesMapEdge) {
					__instance.WildManEverReachedOutside = true;
				}
				if (Find.TickManager.TicksGame % 123 == 0 && __instance.pawn.Spawned && __instance.pawn.RaceProps.IsFlesh && __instance.pawn.needs.mood != null) {
					__instance.pawn.Drawer.renderer.graphics.ResolveAllGraphics();

					TerrainDef terrain = __instance.pawn.Position.GetTerrain(__instance.pawn.Map);
					if (terrain.traversedThought != null) {
						__instance.pawn.needs.mood.thoughts.memories.TryGainMemoryFast(terrain.traversedThought);
					}
					/*WeatherDef curWeatherLerped = __instance.pawn.Map.weatherManager.CurWeatherLerped;
					if (curWeatherLerped.exposedThought != null && !__instance.pawn.Position.Roofed(__instance.pawn.Map)) {
						__instance.pawn.needs.mood.thoughts.memories.TryGainMemoryFast(curWeatherLerped.exposedThought);
					}*/ //this code would assign the Soaking Wet thought to a pawn outside in the rain, but we've already checked that the pawn has an umbrella
				}
				if (GenLocalDate.DayTick(__instance.pawn) == 0) {
					__instance.interactionsToday = 0;
				}
				return false;
            }
			return true;
        }
    }
    /*[HarmonyPatch(typeof(Pawn_MindState),"MindStateTick")]
    class MindStatePatch {

        static void Postfix(Pawn_MindState __instance) {
            if (!__instance.pawn.NonHumanlikeOrWildMan()) { // probably inefficient but whatever
                if (Find.TickManager.TicksGame % 123 == 0 && __instance.pawn.Spawned && __instance.pawn.RaceProps.IsFlesh && __instance.pawn.needs.mood != null) { // same logic as original function
                    if (__instance.pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDef.Named("SoakingWet")) != null && !__instance.pawn.Map.terrainGrid.TerrainAt(__instance.pawn.Position).IsWater) {
                        // statement above: if the pawn has a Soaking Wet thought and the terrain at its position is not water
                        //next: check if pawn has an umbrella
                        if (UmbrellaDefMethods.HasUmbrella(__instance.pawn)) {
                            __instance.pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("SoakingWet"));
                        }
                    }
                    if (!(RimbrellasMod.settings.showUmbrellasWhenInside) && Find.TickManager.TicksGame % 246 == 0) {
                        //this should run about once per second (could later be lowered to improve performance) and all it does is call this function in case the pawn has gone inside
                        __instance.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                    }
                }
            }
        }
    }*/
}

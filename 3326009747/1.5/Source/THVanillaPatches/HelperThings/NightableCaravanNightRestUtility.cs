using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace THVanillaPatches.HelperThings
{
	public static class NightableCaravanNightRestUtility
	{
		private const float WakeUpHour = 6f;
		private const float RestStartHour = 22f;

		private const float NocturnalWakeUpHour = 18f;
		private const float NocturnalRestStartHour = 10f;
		
		public static bool IsMostlyNocturnalPawns(this Caravan caravan)
		{
			int requiredPawns = caravan.pawns.Count / 2;

			return caravan.pawns.InnerListForReading.Count(IsNocturnal) > requiredPawns;
		}
        
		private static bool IsNocturnal(Pawn pawn)
		{
			if (pawn?.story?.traits?.HasTrait(THVanillaPatchesDefsOf.NightOwl) ?? false)
			{
				return true;
			}
			
			return pawn.ideo?.Ideo?.HasMeme(MemeDefOf.Darkness) ?? false;
		}
			
		public static bool RestingNowAt(int tile, bool nocturnal)
		{
			return WouldBeRestingAt(tile, GenTicks.TicksAbs, nocturnal);
		}

		public static bool WouldBeRestingAt(int tile, long ticksAbs, bool nocturnal)
		{
			float currentTime = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
			
			if (nocturnal)
			{
				return (double) currentTime < NocturnalWakeUpHour && (double) currentTime > NocturnalRestStartHour;
			}

			return (double) currentTime < WakeUpHour || (double) currentTime > RestStartHour;
		}
	}
}
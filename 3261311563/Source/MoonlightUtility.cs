using Verse;
using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using static Moonlight.ModSettings_Moonlight;

namespace Moonlight
{
	[StaticConstructorOnStartup]
	static class MoonlightUtility
	{
        public static int day = 8;
        static float[] brightnessEdge = new float[] { 0.7f, 0.7425f, 0.785f, 0.8275f, 0.87f, 0.9125f, 0.955f, 1, 1, 0.955f, 0.9125f, 0.87f, 0.8275f, 0.785f, 0.7425f };
		public static float[] brightnessMid = new float[] { 0.35f, 0.45f, 0.55f, 0.65f, 0.75f, 0.85f, 0.95f, 1, 1, 0.95f, 0.85f, 0.75f, 0.65f, 0.55f, 0.45f };
		static WeatherDef[] weatherDefs;
		static int updateCooldown = 0;

		static MoonlightUtility()
		{
			List<WeatherDef> workingList = new List<WeatherDef>();
			foreach(var def in DefDatabase<WeatherDef>.AllDefs)
				if (!def.HasModExtension<Weather>()) 
					workingList.Add(def);
			
			weatherDefs = workingList.ToArray();
			RefreshCache();
		}

		public static void RefreshCache()
		{
			updateCooldown = 0;
			if (Current.ProgramState != ProgramState.Entry) LongEventHandler.QueueLongEvent(() => FindMiddle(true), null, false, null);
			LongEventHandler.QueueLongEvent(RecalculateArray, null, false, null);
		}

		//Sets the frame of reference (for the timezone) based on the middlemost world position when the player has multiple colonies. It also sets the static field "day"
        public static int FindMiddle(bool forceUpdate = false)
        {
			if (!forceUpdate && --updateCooldown > 0) return 0;

			//Prepare list of coordinates to analyze
			var maps = Find.Maps;
			var length = maps.Count;
			if (length < 1) return 0;

			var worldGrid = Find.WorldGrid;
			var mapslongitudes = new List<float>();
			for (int i = length; i-- > 0;)
			{
				mapslongitudes.Add(worldGrid.LongLatOf(maps.ElementAt(i).Tile).x);
			}
			
			float longitude;

			//If we only have 1 map, the answer is simple and we can skip all the complicated math
			if (mapslongitudes.Count == 1) longitude = mapslongitudes.ElementAt(0);
			else
			{
				//Find averages for solution A (primemerdian) and B (antimerdian)
				float[] longitudes = new float[2] {mapslongitudes.Average(), 0};

				//Solution B
				foreach (var mapCoord in mapslongitudes)
				{
					longitudes[1] += (mapCoord < 0) ? mapCoord + 360 : mapCoord;
				}
				longitudes[1] /= mapslongitudes.Count;

				//Analyze results
				float[] distances = new float[2];
				foreach (var mapCoord in mapslongitudes)
				{
					distances[0] += Math.Abs(longitudes[0] - mapCoord);
					distances[1] += Math.Abs(longitudes[1] - (mapCoord < 0 ? mapCoord + 360 : mapCoord));
				}

				//Return the coordinate back to its original value that Rimworld recognizes
				if (longitudes[1] > 180) longitudes[1] -= 360;
			
				//Pick the best solution
				longitude = distances[0] < distances[1] ? longitudes[0] : longitudes[1];
			}

			day = GenDate.DayOfSeason(Current.gameInt.tickManager.TicksAbs, longitude);
			long time = Current.gameInt.tickManager.TicksAbs + GenDate.LocalTicksOffsetFromLongitude(longitude);

			//Debug
			if (logging && Prefs.DevMode)
			{
				Log.Message("[Moonlight] Local time: " + GenMath.PositiveModRemap(time, 2500, 24) + " and day: " + day);
			}

			//Return
            return GenMath.PositiveModRemap(time, 2500, 24);
        }
        public static void RecalculateArray()
		{
			//Recalculate midnight darkness
			float diff = brightest - darkest;
			if (diff != 0f) diff /= 6;

			brightnessMid = new float[]
			{
				darkest,
				darkest + diff,
				darkest + (diff * 2),
				darkest + (diff * 3),
				darkest + (diff * 4),
				darkest + (diff * 5),
				darkest + (diff * 6),
				brightest,
				brightest,
				darkest + (diff * 6),
				darkest + (diff * 5),
				darkest + (diff * 4),
				darkest + (diff * 3),
				darkest + (diff * 2),
				darkest + diff
			};

			//Recalculate edge darkness
			float halfDarkest = brightest - (diff / 2);
			diff = brightest - halfDarkest;
			if (diff != 0f) diff /= 6;

			brightnessEdge = new float[]
			{ 
				halfDarkest,
				halfDarkest + diff,
				halfDarkest + (diff * 2),
				halfDarkest + (diff * 3),
				halfDarkest + (diff * 4),
				halfDarkest + (diff * 5),
				halfDarkest + (diff * 6),
				brightest,
				brightest,
				halfDarkest + (diff * 6),
				halfDarkest + (diff * 5),
				halfDarkest + (diff * 4),
				halfDarkest + (diff * 3),
				halfDarkest + (diff * 2),
				halfDarkest + diff
			};

			UpdateMoonlight();
		}
		public static void UpdateMoonlight(bool resetCooldown = false)
		{
			if (resetCooldown) updateCooldown = 60;
			//Debug before...
			UnityEngine.Color originalValue = default;
			if (logging && Prefs.DevMode && Current.ProgramState == ProgramState.Playing) originalValue = DefDatabase<WeatherDef>.GetNamed("Clear").workerInt.skyTargets[0].colors.sky;

			if (weatherDefs == null)
			{
				Log.Warning("[Moonlight] No weather defs found.");
				return;
			}
			for (int i = weatherDefs.Length; i-- > 0;)
			{
				var def = weatherDefs[i];
				def.Worker.skyTargets[0].colors.sky = def.skyColorsNightMid.sky * brightnessMid[day]; //0 is the midnight brightness
				def.Worker.skyTargets[1].colors.sky = def.skyColorsNightEdge.sky * brightnessEdge[day]; //1 is the brightness between dusk and midnight
			}

			//Debug after...
			if (logging && originalValue != default)
			{
				Log.Message("[Moonlight] Base sky changed from " + originalValue + " to " + DefDatabase<WeatherDef>.GetNamed("Clear").workerInt.skyTargets[0].colors.sky + " for day " + day);
			}
		}
    }
}

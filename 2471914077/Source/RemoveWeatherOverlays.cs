using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;


namespace yipnz.RemoveWeatherOverlays
{
    [StaticConstructorOnStartup]
    static class OverlayPatcher
    {

        static OverlayPatcher()
        {
            // Harmony.DEBUG = true;

            Log.Message("RemoveWeatherOverlays - Patching Verse.WeatherWorker.DrawOverlays()");

            var instance = new Harmony("yipnz.RWO");
            
            instance.PatchAll();
        }

    }

    class DrawOverlayPatch
    {
        // skip autosave
		[HarmonyPatch(typeof(WeatherWorker), nameof(WeatherWorker.DrawWeather))]
		public class WeatherWorker_DrawWeather_Patch
		{
			public static bool Prefix()
			{
				return false;
			}
		}

    }
}
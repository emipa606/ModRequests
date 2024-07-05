using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
namespace RimWorldRealFoW
{
    [DefOf]
    public class FoWDef
    {
        static FoWDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FoWDef));
        }

        public static JobDef SurveilCameraConsole;

        public static ThingDef CameraConsole;

        public static StatDef DayVisionEffectiveness;

        public static StatDef NightVisionEffectiveness;

		public static ThingDef Mote_SoundWave;

    }
}

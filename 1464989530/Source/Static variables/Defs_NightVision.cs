// Nightvision NightVision NightVisionStat.cs
// 
// 01 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision
{
    /// <summary>
    /// Night vision defs
    /// </summary>
    [DefOf]
    [UsedImplicitly]
    public static class Defs_NightVision
    {
        // Stat defs used for stat reports
        [UsedImplicitly]
        public static StatDef LightSensitivity = StatDef.Named("LightSensitivity");

        [UsedImplicitly]
        public static StatDef NightVision = StatDef.Named("NightVision");

        // Surgery Recipe that is added to animals 
        [UsedImplicitly]
        public static RecipeDef ExtractTapetumLucidum;

    }
}
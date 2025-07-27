using System;
using RimWorld;
using Verse;

namespace ThumperDef
{
    [DefOf]
    public static class ThumperDefOf
    {
        public static ThingDef VIC_Thumper; //The name of my building xml defName
        public static ThingDef VIC_ThumperXL;
        public static SoundDef VIC_Thumper_hit;
        public static SoundDef VIC_Thumper_hit_deep;
        public static SoundDef VIC_Thumper_boom;
        public static ThoughtDef VIC_ThumperAnnoyance;

        static ThumperDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThumperDefOf));
        }
    }
}
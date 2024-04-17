using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CryptoRestore
{                                  
    public static class ResourceBank
    {
        public static readonly HediffDef[] AgeHediffs;     

        static ResourceBank()
        {                      
            //minor optimization
            AgeHediffs = DefDatabase<HediffDef>.AllDefs.Where(hediff => hediff.chronic).ToArray();  
        }


        [DefOf]
        public static class HediffDefOf
        {
            public static HediffDef LuciferiumAddiction;
            public static HediffDef LuciferiumHigh;

            public static HediffDef MissingBodyPart;
        }

        [DefOf] 
        public static class MentalStateDefOf
        {
            public static MentalStateDef Berserk;
        }

        [DefOf]
        public static class ThoughtDefOf
        {
            public static ThoughtDef PsycheNightmare;
        }

        [DefOf]
        public static class NeedDefOf
        {
            public static NeedDef Chemical_Luciferium;
        }

        public static class Settings
        {
            public static int HealChronicHediffCooldownBase = GenDate.TicksPerQuadrum / 2;
            public static int SmallHealChronicHediffCooldownBase = GenDate.TicksPerDay / 2;
            public static int HealBadHediffCooldownBase = GenDate.TicksPerDay * 2;
            public static float InjuryHealPerTick = 0.0005f;
            public static int AgeDownTickRate = 30;
            public static int NightmareTicks = GenDate.TicksPerQuadrum / 2;
        }                 

        public static class Strings
        {
            public static string AgeBiological = "AgeBiological";         
        }
    }
}
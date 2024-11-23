using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;

namespace HDream
{
    public class ItemWishInfo : IExposable
    {
        public ThingDef def;
        public int needAmount = -1;
        public QualityCategory minQuality = (QualityCategory)100;
        public List<Type> neededComp;
        public List<ItemNeededStat> neededStats;
        public List<ThingDef> fromRessource;

        public bool useDefaultParam = true;


        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "thing");
            Scribe_Values.Look(ref needAmount, "needAmount", -1);
            Scribe_Values.Look(ref minQuality, "minQuality", (QualityCategory)100);
            Scribe_Collections.Look(ref neededComp, "neededComp");
            Scribe_Collections.Look(ref neededStats, "neededStats", LookMode.Deep);
            Scribe_Collections.Look(ref fromRessource, "fromRessource");
        }
    }
}

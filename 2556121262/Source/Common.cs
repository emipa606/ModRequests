using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using AlienRace;

namespace HalfDragons
{
    public static class HD_Common
    {
        public static ThingDef_AlienRace halfDragonRace = DefDatabase<ThingDef_AlienRace>.GetNamed("HalfDragon");
        public static float dragonRageMaxSeverity = 0.3f;
        public static NeedDef dragonBlood = DefDatabase<NeedDef>.GetNamed("HD_DragonBlood");
    }

    [DefOf]
    public static class HD_HediffDefOf
    {
        static HD_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HD_HediffDefOf));
        }

        public static HediffDef HD_dragonRage;
        public static HediffDef HD_regenerativeExhaustion;
    }
}

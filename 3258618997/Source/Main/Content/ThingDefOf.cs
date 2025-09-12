using System;
using RimWorld;
using Verse;

namespace Necro
{
    
    [DefOf]
    public static class ThingDefOf
    {
        
        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }

        
        public static ThingDef CorruptedShip;

        
        public static ThingDef CorruptedShipLanding;

        
        public static ThingDef Filth_SpentAcid;

        
        public static ThingDef Necronoid_HowlingSpider;

        
        public static ThingDef Necronoid_ExecutionerGhoul;

        
        public static ThingDef Necronoid_FleshMarine;

        
        public static ThingDef Necronoid_Chupacabra;

        
        public static ThingDef CorruptedDropShip;

        
        public static ThingDef CorruptedDropShipLanding;

        
        public static ThingDef Necro_InfestedBiomass;
    }
}

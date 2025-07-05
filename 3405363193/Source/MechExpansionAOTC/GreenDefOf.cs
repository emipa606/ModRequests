using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorld
{
    [DefOf]
    public static class GreenDefOf
    {
        public static ThingDef BandNodeStellar_GAOTCE_Mech;
        public static ThingDef BasicRechargerStellar_GAOTCE_Mech;
        public static ThingDef StandardRechargerStellar_GAOTCE_Mech;

        public static ThingDef BandNodeCosmic_GAOTCE_Mech;
        public static ThingDef BasicRechargerCosmic_GAOTCE_Mech;
        public static ThingDef StandardRechargerCosmic_GAOTCE_Mech;

        public static ThingDef BandNodeEternium_GAOTCE_Mech;
        public static ThingDef BasicRechargerEternium_GAOTCE_Mech;
        public static ThingDef StandardRechargerEternium_GAOTCE_Mech;

        public static ThingDef BandNodeStable_GAOTCE_Mech;
        public static ThingDef BasicRechargerStable_GAOTCE_Mech;
        public static ThingDef StandardRechargerStable_GAOTCE_Mech;

        static GreenDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GreenDefOf));
        }
    }
}

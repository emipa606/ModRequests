using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolopes;

namespace Evolopes
{
    [DefOf]
    public static class EvolopesDefOf
    {
        public static PawnKindDef Boomalope;
        public static PawnKindDef EvoChocolope;
        public static PawnKindDef EvoDevilope;
        public static PawnKindDef EvoJellope;
        public static PawnKindDef EvoMilkalope;
        public static PawnKindDef EvoNeutroalope;
        public static PawnKindDef EvoMegaBoomalope;
        public static PawnKindDef EvoMegaLuciferope;
        public static ThingDef Boomium;
        public static ThingDef Chocolatium;
        public static ThingDef Devilium;
        public static ThingDef Jellium;
        public static ThingDef Milkium;
        public static ThingDef Neutrium;
        public static ThingDef MegaLuciferium;

        static EvolopesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EvolopesDefOf));
        }
    }
}   
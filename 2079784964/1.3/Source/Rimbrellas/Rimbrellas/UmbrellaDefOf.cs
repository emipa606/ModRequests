using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
namespace Umbrellas {
    [DefOf]
    public static class UmbrellaDefOf {
        public static ThingDef Umbrella;
        public static ThingDef Parasol;
        public static ThingDef FoldableUmbrella;
        public static ThingDef FoldableParasol;
        public static ThingDef SteelUmbrella;
    }
    public static class UmbrellaDefMethods {
        public static readonly List<ThingDef> UmbrellaDefs = new List<ThingDef>() {
            UmbrellaDefOf.Umbrella,
            UmbrellaDefOf.Parasol,
            UmbrellaDefOf.FoldableUmbrella,
            UmbrellaDefOf.FoldableParasol,
            UmbrellaDefOf.SteelUmbrella
        };

        public static readonly List<ThingDef> HideableUmbrellaDefs = new List<ThingDef>() {
            UmbrellaDefOf.FoldableUmbrella,
            UmbrellaDefOf.FoldableParasol
        };

        public static bool HasUmbrella(Pawn pawn) {
            foreach (Thing piece in pawn.apparel.WornApparel) {
                if (UmbrellaDefs.Contains(piece.def)) {
                    return true;
                }
            }
            return false;
        }

        public static bool HasCowboyHat(Pawn pawn) {
            foreach (Thing piece in pawn.apparel.WornApparel) {
                if (piece.def.Equals(ThingDef.Named("Apparel_CowboyHat"))) {
                    return true;
                }
            }
            return false;
        }
    }
    
}

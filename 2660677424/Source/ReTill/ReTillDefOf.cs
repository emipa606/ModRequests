using RimWorld;
using Verse;

namespace ReTill
{
    [DefOf]
    public static class ReTillDefOf
    {
        [MayRequire("")] // VanillaExpanded.VPlantsE, but removed by Oreno.VPlantsE.NoTilledSoil
        public static TerrainDef VCE_TilledSoil;

        [MayRequire("GT.Sam.TilledSoil")]
        public static TerrainDef GT_SoilTilled;

        [MayRequireIdeology]
        public static TerrainDef FungalGravel;

        [MayRequire("LadyCookie.Farmland")]
        public static TerrainDef Farmland;

        [MayRequire("LadyCookie.Farmland")]
        public static TerrainDef FarmlandRich;

        public static JobDef ReTill_FinishFrame;

        public static JobDef ReTill_PlaceAndBuildTilledSoil;

        static ReTillDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ReTillDefOf));
        }
    }
}

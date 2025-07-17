using RimWorld;
using Verse;

namespace THVanillaPatches
{
    [DefOf]
    public static class THVanillaPatchesDefsOf
    {
        //Vanilla stuff
        public static PawnCapacityDef Metabolism;
        [MayRequireBiotech]
        public static XenotypeDef Waster;
        [MayRequireBiotech]
        public static GeneDef DiseaseFree;
        public static TraitDef NightOwl;
        
        //Patches
        public static THPatchDef THVP_PolyculeJealousPatch;
        public static THPatchDef THVP_ApparelCoveragePatch;
        
        //Other
        public static JobDef THVP_TransferPrisoner;
        public static TraitDef THVP_DislikesPeople;
    }
}
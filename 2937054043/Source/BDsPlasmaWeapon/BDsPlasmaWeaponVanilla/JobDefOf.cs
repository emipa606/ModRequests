using RimWorld;
using Verse;

namespace BDsPlasmaWeaponVanilla
{


    [DefOf]
    public static class JobDefOf
    {
        public static JobDef BDP_JobDefRefillFromFiller;

        public static JobDef BDP_FlickLizionCooler;

        public static JobDef BDP_Extinguish;
    }

    [DefOf]
    public static class DamageArmorCategoryDefOf
    {
        public static DamageArmorCategoryDef Heat;

        static DamageArmorCategoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DamageArmorCategoryDefOf));
        }
    }
}

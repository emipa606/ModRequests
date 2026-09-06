using RimWorld;
using Verse;

namespace AddieSolarHunter
{
    [DefOf]
    public static class VPESH_DefOf
    {
        public static ThingDef VPESH_JumpingPawn;
        public static FleckDef AirPuff;

        public static SoundDef JumpPackLand;

        public static SoundDef Addie_GoldenGunSoundDeadshot;

        public static SoundDef Addie_GoldenGunSoundNighthawk;

        public static ThingDef Addie_GoldenGunBullet_Nighthawk;

        public static ThingDef Addie_GoldenGunBullet_Deadshot;

        public static HediffDef Addie_VPESH_DeadshotHediff;

        static VPESH_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(VPESH_DefOf));
        }
    }
}
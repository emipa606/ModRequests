using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Roos_Satyr_Xenotype
{
    [DefOf]
    public static class RBSF_DefOf
    {
        public static AbilityDef RBSF_MelodicSonata;
        public static AbilityDef RBSF_MelodicHealing;
        public static AbilityDef RBSF_MelodicWarcall;
        public static AbilityDef RBSF_MelodicLullaby;
        public static AbilityDef RBSF_MelodicHymn;

        public static HediffDef RBSF_HeardHealing;
        public static HediffDef RBSF_HeardWarcall;
        public static HediffDef RBSF_HeardLullaby;
        public static HediffDef RBSF_HeardHymn;

        public static ThoughtDef RBSF_HeardSonata;

        public static HediffDef RBSF_PlayingTune;

        public static WeaponClassDef RBSF_Instrument;

        public static TraitDef RBSF_Uncouth;

        public static GeneDef RBSF_Virtuoso;
        public static GeneDef RBM_UnguligradeLegs;

        public static InteractionDef RBSF_Vulgarity;

        public static JobDef UseMusicSheet;

        public static SoundDef RBSF_LullabySound;
        public static SoundDef RBSF_HealingSound;
        public static SoundDef RBSF_WarcallSound;
        public static SoundDef RBSF_SonataSound;
        public static SoundDef RBSF_HymnSound;
        public static SoundDef RBSF_MelodicElegySucceed;
        public static SoundDef RBSF_MelodicElegyFail;
    }

    public static class RBSF_DefLists
    {
        public static List<SoundDef> RBSF_SongSounds = new List<SoundDef>()
        {
            RBSF_DefOf.RBSF_LullabySound,
            RBSF_DefOf.RBSF_HealingSound,
            RBSF_DefOf.RBSF_WarcallSound,
            RBSF_DefOf.RBSF_SonataSound,
            RBSF_DefOf.RBSF_HymnSound
        };
    }
}

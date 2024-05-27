using System;
using Verse;
using Verse.Sound;
using RimWorld;
using HLVR;

namespace HLVR
{
    class ApparelEquipSoundIV : ThingComp
    {
        public override void Notify_Equipped(Pawn pawn)
        {
            SoundDef sound = HLVRDefOf.HEV_EquipIV as SoundDef;
            sound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
            base.Notify_Equipped(pawn);
        }


    }

    class CompProperties_ApparelEquipSoundIV : CompProperties
    {
        public CompProperties_ApparelEquipSoundIV()
        {
            this.compClass = typeof(ApparelEquipSoundIV);
        }
    }

    class ApparelEquipSoundV : ThingComp
    {
        public override void Notify_Equipped(Pawn pawn)
        {
            SoundDef sound = HLVRDefOf.HEV_EquipValve as SoundDef;
            sound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
            base.Notify_Equipped(pawn);
        }
    }

    class CompProperties_ApparelEquipSoundV : CompProperties
    {
        public CompProperties_ApparelEquipSoundV()
        {
            this.compClass = typeof(ApparelEquipSoundV);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.Sound;

namespace CQCTakedown
{
    public class GloryKillCombatEffect : CPAbilityUser.Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            var result = base.TryCastShot();
            Log.Message("Try Cast Shot");
            if (result)
            {
                Log.Message("Success");
                var target = TargetsAoE[0];
                if (target.HasThing && target.Thing is Pawn pawn)
                {
                    Log.Message("Pawn is Pawn");
                    switch (pawn.gender)
                    {
                        case Gender.Male:
                            SoundStarter.PlayOneShot(SoundDef.Named("CP_GloryKillTakedownSFXMale"), new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                            return result;
                        case Gender.Female:
                            SoundStarter.PlayOneShot(SoundDef.Named("CP_GloryKillTakedownSFXFemale"), new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                            return result;
                    }
                }
            }
            SoundStarter.PlayOneShot(SoundDef.Named("CP_GloryKillTakedownSFXMale"), new TargetInfo(CasterPawn.PositionHeld, CasterPawn.MapHeld, false));
            return result;
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            base.PostCastShot(inResult, out outResult);
            Log.Message("PostCast Shot");
            var target = TargetsAoE[0];
            if (target.HasThing && target.Thing is Pawn pawn)
            {
                Log.Message("Pawn is Pawn");
                switch (pawn.gender)
                {
                    case Gender.Male:
                        SoundStarter.PlayOneShot(SoundDef.Named("CP_GloryKillTakedownSFXMale"), new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                        return;
                    case Gender.Female:
                        SoundStarter.PlayOneShot(SoundDef.Named("CP_GloryKillTakedownSFXFemale"), new TargetInfo(pawn.PositionHeld, pawn.MapHeld, false));
                        return;
                }
            }
            SoundStarter.PlayOneShot(SoundDef.Named("CP_GloryKillTakedownSFXFemale"), new TargetInfo(CasterPawn.PositionHeld, CasterPawn.MapHeld, false));

        }
    }
}

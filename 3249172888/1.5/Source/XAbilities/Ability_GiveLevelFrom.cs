using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_GiveLevelFrom : Ability
    {

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            Hediff_PsycastAbilities target_psycasts = (target.Thing as Pawn).Psycasts();
            if (target_psycasts == null || pawn.Psycasts() == null)
            {
                Messages.Message("VPE.MustBePsycaster".Translate(), MessageTypeDefOf.CautionInput);
                return false;
            }

            if (pawn.Psycasts().level < 2)
            {
                Messages.Message("VPEAC.NotEnoughXp".Translate(), MessageTypeDefOf.CautionInput);
                return false;
            }
            if (pawn.Psycasts().points < 1)
            {
                Messages.Message("VPEAC.NotEnoughPoints".Translate(), MessageTypeDefOf.CautionInput);
                return false;
            }
            return base.ValidateTarget(target, showMessages);
        }


        public override void Cast(GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets[0].Thing as Pawn;
            Hediff_PsycastAbilities target_psycasts = target.Psycasts();
            Hediff_PsycastAbilities caster_psycasts = pawn.Psycasts();

            var transfered_power = Hediff_PsycastAbilities.ExperienceRequiredForLevel(caster_psycasts.level);
            target_psycasts.GainExperience(transfered_power);

            caster_psycasts.ChangeLevel(-1);
            //MoteMaker.MakeAttachedOverlay(target, VPEP_DefOf.VPEP_BrainCutSlash, Vector3.zero);
        }
    }
}

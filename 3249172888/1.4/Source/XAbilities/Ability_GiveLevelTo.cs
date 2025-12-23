using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class Ability_GiveLevelTo : Ability
    {

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            Hediff_PsycastAbilities target_psycasts = (target.Thing as Pawn).Psycasts();
            if (target_psycasts == null || pawn.Psycasts() == null)
            {
                Messages.Message("VPE.MustBePsycaster".Translate(), MessageTypeDefOf.CautionInput);
                return false;
            }

            if (pawn.getTotalXP() < 
                Hediff_PsycastAbilities.ExperienceRequiredForLevel(target_psycasts.level+1)
                - target_psycasts.experience)
            {
                Messages.Message("VPEAC.NotEnoughXp".Translate(), MessageTypeDefOf.CautionInput);
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

            var transfered_power = Hediff_PsycastAbilities.ExperienceRequiredForLevel(target_psycasts.level + 1)
                - target_psycasts.experience;


            caster_psycasts.experience -= transfered_power;
            while (caster_psycasts.experience < 0 && caster_psycasts.level > 0)
            {
                caster_psycasts.experience += Hediff_PsycastAbilities.ExperienceRequiredForLevel(caster_psycasts.level);
                caster_psycasts.ChangeLevel(-1);
            }

            target.Psycasts().GainExperience( transfered_power + .04f);
            //MoteMaker.MakeAttachedOverlay(target, VPEP_DefOf.VPEP_BrainCutSlash, Vector3.zero);
        }
    }
}


using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using VPEArchocaster;
using Ability = VFECore.Abilities.Ability;

namespace VPEArchocaster
{
    public class AbilityExtension_TargetValidator : AbilityExtension_AbilityMod
    {
        public bool notOfCasterFaction;
        public bool ofCasterFaction;
        public bool allowPrisoners;
        public bool targetMustBePsycaster; //must be >= level 1
        public bool strongerPsycaster;
        public float staticPsycost = -1f;
        public HediffDef requiredHediffOnTarget;
        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            if (target.HasThing)
            {
                if (notOfCasterFaction && target.Thing.Faction == ability.pawn.Faction)
                {
                    if (throwMessages)
                    {
                        Messages.Message("VPEAC.TargetCannotBeSameFactionAsCaster".Translate(), MessageTypeDefOf.CautionInput);
                    }
                    return false;
                }
                if (ofCasterFaction)
                {
                    if (target.Thing.Faction != ability.pawn.Faction && (!allowPrisoners || target.Pawn.IsPrisonerOfColony is false && target.Pawn.IsSlaveOfColony is false))
                    {
                        if (throwMessages)
                        {
                            Messages.Message("VPEAC.TargetMustBeSameFactionAsCaster".Translate(), MessageTypeDefOf.CautionInput);
                        }
                        return false;
                    }
                }
                if (target.Pawn != null)
                {
                    if (allowPrisoners && target.Pawn.IsPrisonerOfColony is false && target.Pawn.IsSlaveOfColony is false)
                    {
                        if (throwMessages)
                        {
                            Messages.Message("VPEAC.TargetMustBePrisonerOrSlave".Translate(), MessageTypeDefOf.CautionInput);
                        }
                        return false;
                    }
                }

                Hediff_PsycastAbilities target_psycast = null;
                Hediff_PsycastAbilities caster_psycast = null;
                if (strongerPsycaster)
                {
                    targetMustBePsycaster = true;
                }
                if ((target.Thing as Pawn) != null)
                {
                    target_psycast = (target.Thing as Pawn).Psycasts();
                }
                if (ability.pawn != null)
                {
                    caster_psycast = ability.pawn.Psycasts();
                }


                if (targetMustBePsycaster)
                {
                    if(target_psycast == null || target_psycast.level < 1){
                        if (throwMessages)
                        {
                            Messages.Message("VPE.MustBePsycaster".Translate(), MessageTypeDefOf.CautionInput);
                        }
                        return false;
                    }
                }
                if (strongerPsycaster)
                {
                    if (caster_psycast.level < target_psycast.level)
                    {
                        if (throwMessages)
                        {
                            Messages.Message( TranslatorFormattedStringExtensions.Translate("VPEAC.NotStrongEnough", 
                                NamedArgumentUtility.Named((object)target.Pawn, "target"), 
                                NamedArgumentUtility.Named((object)ability.pawn, "caster")            )
                                , MessageTypeDefOf.CautionInput);
                        }
                        return false;
                    }
                }

            }
            return base.ValidateTarget(target, ability, throwMessages);
        }
    }
}
﻿using Verse;
using RimWorld;



namespace TargetingModes
{
    public static class TargetingModesUtility
    {

        public static readonly TargetingModeDef DefaultTargetingMode = TargetingModeDefOf.Standard;
        private const float TargModeChanceFactorOffsetPerTrainabilityOrder = 0.05f;
//        private static readonly object pawn;

        public static Command_SetTargetingMode SetTargetModeCommand(ITargetModeSettable isSettable) =>
            new Command_SetTargetingMode
            {
                defaultDesc = "CommandSetTargetingModeDesc".Translate().Resolve(),
                settable = isSettable
            };

        public static bool CanUseTargetingModes(this Thing instigator, ThingDef weapon)
        {
            // No instigator or instigator doesn't have CompTargetingMode
            if (instigator == null || !instigator.def.HasComp(typeof(CompTargetingMode)) || weapon == null)
                return false;

            // Melee attack
            //if (typeof(Pawn).IsAssignableFrom(weapon.thingClass) || weapon.IsMeleeWeapon)
            //    return true;

            if (typeof(Pawn).Equals(instigator))
            {
                
                // Explosive
                if (((Pawn)instigator).CurrentEffectiveVerb.verbProps.CausesExplosion)
                    return false;
            }

            if (typeof(Building_Turret).Equals(instigator))
            {
                // Explosive
                if (((Building_Turret)instigator).CurrentEffectiveVerb.verbProps.CausesExplosion)
                    return false;
            }

            return true;
        }
        
        public static BodyPartRecord ResolvePrioritizedPart(BodyPartRecord part, DamageInfo dinfo, Pawn pawn)
        {
            BodyPartRecord newPart = part;
            if (dinfo.Instigator.CanUseTargetingModes(dinfo.Weapon) && dinfo.Instigator.TryGetComp<CompTargetingMode>() !=null)
            {
                CompTargetingMode targetingComp = dinfo.Instigator.TryGetComp<CompTargetingMode>();
                TargetingModeDef targetingMode = targetingComp.GetTargetingMode();
                if (!part.IsPrioritizedPart(targetingMode))
                    newPart = RerollBodyPart(targetingMode, part, dinfo, pawn);
            }
            return newPart;
        }

        public static BodyPartRecord ResolvePrioritizedPart_External(BodyPartRecord part, DamageInfo dinfo, Pawn pawn)
        {
            BodyPartRecord newPart = part;
            if (dinfo.Instigator.CanUseTargetingModes(dinfo.Weapon) && dinfo.Instigator?.TryGetComp<CompTargetingMode>() != null)
            {
                CompTargetingMode targetingComp = dinfo.Instigator?.TryGetComp<CompTargetingMode>();
                TargetingModeDef targetingMode = targetingComp.GetTargetingMode();
                if (!part.IsPrioritizedPart(targetingMode))
                    newPart = RerollBodyPart(targetingMode, part, dinfo.Def, dinfo.Height, BodyPartDepth.Outside, pawn, dinfo.Instigator);
            }
            return newPart;
        }

        public static bool IsPrioritizedPart(this BodyPartRecord part, TargetingModeDef targetingMode) =>
            targetingMode.HasNoSpecifiedPartDetails ||
            targetingMode.PartsListContains(part.def) ||
            targetingMode.PartsOrAnyChildrenListContains(part) ||
            targetingMode.TagsListContains(part.def.tags);


        public static BodyPartRecord RerollBodyPart(TargetingModeDef targetingMode, BodyPartRecord bodyPart, DamageInfo dinfo, Pawn pawn) =>
            RerollBodyPart(targetingMode, bodyPart, dinfo.Def, dinfo.Height, dinfo.Depth, pawn, dinfo.Instigator);

        public static BodyPartRecord RerollBodyPart(TargetingModeDef targetingMode, BodyPartRecord bodyPart, DamageDef damDef, BodyPartHeight height, BodyPartDepth depth, Pawn pawn, Thing instigator)
        {
            for (int i = 0; i < targetingMode.RerollCount(pawn, instigator); i++)
            {
                BodyPartRecord newPart = pawn.health.hediffSet.GetRandomNotMissingPart(damDef, height, depth);
                if (newPart.IsPrioritizedPart(targetingMode))
                    return newPart;
            }
            return bodyPart;
        }

        public static bool IsCompetentWithWeapon(this Pawn pawn)
        {
            // Just to catch any weird edge cases; this check's conditions should never be satisfied
            if (pawn.skills == null || pawn.equipment == null)
                return false;

            if (pawn.equipment.Primary?.def.IsRangedWeapon == true && pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= TargetingModesSettings.MinimumSkillForRandomTargetingMode)
                return true;
            return pawn.skills.GetSkill(SkillDefOf.Melee).Level >= TargetingModesSettings.MinimumSkillForRandomTargetingMode;
        }

        public static void TryAssignRandomTargetingMode(this Pawn pawn)
        {
            if (TargetingModesSettings.raidersUseTargModes && pawn.TryGetComp<CompTargetingMode>() != null)
            {
                CompTargetingMode targetingComp = pawn.TryGetComp<CompTargetingMode>();
                TargetingModeDef newTargetingMode = DefDatabase<TargetingModeDef>.AllDefsListForReading.RandomElementByWeight(t => t.commonality);
                targetingComp.SetTargetingMode(newTargetingMode);
            }
        }

        public static float AdjustedChanceForAnimal(Pawn animal)
        {
            int orderOverIntermediate = animal.RaceProps.trainability.intelligenceOrder - TrainabilityDefOf.Intermediate.intelligenceOrder;

            // Animal needs at least intermediate intelligence to use targeting modes
            if (orderOverIntermediate >= 0)
                return (TargetingModesSettings.baseManhunterTargModeChance / 100 ) * (1 + orderOverIntermediate * TargModeChanceFactorOffsetPerTrainabilityOrder);
            return 0f;
        }

        public static bool IsPlayerControlledAnimal(this Pawn pawn) => pawn.Spawned && pawn.MentalStateDef == null && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer;

    }
}

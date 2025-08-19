﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace CPAbilityUser
{
    public class Verb_UseAbility : Verb_LaunchProjectile
    {
        public List<LocalTargetInfo> TargetsAoE = new List<LocalTargetInfo>();
        public Action<Thing> timeSavingActionVariable = null;
        public PawnAbility Ability { get; set; } = null;

        public VerbProperties_Ability UseAbilityProps => (VerbProperties_Ability) verbProps;
        public ProjectileDef_Ability AbilityProjectileDef => UseAbilityProps.defaultProjectile as ProjectileDef_Ability;
        public CompAbilityUser AbilityUserComp => CasterPawn.TryGetComp<CompAbilityUser>();

        protected override int ShotsPerBurst => verbProps.burstShotCount;

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            var result = verbProps?.defaultProjectile?.projectile?.explosionRadius ?? 1;
            if (UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties != null)
                if (UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties.showRangeOnSelect)
                    result = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties.range;
            return result;
        }

        protected virtual void UpdateTargets()
        {
            TargetsAoE.Clear();
            if (UseAbilityProps.AbilityTargetCategory == AbilityTargetCategory.TargetAoE)
            {
                //Log.Message("AoE Called");
                if (UseAbilityProps.TargetAoEProperties == null)
                    Log.Error("Tried to Cast AoE-Ability without defining a target class");

                var targets = new List<Thing>();

                //Handle TargetAoE start location.
                var aoeStartPosition = caster.PositionHeld;
                if (!UseAbilityProps.TargetAoEProperties.startsFromCaster)
                    aoeStartPosition = currentTarget.Cell;

                //Handle friendly fire targets.
                if (!UseAbilityProps.TargetAoEProperties.friendlyFire)
                {
                    targets = caster.Map.listerThings.AllThings.Where(x =>
                        x.Position.InHorDistOf(aoeStartPosition, UseAbilityProps.TargetAoEProperties.range) &&
                        UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) &&
                        x.Faction.HostileTo(Faction.OfPlayer)).ToList();
                }
                else if (UseAbilityProps.TargetAoEProperties.targetClass == typeof(Plant) ||
                         UseAbilityProps.TargetAoEProperties.targetClass == typeof(Building))
                {
                    targets = caster.Map.listerThings.AllThings.Where(x =>
                        x.Position.InHorDistOf(aoeStartPosition, UseAbilityProps.TargetAoEProperties.range) &&
                        UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType())).ToList();
                    foreach (var targ in targets)
                    {
                        var tinfo = new LocalTargetInfo(targ);
                        TargetsAoE.Add(tinfo);
                    }
                    return;
                }
                else
                {
                    targets.Clear();
                    targets = caster.Map.listerThings.AllThings.Where(x =>
                        x.Position.InHorDistOf(aoeStartPosition, UseAbilityProps.TargetAoEProperties.range) &&
                        UseAbilityProps.TargetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) &&
                        (x.HostileTo(Faction.OfPlayer) || UseAbilityProps.TargetAoEProperties.friendlyFire)).ToList();
                }

                var maxTargets = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties.maxTargets;
                var randTargets = new List<Thing>(targets.InRandomOrder());
                for (var i = 0; i < maxTargets && i < randTargets.Count(); i++)
                {
                    var tinfo = new TargetInfo(randTargets[i]);
                    if (UseAbilityProps.targetParams.CanTarget(tinfo))
                        TargetsAoE.Add(new LocalTargetInfo(randTargets[i]));
                }
            }
            else
            {
                TargetsAoE.Clear();
                TargetsAoE.Add(currentTarget);
            }
        }

        public virtual void PostCastShot(bool inResult, out bool outResult)
        {
            outResult = inResult;
        }

        public override bool Available() => true;

        protected override bool TryCastShot()
        {
            Log.Message("Try Cast Shot Called");
            //Log.Message("Cast");
            var result = false;
            TargetsAoE.Clear();
            UpdateTargets();
            var burstShots = ShotsPerBurst;
            if (UseAbilityProps.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1)
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            if (UseAbilityProps.mustHaveTarget && TargetsAoE.Count == 0)
            {
                Messages.Message("AU_NoTargets".Translate(), MessageTypeDefOf.RejectInput);
                Ability.Notify_AbilityFailed(true);
                return false;
            }
            for (var i = 0; i < TargetsAoE.Count; i++)
            {
                //                for (int j = 0; j < burstshots; j++)
                //                {
                if (verbProps.defaultProjectile != null) //ranged attacks WILL have projectiles
                {
                    Log.Message("Yes Projectile");
                    var attempt = TryLaunchProjectile(verbProps.defaultProjectile, TargetsAoE[i]);
                    ////Log.Message(TargetsAoE[i].ToString());
                    if (attempt != null)
                    {
                        if (attempt == true) result = true;
                        if (attempt == false) result = false;
                    }   
                }
                else //melee attacks WON'T have projectiles
                {
                    Log.Message("No Projectile");
                    var victim = TargetsAoE[i].Thing;
                    if (victim != null)
                    {
                        Log.Message("Yes victim");
                        if (victim is Pawn pawnVictim)
                        {
                            Log.Message("Yes victim is pawn");
                            AbilityEffectUtility.ApplyMentalStates(pawnVictim, CasterPawn, UseAbilityProps.mentalStatesToApply, UseAbilityProps.abilityDef, null);
                            AbilityEffectUtility.ApplyHediffs(pawnVictim, CasterPawn, UseAbilityProps.hediffsToApply, null);
                            AbilityEffectUtility.SpawnSpawnables(UseAbilityProps.thingsToSpawn, pawnVictim, victim.MapHeld, victim.PositionHeld);   
                        }
                    }
                    else
                    {
                        Log.Message("Victim is null");
                        AbilityEffectUtility.SpawnSpawnables(UseAbilityProps.thingsToSpawn, CasterPawn, CasterPawn.MapHeld, CasterPawn.PositionHeld);
                    }
                }
                //                }
            }

            PostCastShot(result, out result);
            if (result == false)
            {
                Ability.Notify_AbilityFailed(UseAbilityProps.refundsPointsAfterFailing);
            }
            return result;
        }

        private bool debugMode = false;

        private void DebugMessage(string s)
        {
            if (debugMode) Log.Message(s);
        }

      
        protected bool? TryLaunchProjectile(ThingDef projectileDef, LocalTargetInfo launchTarget)
        {
            DebugMessage(launchTarget.ToString());
            var flag = TryFindShootLineFromTo(caster.Position, launchTarget, out var shootLine);
            if (verbProps.stopBurstWithoutLos && !flag)
            {
                DebugMessage("Targeting cancelled");
                return false;
            }
            var drawPos = caster.DrawPos;
            var projectile = (Projectile_AbilityBase) GenSpawn.Spawn(projectileDef, shootLine.Source, caster.Map);
            projectile.extraDamages = UseAbilityProps.extraDamages;
            projectile.localSpawnThings = UseAbilityProps.thingsToSpawn;
            verbProps.soundCast?.PlayOneShot(new TargetInfo(caster.Position, caster.Map, false));
            verbProps.soundCastTail?.PlayOneShotOnCamera();
            if (DebugViewSettings.drawShooting)
                MoteMaker.ThrowText(caster.DrawPos, caster.Map, "ToHit", -1f);
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (this.canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }
            DebugMessage(launchTarget.ToString());
            projectile.Launch(caster, Ability.Def, drawPos, launchTarget, projectileHitFlags4, null,
                UseAbilityProps.hediffsToApply,
                UseAbilityProps.mentalStatesToApply, UseAbilityProps.thingsToSpawn);
            return true;
        }

        public override void WarmupComplete()
        {
            if (verbTracker == null)
                verbTracker = CasterPawn.verbTracker;
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            TryCastNextBurstShot();
            //Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster,
            //    (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing,
            //    (base.EquipmentSource == null) ? null : base.EquipmentSource.def, this.Projectile,
            //    this.ShotsPerBurst > 1));
        }
    }
}
﻿//#define DEBUGLOG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace AbilityUser
{
    public class Verb_UseAbility : Verb_LaunchProjectile
    {
        public List<LocalTargetInfo> TargetsAoE = new List<LocalTargetInfo>();
        public Action<LocalTargetInfo> timeSavingActionVariable = null;
        public PawnAbility Ability { get; set; } = null;

        public VerbProperties_Ability UseAbilityProps => (VerbProperties_Ability)verbProps;
        public ProjectileDef_Ability AbilityProjectileDef => UseAbilityProps.defaultProjectile as ProjectileDef_Ability;
        public CompAbilityUser AbilityUserComp
        {
            get
            {
                var abilityUser = Ability?.AbilityUser;
                if (abilityUser == null)
                {
                    Log.ErrorOnce("Verb_UseAbility.Ability?.AbilityUser is unexpectedly null - " +
                        "defaulting Verb_UseAbility.AbilityUserComp to CasterPawn's first CompAbilityUser", 21938760);
#pragma warning disable CS0618 // Type or member is obsolete
                    abilityUser = CasterPawn.GetCompAbilityUser();
#pragma warning restore CS0618 // Type or member is obsolete
                }
                return abilityUser;
            }
        }

        protected override int ShotsPerBurst => verbProps.burstShotCount;

        [Conditional("DEBUGLOG")]
        private static void DebugMessage(string s) => Log.Message(s);

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            var targetAoEProperties = UseAbilityProps.abilityDef.MainVerb.TargetAoEProperties;
            return targetAoEProperties?.showRangeOnSelect ?? false
                ? targetAoEProperties.range
                : verbProps.defaultProjectile?.projectile?.explosionRadius ?? 1;
        }

        protected virtual void UpdateTargets()
        {
            TargetsAoE.Clear();
            var props = UseAbilityProps;
            if (props.AbilityTargetCategory == AbilityTargetCategory.TargetAoE)
            {
                //Log.Message("AoE Called");
                var targetAoEProperties = props.TargetAoEProperties;
                if (targetAoEProperties == null)
                    Log.Error("Tried to Cast AoE-Ability without defining a target class");

                List<Thing> targets;

                //Handle TargetAoE start location.
                var aoeStartPosition = caster.PositionHeld;
                if (!targetAoEProperties.startsFromCaster)
                    aoeStartPosition = currentTarget.Cell;

                //Handle friendly fire targets.
                if (!targetAoEProperties.friendlyFire)
                {
                    targets = caster.Map.listerThings.AllThings.FindAll(x =>
                        x.Position.InHorDistOf(aoeStartPosition, targetAoEProperties.range) &&
                        targetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) &&
                        x.Faction.HostileTo(Faction.OfPlayer));
                }
                else if (targetAoEProperties.targetClass == typeof(Plant) ||
                         targetAoEProperties.targetClass == typeof(Building))
                {
                    targets = caster.Map.listerThings.AllThings.FindAll(x =>
                        x.Position.InHorDistOf(aoeStartPosition, targetAoEProperties.range) &&
                        targetAoEProperties.targetClass.IsAssignableFrom(x.GetType()));
                    foreach (var targ in targets)
                    {
                        var tinfo = new LocalTargetInfo(targ);
                        TargetsAoE.Add(tinfo);
                    }
                    return;
                }
                else
                {
                    targets = caster.Map.listerThings.AllThings.FindAll(x =>
                        x.Position.InHorDistOf(aoeStartPosition, targetAoEProperties.range) &&
                        targetAoEProperties.targetClass.IsAssignableFrom(x.GetType()) &&
                        (x.HostileTo(Faction.OfPlayer) || targetAoEProperties.friendlyFire));
                }

                var maxTargets = props.abilityDef.MainVerb.TargetAoEProperties.maxTargets;
                GenList.Shuffle(targets);
                for (var i = 0; i < maxTargets && i < targets.Count; i++)
                {
                    var tinfo = new TargetInfo(targets[i]);
                    if (props.targetParams.CanTarget(tinfo))
                        TargetsAoE.Add(new LocalTargetInfo(targets[i]));
                }
            }
            else
            {
                TargetsAoE.Clear();
                TargetsAoE.Add(currentTarget);
            }
        }

        // Based off Verb.TryStartCastOn.
        public bool PreCastShotCheck(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false,
            bool canHitNonTargetPawns = true, bool preventFriendlyFire = false)
        {
            if (caster == null)
            {
                Log.Error("Verb " + GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
                return false;
            }
            if (!caster.Spawned)
            {
                return false;
            }
            if (state == VerbState.Bursting || (!CanHitTarget(castTarg) && verbProps.requireLineOfSight))
            {
                return false;
            }
            if (causesTimeSlowdown(this, castTarg))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }

            this.surpriseAttack = surpriseAttack;
            canHitNonTargetPawnsNow = canHitNonTargetPawns;
            this.preventFriendlyFire = preventFriendlyFire;
            currentTarget = castTarg;
            currentDestination = destTarg;
            if (CasterIsPawn && verbProps.warmupTime > 0f)
            {
                var casterPawn = CasterPawn;
                if (verbProps.requireLineOfSight)
                {
                    if (!TryFindShootLineFromTo(casterPawn.Position, castTarg, out var resultingLine))
                    {
                        Messages.Message("AU_NoLineOfSight".Translate(), MessageTypeDefOf.RejectInput);
                        return false;
                    }
                    casterPawn.Drawer.Notify_WarmingCastAlongLine(resultingLine, casterPawn.Position);
                }
                var statValue = casterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
                var ticks = (verbProps.warmupTime * statValue).SecondsToTicks();
                casterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
            }
            else
            {
                WarmupComplete();
            }
            return true;
        }

        // Note: This is an open instance delegate where the first argument is the instance.
        private static readonly Func<Verb, LocalTargetInfo, bool> causesTimeSlowdown =
            (Func<Verb, LocalTargetInfo, bool>)AccessTools.Method(typeof(Verb), "CausesTimeSlowdown")
            .CreateDelegate(typeof(Func<Verb, LocalTargetInfo, bool>));

        public bool PreCastShot(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false,
            bool canHitNonTargetPawns = true, bool preventFriendlyFire = false)
        {
            var result = PreCastShotCheck(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire);
            DebugMessage($"Verb_UseAbility.PreCastShotCheck({this}, castTarg={castTarg}, destTarg={destTarg}, " +
                $"surpriseAttack={surpriseAttack}, canHitNonTargetPawns={canHitNonTargetPawns}, " +
                $"preventFriendlyFire={preventFriendlyFire}) => {result}");
            if (result == false)
            {
                Ability.Notify_AbilityFailed(refund: true);
            }
            return result;
        }

        public virtual void PostCastShot(bool inResult, out bool outResult)
        {
            outResult = inResult;
        }

        public override bool Available() => true;

        protected override bool TryCastShot()
        {
            DebugMessage($"TryCastShot({this})");
            var result = false;
            TargetsAoE.Clear();
            UpdateTargets();
            var props = UseAbilityProps;
            if (props.AbilityTargetCategory != AbilityTargetCategory.TargetAoE && TargetsAoE.Count > 1)
                TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            if (props.mustHaveTarget && TargetsAoE.Count == 0)
            {
                Messages.Message("AU_NoTargets".Translate(), MessageTypeDefOf.RejectInput);
                Ability.Notify_AbilityFailed(refund: true);
                return false;
            }
            for (var i = 0; i < TargetsAoE.Count; i++)
            {
                //for (var j = 0; j < burstshots; j++)
                //{
                var target = TargetsAoE[i];
                DebugMessage($"TryCastShot({this}) target={target} ({target.Thing}), defaultProjectile={verbProps.defaultProjectile}");
                if (verbProps.defaultProjectile != null) //ranged attacks WILL have projectiles
                {
                    var attempt = TryLaunchProjectile(verbProps.defaultProjectile, target);
                    if (attempt.HasValue)
                        result = attempt.GetValueOrDefault();
                }
                else //melee attacks WON'T have projectiles
                {
                    var victim = target.Thing;
                    if (victim != null)
                    {
                        if (victim is Pawn pawnVictim)
                        {
                            var casterPawn = CasterPawn;
                            AbilityEffectUtility.ApplyMentalStates(pawnVictim, casterPawn, props.mentalStatesToApply, props.abilityDef, null);
                            AbilityEffectUtility.ApplyHediffs(pawnVictim, casterPawn, props.hediffsToApply, null);
                            AbilityEffectUtility.SpawnSpawnables(props.thingsToSpawn, pawnVictim, victim.MapHeld, victim.PositionHeld);
                        }
                    }
                    else
                    {
                        var casterPawn = CasterPawn;
                        AbilityEffectUtility.SpawnSpawnables(props.thingsToSpawn, casterPawn, casterPawn.MapHeld, casterPawn.PositionHeld);
                    }
                }
                //}
            }

            PostCastShot(result, out result);
            if (result == false)
            {
                Ability.Notify_AbilityFailed(props.refundsPointsAfterFailing);
            }
            return result;
        }

        // Loosely based off Verb_LaunchProjectile.TryCastShot.
        public bool TryLaunchProjectileCheck(ThingDef projectileDef, LocalTargetInfo launchTarget)
        {
            var flag = TryFindShootLineFromTo(caster.Position, launchTarget, out var shootLine);
            if (verbProps.requireLineOfSight && verbProps.stopBurstWithoutLos && !flag)
            {
                Messages.Message("AU_NoLineOfSight".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            var drawPos = caster.DrawPos;
            var projectile = (Projectile_AbilityBase)GenSpawn.Spawn(projectileDef, shootLine.Source, caster.Map);
            var props = UseAbilityProps;
            projectile.extraDamages = props.extraDamages;
            projectile.localSpawnThings = props.thingsToSpawn;
            verbProps.soundCast?.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
            verbProps.soundCastTail?.PlayOneShotOnCamera();
            if (DebugViewSettings.drawShooting)
                MoteMaker.ThrowText(caster.DrawPos, caster.Map, "ToHit"); // TODO: Translate()?
            var projectileHitFlags = ProjectileHitFlags.IntendedTarget;
            if (canHitNonTargetPawnsNow)
            {
                projectileHitFlags |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags |= ProjectileHitFlags.NonTargetWorld;
            }
            projectile.Launch(caster, Ability.Def, drawPos, launchTarget, projectileHitFlags, preventFriendlyFire, equipment: null,
                props.hediffsToApply, props.mentalStatesToApply, props.thingsToSpawn);
            return true;
        }

        protected bool? TryLaunchProjectile(ThingDef projectileDef, LocalTargetInfo launchTarget)
        {
            if (TryLaunchProjectileCheck(projectileDef, launchTarget))
            {
                DebugMessage($"Verb_UseAbility.TryLaunchProjectileCheck({this}, projectileDef={projectileDef}, launchTarget={launchTarget}) => true");
                return true;
            }
            DebugMessage($"Verb_UseAbility.TryLaunchProjectileCheck({this}, projectileDef={projectileDef}, launchTarget={launchTarget}) => false");
            Ability.Notify_AbilityFailed(true);
            return false;
        }

        public override void WarmupComplete()
        {
            verbTracker ??= CasterPawn.verbTracker;
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            TryCastNextBurstShot();
            //Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.Thing,
            //    EquipmentSource?.def, Projectile, ShotsPerBurst > 1));
        }
    }
}

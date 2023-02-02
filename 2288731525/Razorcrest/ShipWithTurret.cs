using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Razorcrest
{
    [StaticConstructorOnStartup]
    public class ShipWithTurret : Building, IAttackTarget, ILoadReferenceable, IAttackTargetSearcher
    {
        private const int TryStartShootSomethingIntervalTicks = 10;

        public bool Active
        {
            get
            {
                if ((powerComp == null || powerComp.PowerOn) && (dormantComp == null || dormantComp.Awake))
                {
                    if (initiatableComp != null)
                    {
                        return initiatableComp.Initiated;
                    }
                    return true;
                }
                return false;
            }
        }

        private bool PlayerControlled
        {
            get
            {
                if (base.Faction == Faction.OfPlayer || MannedByColonist)
                {
                    return !MannedByNonColonist;
                }
                return false;
            }
        }

        private bool CanSetForcedTarget
        {
            get
            {
                if (mannableComp != null)
                {
                    return PlayerControlled;
                }
                return false;
            }
        }

        private bool CanToggleHoldFire => PlayerControlled;

        private bool IsMortar => def.building.IsMortar;

        private bool IsMortarOrProjectileFliesOverhead
        {
            get
            {
                if (!AttackVerb.ProjectileFliesOverhead())
                {
                    return IsMortar;
                }
                return true;
            }
        }

        private bool CanExtractShell
        {
            get
            {
                if (!PlayerControlled)
                {
                    return false;
                }
                return gun.TryGetComp<CompChangeableProjectile>()?.Loaded ?? false;
            }
        }

        private bool MannedByColonist
        {
            get
            {
                if (mannableComp != null && mannableComp.ManningPawn != null)
                {
                    return mannableComp.ManningPawn.Faction == Faction.OfPlayer;
                }
                return false;
            }
        }

        private bool MannedByNonColonist
        {
            get
            {
                if (mannableComp != null && mannableComp.ManningPawn != null)
                {
                    return mannableComp.ManningPawn.Faction != Faction.OfPlayer;
                }
                return false;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            dormantComp = GetComp<CompCanBeDormant>();
            initiatableComp = GetComp<CompInitiatable>();
            powerComp = GetComp<CompPowerTrader>();
            mannableComp = GetComp<CompMannable>();
            if (!respawningAfterLoad)
            {
                burstCooldownTicksLeft = def.building.turretInitialCooldownTime.SecondsToTicks();
            }
            UpdateGunVerbs();
        }

        public override void PostMake()
        {
            base.PostMake();
            MakeGun();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            ResetCurrentTarget();
            progressBarEffecter?.Cleanup();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref forcedTarget, "forcedTarget");
            Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
            Scribe_Deep.Look(ref stunner, "stunner", this);
            Scribe_Values.Look(ref lastAttackTargetTick, "lastAttackTargetTick", 0);

            Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
            Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
            Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
            Scribe_Values.Look(ref holdFire, "holdFire", defaultValue: false);
            Scribe_Deep.Look(ref gun, "gun");
            BackCompatibility.PostExposeData(this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UpdateGunVerbs();
            }
        }

        public override bool ClaimableBy(Faction by)
        {
            if (!base.ClaimableBy(by))
            {
                return false;
            }
            if (mannableComp != null && mannableComp.ManningPawn != null)
            {
                return false;
            }
            if (Active && mannableComp == null)
            {
                return false;
            }
            if (((dormantComp != null && !dormantComp.Awake) || (initiatableComp != null && !initiatableComp.Initiated)) && (powerComp == null || powerComp.PowerOn))
            {
                return false;
            }
            return true;
        }

        public void OrderAttack(LocalTargetInfo targ)
        {
            if (!targ.IsValid)
            {
                if (forcedTarget.IsValid)
                {
                    ResetForcedTarget();
                }
                return;
            }
            if ((targ.Cell - base.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            if ((targ.Cell - base.Position).LengthHorizontal > AttackVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            if (forcedTarget != targ)
            {
                forcedTarget = targ;
                if (burstCooldownTicksLeft <= 0)
                {
                    TryStartShootSomething(canBeginBurstImmediately: false);
                }
            }
            if (holdFire)
            {
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput, historical: false);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (forcedTarget.HasThing && (!forcedTarget.Thing.Spawned || !base.Spawned || forcedTarget.Thing.Map != base.Map))
            {
                forcedTarget = LocalTargetInfo.Invalid;
            }
            stunner.StunHandlerTick();

            if (CanExtractShell && MannedByColonist)
            {
                CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
                if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                {
                    ExtractShell();
                }
            }
            if (forcedTarget.IsValid && !CanSetForcedTarget)
            {
                ResetForcedTarget();
            }
            if (!CanToggleHoldFire)
            {
                holdFire = false;
            }
            if (forcedTarget.ThingDestroyed)
            {
                ResetForcedTarget();
            }
            if (Active && (mannableComp == null || mannableComp.MannedNow) && !stunner.Stunned && base.Spawned)
            {
                GunCompEq.verbTracker.VerbsTick();
                if (AttackVerb.state == VerbState.Bursting)
                {
                    return;
                }
                if (WarmingUp)
                {
                    burstWarmupTicksLeft--;
                    if (burstWarmupTicksLeft == 0)
                    {
                        BeginBurst();
                    }
                }
                else
                {
                    if (burstCooldownTicksLeft > 0)
                    {
                        burstCooldownTicksLeft--;
                        if (IsMortar)
                        {
                            if (progressBarEffecter == null)
                            {
                                progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                            }
                            progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                            MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                            mote.progress = 1f - (float)Math.Max(burstCooldownTicksLeft, 0) / (float)BurstCooldownTime().SecondsToTicks();
                            mote.offsetZ = -0.8f;
                        }
                    }
                    if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                    {
                        TryStartShootSomething(canBeginBurstImmediately: true);
                    }
                }
            }
            else
            {
                ResetCurrentTarget();
            }
        }

        protected void TryStartShootSomething(bool canBeginBurstImmediately)
        {
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }
            if (!base.Spawned || (holdFire && CanToggleHoldFire) || (AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !AttackVerb.Available())
            {
                ResetCurrentTarget();
                return;
            }
            bool isValid = currentTargetInt.IsValid;
            if (forcedTarget.IsValid)
            {
                currentTargetInt = forcedTarget;
            }
            else
            {
                currentTargetInt = TryFindNewTarget();
            }
            if (!isValid && currentTargetInt.IsValid)
            {
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map));
            }
            if (currentTargetInt.IsValid)
            {
                if (def.building.turretBurstWarmupTime > 0f)
                {
                    burstWarmupTicksLeft = def.building.turretBurstWarmupTime.SecondsToTicks();
                }
                else if (canBeginBurstImmediately)
                {
                    BeginBurst();
                }
                else
                {
                    burstWarmupTicksLeft = 1;
                }
            }
            else
            {
                ResetCurrentTarget();
            }
        }

        protected LocalTargetInfo TryFindNewTarget()
        {
            IAttackTargetSearcher attackTargetSearcher = TargSearcher();
            Faction faction = attackTargetSearcher.Thing.Faction;
            float range = AttackVerb.verbProps.range;
            if (Rand.Value < 0.5f && AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
            {
                float num = AttackVerb.verbProps.EffectiveMinRange(x, this);
                float num2 = x.Position.DistanceToSquared(base.Position);
                return num2 > num * num && num2 < range * range;
            }).TryRandomElement(out Building result))
            {
                return result;
            }
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (!AttackVerb.ProjectileFliesOverhead())
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
            }
            if (AttackVerb.IsIncendiary())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            if (IsMortar)
            {
                targetScanFlags |= TargetScanFlags.NeedNotUnderThickRoof;
            }
            return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, IsValidTarget);
        }

        private IAttackTargetSearcher TargSearcher()
        {
            if (mannableComp != null && mannableComp.MannedNow)
            {
                return mannableComp.ManningPawn;
            }
            return this;
        }

        private bool IsValidTarget(Thing t)
        {
            Vector3 drawPos = this.DrawPos;
            var drawOffset = this.def.GetModExtension<TurretPosOffset>();
            if (drawOffset != null)
            {
                drawPos += Quaternion.Euler(0, this.Rotation.AsAngle, 0) * drawOffset.posOffset;
            }
            ShootLine resultingLine;
            if (AttackVerb.TryFindShootLineFromTo(drawPos.ToIntVec3(), t, out resultingLine))
            {
                var cells = resultingLine.Points().Where(x => !this.OccupiedRect().Cells.Contains(x));
                var distance = cells.First().DistanceTo(drawPos.ToIntVec3());
                if (distance > 2f)
                {
                    return false;
                }
            }
            else
            {
                Log.Message(this.AttackVerb + " - Cant find a shoot line");
                return false;
            }

            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                if (AttackVerb.ProjectileFliesOverhead())
                {
                    RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        return false;
                    }
                }
                if (mannableComp == null)
                {
                    return !GenAI.MachinesLike(base.Faction, pawn);
                }
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        protected void BeginBurst()
        {
            AttackVerb.TryStartCastOn(CurrentTarget);
            OnAttackedTarget(CurrentTarget);
        }

        protected void BurstComplete()
        {
            burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
        }

        protected float BurstCooldownTime()
        {
            if (def.building.turretBurstCooldownTime >= 0f)
            {
                return def.building.turretBurstCooldownTime;
            }
            return AttackVerb.verbProps.defaultCooldownTime;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            if (!inspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspectString);
            }
            if (AttackVerb.verbProps.minRange > 0f)
            {
                stringBuilder.AppendLine("MinimumRange".Translate() + ": " + AttackVerb.verbProps.minRange.ToString("F0"));
            }
            if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
            {
                stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (base.Spawned && burstCooldownTicksLeft > 0 && BurstCooldownTime() > 5f)
            {
                stringBuilder.AppendLine("CanFireIn".Translate() + ": " + burstCooldownTicksLeft.ToStringSecondsFromTicks());
            }
            CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
            if (compChangeableProjectile != null)
            {
                if (compChangeableProjectile.Loaded)
                {
                    stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
                }
                else
                {
                    stringBuilder.AppendLine("ShellNotLoaded".Translate());
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void DrawExtraSelectionOverlays()
        {
            float range = AttackVerb.verbProps.range;
            if (range < 90f)
            {
                GenDraw.DrawRadiusRing(base.Position, range);
            }
            float num = AttackVerb.verbProps.EffectiveMinRange(allowAdjacentShot: true);
            if (num < 90f && num > 0.1f)
            {
                GenDraw.DrawRadiusRing(base.Position, num);
            }
            if (WarmingUp)
            {
                int degreesWide = (int)((float)burstWarmupTicksLeft * 0.5f);
                GenDraw.DrawAimPie(this, CurrentTarget, degreesWide, (float)def.size.x * 0.5f);
            }
            if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
            {
                Vector3 b = (!forcedTarget.HasThing) ? forcedTarget.Cell.ToVector3Shifted() : forcedTarget.Thing.TrueCenter();
                Vector3 a = this.TrueCenter();
                b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (CanExtractShell)
            {
                CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "CommandExtractShell".Translate();
                command_Action.defaultDesc = "CommandExtractShellDesc".Translate();
                command_Action.icon = compChangeableProjectile.LoadedShell.uiIcon;
                command_Action.iconAngle = compChangeableProjectile.LoadedShell.uiIconAngle;
                command_Action.iconOffset = compChangeableProjectile.LoadedShell.uiIconOffset;
                command_Action.iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.LoadedShell);
                command_Action.action = delegate
                {
                    ExtractShell();
                };
                yield return command_Action;
            }
            CompChangeableProjectile compChangeableProjectile2 = gun.TryGetComp<CompChangeableProjectile>();
            if (compChangeableProjectile2 != null)
            {
                StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
                foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
                {
                    yield return item;
                }
            }
            if (CanSetForcedTarget)
            {
                Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
                command_VerbTarget.defaultLabel = "CommandSetForceAttackTarget".Translate();
                command_VerbTarget.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
                command_VerbTarget.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
                command_VerbTarget.verb = AttackVerb;
                command_VerbTarget.hotKey = KeyBindingDefOf.Misc4;
                command_VerbTarget.drawRadius = false;
                if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
                {
                    command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
                }
                yield return command_VerbTarget;
            }
            if (forcedTarget.IsValid)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "CommandStopForceAttack".Translate();
                command_Action2.defaultDesc = "CommandStopForceAttackDesc".Translate();
                command_Action2.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt");
                command_Action2.action = delegate
                {
                    ResetForcedTarget();
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                };
                if (!forcedTarget.IsValid)
                {
                    command_Action2.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                }
                command_Action2.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Action2;
            }
            if (CanToggleHoldFire)
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "CommandHoldFire".Translate();
                command_Toggle.defaultDesc = "CommandHoldFireDesc".Translate();
                command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire");
                command_Toggle.hotKey = KeyBindingDefOf.Misc6;
                command_Toggle.toggleAction = delegate
                {
                    holdFire = !holdFire;
                    if (holdFire)
                    {
                        ResetForcedTarget();
                    }
                };
                command_Toggle.isActive = (() => holdFire);
                yield return command_Toggle;
            }
        }

        private void ExtractShell()
        {
            GenPlace.TryPlaceThing(gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near);
        }

        private void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
            {
                TryStartShootSomething(canBeginBurstImmediately: false);
            }
        }

        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }
        public void MakeGun()
        {
            gun = ThingMaker.MakeThing(def.building.turretGunDef);
            UpdateGunVerbs();
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = BurstComplete;
            }
        }

        protected int burstCooldownTicksLeft;

        protected int burstWarmupTicksLeft;

        protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        private bool holdFire;

        public Thing gun;

        protected CompPowerTrader powerComp;

        protected CompCanBeDormant dormantComp;

        protected CompInitiatable initiatableComp;

        protected CompMannable mannableComp;

        protected Effecter progressBarEffecter;

        public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

        public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

        public LocalTargetInfo CurrentTarget => currentTargetInt;

        private bool WarmingUp => burstWarmupTicksLeft > 0;

        public Verb AttackVerb => GunCompEq.PrimaryVerb;

        public bool IsMannable => mannableComp != null;

        protected StunHandler stunner;

        protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;

        private LocalTargetInfo lastAttackedTarget;

        private int lastAttackTargetTick;

        private const float SightRadiusTurret = 13.4f;

        Thing IAttackTarget.Thing => this;

        public LocalTargetInfo TargetCurrentlyAimingAt => CurrentTarget;

        Thing IAttackTargetSearcher.Thing => this;

        public Verb CurrentEffectiveVerb => AttackVerb;

        public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

        public int LastAttackTargetTick => lastAttackTargetTick;

        public float TargetPriorityFactor => 1f;

        public ShipWithTurret()
        {
            stunner = new StunHandler(this);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (!absorbed)
            {
                stunner.Notify_DamageApplied(dinfo, affectedByEMP: true);
                absorbed = false;
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            CompPowerTrader comp = GetComp<CompPowerTrader>();
            if (comp != null && !comp.PowerOn)
            {
                return true;
            }
            CompMannable comp2 = GetComp<CompMannable>();
            if (comp2 != null && !comp2.MannedNow)
            {
                return true;
            }
            CompCanBeDormant comp3 = GetComp<CompCanBeDormant>();
            if (comp3 != null && !comp3.Awake)
            {
                return true;
            }
            CompInitiatable comp4 = GetComp<CompInitiatable>();
            if (comp4 != null && !comp4.Initiated)
            {
                return true;
            }
            return false;
        }

        protected void OnAttackedTarget(LocalTargetInfo target)
        {
            lastAttackTargetTick = Find.TickManager.TicksGame;
            lastAttackedTarget = target;
        }
    }
}
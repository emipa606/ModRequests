using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;


namespace Beam
{
    public class ContinousBeamVerbLaser : Verb
    {

        private Vector3 initialTargetPosition;

        private MoteDualAttached mote;

        private Effecter endEffecter;

        private Sustainer sustainer;

        //Beam heat that is added each second
        private float beamHeat = 0f;

        private SoundDef sound = null;

        private bool doExplosionSound = false;

        //Damage that ramps
        private int newDamage = 0;

        //Armor pen
        private float newArmorPen = 0;

        private Job thisJob;

        private bool shotHits = false;

        private bool randomTileAssigned = false;

        Pawn targetPawnGlobal = null;

        public ContinousBeamComp Props => EquipmentSource.GetComp<ContinousBeamComp>();

        //Shots per burst
        protected override int ShotsPerBurst => verbProps.burstShotCount;

        //NEW STUFF FOR SMOOTH TRANSITION
        private int currentTick = 0;
        private Vector3 randomCell = new Vector3(0f, 0f, 0f);
        private Vector3 newTarget = new Vector3(0f, 0f, 0f);
        private Vector3 distance = new Vector3(0f, 0f, 0f);

        //Start label
        String originalLabel1 = null;
        String originalLabel2 = null;
        String originalLabel3 = null;

        //Angle when fireing
        public override float? AimAngleOverride
        {
            get
            {
                if (state != VerbState.Bursting)
                {
                    return null;
                }
                if (shotHits)
                {
                    return (currentTarget.CenterVector3 - caster.DrawPos).AngleFlat();
                }
                else
                {
                    return (newTarget - caster.DrawPos).AngleFlat();
                }
            }
        }

        //Checks if gun is able to shoot pawn
        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            ShootLine resultingLine;
            bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
            if (verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.GetComp<CompReloadable>()?.UsedOnce();
            }
            lastShotTick = Find.TickManager.TicksGame;
            HitCell(currentTarget.CenterVector3.ToIntVec3());
            return true;
        }

        //Checks if it can start the shooting
        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            //If fire at will is off, dont shoot, check if forced
            if (CasterPawn.Faction.IsPlayer)
            {
                Log.Message("Is player");
                if (!CasterPawn.drafter.FireAtWill && CasterPawn.jobs.curJob.playerForced)
                {
                    return false;
                }
            }
            return base.TryStartCastOn(verbProps.beamTargetsGround ? ((LocalTargetInfo)castTarg.Cell) : castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
        }

        //Happens every tick while fireing.
        public override void BurstingTick()
        {
            if (verbProps.muzzleFlashScale > 0.01f)
            {
                FleckMaker.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, verbProps.muzzleFlashScale);
            }

            if ((currentTarget.Pawn is Pawn) && (currentTarget.Pawn != null))
            {
                targetPawnGlobal = currentTarget.Pawn;
            }
            else
            {
                targetPawnGlobal = null;
            }

            Vector3 vector = currentTarget.CenterVector3;
            IntVec3 intVec = vector.ToIntVec3();

            if (!shotHits && (targetPawnGlobal != null) && !randomTileAssigned)
            {
                randomTileAssigned = true;
            }
            if (randomTileAssigned)
            {
                if (currentTick <= 60)
                {
                    if (currentTick <= 7)
                    {
                        newTarget = newTarget + (distance / 7);
                    }
                    else if (currentTick <= 14)
                    {
                        newTarget = newTarget - (distance / 7);
                    }
                    else
                    {
                        newTarget = currentTarget.CenterVector3;
                    }
                    vector = newTarget;
                    intVec = vector.ToIntVec3();
                    currentTick++;
                }
                else
                {
                    currentTick = 0;
                    newTarget = vector;
                    randomCell = newTarget.ToIntVec3().RandomAdjacentCell8Way().ToVector3();
                    distance = newTarget - randomCell;
                }
            }
            Vector3 vector2 = newTarget - caster.Position.ToVector3Shifted();
            float num = vector2.MagnitudeHorizontal();
            Vector3 normalized = vector2.Yto0().normalized;
            IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(caster.Position, intVec, (IntVec3 c) => c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
            if (intVec2.IsValid)
            {
                num -= (intVec - intVec2).LengthHorizontal;
                vector = caster.Position.ToVector3Shifted() + normalized * num;
                intVec = vector.ToIntVec3();
            }
            Vector3 offsetA = normalized * verbProps.beamStartOffset;
            Vector3 vector3 = vector - intVec.ToVector3Shifted();
            //Does the whole visual thing here
            if (mote != null)
            {
                if (randomTileAssigned)
                {
                    mote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(newTarget.ToIntVec3(), caster.Map), offsetA, vector3);
                }
                else
                {
                    mote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(intVec, caster.Map), offsetA, vector3);
                }
                mote.Maintain();
            }
            if (verbProps.beamGroundFleckDef != null && Rand.Chance(verbProps.beamFleckChancePerTick))
            {
                if (shotHits)
                {
                    FleckMaker.Static(vector, caster.Map, verbProps.beamGroundFleckDef);
                }
                else
                {
                    FleckMaker.Static(newTarget.ToIntVec3(), caster.Map, verbProps.beamGroundFleckDef);
                }
            }
            if (endEffecter == null && verbProps.beamEndEffecterDef != null)
            {
                if (shotHits)
                {
                    endEffecter = verbProps.beamEndEffecterDef.Spawn(intVec, caster.Map, vector3);
                }
                else
                {
                    endEffecter = verbProps.beamEndEffecterDef.Spawn(newTarget.ToIntVec3(), caster.Map, vector3);
                }
                
            }
            if (endEffecter != null)
            {
                endEffecter.offset = vector3;
                if (shotHits)
                {
                    endEffecter.EffectTick(new TargetInfo(intVec, caster.Map), TargetInfo.Invalid);
                }
                else
                {
                    endEffecter.EffectTick(new TargetInfo(newTarget.ToIntVec3(), caster.Map), TargetInfo.Invalid);
                }
                endEffecter.ticksLeft--;
            }
            if (verbProps.beamLineFleckDef != null)
            {
                float num2 = 1f * num;
                for (int i = 0; (float)i < num2; i++)
                {
                    if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate((float)i / num2)))
                    {
                        Vector3 vector4 = i * normalized - normalized * Rand.Value + normalized / 2f;
                        FleckMaker.Static(caster.Position.ToVector3Shifted() + vector4, caster.Map, verbProps.beamLineFleckDef);
                    }
                }
            }
            sustainer?.Maintain();
            if (doExplosionSound)
            {
                SoundInfo info = SoundInfo.InMap(caster, MaintenanceType.PerTick);
                sound.PlayOneShot(info);
                doExplosionSound = false;
            }

            //Checks if pawn is trying to move or gets undrafted, if so stops shooting.
            if (CasterPawn != null)
            {
                if (thisJob != null && thisJob != CasterPawn.jobs.curJob)
                {
                    Reset();
                }
                if (CasterPawn.drafter != null && !CasterPawn.drafter.Drafted)
                {
                    Reset();
                }
            }

            //Remove?
            if (currentTarget.Pawn != null)
            {
                Pawn targetPawn = currentTarget.Pawn;
                HediffDef debuff = EquipmentSource.GetComp<ContinousBeamComp>().getHediffDef();
                Hediff hediff = targetPawn.health.hediffSet.hediffs.Find((Predicate<Hediff>)(x => x.def == debuff));
                if (hediff != null)
                {

                }

            }

            //If fire at will is turned off during shooting and it is not a forced 
            if (CasterPawn.Faction.IsPlayer)
            {
                Log.Message("Is player");
                if (!CasterPawn.drafter.FireAtWill)
                {
                    Reset();
                }
            }
        }

        //Warmup done, before shot is fired
        public override void WarmupComplete()
        {
            //Mote reset
            newTarget = currentTarget.CenterVector3;
            randomCell = new Vector3(0f, 0f, 0f);
            distance = new Vector3(0f, 0f, 0f);

            //Heat amount from gun
            beamHeat = EquipmentSource.GetComp<ContinousBeamComp>().getBeamHeat()/100;
            sound = EquipmentSource.GetComp<ContinousBeamComp>().GetSoundDef();
            //job
            thisJob = CasterPawn.jobs.curJob;

            //Temporary/Unsure - to be used to check whether gun is firing or not.
            burstShotsLeft = ShotsPerBurst;

            state = VerbState.Bursting;
            initialTargetPosition = currentTarget.CenterVector3;

            if (verbProps.beamMoteDef != null)
            {
                mote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(currentTarget.CenterVector3.ToIntVec3(), caster.Map));
            }
            TryCastNextBurstShot();
            endEffecter?.Cleanup();
            if (verbProps.soundCastBeam != null)
            {
                sustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
            }
        }

        //Can be hit/Line of sight
        private bool CanHit(Thing thing)
        {
            if (!thing.Spawned)
            {
                return false;
            }
            return !CoverUtility.ThingCovered(thing, caster.Map);
        }

        //On hit on cell
        private void HitCell(IntVec3 cell)
        {
            ApplyDamage(VerbUtility.ThingsToHit(cell, caster.Map, CanHit).RandomElementWithFallback());
        }

        //Damage applied
        private void ApplyDamage(Thing thing)
        {
            Pawn targetPawn;

            Hediff hediff = null;

            HediffDef debuff;

            IntVec3 cell;

            float burnChance = 0.05f;

            String editedLabel = "";

            randomTileAssigned = false;

            if (thing is Pawn)
            {
                targetPawn = thing as Pawn;
                cell = targetPawn.Position;
            }
            else
            {
                cell = currentTarget.CenterVector3.ToIntVec3();
            }

            float distance = (cell - CasterPawn.Position).LengthHorizontal;

            float hitChance = verbProps.GetHitChanceFactor(EquipmentSource, distance);

            if (Rand.Chance(hitChance))
            {
                shotHits = true;
            }
            else
            {
                shotHits = false;
            }

            if (thing is Pawn && shotHits)
            {
                targetPawn = thing as Pawn;

                debuff = EquipmentSource.GetComp<ContinousBeamComp>().getHediffDef();

                hediff = targetPawn.health.hediffSet.hediffs.Find((Predicate<Hediff>)(x => x.def == debuff));

                if (hediff == null)
                {
                    targetPawn.health.AddHediff(debuff);
                }

                if (hediff != null)
                {
                    //Adds beamHeat to hediff severity, capping it at 1
                    if (hediff.Severity < 1)
                    {     
                        if (hediff.Severity + beamHeat > 1)
                        {
                            hediff.Severity = 1;
                        }
                        else
                        {
                            hediff.Severity += (beamHeat);
                        }
                    }
                }
            }
            else if (!shotHits && thing is Pawn)
            {
                targetPawn = thing as Pawn;

                debuff = EquipmentSource.GetComp<ContinousBeamComp>().getHediffDef();
                hediff = targetPawn.health.hediffSet.hediffs.Find((Predicate<Hediff>)(x => x.def == debuff));
                if (hediff != null)
                {
                    //Decreases severity when missing by either 1 or 5 + 5/90 * beamheat - 10
                    if (hediff.Severity < 0.1)
                    {
                        hediff.Severity -= 0.01f;
                    }
                    else if (hediff.Severity < 1)
                    {
                        hediff.Severity -= 0.05f + (0.05555556f * (hediff.Severity - 0.1f) );
                    }
                    else if (hediff.Severity == 1)
                    {
                        hediff.Severity -= 0.1f;
                    }
                }              
            }

            //Damage, burn chance and armor pen
            if (hediff != null)
            {
                if (hediff.Severity < 0.5f)
                {
                    burnChance = 0.05f;
                    newArmorPen = 0.05f + (0.05f * hediff.Severity);
                }
                else
                {
                    burnChance = 0.2f;
                    newArmorPen = 0.1f + (0.3f * hediff.Severity);
                }
                //Damage
                if (hediff.Severity >= 0.1f)
                {
                    newDamage = (int)(10f + (44.444444f * (hediff.Severity - 0.1f)));
                }
                if (hediff.Severity == 1)
                {
                    newDamage = 50;
                    newArmorPen = 0.4f;
                }

                //Sets original label.
                if (hediff.Severity < 0.25)
                {
                    if (originalLabel1 == null)
                    {
                        originalLabel1 = hediff.CurStage.label;
                    }
                    editedLabel = "" + originalLabel1;
                }
                else if (hediff.Severity < 0.75 && hediff.Severity >= 0.25)
                {
                    if (originalLabel2 == null)
                    {
                        originalLabel2 = hediff.CurStage.label;
                    }
                    editedLabel = "" + originalLabel2;
                }
                else if (hediff.Severity <= 1 && hediff.Severity >= 0.75)
                {
                    if (originalLabel3 == null)
                    {
                        originalLabel3 = hediff.CurStage.label;
                    }
                    editedLabel = "" + originalLabel3;
                }

                editedLabel = editedLabel.Replace("x", "" + newDamage);
                editedLabel = editedLabel.Replace("y", "" + newArmorPen*100);

                hediff.CurStage.label = editedLabel;
            }

            IntVec3 intVec = currentTarget.CenterVector3.ToIntVec3();
            Map map = caster.Map;
            if (thing == null || verbProps.beamDamageDef == null)
            {
                return;
            }
            float angleFlat = (currentTarget.Cell - caster.Position).AngleFlat;
            BattleLogEntry_RangedImpact log = new BattleLogEntry_RangedImpact(base.EquipmentSource, thing, currentTarget.Thing, base.EquipmentSource.def, null, null);

            if ((thing is Pawn && shotHits) || !(thing is Pawn))
            {
                //Damage applied here
                DamageInfo dinfo = new DamageInfo(verbProps.beamDamageDef, newDamage, newArmorPen, angleFlat, base.EquipmentSource, null, base.EquipmentSource.def, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing);
                thing.TakeDamage(dinfo).AssociateWithLog(log);
            }

            if (Rand.Chance(burnChance))
            {
                if (thing.CanEverAttachFire())
                {
                    thing.TryAttachFire(verbProps.beamFireSizeRange.RandomInRange);
                }
                else
                {
                    FireUtility.TryStartFireIn(intVec, map, verbProps.beamFireSizeRange.RandomInRange);
                }
                doExplosionSound = true;
            }

            //Gives shooting xp
            if (thing is Pawn)
            {
                targetPawn = thing as Pawn;

                float num = (targetPawn.HostileTo(caster) ? 170f : 20f);
                float num2 = verbProps.warmupTime + verbProps.AdjustedCooldown(this, CasterPawn) + 1;

                CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initialTargetPosition, "initialTargetPosition");
        }

        public override void Reset()
        {
            if (targetPawnGlobal != null)
            {
                HediffDef debuff = EquipmentSource.GetComp<ContinousBeamComp>().getHediffDef();
                if (debuff != null)
                {
                    Hediff hediff = targetPawnGlobal.health.hediffSet.hediffs.Find((Predicate<Hediff>)(x => x.def == debuff));
                    if (hediff != null)
                    {
                        HediffComp hediffComp = hediff.TryGetComp<BeamHediffComp_SeverityPerDay>();
                        if (hediffComp != null)
                        {
                            hediffComp.CompPostInjuryHeal(-46f);
                        }
                    }
                }
            }
            base.Reset();
        }
    }
}
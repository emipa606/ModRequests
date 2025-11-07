using CombatExtended;
using CombatExtended.Compatibility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    [Obsolete]
    public class ProjectileCE_Homing : BulletCE
    {
        public ProjectileCE_Homing()
        {
            BlockerRegistry.RegisterCheckForCollisionCallback(CheckForProxFuseStatic);
        }

        Projectile_HomingExtension Extension => def.GetModExtension<Projectile_HomingExtension>();

        Weapon_HomingExtension WeaponExtension => equipmentDef?.GetModExtension<Weapon_HomingExtension>();

        CompHomingProjectileHeightWorker HeightWorker => GetComp<CompHomingProjectileHeightWorker>();

        public Vector3 targetPos => intendedTarget.Cell.ToVector3Shifted();
        public Vector3 Velocity => velocity;
        public float DistanceToTarget => Vector3.Distance(ExactPosition.Yto0(), targetPos);
        public bool Guidance => GuidanceOn && !malfunctioned;

        public bool GuidanceOn, proxFuseArmed, malfunctioned, shouldMalfunct;
        public int lifeTime, tickToGuidance;
        public float lastTargetDist, aimHeight;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref GuidanceOn, "GuidanceOn");
            Scribe_Values.Look(ref proxFuseArmed, "proxFuseArmed");
            Scribe_Values.Look(ref lastTargetDist, "lastTargetDist");
            Scribe_Values.Look(ref lifeTime, "lifeTime");
            Scribe_Values.Look(ref malfunctioned, "malfunctioned");
            Scribe_Values.Look(ref shouldMalfunct, "shouldMalfunct");
            Scribe_Values.Look(ref tickToGuidance, "tickToGuidance");
            Scribe_Values.Look(ref aimHeight, "aimHeight");
        }

        public virtual float speed
        {
            get
            {
                if (Extension == null || Extension.speedCurve == null)
                {
                    return def.projectile.speed / 60;
                }
                return Extension.speedCurve.Evaluate(lifeTime) * def.projectile.speed / 60;
            }
        }


        PropertyInfo AmbientSustInfo => typeof(ProjectileCE).GetProperty("ambientSustainer", BindingFlags.NonPublic | BindingFlags.Instance);

        Sustainer AmbientSustainer => (Sustainer)AmbientSustInfo.GetValue(this);

        public override void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0, float shotSpeed = -1, Thing equipment = null, float distance = -1)
        {
            base.Launch(launcher, origin, shotAngle, shotRotation, shotHeight, shotSpeed, equipment, distance);
            velocity = new Vector3(origin.x, shotHeight, origin.y) - (Quaternion.AngleAxis(shotAngle, Vector3.left) * Vector3.forward).RotatedBy(shotRotation) * (def.projectile.speed / 60);
            lastTargetDist = DistanceToTarget;
            aimHeight = getAimHeight();
            if (Extension == null)
            {
                GuidanceOn = true;
            }
            else
            {
                if (Rand.Chance(Extension.malfunctionChance))
                {
                    shouldMalfunct = true;
                }
                tickToGuidance = Extension.guidanceDelay.RandomInRange;
                if (tickToGuidance == 0) GuidanceOn = true;
            }

            if (HeightWorker != null)
            {
                HeightWorker.Launch();
            }

            Redirect();
        }

        public void Redirect()
        {
            if (Extension != null)
            {
                if (intendedTarget.HasThing && Rand.Chance(Extension.redirectionChance) && intendedTarget.Thing.Faction != null)
                {
                    IEnumerable<Thing> possibleTargets = new List<Thing>();
                    possibleTargets = launcher.Map.mapPawns.AllPawnsSpawned.Where(t => isValidTarget(t));
                    if (!possibleTargets.Any())
                    {
                        possibleTargets = launcher.Map.spawnedThings.Where(t => isValidTarget(t));
                    }
                    if (possibleTargets.Any())
                    {
                        this.intendedTarget = possibleTargets.RandomElement();
                    }
                }
            }
            bool isValidTarget(Thing thing)
            {
                if (Vector3.Distance(thing.TrueCenter().Yto0(), targetPos) > Extension.redirectionRadius)
                {
                    return false;
                }
                if (!intendedTarget.HasThing)
                {
                    return false;
                }
                if (thing.Faction == intendedTarget.Thing.Faction && !(thing is Pawn pawn && pawn.Downed))
                {
                    return true;
                }
                return false;
            }
        }

        public override void Tick()
        {
            lifeTime++;
            if (!GuidanceOn && lifeTime == tickToGuidance)
            {
                GuidanceOn = true;
            }
            base.Tick();
            if (Extension.maxLifeTime > 0 && lifeTime > Extension.maxLifeTime)
            {
                Log.Message("Expired");
                Impact(null);
                return;
            }
            if (shouldMalfunct && !malfunctioned && Extension.malfunctionChanceCurve != null && Rand.Chance(Extension.malfunctionChanceCurve.Evaluate(lifeTime)))
            {
                DoMalfunction();
            }
        }

        //protected override void Accelerate()
        //{
        //    if (Guidance)
        //    {
        //        if (this.IsHashIntervalTick(60) && (intendedTarget.ThingDestroyed || (intendedTarget.Pawn != null && intendedTarget.Pawn.Dead)))
        //        {
        //            Redirect();
        //        }
        //        Vector3 targetVelocity = (targetPos - ExactPosition).Yto0();
        //        if (Extension != null && Extension.turningSpeed > 0)
        //        {
        //            float angle = Vector3.SignedAngle(velocity, targetVelocity, Vector3.up);
        //            if (Extension.lostTargetOutOfView && Math.Abs(angle) > Extension.viewConeAngle)
        //            {
        //                GuidanceOn = false;
        //            }
        //            if (Math.Abs(angle) * 60 > Extension.turningSpeed)
        //            {
        //                velocity = velocity.RotatedBy(Math.Sign(angle) * Extension.turningSpeed / 60);
        //            }
        //            else
        //            {
        //                velocity = targetVelocity;
        //            }
        //        }
        //        else
        //        {
        //            velocity = targetVelocity;
        //        }
        //        velocity = velocity.normalized * speed;
        //        if (HeightWorker == null)
        //        {
        //            AffectedByGravity();
        //        }
        //        else
        //        {
        //            velocity *= HeightWorker.ActivatedPhase.SpeedPct;
        //            velocity.y = HeightWorker.GetTargetHeight(this, intendedTarget);
        //        }
        //    }
        //    else
        //    {
        //        AffectedByGravity();
        //    }
        //}

        public float getAimHeight()
        {
            CompFireModes CompFireModes = equipment.TryGetComp<CompFireModes>();
            if (intendedTargetThing == null)
            {
                return shotHeight;
            }
            var victimVert = new CollisionVertical(intendedTarget.Thing);
            var targetRange = victimVert.HeightRange;   //Get lower and upper heights of the target
            if (intendedTarget.Thing is Pawn Victim)
            {
                targetRange.min = victimVert.BottomHeight;
                targetRange.max = victimVert.MiddleHeight;

                if (CompFireModes?.targetMode == TargettingMode.head)
                {
                    // Aim upper thoraxic to head
                    targetRange.min = victimVert.MiddleHeight;
                    targetRange.max = victimVert.Max;
                }
                else if (CompFireModes?.targetMode == TargettingMode.legs)
                {
                    // Aim legs to lower abdomen
                    targetRange.min = victimVert.Min;
                    targetRange.max = victimVert.BottomHeight;
                }
                else
                {
                    // Aim center mass
                    targetRange.min = victimVert.BottomHeight;
                    targetRange.max = victimVert.MiddleHeight;
                }
            }

            return targetRange.Average;
        }

        public override void Impact(Thing hitThing)
        {
            Log.Message(hitThing?.ToString() ?? "null");
            base.Impact(hitThing);
        }

        public static bool CheckForProxFuseStatic(ProjectileCE projectile, IntVec3 cell, Thing launcher)
        {
            if (projectile is ProjectileCE_Homing projHoming)
            {
                return projHoming.CheckForProxFuse(cell.ToVector3Shifted());
            }
            return false;
        }

        public bool CheckForProxFuse(Vector3 vect)
        {
            if (Extension == null || !Extension.proxFuse || !Guidance)
            {
                return false;
            }
            float dist = Vector3.Distance(vect.Yto0(), targetPos);
            Log.Message(dist);
            if (dist <= Extension.proxFuseRadius)
            {
                proxFuseArmed = true;
            }
            if (proxFuseArmed && dist > lastTargetDist)
            {
                Position = vect.ToIntVec3();
                Log.Message("ProxFuse");
                Impact(null);
                return true;
            }
            lastTargetDist = dist;
            return false;
        }

        public void DoMalfunction()
        {
            malfunctioned = true;
            FleckMaker.AttachedOverlay(this, FleckDefOf.LineEMP, Vector3.zero);
            if (Rand.Chance(0.25f))
            {
                Log.Message("Malfunct");
                Impact(null);
            }
            if (Rand.Chance(0.25f))
            {
                Redirect();
            }
            if (Rand.Chance(0.25f))
            {
                GuidanceOn = false;
                velocity = velocity.RotatedBy(Rand.Value * Extension.turningSpeed);
            }
        }
    }
}

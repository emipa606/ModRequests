using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    public class Projectile_Homing : Bullet
    {
        Projectile_HomingExtension Extension => def.GetModExtension<Projectile_HomingExtension>();

        Weapon_HomingExtension WeaponExtension => equipmentDef?.GetModExtension<Weapon_HomingExtension>();
        public override Vector3 ExactPosition => exactPos.Yto0();
        public override Vector3 DrawPos => ExactPosition + Vector3.up * def.Altitude;
        public Vector3 targetPos => intendedTarget.CenterVector3.Yto0();
        public bool Guidance => GuidanceOn && !malfunctioned;
        public override Quaternion ExactRotation => Quaternion.LookRotation(FlightVector);

        public Vector3 exactPos;
        public Vector3 LastExactPosition;
        public Vector3 FlightVector;
        public bool GuidanceOn;
        public bool proxFuseArmed;
        public float lastTargetDist;
        public int lifeTime;
        public bool malfunctioned;
        public bool shouldMalfunct;
        public int tickToGuidance;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref exactPos, "exactPos");
            Scribe_Values.Look(ref LastExactPosition, "LastExactPosition");
            Scribe_Values.Look(ref FlightVector, "FlightVector");
            Scribe_Values.Look(ref GuidanceOn, "GuidanceOn");
            Scribe_Values.Look(ref proxFuseArmed, "proxFuseArmed");
            Scribe_Values.Look(ref lastTargetDist, "lastTargetDist");
            Scribe_Values.Look(ref lifeTime, "lifeTime");
            Scribe_Values.Look(ref malfunctioned, "malfunctioned");
            Scribe_Values.Look(ref shouldMalfunct, "shouldMalfunct");
            Scribe_Values.Look(ref tickToGuidance, "tickToGuidance");
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

        MethodInfo CheckInterceptInfo => typeof(Projectile).GetMethod("CheckForFreeIntercept", BindingFlags.NonPublic | BindingFlags.Instance);

        PropertyInfo AmbientSustInfo => typeof(Projectile).GetProperty("ambientSustainer", BindingFlags.NonPublic | BindingFlags.Instance);

        Sustainer AmbientSustainer => (Sustainer)AmbientSustInfo.GetValue(this);

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            exactPos = origin;
            LastExactPosition = origin + (origin - usedTarget.CenterVector3).normalized * (def.projectile.speed / 60);
            FlightVector = (exactPos - LastExactPosition).Yto0();
            lastTargetDist = Vector3.Distance(exactPos, targetPos);
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
            }


            Redirect();
        }

        public void Redirect()
        {
            if (Extension != null)
            {
                if (intendedTarget.HasThing && intendedTarget.Thing.Faction != null && Rand.Chance(Extension.redirectionChance))
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
            if (AllComps != null)
            {
                int i = 0;
                for (int count = AllComps.Count; i < count; i++)
                {
                    AllComps[i].CompTick();
                }
            }
            if (!GuidanceOn && lifeTime == tickToGuidance)
            {
                GuidanceOn = true;
            }
            if (landed)
            {
                return;
            }
            LastExactPosition = ExactPosition;
            Move();
            lifeTime++;
            if (!ExactPosition.InBounds(Map))
            {
                Destroy();
                return;
            }
            if (Guidance && this.IsHashIntervalTick(60) && (intendedTarget.ThingDestroyed || (intendedTarget.Pawn != null && intendedTarget.Pawn.Dead)))
            {
                Redirect();
            }
            if (CheckForFreeInterceptBetween(LastExactPosition, ExactPosition)) { return; }
            if (CheckForProxFuse(exactPos)) { return; }
            if (Extension.maxLifeTime > 0 && lifeTime > Extension.maxLifeTime)
            {
                Impact(null);
                return;
            }
            base.Position = ExactPosition.ToIntVec3();
            if (intendedTarget.HasThing && intendedTarget.Thing.Spawned)
            {
                if (Vector3.Distance(ExactPosition, targetPos) < 0.5f)
                {
                    Impact(intendedTarget.Thing);
                    return;
                }
            }
            else if (Position == intendedTarget.Cell)
            {
                if (DestinationCell.InBounds(base.Map))
                {
                    base.Position = DestinationCell;
                }
                ImpactSomething();
            }
            else if (AmbientSustInfo != null)
            {
                AmbientSustainer.Maintain();
            }
            if (shouldMalfunct && !malfunctioned && Extension.malfunctionChanceCurve != null && Rand.Chance(Extension.malfunctionChanceCurve.Evaluate(lifeTime)))
            {
                DoMalfunction();
            }
        }

        public void Move()
        {
            if (Guidance)
            {
                Vector3 targetVelocity = (targetPos - ExactPosition).Yto0();
                if (Extension != null && Extension.turningSpeed > 0)
                {
                    float angle = Vector3.SignedAngle(FlightVector, targetVelocity, Vector3.up);
                    if (Extension.lostTargetOutOfView && Math.Abs(angle) > Extension.viewConeAngle)
                    {
                        GuidanceOn = false;
                    }
                    if (Math.Abs(angle) * 60 > Extension.turningSpeed)
                    {
                        FlightVector = FlightVector.RotatedBy(Math.Sign(angle) * Extension.turningSpeed / 60);
                    }
                    else
                    {
                        FlightVector = targetVelocity;
                    }
                }
                else
                {
                    FlightVector = targetVelocity;
                }
            }
            FlightVector = FlightVector.normalized * speed;
            exactPos += FlightVector;
        }

        public static List<IntVec3> checkedCells = new List<IntVec3>();

        public virtual bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            if (lastExactPos == newExactPos)
            {
                return false;
            }
            List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, lastExactPos, newExactPos))
                {
                    Impact(null, blockedByShield: true);
                    return true;
                }
            }
            IntVec3 intVec = lastExactPos.ToIntVec3();
            IntVec3 intVec2 = newExactPos.ToIntVec3();
            if (intVec2 == intVec)
            {
                return false;
            }
            if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
            {
                return false;
            }
            if (intVec2.AdjacentToCardinal(intVec))
            {
                return (bool)CheckInterceptInfo.Invoke(this, new object[] { intVec2 });
            }
            if (VerbUtility.InterceptChanceFactorFromDistance(origin, intVec2) <= 0f)
            {
                return false;
            }
            Vector3 vect = lastExactPos;
            Vector3 v = newExactPos - lastExactPos;
            Vector3 vector = v.normalized * 0.2f;
            int num = (int)(v.MagnitudeHorizontal() / 0.2f);
            checkedCells.Clear();
            int num2 = 0;
            IntVec3 intVec3;
            do
            {
                vect += vector;
                intVec3 = vect.ToIntVec3();
                if (!checkedCells.Contains(intVec3))
                {
                    if ((bool)CheckInterceptInfo.Invoke(this, new object[] { intVec3 })) { return true; }
                    if (CheckForProxFuse(vect)) { return true; }
                    checkedCells.Add(intVec3);
                }
                num2++;
                if (num2 > num)
                {
                    return false;
                }
            }
            while (!(intVec3 == intVec2));
            return false;
        }

        public bool CheckForProxFuse(Vector3 vect)
        {
            if (Extension == null || !Extension.proxFuse || !Guidance)
            {
                return false;
            }
            float dist = Vector3.Distance(vect.Yto0(), targetPos);
            if (dist <= Extension.proxFuseRadius)
            {
                proxFuseArmed = true;
            }
            if (proxFuseArmed && dist > lastTargetDist)
            {
                Position = vect.ToIntVec3();
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
                Impact(null);
            }
            if (Rand.Chance(0.25f))
            {
                Redirect();
            }
            if (Rand.Chance(0.25f))
            {
                GuidanceOn = false;
                FlightVector = FlightVector.RotatedBy(Rand.Value * Extension.turningSpeed);
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            foreach (ThingComp comp in AllComps)
            {
                if (comp is CompExplosiveExposed compEx)
                {
                    compEx.DoDetonate(Map);
                }
            }
            base.Impact(hitThing, blockedByShield);
        }
    }

    public class Projectile_HomingExtension : DefModExtension
    {
        public IntRange guidanceDelay = new IntRange(0, 0);

        public float guidanceFailChance = 0;

        public float redirectionChance = 0.5f;

        public float redirectionRadius = 5;

        public float malfunctionChance = 0;



        public SimpleCurve malfunctionChanceCurve;

        public bool proxFuse = false;

        public float proxFuseRadius = 3;

        public float turningSpeed = -1;

        public bool lostTargetOutOfView = false;

        public float viewConeAngle = 45;

        public int maxLifeTime = 600;

        public SimpleCurve speedCurve;

        public float cruiseHeight = 2.5f;
    }

    public class Weapon_HomingExtension : DefModExtension
    {
        public float unaimedPenalty = 2;

        public List<HomingMultiplier> qualityMultipliers;
    }

    public class HomingMultiplier : IExposable
    {
        public QualityCategory quality;

        public float guidanceDelayMulti = 1f;

        public float guidanceFailChanceMulti = 1f;

        public float redirectionChanceMulti = 1f;

        public float redirectionRadiusMulti = 1f;

        public float malfunctionChanceMulti = 1f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref quality, "quality");
            Scribe_Values.Look(ref guidanceDelayMulti, "guidanceDelayMulti");
            Scribe_Values.Look(ref guidanceFailChanceMulti, "guidanceFailChanceMulti");
            Scribe_Values.Look(ref redirectionChanceMulti, "redirectionChanceMulti");
            Scribe_Values.Look(ref redirectionRadiusMulti, "redirectionRadiusMulti");
            Scribe_Values.Look(ref malfunctionChanceMulti, "malfunctionChanceMulti");
        }
    }
}

﻿using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AbilityUser
{
    public class Projectile_AbilityLaser : Projectile_AbilityBase
    {
        public bool canStartFire;
        public float drawingIntensity;
        public Matrix4x4 drawingMatrix;
        public Vector3 drawingPosition;
        public Vector3 drawingScale;
        public Material drawingTexture;
        public Thing hitThing;
        public int postFiringDuration;
        public float postFiringFinalIntensity;
        public float postFiringInitialIntensity;
        public Material postFiringTexture;
        public int preFiringDuration;
        public float preFiringFinalIntensity;

        // Custom XML variables.
        public float preFiringInitialIntensity;

        // Draw variables.
        public Material preFiringTexture;

        public float startFireChance;

        // Variables.
        public int tickCounter;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            drawingTexture = def.DrawMatSingle;
        }

        /// <summary>
        ///     Get parameters from XML.
        /// </summary>
        public void GetParametersFromXml()
        {
            var additionalParameters = def as ProjectileDef_AbilityLaser;

            preFiringDuration = additionalParameters.preFiringDuration;
            postFiringDuration = additionalParameters.postFiringDuration;

            // Draw.
            preFiringInitialIntensity = additionalParameters.preFiringInitialIntensity;
            preFiringFinalIntensity = additionalParameters.preFiringFinalIntensity;
            postFiringInitialIntensity = additionalParameters.postFiringInitialIntensity;
            postFiringFinalIntensity = additionalParameters.postFiringFinalIntensity;
            startFireChance = additionalParameters.StartFireChance;
            canStartFire = additionalParameters.CanStartFire;
        }

        /// <summary>
        ///     Save/load data from a savegame file (apparently not used for projectile for now).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickCounter, nameof(tickCounter));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                GetParametersFromXml();
        }

        /// <summary>
        ///     Main projectile sequence.
        /// </summary>
        public override void Tick()
        {
            //  //Log.Message("Tickng Ma Lazor");
            // Directly call the Projectile base Tick function (we want to completely override the Projectile Tick() function).
            //((ThingWithComponents)this).Tick(); // Does not work...
            try
            {
                if (tickCounter == 0)
                {
                    hitThing = intendedTarget.Thing;
                    GetParametersFromXml();
                    PerformPreFiringTreatment();
                }

                // Pre firing.
                if (tickCounter < preFiringDuration)
                {
                    GetPreFiringDrawingParameters();
                }
                // Firing.
                else if (tickCounter == preFiringDuration)
                {
                    Fire();
                    GetPostFiringDrawingParameters();
                }
                // Post firing.
                else
                {
                    GetPostFiringDrawingParameters();
                }
                if (tickCounter == preFiringDuration + postFiringDuration && !Destroyed)
                    Destroy();
                if (launcher != null)
                    if (launcher is Pawn)
                    {
                        var launcherPawn = launcher as Pawn;
                        if (launcherPawn.Dead && !Destroyed)
                            Destroy();
                    }
                tickCounter++;
            }
            catch
            {
                if (!Destroyed)
                    Destroy();
            }
        }

        /// <summary>
        ///     Performs prefiring treatment: data initalization.
        /// </summary>
        public virtual void PerformPreFiringTreatment()
        {
            DetermineImpactExactPosition();
            var cannonMouthOffset = (destination - origin).normalized * 0.9f;
            drawingScale = new Vector3(1f, 1f, (destination - origin).magnitude - cannonMouthOffset.magnitude);
            drawingPosition = origin + cannonMouthOffset / 2 + (destination - origin) / 2 + Vector3.up * def.Altitude;
            drawingMatrix.SetTRS(drawingPosition, ExactRotation, drawingScale);
        }

        /// <summary>
        ///     Gets the prefiring drawing parameters.
        /// </summary>
        public virtual void GetPreFiringDrawingParameters()
        {
            if (preFiringDuration != 0)
                drawingIntensity = preFiringInitialIntensity + (preFiringFinalIntensity - preFiringInitialIntensity) *
                                   tickCounter / preFiringDuration;
        }

        /// <summary>
        ///     Gets the postfiring drawing parameters.
        /// </summary>
        public virtual void GetPostFiringDrawingParameters()
        {
            if (postFiringDuration != 0)
                drawingIntensity = postFiringInitialIntensity +
                                   (postFiringFinalIntensity - postFiringInitialIntensity) *
                                   ((tickCounter - (float)preFiringDuration) / postFiringDuration);
        }

        /// <summary>
        ///     Checks for colateral targets (cover, neutral animal, pawn) along the trajectory.
        /// </summary>
        protected void DetermineImpactExactPosition()
        {
            // We split the trajectory into small segments of approximatively 1 cell size.
            var trajectory = destination - origin;
            var numberOfSegments = (int)trajectory.magnitude;
            var trajectorySegment = trajectory / trajectory.magnitude;

            var temporaryDestination = origin; // Last valid tested position in case of an out of boundaries shot.
            var exactTestedPosition = origin;

            for (var segmentIndex = 1; segmentIndex <= numberOfSegments; segmentIndex++)
            {
                exactTestedPosition += trajectorySegment;
                var testedPosition = exactTestedPosition.ToIntVec3();

                if (!exactTestedPosition.InBounds(Map))
                {
                    destination = temporaryDestination;
                    break;
                }

                if (!def.projectile.flyOverhead && segmentIndex >= 5)
                {
                    var list = Map.thingGrid.ThingsListAt(Position);
                    for (var i = 0; i < list.Count; i++)
                    {
                        var current = list[i];

                        // Check impact on a wall.
                        if (current.def.Fillage == FillCategory.Full)
                        {
                            destination = testedPosition.ToVector3Shifted() +
                                          new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                            hitThing = current;
                            break;
                        }

                        // Check impact on a pawn.
                        if (current.def.category == ThingCategory.Pawn)
                        {
                            var pawn = current as Pawn;
                            var chanceToHitCollateralTarget = 0.45f;
                            if (pawn.Downed)
                                chanceToHitCollateralTarget *= 0.1f;
                            var targetDistanceFromShooter = (ExactPosition - origin).MagnitudeHorizontal();
                            if (targetDistanceFromShooter < 4f)
                            {
                                chanceToHitCollateralTarget *= 0f;
                            }
                            else
                            {
                                if (targetDistanceFromShooter < 7f)
                                {
                                    chanceToHitCollateralTarget *= 0.5f;
                                }
                                else
                                {
                                    if (targetDistanceFromShooter < 10f)
                                        chanceToHitCollateralTarget *= 0.75f;
                                }
                            }
                            chanceToHitCollateralTarget *= pawn.RaceProps.baseBodySize;

                            if (Rand.Value < chanceToHitCollateralTarget)
                            {
                                destination = testedPosition.ToVector3Shifted() +
                                              new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                                hitThing = pawn;
                                break;
                            }
                        }
                    }
                }

                temporaryDestination = exactTestedPosition;
                //if (hitThing != null) Log.Message("Hit thing = " + hitThing.ToString());
            }
        }

        /// <summary>
        ///     Manages the projectile damage application.
        /// </summary>
        public virtual void Fire()
        {
            ApplyDamage(hitThing);
        }


        /// <summary>
        ///     Impacts a pawn/object or the ground.
        /// </summary>
        public override void Impact_Override(Thing hitThing)
        {
            //Log.Message("Impact override");
            base.Impact_Override(hitThing);
            if (hitThing != null)
            {
                //Log.Message("Hit thing found: " + hitThing.ToString() );

                var damageAmountBase = def.projectile.GetDamageAmount(1f);
                var dinfo = new DamageInfo(def.projectile.damageDef, damageAmountBase,
                    def.projectile.GetArmorPenetration(1f), ExactRotation.eulerAngles.y,
                    launcher, weapon: equipmentDef, intendedTarget: hitThing);
                var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, launcher.def, def, targetCoverDef);
                Find.BattleLog.Add(battleLogEntry_RangedImpact);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                //Log.Message("Hit thing taken damage: " + dinfo.Amount.ToString() + " " + dinfo.Def.label);

                //hitThing.TakeDamage(dinfo);
                if (canStartFire && Rand.Range(0f, 1f) > startFireChance)
                    hitThing.TryAttachFire(0.05f, launcher);
                if (hitThing is Pawn pawn)
                {
                    PostImpactEffects(launcher as Pawn, pawn);
                    FleckMaker.ThrowMicroSparks(destination, Map);
                    FleckMaker.Static(destination, Map, FleckDefOf.ShotHit_Dirt);
                }
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(SoundInfo.InMap(new TargetInfo(Position, Map)));
                FleckMaker.Static(ExactPosition, Map, FleckDefOf.ShotHit_Dirt);
                FleckMaker.ThrowMicroSparks(ExactPosition, Map);
            }
            var hitPawn = hitThing as Pawn;
            if (hitPawn?.stances != null && hitPawn.BodySize <= def.projectile.stoppingPower + 0.001f)
            {
                hitPawn.stances.StaggerFor(95);
            }
        }

        /// <summary>
        ///     JECRELL:: Added this to make derived classes work easily.
        /// </summary>
        /// <param name="launcher"></param>
        /// <param name="hitTarget"></param>
        public virtual void PostImpactEffects(Pawn launcher, Pawn hitTarget)
        {
        }

        /// <summary>
        ///     Draws the laser ray.
        /// </summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Comps_PostDraw();
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrix,
                FadedMaterialPool.FadedVersionOf(drawingTexture, drawingIntensity), 0);
        }
    }
}

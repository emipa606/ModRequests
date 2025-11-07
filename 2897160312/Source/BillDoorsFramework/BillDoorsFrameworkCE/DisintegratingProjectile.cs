using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;

namespace BillDoorsFramework
{
    public class DisintegratingProjectileCE : BulletCE
    {
        public DefModExtension_DisintegratingProjectile Data
        {
            get
            {
                return def.GetModExtension<DefModExtension_DisintegratingProjectile>();
            }
        }

        public DefModExtension_DisintegratingProjectileSoundExtension SoundData
        {
            get
            {
                return def.GetModExtension<DefModExtension_DisintegratingProjectileSoundExtension>();
            }
        }

        public CompColorableFaction compColorableFaction;

        public Material glowMat;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compColorableFaction = this.TryGetComp<CompColorableFaction>();
            if (Data.glowTexPath != null)
            {
                glowMat = MaterialPool.MatFrom(Data.glowTexPath);
                glowMat.shader = ShaderDatabase.TransparentPostLight;
            }

        }
        public override void Launch(Thing launcher, Vector2 origin, Thing equipment = null)
        {
            if (equipment is Building_TurretGun turret)
            {
                equipment = turret.gun;
            }
            base.Launch(launcher, origin, equipment);
        }

        public override void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0, float shotSpeed = -1, Thing equipment = null, float distance = -1)
        {
            if (equipment is Building_TurretGun turret)
            {
                equipment = turret.gun;
            }
            base.Launch(launcher, origin, shotAngle, shotRotation, shotHeight, shotSpeed, equipment, distance);
        }
        System.Random random = new System.Random();

        private float FadeOutStartPercentage
        {
            get
            {
                if (Data != null && Data.fadeOutStartPercentage > 0 && Data.fadeOutStartPercentage < 1)
                {
                    return Data.fadeOutStartPercentage;
                }
                return (2 / 3f);
            }
        }

        private float FadeOutExpandMultiplier
        {
            get
            {
                if (Data != null)
                {
                    return Data.fadeOutExpandMultiplier;
                }
                return 1;
            }
        }
        public float DistancePercent
        {
            get
            {
                if (origin == new Vector2(0, 0))
                {
                    return 2;
                }
                Vector3 v3Pos = ExactPosition;
                float distance = (origin - new Vector2(v3Pos.x, v3Pos.z)).magnitude;
                return distance / equipmentDef.Verbs[0].range;
            }
        }
        public float FadeOutPercent => Math.Max(0f, (float)(DistancePercent - FadeOutStartPercentage) / ((4 / 3f) - FadeOutStartPercentage));
        public override void Tick()
        {
            if (DistancePercent <= 1f)
            {
                base.Tick();
                return;
            }
            if (DistancePercent > (4 / 3f))
            {
                Destroy();
                return;
            }
            if (!landed)
            {
                ticksToImpact--;
                FlightTicks++;
                Vector3 nextPosition;
                nextPosition = MoveForward();
                if (!nextPosition.InBounds(base.Map))
                {
                    Position = LastPos.ToIntVec3();
                    Destroy();
                }
                ExactPosition = nextPosition;
                Position = ExactPosition.ToIntVec3();
                if (ticksToImpact <= 0)
                {
                    Destroy();
                }
            }
        }

        float gravityTemp = -1;

        float GravityTemp
        {
            get
            {
                if (gravityTemp < 0f)
                {
                    gravityTemp = CE_Utility.GravityConst;
                    if (def.projectile is ProjectilePropertiesCE props)
                    {
                        this.gravityTemp = props.Gravity;
                    }
                }
                return gravityTemp;
            }
        }

        protected float GetHeightAtTicks(int ticks)
        {
            var seconds = ((float)ticks) / GenTicks.TicksPerRealSecond;
            return (float)Math.Round(shotHeight + shotSpeed * Mathf.Sin(shotAngle) * seconds - (GravityTemp * seconds * seconds) / 2f, 3);
        }

        public override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            if (Data != null && Data.shouldStartFire)
            {
                if (landed)
                {
                    startFire(map);
                }
                else
                {
                    startFire(hitThing, map);
                }
            }
            if (SoundData != null)
            {
                if (hitThing != null)
                {
                    if (hitThing is Pawn pawn)
                    {
                        if (pawn.IsNotFresh())
                        {
                            SoundData.metalImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                        else
                        {
                            SoundData.fleshImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                    }
                    if (hitThing is Plant plant && ((plant.def.ingestible != null && plant.def.ingestible.foodType == FoodTypeFlags.Tree) || plant.def.defName == "BurnedTree"))
                    {
                        SoundData.woodImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                    }
                    if (hitThing is Building building && building.Stuff != null)
                    {
                        List<StuffCategoryDef> stuffs = building.Stuff.stuffProps.categories;

                        if (stuffs.Contains(StuffCategoryDefOf.Stony))
                        {
                            SoundData.stoneImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                        if (stuffs.Contains(StuffCategoryDefOf.Woody))
                        {
                            SoundData.woodImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                        if (stuffs.Contains(StuffCategoryDefOf.Metallic))
                        {
                            SoundData.metalImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                    }
                    if (hitThing is Mineable)
                    {
                        SoundData.stoneImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                    }
                }
                else if (base.Position.GetTerrain(map).takeSplashes)
                {
                    SoundData.waterImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                }
                else
                {
                    SoundData.terrainImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                }
            }
        }

        private void startFire(Map map)
        {
            if (Rand.Chance(Data.chanceOfFire))
            {
                float fireSize = Data.minFireSize + (float)(random.NextDouble() * (Data.maxFireSize - Data.minFireSize));
                FireUtility.TryStartFireIn(ExactPosition.ToIntVec3(), map, fireSize, launcher);
            }
        }

        private void startFire(Thing thing, Map map)
        {
            if (Rand.Chance(Data.chanceOfFire))
            {
                float fireSize = Data.minFireSize + (float)(random.NextDouble() * (Data.maxFireSize - Data.minFireSize));

                if (thing is Pawn)
                {
                    FireUtility.TryAttachFire(thing, fireSize, launcher);
                }
                else if (thing.FlammableNow)
                {
                    FireUtility.TryStartFireIn(thing.Position, map, fireSize, launcher);
                }
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (!(FlightTicks == 0 && launcher != null && launcher is Pawn))
            {
                Material material = new Material(def.DrawMatSingle);
                Color color = material.color;
                if (Data != null && !Data.shouldIgnoreColorable && compColorableFaction != null && BDPMod.enableProjectileColoring)
                {
                    color = compColorableFaction.FactionColor();
                }
                color.a *= 1 - FadeOutPercent;
                material.color = color;
                float drawSize = 1 + (FadeOutPercent * FadeOutExpandMultiplier);
                Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos, DrawRotation, material, 0);

                if (glowMat != null)
                {
                    Material glowMatTemp = new Material(glowMat);
                    glowMatTemp.color *= 1 - FadeOutPercent;
                    Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos + new Vector3(0, 0.1f, 0), DrawRotation, glowMatTemp, 0);
                }

                if (castShadow)
                {
                    Vector3 position = new Vector3(ExactPosition.x, def.Altitude - 0.01f, ExactPosition.z - Mathf.Max(0f, ExactPosition.y));
                    Material shadowMaterial = ShadowMaterial;
                    color = shadowMaterial.color;
                    color.a *= 1 - FadeOutPercent;
                    shadowMaterial.color = color;
                    Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), position, DrawRotation, shadowMaterial, 0);
                }
            }
        }
    }
}

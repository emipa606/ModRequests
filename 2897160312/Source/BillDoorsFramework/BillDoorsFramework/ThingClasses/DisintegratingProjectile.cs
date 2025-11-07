using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    public class DisintegratingProjectile : Bullet
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

        private float RangeMulti
        {
            get
            {
                if (Data != null)
                {
                    return Data.rangeMulti;
                }
                return 1;
            }
        }

        public PropertyInfo P_ShadowMaterial
        {
            get
            {
                if (P_material == null)
                {
                    Reflect();
                }
                return P_material;
            }
        }
        public PropertyInfo P_material = null;
        public PropertyInfo P_LastPos
        {
            get
            {
                if (P_lastPos == null)
                {
                    Reflect();
                }
                return P_lastPos;
            }
        }
        public PropertyInfo P_lastPos = null;
        private void Reflect()
        {
            foreach (PropertyInfo x in typeof(Projectile).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (x.Name == "ShadowMaterial")
                {
                    P_material = x;
                    continue;
                }
                if (x.Name == "LastPos")
                {
                    P_lastPos = x;
                    continue;
                }
            }
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            if (equipment is Building_TurretGun turret)
            {
                equipment = turret.gun;
            }
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        }

        public float DistancePercent
        {
            get
            {
                float distance = (origin - ExactPosition).magnitude;
                return distance / (equipmentDef.Verbs[0].range * RangeMulti);
            }
        }

        public float FadeOutPercent => Math.Max(0f, (float)(DistancePercent - FadeOutStartPercentage) / (1 - FadeOutStartPercentage));
        public override void Tick()
        {
            if (DistancePercent <= 1f)
            {
                base.Tick();
                return;
            }
            if (DistancePercent > 1)
            {
                Destroy();
                return;
            }
            if (!landed)
            {
                P_LastPos.SetValue(this, ExactPosition);
                ticksToImpact--;
                if (!ExactPosition.InBounds(base.Map))
                {
                    Position = ((Vector3)P_LastPos.GetValue(this)).ToIntVec3();
                    Destroy();
                }
                Position = ExactPosition.ToIntVec3();
                if (ticksToImpact <= 0)
                {
                    Destroy();
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (launcher != null)
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
                Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos, ExactRotation, material, 0);
                if (glowMat != null)
                {
                    Material glowMatTemp = new Material(glowMat);
                    glowMatTemp.color *= 1 - FadeOutPercent;
                    Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos + new Vector3(0, 0.1f, 0), ExactRotation, glowMatTemp, 1);
                }
            }
        }
    }

    public class DisintegratingProjectileExplosive : Projectile_Explosive
    {
        public DefModExtension_DisintegratingProjectile Data
        {
            get
            {
                return def.GetModExtension<DefModExtension_DisintegratingProjectile>();
            }
        }

        public CompColorableFaction compColorableFaction;

        public Material glowMat = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compColorableFaction = this.TryGetComp<CompColorableFaction>();
            if (Data?.glowTexPath != null)
            {
                glowMat = MaterialPool.MatFrom(Data.glowTexPath);
                glowMat.shader = ShaderDatabase.TransparentPostLight;
            }
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

        public PropertyInfo P_ShadowMaterial
        {
            get
            {
                if (P_material == null)
                {
                    Reflect();
                }
                return P_material;
            }
        }
        public PropertyInfo P_material = null;
        public PropertyInfo P_LastPos
        {
            get
            {
                if (P_lastPos == null)
                {
                    Reflect();
                }
                return P_lastPos;
            }
        }
        public PropertyInfo P_lastPos = null;
        private void Reflect()
        {
            foreach (PropertyInfo x in typeof(Projectile).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (x.Name == "ShadowMaterial")
                {
                    P_material = x;
                    continue;
                }
                if (x.Name == "LastPos")
                {
                    P_lastPos = x;
                    continue;
                }
            }
        }
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            if (equipment is Building_TurretGun turret)
            {
                equipment = turret.gun;
            }
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        }
        public float DistancePercent
        {
            get
            {
                float distance = (origin - ExactPosition).magnitude;
                return distance / equipmentDef.Verbs[0].range;
            }
        }
        public float FadeOutPercent => Math.Max(0f, (float)(DistancePercent - FadeOutStartPercentage) / (1 - FadeOutStartPercentage));
        public override void Tick()
        {
            if (DistancePercent <= 1f)
            {
                base.Tick();
                return;
            }
            if (DistancePercent > 1)
            {
                Destroy();
                return;
            }
            if (!landed)
            {
                P_LastPos.SetValue(this, ExactPosition);
                ticksToImpact--;
                if (!ExactPosition.InBounds(base.Map))
                {
                    Position = ((Vector3)P_LastPos.GetValue(this)).ToIntVec3();
                    Destroy();
                }
                Position = ExactPosition.ToIntVec3();
                if (ticksToImpact <= 0)
                {
                    Destroy();
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
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
        }

        protected override void Explode()
        {
            DefModExtension_ConeExplosion extension = def.GetModExtension<DefModExtension_ConeExplosion>();
            if (extension == null)
            {
                base.Explode();
                return;
            }
            else
            {
                float direction = origin.AngleToFlat(destination);
                FloatRange angle = new FloatRange(direction - extension.angle, direction + extension.angle);
                Map map = base.Map;
                Destroy();
                if (def.projectile.explosionEffect != null)
                {
                    Effecter effecter = def.projectile.explosionEffect.Spawn();
                    effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
                    effecter.Cleanup();
                }
                ConeExplosionUtility.DoExplosion(base.Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: direction, ignoredThings: null, affectedAngle: angle, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor);
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (launcher != null)
            {
                Material material = new Material(def.DrawMatSingle);
                Color color = material.color;
                if (!(Data?.shouldIgnoreColorable ?? false) && compColorableFaction != null)
                {
                    color = compColorableFaction.FactionColor();
                }
                color.a *= 1 - FadeOutPercent;
                material.color = color;
                float drawSize = 1 + (FadeOutPercent * FadeOutExpandMultiplier);
                Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos, ExactRotation, material, 0);
                if (glowMat != null)
                {
                    Material glowMatTemp = new Material(glowMat);
                    glowMatTemp.color *= 1 - FadeOutPercent;
                    Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos + new Vector3(0, 0.1f, 0), ExactRotation, glowMatTemp, 1);
                }
            }
        }
    }
    public class DefModExtension_DisintegratingProjectile : DefModExtension
    {
        public string glowTexPath;
        public float fadeOutStartPercentage = (2 / 3f);
        public float fadeOutExpandMultiplier = 1;
        public bool shouldStartFire = false;
        public float chanceOfFire = 1f;
        public float minFireSize = 0.1f;
        public float maxFireSize = 1;
        public bool shouldIgnoreColorable = true;
        public float rangeMulti = 1;
    }

    public class DefModExtension_DisintegratingProjectileSoundExtension : DefModExtension
    {
        public SoundDef fleshImpactSound;
        public SoundDef metalImpactSound;
        public SoundDef stoneImpactSound;
        public SoundDef terrainImpactSound;
        public SoundDef waterImpactSound;
        public SoundDef woodImpactSound;
    }
}

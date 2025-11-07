using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompExplosiveCECone : CompExplosiveCE
    {
        private CompProperties_ExplosiveCECone Props => props as CompProperties_ExplosiveCECone;

        public override void Explode(Thing instigator, Vector3 pos, Map map, float scaleFactor = 1f, float? direction = null, List<Thing> ignoredThings = null)
        {
            IntVec3 intVec = pos.ToIntVec3();
            if (map == null)
            {
                Log.Warning("Tried to do explodeCE in a null map.");
                return;
            }
            if (!intVec.InBounds(map))
            {
                Log.Warning("Tried to explodeCE out of bounds");
                return;
            }
            parent.TryGetComp<CompFragments>()?.Throw(pos, map, instigator);

            float angle;
            if (instigator is ProjectileCE projectile)
            {
                angle = ConeExplosionUtility.CEangleToRWangle(projectile.shotRotation);
            }
            else
            {
                angle = instigator.Rotation.AsAngle - 90;
            }
            FloatRange angleRange = new FloatRange(angle - Props.angle, angle + Props.angle);

            if (Props.explosiveRadius > 0f && parent.def != null)
            {
                float explosiveRadius = Props.explosiveRadius;
                DamageDef explosiveDamageType = Props.explosiveDamageType;
                int damAmount = GenMath.RoundRandom(Props.damageAmountBase);
                float explosionArmorPenetration = Props.GetExplosionArmorPenetration();
                SoundDef explosionSound = Props.explosionSound;
                ThingDef def = parent.def;
                ThingDef postExplosionSpawnThingDef = Props.postExplosionSpawnThingDef;
                float postExplosionSpawnChance = Props.postExplosionSpawnChance;
                int postExplosionSpawnThingCount = Props.postExplosionSpawnThingCount;
                GasType? postExplosionGasType = Props.postExplosionGasType;
                bool applyDamageToExplosionCellsNeighbors = Props.applyDamageToExplosionCellsNeighbors;
                ThingDef preExplosionSpawnThingDef = Props.preExplosionSpawnThingDef;
                float preExplosionSpawnChance = Props.preExplosionSpawnChance;
                int preExplosionSpawnThingCount = Props.preExplosionSpawnThingCount;
                float chanceToStartFire = Props.chanceToStartFire;
                bool damageFalloff = Props.damageFalloff;
                float propagationSpeed = Props.propagationSpeed;
                float y = pos.y;
                ConeExplosionUtility.DoExplosion(intVec, map, explosiveRadius, explosiveDamageType, instigator, damAmount, explosionArmorPenetration, explosionSound, null, def, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, direction, ignoredThings, angleRange, doVisualEffects: true, propagationSpeed, 0f, doSoundEffects: true, null, scaleFactor);
            }
        }
    }


    public class CompProperties_ExplosiveCECone : CompProperties_ExplosiveCE
    {
        public float angle = 30;

        public float propagationSpeed = 1;

        public CompProperties_ExplosiveCECone()
        {
            compClass = typeof(CompExplosiveCECone);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace ChickenExplosives
{
    class CompProjectileSprayer : ThingComp
    {
        public CompProperties_ProjectileSprayer Props => (CompProperties_ProjectileSprayer)base.props;
        public bool fired = false;

        public void Fire()
        {
            if (fired) return;
            // throw projectiles
            for(int i = 0; i < Props.projectileCount; i++)
            {
                Projectile cur = GenSpawn.Spawn(Props.projectileDef, base.parent.Position, base.parent.Map) as Projectile;
                IntVec3 targetCell = RandomTargetCell();
                cur.Launch(base.parent, targetCell, null, ProjectileHitFlags.All);
            }
            fired = true;
        }
        public IntVec3 RandomTargetCell()
        {
            Rot4 rot = base.parent.Rotation;
            IntVec3 pos = base.parent.Position;
            int distance = (int)Props.projectileDistanceCurve.Evaluate(Rand.Value);
            float horizontalAngle = Radians(Mathf.Sin(Rand.Range(-1 * Props.angle, Props.angle)));
            int horizontalOffset = (int)(horizontalAngle * distance);
            switch (rot.AsByte)
            {
                // North
                case 0:
                    return new IntVec3(pos.x + horizontalOffset, pos.y, pos.z + distance);
                // East
                case 1:
                    return new IntVec3(pos.x + distance, pos.y, pos.z - horizontalOffset);
                // South
                case 2:
                    return new IntVec3(pos.x - horizontalOffset, pos.y, pos.z - distance);
                // West
                case 3:
                    return new IntVec3(pos.x - distance, pos.y, pos.z + horizontalOffset);
                default:
                    Log.Warning("[ChickenExplosives] RandomTargetCell() has invalid rotation. Returning parent cell...");
                    return pos;
            }
        }
        public const float TAU = Mathf.PI * 2;
        public static float Radians(float degrees)
        {
            return TAU * (degrees / 180f);
        }
    }
    class CompProperties_ProjectileSprayer : CompProperties
    {
#pragma warning disable CS0649
        public ThingDef projectileDef;
        public int projectileCount;
        public SimpleCurve projectileDistanceCurve;
        public float angle;
#pragma warning restore CS0649

        public CompProperties_ProjectileSprayer()
        {
            base.compClass = typeof(CompProjectileSprayer);
        }
    }
}

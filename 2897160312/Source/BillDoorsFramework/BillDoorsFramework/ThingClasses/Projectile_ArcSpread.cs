using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Projectile_ArcSpread : Bullet
    {
        RotationalArcSpreadExtension Extension => def.GetModExtension<RotationalArcSpreadExtension>();
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            if (Extension.spreadWhenFired)
            {
                Vector3 pos = Extension.useActualTarget ? usedTarget.Cell.ToVector3() : intendedTarget.Cell.ToVector3();
                if (Extension.random)
                {
                    int num = GenRadial.NumCellsInRadius(Extension.range.max);
                    List<IntVec3> affectedCells = new List<IntVec3>();
                    for (int i = 1; i < num; i++)
                    {
                        IntVec3 intVec = launcher.Position + GenRadial.RadialPattern[i];
                        Vector3 vec3 = (Extension.invert ? launcher.Position - intVec : intVec - launcher.Position).ToVector3();
                        vec3.y = 0;
                        if (Math.Abs(Vector3.Angle(pos - origin, vec3)) < Extension.arc && Vector3.Distance(origin, vec3) > Extension.range.min)
                        {
                            affectedCells.Add(intVec);
                        }
                    }
                    for (int i = 0; i < Extension.amount; i++)
                    {
                        Projectile thing = (Projectile)GenSpawn.Spawn(Extension.projectile, launcher.Position, Map);
                        thing.Launch(launcher, origin, affectedCells.RandomElement(), intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                    }
                }
                else
                {
                    Vector3 vector = (pos - origin);
                    vector.Normalize();
                    vector = vector.RotatedBy(-Extension.arc);
                    for (int i = 0; i < Extension.amount; i++)
                    {
                        Projectile thing = (Projectile)GenSpawn.Spawn(Extension.projectile, launcher.Position, Map);
                        thing.Launch(launcher, origin, (origin + vector * Extension.range.RandomInRange).ToIntVec3(), intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

                        vector = vector.RotatedBy(Extension.arc / (Extension.amount - 1) * 2);
                    }
                }
                Destroy();
                return;
            }
            else
            {
                base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!Extension.spreadWhenFired)
            {
                if (Extension.random)
                {
                    int num = GenRadial.NumCellsInRadius(Extension.range.max);
                    List<IntVec3> affectedCells = new List<IntVec3>();
                    for (int i = 1; i < num; i++)
                    {
                        IntVec3 intVec = Position + GenRadial.RadialPattern[i];
                        Vector3 vec3 = (Extension.invert ? Position - intVec : intVec - Position).ToVector3();
                        vec3.y = 0;
                        if (Math.Abs(Vector3.Angle(intendedTarget.Cell.ToVector3() - origin, vec3)) < Extension.arc && Vector3.Distance(ExactPosition, vec3) > Extension.range.min)
                        {
                            affectedCells.Add(intVec);
                        }
                    }
                    for (int i = 0; i < Extension.amount; i++)
                    {
                        Projectile thing = (Projectile)GenSpawn.Spawn(Extension.projectile, Position, Map);
                        thing.Launch(launcher, ExactPosition, affectedCells.RandomElement(), intendedTarget, HitFlags, preventFriendlyFire, this, targetCoverDef);
                    }
                }
                else
                {
                    Vector3 vector = (intendedTarget.Cell.ToVector3() - origin);
                    vector.Normalize();
                    vector = vector.RotatedBy(-Extension.arc);
                    for (int i = 0; i < Extension.amount; i++)
                    {
                        Projectile thing = (Projectile)GenSpawn.Spawn(Extension.projectile, Position, Map);
                        thing.Launch(launcher, ExactPosition, (ExactPosition + vector * Extension.range.RandomInRange).ToIntVec3(), intendedTarget, HitFlags, preventFriendlyFire, this, targetCoverDef);

                        vector = vector.RotatedBy(Extension.arc / (Extension.amount - 1) * 2);
                    }
                }
                Destroy();
                return;
            }
            else
            {
                base.Impact(hitThing, blockedByShield);
            }
        }
    }

    public class RotationalArcSpreadExtension : DefModExtension
    {
        public bool random = false;
        public bool invert = false;
        public bool useActualTarget = false;
        public ThingDef projectile;
        public int amount = 5;
        public float arc = 45;
        public bool spreadWhenFired = true;
        public IntRange range = new IntRange(3, 7);
    }
}

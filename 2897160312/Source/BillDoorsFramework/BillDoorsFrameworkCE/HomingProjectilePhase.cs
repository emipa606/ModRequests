using CombatExtended;
using CombatExtended.Compatibility;
using ProjectileImpactFX;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BillDoorsFramework
{
    public abstract class HomingProjectilePhase
    {
        float maxVerticalAngle = 45f;

        float speedPct = 1;

        public float SpeedPct => speedPct;

        protected float MaxVerticalSpeed(ProjectileCE_Homing projectile)
        {
            if (maxVerticalAngle >= 90 || maxVerticalAngle <= 0)
            {
                return float.MaxValue;
            }
            return (float)Math.Tan(maxVerticalAngle * Mathf.Deg2Rad) * projectile.Velocity.magnitude;
        }

        List<HomingProjectilePhaseSwitchCondition> switchConditions = new List<HomingProjectilePhaseSwitchCondition>();

        public virtual bool ShouldInvoke(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            if (switchConditions.NullOrEmpty())
            {
                return true;
            }
            foreach (var switchCondition in switchConditions)
            {
                if (switchCondition.ShouldInvoke(projectile, targetInfo))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual float HeightDelta(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            float f = TargetHeight(projectile, targetInfo) - projectile.Height;
            float v = MaxVerticalSpeed(projectile);
            if (Math.Abs(f) < v) return f;
            return v * Math.Sign(f);
        }


        public abstract float TargetHeight(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo);
    }

    public class HomingProjectilePhase_Constant : HomingProjectilePhase
    {
        float height = 3.5f;

        public override float TargetHeight(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            return height;
        }
    }

    public class HomingProjectilePhase_Maintain : HomingProjectilePhase
    {
        public override float TargetHeight(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            return projectile.Height;
        }
        public override float HeightDelta(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            return 0;
        }
    }

    public class HomingProjectilePhase_Dive : HomingProjectilePhase
    {
        CollisionVertical victimVert;
        Thing thing;
        float? targetDescentSpeed;

        public override float HeightDelta(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            if (targetInfo.HasThing)
            {
                if (targetDescentSpeed == null)
                {
                    float pct = projectile.Velocity.magnitude / projectile.DistanceToTarget;
                    targetDescentSpeed = pct * (TargetHeight(projectile, targetInfo) - projectile.Height);
                }
            }
            return targetDescentSpeed ?? base.HeightDelta(projectile, targetInfo);
        }

        public override float TargetHeight(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            if (targetInfo.HasThing)
            {
                if (targetInfo.Thing != thing)
                {
                    victimVert = new CollisionVertical(targetInfo.Thing);
                    thing = targetInfo.Thing;
                }
                return victimVert.HeightRange.Span * 0.5f + victimVert.HeightRange.min;
            }
            return 0;
        }
    }
}

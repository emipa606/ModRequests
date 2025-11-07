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
using Verse.Sound;

namespace BillDoorsFramework
{
    public abstract class HomingProjectilePhaseSwitchCondition
    {
        public abstract bool ShouldInvoke(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo);
    }

    public class HomingProjectilePhaseSwitchCondition_TimePassed : HomingProjectilePhaseSwitchCondition
    {
        int tickPassed;

        public override bool ShouldInvoke(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            return projectile.lifeTime > tickPassed;
        }
    }

    public class HomingProjectilePhaseSwitchCondition_TargetDistance : HomingProjectilePhaseSwitchCondition
    {
        float range;

        public override bool ShouldInvoke(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            return projectile.DistanceToTarget < range;
        }
    }

    public class HomingProjectilePhaseSwitchCondition_HeightAbove : HomingProjectilePhaseSwitchCondition
    {
        float height;

        public override bool ShouldInvoke(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            return projectile.Height >= height;
        }
    }
}

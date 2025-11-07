using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompShieldSpawnerCE : CompShieldSpawner
    {

        public override void addCEprojectile(ThingWithComps thing, List<ThingWithComps> projectiles)
        {
            if (thing is ProjectileCE b && IsUnfriendly(b, parent) && CollisionDetermination(b.ExactPosition, parent.TrueCenter(), Props.radius, Props.invalidRadius))
            {
                projectiles.Add(b);
            }
        }

        public override float getDamageCE(ThingWithComps thing)
        {
            if (thing is BulletCE bullet)
            {
                return bullet.DamageAmount;
            }
            if (thing is ProjectileCE projectileCE)
            {
                return projectileCE.def.projectile.GetDamageAmount(1f, null);
            }
            return -1;
        }

        public static bool IsUnfriendly(ProjectileCE a, Thing b)
        {
            return a.launcher == null || b == null || a.launcher.Faction == null || b.Faction == null || a.launcher.Faction.HostileTo(b.Faction);
        }
    }

    public static class ProjectilesStore
    {
        public static List<ThingWithComps> projectiles = new List<ThingWithComps>();
        public static bool IsProjectile(ThingWithComps thing)
        {
            if (thing is Projectile) return true;
            if (thing is ProjectileCE) return true;
            return false;
        }
    }
}

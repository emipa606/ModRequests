using UnityEngine;
using Verse;

namespace ArchotechWeaponry.Utils
{
    public class ProjectileUtils
    {
        public static float RecalculateNewTicksToImpact(Projectile projectile, Vector3 newDestination)
        {
            float num = (projectile.ExactPosition - newDestination).magnitude / projectile.def.projectile.SpeedTilesPerTick;
            return num;
        }
    }
}
using System;
using Vehicles;
using Verse;

namespace BillDoorsFramework
{
    [Obsolete]
    public class Reactor_ExplosiveExplicitDamage : Reactor
    {
        public float chance = 1;
        public float maxHealth = 1;
        public int radius;
        public int damage = 1;
        public DamageDef damageDef;

        public override void Hit(VehiclePawn vehicle, VehicleComponent component, ref DamageInfo dinfo, VehicleComponent.Penetration penetration)
        {
            if ((component.health / component.props.health) <= maxHealth && Rand.Chance(chance))
            {
                if (!component.props.hitbox.cells.TryRandomElement(out IntVec2 offset))
                {
                    offset = IntVec2.Zero;
                }
                IntVec3 loc = new IntVec3(vehicle.Position.x + offset.x, 0, vehicle.Position.z + offset.z);
                GenExplosion.DoExplosion(loc, vehicle.Map, radius, damageDef, vehicle, damage, damageDef.defaultArmorPenetration);
            }
        }
    }
}

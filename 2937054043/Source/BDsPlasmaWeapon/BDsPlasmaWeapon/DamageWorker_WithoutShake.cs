using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace BDsPlasmaWeapon
{
    public class DamageWorker_ExtinguishWithoutShake : DamageWorker_Extinguish
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (def.explosionHeatEnergyPerCell > float.Epsilon)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            }
            ExplosionVisualEffectCenter(explosion);
        }
    }
}

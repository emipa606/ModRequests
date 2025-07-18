using UnityEngine;
using Verse;

namespace Kraltech_Industries;

public class Projectile_MjolnirThunder : Projectile_Explosive
{
    private readonly Map map;

    private int _ticksToDetonation;

    protected override void Explode()
    {
        Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_Mjolnir_ThunderStrike(Map, Position));
    }

    protected virtual void Impact(Thing hitThing)
    {
        if (def.projectile.explosionDelay == 0)
        {
            Explode();
            return;
        }

        landed = true;
        _ticksToDetonation = Mathf.CeilToInt(Rand.Range(def.projectile.explosionDelay / 1.5f, def.projectile.explosionDelay * 1.5f));
        GenExplosion.DoExplosion(Position, map, 4.9f, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, null, equipmentDef, def, intendedTarget.Thing, null, 0f, 1, null, false, null, 0f, 1, def.projectile.explosionChanceToStartFire, false, null, null, null, true, def.projectile.damageDef.expolosionPropagationSpeed);
    }

    public override void Tick()
    {
        base.Tick();
        if (_ticksToDetonation > 0)
        {
            _ticksToDetonation--;
            if (_ticksToDetonation <= 0) Explode();
        }
    }

    public override void SpawnSetup(Map map, bool disapearAfterLoad)
    {
        base.SpawnSetup(map, disapearAfterLoad);
        Impact(null);
    }
}
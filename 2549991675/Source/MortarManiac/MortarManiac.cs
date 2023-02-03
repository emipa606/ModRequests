using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using RimWorld;
using Unity;
using Random = System.Random;


namespace MortarManiac
{

    
    public class Projectile_ClusterShell : Projectile_Explosive
    {
        private int ticksToDetonation;

        protected override void Impact(Thing hitThing)
        {
            if (def.projectile.explosionDelay == 0)
            {
                Explode();
                return;
            }
            landed = true;
            ticksToDetonation = Mathf.CeilToInt(Rand.Range(def.projectile.explosionDelay / 1.5f, def.projectile.explosionDelay * 1.5f));
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef);
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksToDetonation > 0)
            {
                ticksToDetonation--;
                if (ticksToDetonation <= 0)
                {
                    Explode();
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Impact(null);
            Log.Message("SPAWNED");
        }
    }


    public class Projectile_StormShell : Projectile_Explosive
    {
        private int _ticksToDetonation;

        protected override void Explode()
        {
            Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Map, this.Position));
            
        }
        protected override void Impact(Thing hitThing)
        {
            if (def.projectile.explosionDelay == 0)
            {
                Explode();
                return;
            }

            landed = true;
            _ticksToDetonation =
                Mathf.CeilToInt(Rand.Range(def.projectile.explosionDelay / 1.5f, def.projectile.explosionDelay * 1.5f));
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef);
            
        }


        public override void Tick()
        {
            base.Tick();
            if (_ticksToDetonation > 0)
            {
                _ticksToDetonation--;
                if (_ticksToDetonation <= 0)
                {
                    Explode();
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Impact(null);
            Log.Message("SPAWNED");
        }
    }
}
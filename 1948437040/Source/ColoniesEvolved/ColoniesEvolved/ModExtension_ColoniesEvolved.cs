using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ColoniesEvolved
{
    public class ModExtension_ColoniesEvolved : DefModExtension
    {
        public float severityPerHit = 0.15f;
        public HediffDef hediffToAdd;

        public EffecterDef explosionEffect = null;

        public int damageAmountBase = -1;

        public DamageDef explosiveDamageType;
        public SoundDef explosionSound = null;
        public ThingDef preExplosionSpawnThingDef = null;
        public ThingDef postExplosionSpawnThingDef = null;
        public int preExplosionSpawnThingCount = 1;
        public int postExplosionSpawnThingCount = 1;
        public float explosiveRadius = 1.0f;
        public float armorPenetrationBase = -1;
        public float preExplosionSpawnChance = 0;
        public float postExplosionSpawnChance = 1;
        public float chanceToStartFire = 0;
        public bool applyDamageToExplosionCellsNeighbors = false;
        public bool damageFalloff = false;
    }
}

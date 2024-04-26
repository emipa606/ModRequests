using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
    public class GasDef: ThingDef
    {
        public int damage = 0;

        public DamageDef damagetype = DamageDefOf.Bomb;

        public List<HediffDef> appliedHediffs = new List<HediffDef>();

        public bool applyColonists = false;

        public bool applyAllies = false;

        public bool applyEnemies = true;        

    }
}

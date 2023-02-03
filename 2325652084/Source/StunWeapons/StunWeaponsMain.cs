using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace StunWeapons
{
    public class ThingDef_StunBullet : ThingDef
    {
        public float addHediffChance = 0.5f;
        public HediffDef hediffToAdd;
    }
}

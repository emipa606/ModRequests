using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace JAHV
{
    public class CompProperties_CombatBoost : CompProperties
    {
        public float range;

        public TargetingParameters targetingParameters;

        public HediffDef hediff;

        public int duration = 30;

        public int cooldown = 1000;

        public CompProperties_CombatBoost()
        {
            compClass = typeof(Comp_CombatBoost);
        }
    }
}

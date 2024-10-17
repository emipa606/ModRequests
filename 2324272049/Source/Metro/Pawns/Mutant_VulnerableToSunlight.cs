using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
	public class Mutant_VulnerableToSunlight : Mutant
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (!this.health.hediffSet.HasHediff(MetroDefOf.Metro_SunLightDamage))
                {
                    var hediff = HediffMaker.MakeHediff(MetroDefOf.Metro_SunLightDamage, this);
                    this.health.AddHediff(hediff);
                    hediff.severityInt = 0;
                }
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AustralianAnimals
{
    public class Koala : Pawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var alreadySleepy = health?.hediffSet?.GetFirstHediffOfDef(DefOfs.LowCarbDiet);
            if(alreadySleepy == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(DefOfs.LowCarbDiet, this, null);
                health.AddHediff(hediff, null, null);
            }
        }

        
    }
}

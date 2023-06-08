using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace ColoniesEvolved
{
    class Projectile_FuelRodBullet : Projectile_Explosive
    {
        public ModExtension_ColoniesEvolved Props => base.def.GetModExtension<ModExtension_ColoniesEvolved>();

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Props != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                Log.Message("Pawn Hit and Hediff Severity less than 1");
                Hediff BlamOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Props.hediffToAdd);



                if (BlamOnPawn != null)
                {
                    BlamOnPawn.Severity += Props.severityPerHit;

                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(Props.hediffToAdd, hitPawn);
                    hediff.Severity = Props.severityPerHit;
                    hitPawn.health.AddHediff(hediff);

                }
            }
        }
    }
}

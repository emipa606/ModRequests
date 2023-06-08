using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace ColoniesEvolved
{
    public class Projectile_NeedlerBullet : Bullet
    {
        public ModExtension_ColoniesEvolved Props => base.def.GetModExtension<ModExtension_ColoniesEvolved>();
     
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Props != null  && hitThing != null && hitThing is Pawn hitPawn)
            {
                Log.Message("Pawn Hit and Hediff Severity less than 1");
                Hediff BlamOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Props.hediffToAdd);

               

                if (BlamOnPawn != null)
                {
                    BlamOnPawn.Severity += Props.severityPerHit;
                    if (BlamOnPawn.Severity >= 0.90)
                    {
                        Log.Message("Supercombine state reached!");


                        GenExplosion.DoExplosion(hitPawn.PositionHeld, hitPawn.Map, Props.explosiveRadius, Props.explosiveDamageType, hitPawn, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, null, null, null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff);
                        BlamOnPawn.Severity = 0;


                    }
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


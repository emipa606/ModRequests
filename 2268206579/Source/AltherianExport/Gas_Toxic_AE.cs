using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
    // Modifies gas to deal damage-over-time while in it, and apply specific hediffs
    public class Gas_Toxic_AE : Gas
    {
        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                List<Thing> targets = this.ThingsInCloud();
                foreach (Thing t in targets)
                {
                    this.DoDamage(t, 1f);
                    Pawn p = t as Pawn;
                    if (p != null) 
                    {
                        if(!p.RaceProps.IsMechanoid) HealthUtility.AdjustSeverity(p, HediffDefOf_AE.ToxicIllness_AE, 0.02f);
                    }
                }
            }

            base.Tick();
        }

        private List<Thing> ThingsInCloud()
        {
            List<Thing> allPawnsSpawned = this.Map.listerThings.AllThings;
            List<Thing> inCell = new List<Thing>();

            foreach (Thing t in allPawnsSpawned)
            {
                if (t.Position == this.Position) inCell.Add(t);
            }

            return inCell;

        }

        private void DoDamage(Thing target, float damage)
        {
            Pawn pawn = target as Pawn;
            if (pawn != null)
            {
                float factor = 1;

                if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid) factor = 3;
                else if (pawn.RaceProps.IsMechanoid) factor = 0;

                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
                Find.BattleLog.Add(battleLogEntry_DamageTaken);
                DamageInfo dinfo = new DamageInfo(DamageDefOf_AE.Toxic_AE, (float)damage*factor, 1f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                target.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);

                Apparel apparel;
                if (pawn.apparel != null && pawn.apparel.WornApparel.TryRandomElement(out apparel))
                {
                    apparel.TakeDamage(new DamageInfo(DamageDefOf_AE.Toxic_AE, (float)damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    return;
                }
            }
            else
            {
                target.TakeDamage(new DamageInfo(DamageDefOf_AE.Toxic_AE, (float)damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
            }
        }



    }
}

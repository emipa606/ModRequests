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
    public class Gas_Corrosive_AE : Gas
    {
        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                List<Thing> targets = this.ThingsInCloud();
                foreach (Thing t in targets)
                {
                    this.DoDamage(t, 1f);
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

                if (pawn.RaceProps.IsMechanoid) factor = 2;

                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf_AE.DamageEvent_Corrosive_AE, null);
                Find.BattleLog.Add(battleLogEntry_DamageTaken);
                DamageInfo dinfo = new DamageInfo(DamageDefOf_AE.Corrosive_AE, (float)damage*factor, 1f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true);
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                target.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);

                Apparel apparel;
                if (pawn.apparel != null && pawn.apparel.WornApparel.TryRandomElement(out apparel))
                {
                    apparel.TakeDamage(new DamageInfo(DamageDefOf_AE.Corrosive_AE, (float)damage*5, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
                    return;
                }
            }
            else
            {
                target.TakeDamage(new DamageInfo(DamageDefOf_AE.Corrosive_AE, (float)damage, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
            }
        }



    }
}

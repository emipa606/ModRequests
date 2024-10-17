using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Metro
{
    public class Hediff_SunLightDamage : HediffWithComps
    {
        public override bool ShouldRemove => false;
        public override void Tick()
        {
            base.Tick();
            var options = this.def.GetModExtension<SunLightDamage>();
            if (pawn.Map != null && Find.TickManager.TicksGame % options.tickInterval == 0)
            {
                if (!this.pawn.Position.Roofed(this.pawn.Map) && !MetroUtils.IsNightNow(pawn.Map))
                {
                    this.severityInt += options.hediffSeverity;
                }
                else if (this.severityInt > 0 && (this.pawn.Position.Roofed(this.pawn.Map) || MetroUtils.IsNightNow(pawn.Map)))
                {
                    this.severityInt -= options.hediffSeverity;
                }
            }
            if (pawn.Map != null && Find.TickManager.TicksGame % 600 == 0)
            {
                if (this.CurStage.lifeThreatening && !this.pawn.Position.Roofed(this.pawn.Map) && !MetroUtils.IsNightNow(pawn.Map))
                {
                    if (this.pawn.IsSpider())
                    {
                        this.pawn.Kill(new DamageInfo(DamageDefOf.Flame, 1000f));
                    }
                    else if (this.pawn.IsNosalis() && !this.pawn.Downed)
                    {
                        HealthUtility.DamageUntilDowned(this.pawn);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}

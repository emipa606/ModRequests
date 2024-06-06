using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace WarcasketPersona
{
	//just a variation on VEF's damage workers; I should write a fully configurable one at some point
	//like, XML fields stating damageDef, damage, number of instances, armorPen, basic conditions (on flesh type, def whitelist/blacklist)
    public class DamageWorker_ExtraCutHumanlikes : DamageWorker_Cut
    {
        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            if (pawn.RaceProps.Humanlike)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 30, 1f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            }

        }
    }
}
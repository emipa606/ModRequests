using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class DamageWorker_ExtraBluntInsectoids : DamageWorker_AddInjury
    {
        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 30, 1f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
				pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 30, 1f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            }

        }
    }
}
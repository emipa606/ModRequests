using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AustralianAnimals
{
    class Echidna : Pawn
    {
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            Pawn p = dinfo.Instigator as Pawn;
            if (p != null && dinfo.WeaponBodyPartGroup != null)
            {
                DamageInfo spike = new DamageInfo(DamageDefOf.Stab, 1);
                BodyPartRecord bp = (from x in p.health.hediffSet.GetNotMissingParts()
                                     where x.IsInGroup(dinfo.WeaponBodyPartGroup)
                                     select x).InRandomOrder<BodyPartRecord>().FirstOrDefault<BodyPartRecord>();
                spike.SetForcedHitPart(bp);
                p.TakeDamage(spike);
            }
        }
    }
}

using System;
using Verse;

namespace Necro
{
    
    public class HediffComp_ConditionalHediff : HediffComp
    {
        
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dInfo)
        {
            base.Pawn.health.AddHediff(NecroDefOf.Invisibility, null, null, null);
        }

        
        public HediffComp_ConditionalHediff()
        {
        }
    }
}

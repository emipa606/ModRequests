using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class CompTargetEffect_Berserk : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn targetPawn = target as Pawn;
            if (targetPawn == null)
            {
                return;
            }
            //proceeds if target is alive
            if (targetPawn.Dead)
            {
                return;
            }
            //if target is part of player's colny, has 8% chance of affecting
            if (targetPawn.Faction != null && targetPawn.Faction.IsPlayer)
            {
                if (Rand.Value > 0.16f)
                {
                    return;
                }
            }
            //gives berserk mental state
            targetPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
        }
    }
}

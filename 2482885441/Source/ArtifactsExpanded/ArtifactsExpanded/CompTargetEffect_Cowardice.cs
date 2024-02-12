using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class CompTargetEffect_Cowardice : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            //proceeds if target is pawn and is not dead
            Pawn targetPawn = (Pawn)target;
            if (targetPawn.Dead)
            {
                return;
            }
            //gives pawn fleeing mental state
            targetPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, true, false, null, false);
            Find.BattleLog.Add(new BattleLogEntry_ItemUsed(user, target, this.parent.def, RulePackDefOf.Event_ItemUsed));
        }
    }
}
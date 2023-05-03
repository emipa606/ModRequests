using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Mashed_BlackPlague
{
    class HediffComp_TuurngaitVirus : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent.Severity >= 0.9f)
            {
                MakeTheChange(parent.pawn);
            }

            base.CompPostTick(ref severityAdjustment);
        }

        public Pawn MakeTheChange(Pawn originalPawn)
        {
            originalPawn.DropAndForbidEverything();
            originalPawn.Strip();

            
            Pawn newPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.BlackPlague_TuurngaitKind, Faction.OfPlayer);
            newPawn.Name = originalPawn.Name;
            newPawn.story.traits = originalPawn.story.traits;
            newPawn.ageTracker.AgeBiologicalTicks = originalPawn.ageTracker.AgeBiologicalTicks;
            if (ModsConfig.IdeologyActive)
            {
                newPawn.ideo.SetIdeo(originalPawn.Ideo);
            }
            //newPawn.gender = originalPawn.gender;
            originalPawn.Kill(null);
            PawnUtility.TrySpawnHatchedOrBornPawn(newPawn, originalPawn.Corpse);
            originalPawn.Corpse.Destroy();

            return newPawn;
        }
    }
}

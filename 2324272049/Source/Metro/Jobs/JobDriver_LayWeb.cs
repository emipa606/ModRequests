using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Metro
{
    public class JobDriver_LayWeb : JobDriver_Mutant
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.Wait(100, TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    var web = (Filth_Web)ThingMaker.MakeThing(MetroDefOf.Metro_Web);
                    GenSpawn.Spawn(web, this.pawn.Position, this.pawn.Map);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}


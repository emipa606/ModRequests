using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;
using UnityEngine;

namespace HarderGearLooting
{
    internal class JobDriver_BiocodeReformat : JobDriver
    {
        private Mote warmupMote;
        private ThingWithComps TargetItem => (ThingWithComps)this.job.GetTarget(TargetIndex.A).Thing;
        private Thing Reformatter => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve((LocalTargetInfo)(Thing)this.TargetItem, this.job, errorOnFailed: errorOnFailed) && this.pawn.Reserve((LocalTargetInfo)this.Reformatter, this.job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_BiocodeReformat jobDriverResurrect = this;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull<Toil>(TargetIndex.B).FailOnDespawnedOrNull<Toil>(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull<Toil>(TargetIndex.A);
            Toil toil = Toils_General.Wait(600, TargetIndex.None);
            toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return toil;
            yield return Toils_General.Do(new Action(this.BiocodeReformat));
            yield break;
        }

        private void BiocodeReformat()
        {
            CompBiocodable biocode;
            if (!TargetItem.TryGetComp<CompBiocodable>(out biocode))
            {
                Log.Error($"Trying to apply biorecode on a non-biocodable Thing.");
                return;
            }
            biocode.UnCode();
            biocode.CodeFor(this.pawn);
        }
    }

}

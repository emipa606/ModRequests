using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse.AI;

namespace Clutter_Furniture
{
    public class JobDriver_LockerBasic : JobDriver
    {

        private const TargetIndex Locker = TargetIndex.A;
        private const TargetIndex CellInd = TargetIndex.B;
        private const TargetIndex WaitDuration = TargetIndex.C;


        protected override IEnumerable<Toil> MakeNewToils()
        {

            ToilFailConditions.FailOnDestroyedOrNull<JobDriver_LockerBasic>(this, TargetIndex.A);
            ToilFailConditions.FailOnDespawnedOrNull<JobDriver_LockerBasic>(this, TargetIndex.A);


            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_WaitWithSoundAndEffect("Recipe_Tailor", "ConstructMetal", TargetIndex.B);

            yield break;
        }

        private Toil Toils_WaitWithSoundAndEffect(string soundDefName, string effecterDefName, TargetIndex targetIndex)
        {
            Toil toil = new Toil();
            int duration = 0;
            if (pawn.apparel.WornApparelCount > 3)
            {
                duration = (pawn.apparel.WornApparelCount * 40);
            }
            else
            {
                duration = 300;
            }
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
                ClothLocker locker = TargetA.Thing as ClothLocker;
                locker.Interaction = true;
                locker.GunChange = true;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = duration;
            
            return toil;
        }



    }
}

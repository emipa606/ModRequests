using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ApparelColorChange
{
    public class RainbowSquieerl_Job : JobDriver
    {
        private const TargetIndex ColorChanger = TargetIndex.A;
        private const TargetIndex CellInd = TargetIndex.B;
        private static string ErrorMessage = "Hairstyling job called on building that is not Cabinet";
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_DialogLauncher();
            yield break;
        }
        private Toil Toils_DialogLauncher()
        {
            return new Toil
            {
                initAction = delegate
                {
                    RainbowSquieerl rainbowSquieerl = TargetA.Thing as RainbowSquieerl;
                    if (rainbowSquieerl != null)
                    {
                        RainbowSquieerl rainbowSquieerl2 = TargetA.Thing as RainbowSquieerl;
                        if (GetActor().Position == TargetA.Thing.InteractionCell)
                        {
                            rainbowSquieerl2.ColorChanger(GetActor());
                        }
                    }
                    else
                    {
                        Log.Error(ErrorMessage.Translate());
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Krelinos_BiocodeIt
{
    class JobDriver_Biocode : JobDriver     // This is basically the resurrect serum job driver by the way.
    {
        private Thing Weapon
        {
            get
            {
                return this.job.GetTarget(WeaponIndex).Thing;
            }
        }

        private Thing Biocoder
        {
            get
            {
                return this.job.GetTarget(BiocoderIndex).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(Weapon, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(Biocoder, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(BiocoderIndex, PathEndMode.Touch).FailOnDespawnedOrNull(BiocoderIndex).FailOnDespawnedOrNull(WeaponIndex);
            yield return Toils_Haul.StartCarryThing(BiocoderIndex, false, false, false);
            yield return Toils_Goto.GotoThing(WeaponIndex, PathEndMode.Touch).FailOnDespawnedOrNull(WeaponIndex);
            Toil useWaitToil = Toils_General.Wait(DurationTicks, TargetIndex.None);
            useWaitToil.WithProgressBarToilDelay(WeaponIndex, false, -0.5f);
            useWaitToil.FailOnDespawnedOrNull(WeaponIndex);
            useWaitToil.FailOnCannotTouch(WeaponIndex, PathEndMode.Touch);
            yield return useWaitToil;
            yield return Toils_General.Do(new Action(this.Biocode));
            yield break;
        }

        private void Biocode()
        {
            CompBiocodable targetCompBiocode = Weapon.TryGetComp<CompBiocodable>();
            if (targetCompBiocode == null)
            {
                targetCompBiocode = new CompBiocodable();
            }

            Messages.Message( String.Format( "BiocodedToolApplied".Translate(), Weapon.LabelShort, this.pawn.LabelShort ), Weapon, MessageTypeDefOf.PositiveEvent, true );
            targetCompBiocode.CodeFor(this.pawn);
            SoundDefOf.TechMedicineUsed.PlayOneShot(SoundInfo.InMap(Weapon, MaintenanceType.None));

            Biocoder.SplitOff(1).Destroy(DestroyMode.Vanish);
        }

        private const TargetIndex WeaponIndex = TargetIndex.A;
        private const TargetIndex BiocoderIndex = TargetIndex.B;
        private const int DurationTicks = 600;
    }
}

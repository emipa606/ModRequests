using Verse;
using CombatExtended;
using RimWorld;
using PipeSystem;
using System.Runtime.Remoting.Messaging;
using Verse.AI;

namespace BDsPlasmaWeapon
{
    public class Verb_ShootFromBackpackTank : Verb_ShootCE
    {

        public CompTankFeedWeapon compTankFeedWeapon => EquipmentSource.TryGetComp<CompTankFeedWeapon>();

        public CompReloadableFromFiller compTank
        {
            get
            {
                CompReloadableFromFiller comp = EquipmentSource.TryGetComp<CompReloadableFromFiller>();
                if (comp != null && (compTankFeedWeapon != null && comp.remainingCharges >= ammoConsumption))
                {
                    return comp;
                }
                else if (compTankFeedWeapon != null)
                {
                    return compTankFeedWeapon.compReloadableFromFiller;
                }
                return null;
            }
        }

        private int ammoConsumption => (verbProps as VerbPropertiesCE).ammoConsumedPerShotCount;

        public void DoOutOfAmmoAction()
        {
            CompInventory CompInventory = ShooterPawn.TryGetComp<CompInventory>();
            if (CompInventory != null && (ShooterPawn.CurJob == null || ShooterPawn.CurJob.def != RimWorld.JobDefOf.Hunt) && CompInventory.TryFindViableWeapon(out var weapon, !ShooterPawn.IsColonist))
            {
                ShooterPawn.jobs.StartJob(JobMaker.MakeJob(CE_JobDefOf.EquipFromInventory, weapon), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
                return;
            }
            CompInventory?.SwitchToNextViableWeapon(!ShooterPawn.def.weaponTags.Contains("NoSwitch"), !ShooterPawn.IsColonist, stopJob: false);
        }
        public override bool Available()
        {
            if (base.Available())
            {
                if (compTank == null)
                {
                    return false;
                }
                if (compTank.remainingCharges < ammoConsumption)
                {
                    compTankFeedWeapon?.searchTank(ammoConsumption, false);
                    return false;
                }
                int storedGas = compTank.remainingCharges;
                return storedGas >= ammoConsumption;
            }
            return false;
        }

        public override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (compTank == null || compTank.remainingCharges < ammoConsumption)
                {
                    compTankFeedWeapon?.searchTank();
                    return false;
                }
                compTank.DrawGas(ammoConsumption);
                return true;
            }
            return false;
        }
    }
}

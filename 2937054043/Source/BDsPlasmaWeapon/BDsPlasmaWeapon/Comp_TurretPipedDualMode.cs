using Verse;
using CombatExtended;
using RimWorld;
using PipeSystem;
using BillDoorsFramework;
using static UnityEngine.GraphicsBuffer;

namespace BDsPlasmaWeapon
{
    public class CompTurretPipedDualMode : CompResource
    {
        ThingWithComps gun; CompSecondaryAmmo compSecondaryAmmoUser;
        Building_TurretGunCE turret;
        CompBreakdownable compBreakdownable;
        CompAmmoUser compAmmoUser;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent is Building_TurretGunCE)
            {
                turret = parent as Building_TurretGunCE;
                gun = turret.Gun as ThingWithComps;
                compSecondaryAmmoUser = gun.TryGetComp<CompSecondaryAmmo>();
                compBreakdownable = turret.TryGetComp<CompBreakdownable>();
            }
        }

        private bool IsAvailableForRecharge => (turret != null) && (turret.powerComp == null || turret.powerComp.PowerOn) && (compBreakdownable == null || !compBreakdownable.BrokenDown) && (compSecondaryAmmoUser != null && compSecondaryAmmoUser.IsSecondaryAmmoSelected) && PipeNet.Stored > 0;

        public override void CompTick()
        {
            base.CompTick();
            if (IsAvailableForRecharge)
            {
                int ammoDifference = compSecondaryAmmoUser.SecondaryAmmoSetData.magazineSize - compSecondaryAmmoUser.CompAmmo.CurMagCount;
                if (ammoDifference > 0)
                {
                    int stored = (int)PipeNet.CurrentStored();
                    if (stored >= ammoDifference)
                    {
                        PipeNet.DrawAmongStorage(ammoDifference, PipeNet.storages);
                        compSecondaryAmmoUser.CompAmmo.CurMagCount += ammoDifference;
                    }
                    else if (stored > 0)
                    {
                        compSecondaryAmmoUser.CompAmmo.CurMagCount += stored;
                        PipeNet.DrawAmongStorage(stored, PipeNet.storages);
                    }
                }
            }
        }
    }
}

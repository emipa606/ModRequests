using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsFramework
{
    class CompTurretWithSecondaryAmmo : ThingComp
    {
        public CompSecondaryAmmo SecondaryAmmo
        {
            get
            {
                if (secondaryAmmo == null)
                {
                    secondaryAmmo = turretGun.TryGetComp<CompSecondaryAmmo>();
                }
                return secondaryAmmo;
            }
        }
        private CompSecondaryAmmo secondaryAmmo;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Building_TurretGunCE turret = this.parent as Building_TurretGunCE;
            if (turret == null)
            {
                return;
            }
            this.turretGun = turret.Gun;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (turretGun == null)
            {
                yield break;
            }

            if (SecondaryAmmo == null)
            {
                yield break;
            }

            foreach (Gizmo gizmo in secondaryAmmo.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield break;
        }

        private Thing turretGun;
    }

    class CompProperties_TurretWithSecondaryAmmo : CompProperties
    {
        public CompProperties_TurretWithSecondaryAmmo()
        {
            this.compClass = typeof(CompTurretWithSecondaryAmmo);
        }
    }
}

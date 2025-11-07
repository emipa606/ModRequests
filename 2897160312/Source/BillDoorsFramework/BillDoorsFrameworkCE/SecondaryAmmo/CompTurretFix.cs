using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsFramework
{
    public class CompTurretFix : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            turretGunInt = (parent as Building_TurretGunCE)?.Gun as ThingWithComps;
        }

        public ThingWithComps TurretGun
        {
            get
            {
                if (turretGunInt == null)
                {
                    turretGunInt = (parent as Building_TurretGunCE)?.Gun as ThingWithComps;
                }
                return turretGunInt;
            }
        }

        private ThingWithComps turretGunInt;
        private bool initialized = false;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (turretGunInt == null) yield break;
            foreach (var comp in TurretGun.AllComps)
            {
                foreach (var gizmo in comp.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }

        public override void CompTick()
        {
            if (!initialized)
            {
                initialized = true;
                TurretGun.TryGetComp<CompSecondaryAmmo>()?.InitData();
                var compAmmo = TurretGun.TryGetComp<CompAmmoUser>();
                if (compAmmo != null)
                {
                    compAmmo.turret = parent as Building_Turret; ;
                }

                TurretGun.TryGetComp<CompSecondaryVerbCE>()?.InitData();
                var compVerb = TurretGun.TryGetComp<CompAmmoUser>();
                if (compVerb != null)
                {
                    compVerb.turret = parent as Building_Turret; ;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using CombatExtended.AI;
using Verse;

namespace BDsPlasmaWeapon
{
    public class CompAILizionWeaponDualMode
    {
        public int Priority => 1;

        public bool StartCastChecks(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
        {
            return false;
            CompTankFeedWeapon gun = verb.EquipmentSource.TryGetComp<CompTankFeedWeapon>();

            if (gun == null && !gun.isOn)
            {
                return false;
            }
            if (gun.compReloadableFromFiller.remainingCharges >= (verb.verbProps as VerbPropertiesCE).ammoConsumedPerShotCount || gun.searchTank((verb.verbProps as VerbPropertiesCE).ammoConsumedPerShotCount, false))
            {
                return true;
            }
            return false;
        }
    }
}

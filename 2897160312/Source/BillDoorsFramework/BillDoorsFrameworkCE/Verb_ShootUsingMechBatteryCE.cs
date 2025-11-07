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
    public class Verb_ShootUsingMechBatteryCE : Verb_ShootCE
    {
        public DefModExtension_ShootUsingMechBattery Data
        {
            get
            {
                return EquipmentCompSource.parent.def.GetModExtension<DefModExtension_ShootUsingMechBattery>();
            }
        }

        public override bool Available()
        {
            if (ModLister.BiotechInstalled && Data != null)
            {
                Need_MechEnergy battery = CasterPawn.needs.TryGetNeed<Need_MechEnergy>();
                if (battery != null)
                {
                    return battery.CurLevel > Data.energyConsumption;
                }
            }
            return base.Available();
        }

        public override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (ModLister.BiotechInstalled && Data != null)
                {
                    Need_MechEnergy battery = CasterPawn.needs.TryGetNeed<Need_MechEnergy>();
                    if (battery != null)
                    {
                        battery.CurLevel -= Data.energyConsumption;
                    }
                }
                return true;
            }
            return false;
        }
    }

    public class Verb_ShootUsingMechBatteryNotUnderRoofCE : Verb_ShootUsingMechBatteryCE
    {
        ModExtension_VerbNotUnderRoof extension => EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();

        CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();
        public override bool Available()
        {
            if (Caster.Position.Roofed(Caster.Map)
                && (compSecondaryVerb == null || extension == null || (compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInPrimaryMode)))
            {
                return false;
            }
            return base.Available();
        }
    }
}

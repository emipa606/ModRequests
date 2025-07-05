using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimWorld
{
    public class JobGiver_GetEnergy_ChargerGreenIncluded_GAOTCE_Mech : JobGiver_GetEnergy_Charger//rename the classd to include all chargers, not just stellar
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = base.TryGiveJob(pawn);

            if (job == null && ShouldAutoRecharge(pawn))
            {
                var charger = GetFreeChargerGreen(pawn);
                
                if (charger != null)
                    return JobMaker.MakeJob(JobDefOf.MechCharge, charger);


            }

            return job;
        }

        private Building_MechCharger GetFreeChargerGreen(Pawn mech)
        {
            var chargersBasicStellar = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.BasicRechargerStellar_GAOTCE_Mech);

            foreach (var charger in chargersBasicStellar)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }

            var chargersBasicCosmic = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.BasicRechargerCosmic_GAOTCE_Mech);

            foreach (var charger in chargersBasicCosmic)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }

            var chargersBasicEternium = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.BasicRechargerEternium_GAOTCE_Mech);

            foreach (var charger in chargersBasicStellar)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }

            var chargersBasicStable = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.BasicRechargerStable_GAOTCE_Mech);

            foreach (var charger in chargersBasicCosmic)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }






            var chargersStandardStellar = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.StandardRechargerStellar_GAOTCE_Mech);

            foreach (var charger in chargersStandardStellar)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }

            var chargersStandardCosmic = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.StandardRechargerCosmic_GAOTCE_Mech);

            foreach (var charger in chargersStandardStellar)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }

            var chargersStandardEternium = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.StandardRechargerEternium_GAOTCE_Mech);

            foreach (var charger in chargersStandardStellar)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }

            var chargersStandardStable = mech.Map.listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.StandardRechargerStable_GAOTCE_Mech);

            foreach (var charger in chargersStandardStellar)
            {
                if ((charger as Building_MechCharger).CanPawnChargeCurrently(mech))
                    return charger as Building_MechCharger;
            }


            return null;
        }
    }
}

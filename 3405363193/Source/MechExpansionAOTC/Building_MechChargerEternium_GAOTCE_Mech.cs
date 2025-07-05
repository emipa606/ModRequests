using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorld
{
    public class Building_MechChargerEternium_GAOTCE_Mech : Building_MechCharger
    {
        static AccessTools.FieldRef<Building_MechCharger, Pawn> _currentlyChargingMech = AccessTools.FieldRefAccess<Pawn>(typeof(Building_MechCharger), "currentlyChargingMech");
        private float baseChargePerTick = 0.00083333335f;
        private float chargeMultiplier = 8;
        public override void Tick()
        {
            base.Tick();

            var chargingMech = _currentlyChargingMech(this);
            /*
            Pawn chargingMech = typeof(Building_MechChargerStellar_GAOTCE_Mech).GetField("currentlyChargingMech", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) as Pawn;
            
            */

            if (chargingMech != null)
                chargingMech.needs.energy.CurLevel += baseChargePerTick * (chargeMultiplier - 1);
        }
    }
}

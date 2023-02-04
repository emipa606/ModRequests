using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LifeSupport {
    /// <summary>
    /// Tags this Thing as being valid life support.
    /// </summary>
    public class LifeSupportComp : ThingComp {
        public bool Active => !(parent.TryGetComp<CompPowerTrader>() is CompPowerTrader power) || power.PowerOn;

        public override void ReceiveCompSignal(string signal) {
            if (signal != "PowerTurnedOn" && signal != "PowerTurnedOff") {
                return;
            }
            
            //Check for state change in surrounding pawns in beds.
            Map map = parent.Map;
            var pawns = new List<Pawn>();
            foreach (IntVec3 cell in parent.CellsAdjacent8WayAndInside()) {
                foreach (Thing thing in cell.GetThingList(map)) {
                    if (thing is Building_Bed bed) {
                        for (int i = 0, l = bed.SleepingSlotsCount; i < l; i++) {
                            var pawn = bed.GetSleepingSlotPos(i).GetFirstPawn(map);
                            if (!(pawn is null)) {
                                pawns.Add(pawn);
                            }
                        }
                    }
                }
            }
            foreach (var pawn in pawns) {
                if (!pawn.health.Dead) {
                    pawn.SetHediffs(validLifeSupportNearby: false);
                    pawn.health.CheckForStateChange(null, null);
                }
            }
        }
    }
}

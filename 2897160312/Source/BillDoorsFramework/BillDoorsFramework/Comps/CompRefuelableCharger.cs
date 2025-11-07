using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class CompRefuelableCharger : ThingComp
    {
        public CompProperties_RefuelableCharger Props => props as CompProperties_RefuelableCharger;

        public CompPowerTrader powerTrader => parent.TryGetComp<CompPowerTrader>();

        public bool isActive;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isActive, "isActive");
        }

        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(Props.intervals) && (powerTrader == null || powerTrader.PowerOn))
            {
                bool active = false;
                if (Props.rechargeAdjacent)
                {
                    foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(parent))
                    {
                        ThingWithComps thing = cell.GetFirstThingWithComp<CompRefuelableChargerTarget>(parent.Map);
                        if (tryRefuel(thing)) { active = true; }
                    }
                }
                else
                {
                    ThingWithComps thing = parent.Position.GetFirstThingWithComp<CompRefuelableChargerTarget>(parent.Map);
                    if (tryRefuel(thing)) { active = true; }
                }
                isActive = active;

                if (powerTrader != null)
                {
                    powerTrader.PowerOutput = 0 - (isActive ? powerTrader.Props.PowerConsumption : powerTrader.Props.idlePowerDraw);
                }
            }
        }

        bool tryRefuel(ThingWithComps thing)
        {
            return thing != null && thing.GetComp<CompRefuelableChargerTarget>().tryRefuel(Props.refillAmount);
        }
    }

    public class CompProperties_RefuelableCharger : CompProperties
    {
        public CompProperties_RefuelableCharger()
        {
            compClass = typeof(CompRefuelableCharger);
        }
        public bool rechargeAdjacent = false;
        public int intervals = 1800;
        public float refillAmount = 1;
    }

    public class CompRefuelableChargerTarget : ThingComp
    {
        public CompRefuelable compRefuelable => parent.TryGetComp<CompRefuelable>();

        public bool tryRefuel(float amount)
        {
            if (compRefuelable != null && !compRefuelable.IsFull)
            {
                compRefuelable.Refuel(amount);
                return true;
            }
            return false;
        }
    }
}

using System;
using Verse;

namespace BillDoorsUnifiedHaulJob
{
    public abstract class CompRefuelableInterfaced : ThingComp, IRefuelable
    {
        public abstract ThingRequest CurrentRequest { get; }

        public virtual int RequestAmount
        {
            get
            {
                return (int)(FuelCapacity - currentFuel);
            }
        }

        public virtual bool WantsReload => true;

        //要是有别的奇奇怪怪的容量计数可以不使用这两个
        public float currentFuel = 0;
        public virtual float FuelCapacity => 100;

        public virtual bool ShouldRefuelNow => WantsReload && RequestAmount > 0;

        public virtual Predicate<Thing> FuelValidator => (Thing fuel) => true;

        public virtual float SearchRadius => 100;

        public virtual int RefuelWaitTick => 0;

        private Thing foundFuel = null;
        public virtual Thing FoundFuel { get => foundFuel; set => foundFuel = value; }

        public virtual Thing Parent => parent;

        public virtual void RefuelEffect(Thing fuel, Pawn pawn, params object[] parms)
        {
            int actualCnt = CalcuFuelConsumeAmt(fuel);
            if (actualCnt == 0) return;

            currentFuel += actualCnt;
            if (fuel.stackCount <= actualCnt) fuel.Destroy();
            else fuel.SplitOff(actualCnt).Destroy();

        }

        //实际消耗的燃料的数量 要是有别的奇奇怪怪的容量计数可以不使用这个
        public virtual int CalcuFuelConsumeAmt(Thing fuel)
        {
            return (int)Math.Min(fuel.stackCount, FuelCapacity - currentFuel);
        }

        public abstract string GetUniqueLoadID();
    }

    public class CompRefuelableInterfaced_Weapon : CompRefuelableInterfaced
    {
        public override ThingRequest CurrentRequest => throw new NotImplementedException();

        public override Thing Parent => parent.SpawnedParentOrMe;

        public override string GetUniqueLoadID()
        {
            throw new NotImplementedException();
        }
    }
}

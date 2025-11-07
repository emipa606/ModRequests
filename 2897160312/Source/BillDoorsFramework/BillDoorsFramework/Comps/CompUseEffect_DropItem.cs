using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class CompUseEffect_DropItem : CompUseEffect
    {
        public new CompProperties_DropThing Props
        {
            get { return (CompProperties_DropThing)this.props; }
        }

        public override void DoEffect(Pawn usedBy)
        {
            foreach (ThingDefCountClass thingDefCountClass in Props.things)
            {
                Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef, null);
                thing.stackCount = thingDefCountClass.count;
                GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near, out _, null, null, default);
            }
            if (Props.selfDestroy)
            {
                parent.DeSpawn();
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Open")
            {
                DoEffect(null);
            }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            return base.CanBeUsedBy(p);
        }
    }

    public class CompProperties_DropThing : CompProperties_UseEffect
    {
        public CompProperties_DropThing()
        {
            this.compClass = typeof(CompUseEffect_DropItem);
        }

        public List<ThingDefCountClass> things;
        public bool selfDestroy = true;
    }
}

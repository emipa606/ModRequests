using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using Verse;
using PipeSystem;

namespace BDsPlasmaWeapon
{
    public class CompProximityLizionPopper : CompResource
    {
        public CompProperties_ProximityLizionPopper Props => (CompProperties_ProximityLizionPopper)props;
        private float storedLizion;
        CompBreakdownable CompBreakdownable;
        CompLizionPopperExplosion CompExplosive;

        private bool isAvailableForRecharge => ((CompBreakdownable == null) ? true : !CompBreakdownable.BrokenDown) && (capacity > storedLizion);
        private float capacity => Props.lizionNeeded;
        public bool isAvailableForPop => ((CompBreakdownable == null) ? true : !CompBreakdownable.BrokenDown) && (storedLizion >= capacity);

        private bool isBrokenLastTick;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storedLizion, "storedLizion", 0);
        }
        public override void CompTick()
        {
            if (isBrokenLastTick && !CompBreakdownable.BrokenDown)
            {
                parent.DirtyMapMesh(parent.Map);
            }
            isBrokenLastTick = CompBreakdownable.BrokenDown;
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                CompTickRare();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CompBreakdownable = parent.GetComp<CompBreakdownable>();
            CompExplosive = parent.GetComp<CompLizionPopperExplosion>();
        }

        public override void CompTickRare()
        {
            if (isAvailableForRecharge)
            {
                DrawGas();
            }

            if (isAvailableForPop && (GenClosest.ClosestThingReachable(parent.Position, parent.Map, ThingRequest.ForDef(Props.target), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.radius) != null))
            {
                CompExplosive.DoDetonation();
            }
        }

        public void trigger()
        {
            if (isAvailableForPop)
            {
                CompExplosive?.DoDetonation();
            }
        }

        public override string CompInspectStringExtra()
        {
            string inspectStringExtra = "BDP_LizionPopperStatus".Translate() + ": ";
            if (isAvailableForPop)
            {
                inspectStringExtra += "BDP_PopperReady".Translate();
                return inspectStringExtra;
            }
            if (isAvailableForRecharge)
            {
                inspectStringExtra += storedLizion + "/" + capacity;
                return inspectStringExtra;
            }
            inspectStringExtra += "BDP_PopperBroken".Translate();
            return inspectStringExtra;
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            if ((CompBreakdownable == null) || !CompBreakdownable.BrokenDown)
            {
                GraphicData graphicData = Props.popperGraphicData;
                graphicData.Graphic.Print(layer, parent, 0);
            }
        }

        private void DrawGas()
        {
            float gasNeeded = capacity - storedLizion;
            if (PipeNet.Stored < gasNeeded)
            {
                gasNeeded = PipeNet.Stored;
            }
            storedLizion += gasNeeded;
            PipeNet.DrawAmongStorage(gasNeeded, PipeNet.storages);
        }

        public void Pop()
        {
            if (CompBreakdownable != null)
            {
                CompBreakdownable.DoBreakdown();
            }
            storedLizion = 0;
            Effecter effecter = BDStatDefOf.LizionCoolerHigh.Spawn(parent.Position, parent.Map);
            effecter.Trigger(parent, TargetInfo.Invalid);
        }
    }

    public class CompProperties_ProximityLizionPopper : CompProperties_Resource
    {
        public ThingDef target;

        public float radius;

        public float lizionNeeded = 20;
        public GraphicData popperGraphicData;

        public CompProperties_ProximityLizionPopper()
        {
            compClass = typeof(CompProximityLizionPopper);
        }
    }
}

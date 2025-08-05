using System.Collections.Generic;
using System.Linq;
using System.Text;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace BDsPlasmaWeapon
{
    public class CompLizionCellFiller : CompResourceTrader
    {
        public CompProperties_LizionCellFiller Props
        {
            get
            {
                return (CompProperties_LizionCellFiller)props;
            }
        }
        private static readonly Texture2D emptyIcon = ContentFinder<Texture2D>.Get("UI/EmptyImage");

        private List<IntVec3> adjCells;

        private Command_Action chooseOuputGizmo;

        private int resultIndex;

        private int nextProcessTick;

        private CompRefuelable compRefuelable;

        private CompLizionCellBuffer compThingContainer;

        public FillerResult ChoosedResult => Props.results[resultIndex];

        public override void PostPostMake()
        {
            base.PostPostMake();
            nextProcessTick = Find.TickManager.TicksGame + ChoosedResult.eachTicks;
            resultIndex = 0;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = parent.TryGetComp<CompRefuelable>();
            compThingContainer = parent.TryGetComp<CompLizionCellBuffer>();
            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();
            if (Props.results.Count > 1)
            {
                chooseOuputGizmo = new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        for (int i = 0; i < Props.results.Count; i++)
                        {
                            FillerResult res = Props.results[i];
                            string result = (res.thing.label);
                            string gas = Props.pipeNet.resource.name;
                            int num = (res.countNeeded);
                            list.Add(new FloatMenuOption("BDP_Produce".Translate(result, gas, num), delegate
                            {
                                resultIndex = Props.results.IndexOf(res);
                                SetupForChoice();
                                nextProcessTick = Find.TickManager.TicksGame + ChoosedResult.eachTicks;
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    },
                    defaultLabel = "BDP_ChooseResult".Translate(),
                    defaultDesc = "BDP_ChooseResultDesc".Translate(),
                    icon = ((ChoosedResult.thing != null) ? ChoosedResult.thing.uiIcon : emptyIcon)
                };
            }
            SetupForChoice();
        }

        private void SetupForChoice()
        {
            if (chooseOuputGizmo != null)
            {
                chooseOuputGizmo.icon = ((ChoosedResult.thing != null) ? ChoosedResult.thing.uiIcon : emptyIcon);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextProcessTick, "nextProcessTick", 0);
            Scribe_Values.Look(ref resultIndex, "resultIndex", 0);
        }

        public override void CompTick()
        {
            base.CompTick();
            PipeNet pipeNet = PipeNet;
            int ticksGame = Find.TickManager.TicksGame;
            if (ticksGame >= nextProcessTick)
            {
                CompResourceStorage compResourceStorage = parent.TryGetComp<CompResourceStorage>();
                if ((pipeNet.CurrentStored() >= (float)ChoosedResult.countNeeded || parent.TryGetComp<CompResourceStorage>()?.AmountStored >= (float)ChoosedResult.countNeeded) && CanBeOn() && compRefuelable.Fuel >= ChoosedResult.thingCount)
                {
                    DropProduct();
                }
                nextProcessTick = ticksGame + ChoosedResult.eachTicks;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerable<Gizmo> gizmos = base.CompGetGizmosExtra();
            for (int i = 0; i < gizmos.Count(); i++)
            {
                yield return gizmos.ElementAt(i);
            }
            if (chooseOuputGizmo != null)
            {
                yield return chooseOuputGizmo;
            }
        }

        public bool IsAvaliable()
        {
            CompFlickable compFlickable = parent.TryGetComp<CompFlickable>();
            CompSchedule compSchedule = parent.TryGetComp<CompSchedule>();
            CompBreakdownable compBreakdownable = parent.TryGetComp<CompBreakdownable>();
            CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
            return (compFlickable == null || compFlickable.SwitchIsOn) && (compSchedule == null || compSchedule.Allowed) && (compBreakdownable == null || !compBreakdownable.BrokenDown) && (compPowerTrader == null || compPowerTrader.PowerOn);
        }

        private void DropProduct()
        {
            Thing firstThing = ThingMaker.MakeThing(ChoosedResult.thing);
            firstThing.stackCount = ChoosedResult.thingCount;
            if (compThingContainer != null && !compThingContainer.Full)
            {
                compThingContainer.innerContainer.TryAdd(firstThing);
                compThingContainer.NotifyOperational();
            }
            else
            {
                for (int i = 0; i < adjCells.Count; i++)
                {
                    IntVec3 intVec = adjCells[i];
                    Map map = parent.Map;
                    firstThing = intVec.GetFirstThing(map, ChoosedResult.thing);
                    if (!intVec.Walkable(map))
                    {
                        continue;
                    }

                    if (firstThing != null)
                    {
                        if (firstThing.stackCount + ChoosedResult.thingCount <= firstThing.def.stackLimit)
                        {
                            firstThing.stackCount += ChoosedResult.thingCount;
                            break;
                        }
                        continue;
                    }
                    firstThing = ThingMaker.MakeThing(ChoosedResult.thing);
                    firstThing.stackCount = ChoosedResult.thingCount;
                    if (GenPlace.TryPlaceThing(firstThing, intVec, map, ThingPlaceMode.Near))
                    {
                        break;
                    }
                }
            }
            CompResourceStorage compResourceStorage = parent.TryGetComp<CompResourceStorage>();
            if (compResourceStorage != null && compResourceStorage.AmountCanAccept > ChoosedResult.countNeeded)
            {
                compResourceStorage.DrawResource(ChoosedResult.countNeeded);
                compRefuelable.ConsumeFuel(ChoosedResult.thingCount);
            }
            else
            {
                PipeNet pipeNet = PipeNet;
                pipeNet.DrawAmongStorage(ChoosedResult.countNeeded, pipeNet.storages);
                compRefuelable.ConsumeFuel(ChoosedResult.thingCount);
            }
        }
    }

    public class CompProperties_LizionCellFiller : CompProperties_ResourceTrader
    {
        public bool showBufferInfo = true;

        public string notWorkingKey;

        public List<FillerResult> results;

        public CompProperties_LizionCellFiller()
        {
            compClass = typeof(CompLizionCellFiller);
        }
    }

    public class FillerResult
    {
        public int countNeeded;
        public int eachTicks;
        public ThingDef thing;
        public int thingCount;
    }
}

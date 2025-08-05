using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using PipeSystem;
using UnityEngine;

namespace BDsPlasmaWeaponVanilla
{
    public class CompAirIntake : ThingComp
    {
        public CompProperties_AirIntake Props
        {
            get
            {
                return (CompProperties_AirIntake)props;
            }
        }
        private CompResource otherComp;

        private int nextProduceTick;

        private bool noCapacity = false;

        private CompPowerTrader powerComp;

        private List<IntVec3> windPathCells = new List<IntVec3>();

        private List<Thing> windPathBlockedByThings = new List<Thing>();

        private List<IntVec3> windPathBlockedCells = new List<IntVec3>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();

            IEnumerable<CompResource> comps = parent.GetComps<CompResource>();
            for (int i = 0; i < comps.Count(); i++)
            {
                CompResource compResource = comps.ElementAt(i);
                if (compResource.Props.pipeNet == Props.pipeNet)
                {
                    otherComp = compResource;
                    break;
                }
            }
            if (parent.def.rotatable)
            {
                windPathCells = AirIntakeUtility.CalculateAvaliableCells(parent.Position, parent.Rotation, Props.radius, parent.def.size).ToList();
            }
            else
            {
                windPathCells = AirIntakeUtility.CalculateAvaliableCells(parent.Position, Props.radius, parent.def.size).ToList();
            }
        }


        public override void PostExposeData()
        {
            Scribe_Values.Look(ref nextProduceTick, "nextProduceTick", 0);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            GenDraw.DrawFieldEdges(windPathCells);
            GenDraw.DrawFieldEdges(windPathBlockedCells, new Color(1, 0, 0));
        }

        public override void CompTick()
        {
            base.CompTick();
            int ticksGame = Find.TickManager.TicksGame;
            if (nextProduceTick == -1)
            {
                nextProduceTick = ticksGame + Props.productionTicks;
            }
            else if (ticksGame >= nextProduceTick)
            {
                TryProducePortion();
                nextProduceTick = ticksGame + Props.productionTicks;
            }
        }

        private float effeciency
        {
            get
            {
                GetBlockedCells(windPathCells);
                return 1 - ((float)windPathBlockedCells.Count() / (float)(windPathCells.Count() + (parent.def.size.x * parent.def.size.z)));
            }
        }


        public override string CompInspectStringExtra()
        {
            if (parent.Spawned)
            {
                return ("BDP_AirIntakeEfficiency".Translate() + (Math.Round(effeciency, 3) * 100) + "%");
            }
            return null;
        }

        private void TryProducePortion()
        {
            if (powerComp != null && !powerComp.PowerOn)
            {
                return;
            }
            Map map = parent.Map;
            PipeNet pipeNet = otherComp.PipeNet;
            if (pipeNet.connectors.Count > 1)
            {
                noCapacity = pipeNet.AvailableCapacity <= 0f;
                if (!noCapacity)
                {
                    pipeNet.DistributeAmongStorage(Props.airPerProduceDefault * effeciency);
                }
                return;
            }
        }

        public void GetBlockedCells(IEnumerable<IntVec3> cells)
        {
            windPathBlockedByThings.Clear();
            windPathBlockedCells.Clear();
            foreach (IntVec3 cell in cells)
            {
                List<Thing> list = parent.Map.thingGrid.ThingsListAt(cell);
                for (int j = 0; j < list.Count; j++)
                {
                    Thing thing = list[j];
                    if (thing.def.blockWind)
                    {
                        windPathBlockedByThings.Add(thing);
                        windPathBlockedCells.Add(cell);
                        break;
                    }
                }
                if (parent.Map.roofGrid.Roofed(cell))
                {
                    windPathBlockedByThings.Add(null);
                    windPathBlockedCells.Add(cell);
                    continue;
                }
            }
        }
    }

    public class CompProperties_AirIntake : CompProperties
    {
        public int productionTicks = 60;

        public int airPerProduceDefault = 60;

        public PipeNetDef pipeNet;

        public int radius = 2;

        public CompProperties_AirIntake()
        {
            compClass = typeof(CompAirIntake);
        }
    }
}

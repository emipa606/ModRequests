using System;
using System.Collections.Generic;
using RimWorld;
using Trash_Collector_SoS2;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimworld
{
    // this class created only for salvage bay (too switch textures). Things/Building/Ship/SalvageBay_off.png used to display active stage of wreck deconstruction
    public class Graphic_ShipSalvageBay : Graphic_SingleOnOff
    {
        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            if (CompWreckCollect.Enable && thing.TryGetComp<CompWreckCollect>().PowerOn)
                return matOff;
            return mat;
        }
    }

    public class CompWreckCollect : CompPowerTrader
    {
        public static Area_WreckDisassemble areaWreck = null;

        private static bool enable = false;
        public static bool Enable { get => enable;
            set
            {
                enable = value;
                if (value)
                {
                    thingDisassembleProgress = 0;
                    areaCellsEnumerator = areaWreck.ActiveCells.GetEnumerator();//enumerator update
                }
            }
        }

        public static IEnumerator<IntVec3> areaCellsEnumerator = null;

        private static float thingDisassembleProgress = 0;

        // My best builder (18 skill points) can deconstruct 1x1 ship corner in 5 seconds. 
        // One salavage bay can do it in 10 seconds (and collect all resouress of course). 
        // 4 salavage bays can do it in 2,50 seconds. Hope this is not OP...
        public override void CompTick()
        {
            base.CompTick();
            if (PowerOutput < Props.basePowerConsumption - 1 && PowerOn && !enable) PowerOutput = Props.basePowerConsumption;
            if (!enable || !PowerOn || Find.TickManager.TicksGame % 5 > 0) return; //break tick condition //
            if (PowerOutput > -SoS2WreckDeconstructionAddon.activePowerConsumption + 1) PowerOutput = -SoS2WreckDeconstructionAddon.activePowerConsumption; //if deconstructing
            var lst = areaCellsEnumerator.Current.GetThingList(parent.Map).FindAll(x => (x.def.building != null) && x.def.building.shipPart);
            while (lst.Count < 1)//skip all empty cells
            {
                areaWreck[areaCellsEnumerator.Current] = false;
                if (areaCellsEnumerator.MoveNext())
                    lst = areaCellsEnumerator.Current.GetThingList(parent.Map).FindAll(x => (x.def.building != null) && x.def.building.shipPart);
                else
                {
                    if (areaWreck.TrueCount < 1) Enable = false;
                    else areaCellsEnumerator = areaWreck.ActiveCells.GetEnumerator();
                    return;
                }
            }
            if (lst.Count > 0 && !lst[0].Destroyed) //thing found
            {
                //Log.Message("cord:(" + area_cells_enumerator.Current.x + "|" + area_cells_enumerator.Current.y + "|" + area_cells_enumerator.Current.z + ") name:" + lst[0].def.defName + " dis_proc:" + ThingDisassembleProgress + " max:" + lst[0].def.building.uninstallWork * 5);
                if (thingDisassembleProgress > lst[0].def.building.uninstallWork) //thing must be disasembled
                {
                    lst[0].Destroy();
                    SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(areaCellsEnumerator.Current, parent.Map));
                    foreach (var l in lst[0].def.CostList)
                    {
                        int count = l.count;
                        while (count > 0)//to spawn correctly
                        {
                            var th = ThingMaker.MakeThing(l.thingDef);
                            if (count - th.def.stackLimit > 0)
                            {
                                count -= th.def.stackLimit;
                                th.stackCount = th.def.stackLimit;
                                GenSpawn.Spawn(th, parent.Position, parent.Map, WipeMode.VanishOrMoveAside);
                            }
                            else
                            {
                                th.stackCount = count;
                                GenSpawn.Spawn(th, parent.Position, parent.Map, WipeMode.VanishOrMoveAside);
                                count = 0;
                            }
                        }
                    }
                    if (lst.Count < 2) //only last thing was in list (the destroed one)
                    {
                        areaWreck[areaCellsEnumerator.Current] = false;
                        if (!areaCellsEnumerator.MoveNext()) 
                            if (areaWreck.TrueCount < 1) Enable = false;
                            else areaCellsEnumerator = areaWreck.ActiveCells.GetEnumerator();
                    }
                    thingDisassembleProgress = 0;
                }
                thingDisassembleProgress += SoS2WreckDeconstructionAddon.deconstructSpeed.Value;
            }
        }

        public override string CompInspectStringExtra() => base.CompInspectStringExtra() + "\nWreck deconstruction " + (enable && PowerOn ? "in progress..." : "stopped.");

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent.Map.Parent.def.defName.Equals("ShipOrbiting"))
            {
                areaWreck = new Area_WreckDisassemble(parent.Map.areaManager);
                areaCellsEnumerator = areaWreck.ActiveCells.GetEnumerator();
            }
        }
    }

    public class CompProperties_WreckCollect : CompProperties_Power
    {
        public CompProperties_WreckCollect()
        {
            this.compClass = typeof(CompWreckCollect);
        }
    }

    public class Area_WreckDisassemble : Area
    {
        public Area_WreckDisassemble(AreaManager areaManager) : base(areaManager){}

        public override string Label => "Wreck Disassemble Area";

        public override Color Color => new Color(1f, 0f, 0f, 0.4f);

        public override int ListPriority => 3000;

        public override string GetUniqueLoadID()
        {
            return this.Map.GetUniqueLoadID() + " " + Label;
        }
    }

    public class Designator_AreaWreckDisassemble : Designator_Area
    {
        protected bool mode;

        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        public Designator_AreaWreckDisassemble()
        {
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            mode = true;
            defaultLabel = "Expand deconstruction zone";
            defaultDesc = "Control salvage bays deconstruction area.";
            icon = ContentFinder<Texture2D>.Get("UI/Setup_wreck_area");
            this.useMouseIcon = true;
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
                foreach (var c in cells) CompWreckCollect.areaWreck[c] = mode;
                (mode ? SoundDefOf.Designate_ZoneAdd : SoundDefOf.Designate_ZoneDelete).PlayOneShotOnCamera();
                CompWreckCollect.Enable = true;
        }

        public override void ProcessInput(Event ev)
        {
            if (Current.Game.CurrentMap.Parent.def.defName.Equals("ShipOrbiting"))
            {
                if (CompWreckCollect.areaWreck == null)
                    CompWreckCollect.areaWreck = new Area_WreckDisassemble(Current.Game.CurrentMap.areaManager);
                base.ProcessInput(ev);
            }
            else Messages.Message("You can set deconstruction area only on your orbital ship map.", MessageTypeDefOf.RejectInput, false);
        }

        public override void SelectedUpdate()
        {
            CompWreckCollect.areaWreck.MarkForDraw();
            CompWreckCollect.areaWreck.AreaUpdate();
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => true;
    }

    public class Designator_AreaWreckDisassembleClear : Designator_AreaWreckDisassemble
    {
        public Designator_AreaWreckDisassembleClear()
        {
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            mode = false;
            defaultLabel = "Shrink deconstruction zone";
            defaultDesc = "Control salvage bays deconstruction area.";
            icon = ContentFinder<Texture2D>.Get("UI/Remove_wreck_area");
            this.useMouseIcon = true;
        }
    }
}

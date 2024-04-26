using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Diagnostics;
using UnityEngine;

namespace WallStuff
{
    public class WallMountedCompRepair : ThingComp
    {
        IntVec3 vecFacing;
        private Boolean spawned = false;
        private int ticksSinceMacroTick = 0;
        private const int MacroTickInterval = 2500;
        private float protectionRadius = 6f;
        private Boolean roomMode = true;
        private const String errorString = "Error ! Room not sealed, system not operating";

        WallMountedCompProperties_Repair WallMountedPropsRepair
        {
            get
            {                
                return (WallMountedCompProperties_Repair)this.props;
            }
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        private Room RoomFacing()
        {
            return vecFacing.GetRoom(this.parent.Map);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            spawned = true;
            vecFacing = parent.Position + IntVec3.North.RotatedBy(parent.Rotation);
        }

        public override void CompTick()
        {
        }

        public override void CompTickRare()
        {
            SetStatus();

            this.ticksSinceMacroTick = this.ticksSinceMacroTick + 250;

            if (ticksSinceMacroTick >= MacroTickInterval)
            {
                this.MacroTick();
                this.ticksSinceMacroTick = 0;
            }            
        }

        private void SetStatus()
        {
            if (RoomFacing().UsesOutdoorTemperature)
            {
                this.parent.TryGetComp<CompPowerTrader>().PowerOutput = 0f;
            }
            else
            {
                SetPowerUsage();
            }
        }

        private void MacroTick()
        {
            if (RoomFacing().UsesOutdoorTemperature) return;

            List<Thing> things = null;

            if (roomMode)
            {
                things = GetAllThingsInRoom();
            }
            else
            {
                things = GetAllThingsInRadius();
            }

            foreach (Thing thing in things)
            {
                // If it's a weapon or a piece of apparel just repair it at the defined rate
                if (thing.def.IsWithinCategory(ThingCategoryDefOf.Weapons) || thing.def.IsWithinCategory(ThingCategoryDefOf.Apparel))
                {
                    RepairThing(thing);
                }

                //If it's a Pawn get all their equipment and apparel
                if (thing is Pawn pawn)
                {
                    RepairPawnEquipment(pawn);
                    RepairPawnApparel(pawn);
                }
            }
        }

        private List<Thing> GetAllThingsInRoom()
        {
            List<Thing> result = new List<Thing>();

            foreach (IntVec3 cell in RoomFacing().Cells)
            {
                result.AddRange(this.parent.Map.thingGrid.ThingsListAt(cell));
            }      

            return result;
        }

        private List<Thing> GetAllThingsInRadius()
        {
            IEnumerable<IntVec3> repairCells = GenRadial.RadialCellsAround(this.parent.Position, this.protectionRadius, true);

            List<Thing> result = new List<Thing>();

            foreach (IntVec3 cell in repairCells)
            {
                result.AddRange(this.parent.Map.thingGrid.ThingsListAt(cell));
            }

            return result;
        }

        private void RepairPawnApparel(Pawn pawn)
        {
            if(pawn.apparel != null && pawn.apparel.WornApparel != null)
            {
                List<Apparel> pawnApparel = pawn.apparel.WornApparel;
                //Repair all apparel
                foreach (Apparel apparel in pawnApparel)
                {
                    RepairThing(apparel);
                }
            }
        }

        private void RepairPawnEquipment(Pawn pawn)
        {
            if(pawn.equipment != null)
            {
                List<ThingWithComps> pawnEquipment = pawn.equipment.AllEquipmentListForReading;

                // Repair all equipment
                foreach (ThingWithComps equipment in pawnEquipment)
                {
                    RepairThing(equipment);
                }
            }
            
        }

        private void RepairThing(Thing thing)
        {
            if(thing.HitPoints < thing.MaxHitPoints)
            {
                int repairRateForThing = Math.Max(1, (int)Math.Round(thing.MaxHitPoints / 100f * this.GetRepairRate()));// Use Math.max to ensure we're always adding at least 1 HP
                thing.HitPoints = Math.Min(thing.MaxHitPoints, thing.HitPoints + repairRateForThing);
            }            
        }

        private double GetRepairRate()
        {
            return 100/WallStuffSettings.repairRateHours;
        }/*

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.ChangePowerMode();
                act.defaultLabel = "Change Power Mode";
                act.defaultDesc = "On";
                act.activateSound = SoundDef.Named("Click");
                act.icon = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);
                yield return act;
            }
        }

        private void ChangePowerMode()
        {
            this.overpowerMode = !this.overpowerMode;
            SetPowerUsage();
        }*/

        private void SetPowerUsage()
        {
            this.parent.TryGetComp<CompPowerTrader>().PowerOutput = -GetPowerUsage();
        }

        public override void PostExposeData()
        {
            string str = (!this.WallMountedPropsRepair.saveKeysPrefix.NullOrEmpty()) ? (this.WallMountedPropsRepair.saveKeysPrefix + "_") : null;
            /*Scribe_Values.Look<Boolean>(ref this.overpowerMode, str + "overpowerMode", false, false);*/
            Scribe_Values.Look<int>(ref this.ticksSinceMacroTick, str + "ticksSinceMacroTick", 0, false);
        }

        public override string CompInspectStringExtra()
        {
            if (this.parent.ParentHolder is MinifiedThing) return "";

            if (RoomFacing().UsesOutdoorTemperature)
            {
                return errorString;
            }
            if (this.WallMountedPropsRepair.requiresPower || this.PowerOn)
            {
                return "Power usage is: " + this.GetPowerUsage() + " W";
            }
            return null;
        }

        private int GetPowerUsage()
        {
            return RoomFacing().CellCount * WallStuffSettings.repairPowerUsage; ;
        }
    }
}
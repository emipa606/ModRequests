using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Minecart
{
    // Stored mass stat
    public class StatPart_StoredMass : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            Building_Minecart minecart = req.Thing as Building_Minecart;
            if (minecart != null)
            {
                CompTransporter compTransporter = minecart.GetComp<CompTransporter>();
                if (compTransporter != null)
                {
                    return "Mass of loaded contents: {0}kg".Formatted(
                        compTransporter.innerContainer.Sum((t) => t.GetStatValue(StatDefOf.Mass) * t.stackCount)
                        .ToStringWithSign());
                }
            }
            return "";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Building_Minecart minecart = req.Thing as Building_Minecart;
            if (minecart != null)
            {
                CompTransporter compTransporter = minecart.GetComp<CompTransporter>();
                if (compTransporter != null)
                {
                    val += compTransporter.innerContainer.Sum((t) => t.GetStatValue(StatDefOf.Mass) * t.stackCount);
                }
            }
        }
    }

    public class Building_RailSwitch : Building
    {
        public override string GetInspectString()
        {
            CompFlickable flickable = GetComp<CompFlickable>();
            StringBuilder sb = new StringBuilder();
            sb.Append("Turn: ");
            sb.Append(flickable.SwitchIsOn ? "Right" : "Left");
            if (flickable.WantsFlick())
            {
                sb.Append(" (Flicking to ");
                sb.Append(flickable.SwitchIsOn ? "Left" : "Right");
                sb.Append("...)");
            }
            return sb.ToString();
        }
    }

    public class Building_RailDump : Building
    {
        public bool Mode { get; set; } = true;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Command_Toggle mode = new Command_Toggle();
            mode.icon = TexCommand.Install;
            mode.defaultLabel = Mode ? "Dump Mode" : "Load Mode";
            mode.isActive = () => Mode;
            mode.toggleAction = delegate()
            {
                Mode = !Mode;
            };
            yield return mode;
        }

        public override void Tick()
        {
            Building_Minecart minecart = Position.GetFirstThing<Building_Minecart>(Map);
            if (minecart != null)
            {
                CompTransporter compTransporter = minecart.GetComp<CompTransporter>();
                if (compTransporter != null)
                {
                    if (Mode)
                    {
                        compTransporter.innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
                        compTransporter.CancelLoad();
                    }
                    else
                    {
                        foreach (IntVec3 cell in this.CellsAdjacent8WayAndInside())
                        {
                            Thing thing = cell.GetFirstItem(Map);
                            if (thing != null
                                && thing.IsInValidStorage()
                                && compTransporter.innerContainer.Sum((t) => t.GetStatValue(StatDefOf.Mass) * t.stackCount)
                                + (thing.GetStatValue(StatDefOf.Mass) * thing.stackCount) < compTransporter.Props.massCapacity)
                                compTransporter.innerContainer.TryAdd(thing.SplitOff(thing.stackCount));
                        }
                    }
                }
                CompRefuelable compRefuelable = minecart.GetComp<CompRefuelable>();
                if (compRefuelable != null && !Mode && compRefuelable.TargetFuelLevel - compRefuelable.Fuel > 1f)
                {
                    foreach (IntVec3 cell in this.CellsAdjacent8WayAndInside())
                    {
                        Thing thing = cell.GetFirstItem(Map);
                        if (thing != null
                            && thing.IsInValidStorage())
                            compRefuelable.Refuel(new List<Thing> { thing });
                    }
                }
            }
        }
    }

    public class ThingDef_RailBoost : ThingDef
    {
        public float pushSpeed;
        public float pushAccel;
        public float pushPower;
    }

    public class Building_RailBoost : Building
    {
        public ThingDef_RailBoost Def { get { return def as ThingDef_RailBoost; } }

        public bool isActive()
        {
            CompFlickable flickable = GetComp<CompFlickable>();
            CompPowerTrader power = GetComp<CompPowerTrader>();
            return (flickable == null ? true : flickable.SwitchIsOn) && (power == null ? true : power.PowerOn);
        }

        public override void Tick()
        {
            Building_Minecart minecart = Position.GetFirstThing<Building_Minecart>(Map);
            if (minecart != null)
            {
                if (isActive())
                {
                    minecart.Push(Def.pushSpeed, Def.pushAccel, Def.pushPower);
                }
                else
                {
                    minecart.Brake(Def.pushAccel, Def.pushPower);
                }
            }
        }
    }

    public class PlaceWorker_TrackUtility : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef bcheckingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            if (loc.Impassable(map)) return new AcceptanceReport("Blocked");
            if (loc.GetFirstThing(map, ThingDefOf.ThingRail) == null) return new AcceptanceReport("No Rail");
            else return AcceptanceReport.WasAccepted;
        }
    }
}

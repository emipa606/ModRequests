using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Minecart
{
    public class CompProperties_MinecartPropulsion : CompProperties
    {
        public float pushAccel; // How fast the minecart of 1kg will speed up per tick
        public float pushPower; // Effective mass is divided by this
        public float pushSpeed; // Maximum theoretical speed under it's own power

        public CompProperties_MinecartPropulsion()
        {
            compClass = typeof(CompMinecartPropulsion);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
                yield return s;

            yield return new StatDrawEntry(StatCategoryDefOf.Building, "Acceleration", pushAccel.ToString("0.00"), string.Empty, 0);
            yield return new StatDrawEntry(StatCategoryDefOf.Building, "Power", pushPower.ToString("0.00"), string.Empty, 0);
            yield return new StatDrawEntry(StatCategoryDefOf.Building, "Speed", pushSpeed.ToString("0.00'c'"), string.Empty, 0);
        }
    }

    public class CompMinecartPropulsion : ThingComp
    {

        public bool Throttle { get; set; } = false;

        public override void Initialize(CompProperties props)
        {
            this.props = props;
        }

        public CompProperties_MinecartPropulsion Props
        {
            get { return props as CompProperties_MinecartPropulsion; }
        }

        public Building_Minecart Parent
        {
            get { return parent as Building_Minecart; }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Toggle throttleToggle = new Command_Toggle();

            throttleToggle.toggleAction = () => Throttle = !Throttle;
            throttleToggle.isActive = () => Throttle;
            throttleToggle.defaultLabel = "Throttle";
            throttleToggle.defaultDesc = "Toggle minecart throttle";
            throttleToggle.icon = CommandTex;

            yield return throttleToggle;
        }

        private Texture2D CommandTex
        {
            get
            {
                if (cachedCommandTex == null)
                    cachedCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true);
                return cachedCommandTex;
            }
        }

        private Texture2D cachedCommandTex;

        public override void CompTick()
        {
            CompRefuelable refuelable = Parent.GetComp<CompRefuelable>();
            CompBreakdownable breakdownable = Parent.GetComp<CompBreakdownable>();
            CompTransporter transporter = Parent.GetComp<CompTransporter>();
            CompFlickable flickable = Parent.GetComp<CompFlickable>();

            if (Throttle)
            {
                if (
                    (refuelable == null ? true : refuelable.Fuel > 0f) &&
                    (breakdownable == null ? true : !breakdownable.BrokenDown) &&
                    (flickable == null ? true : flickable.SwitchIsOn)
                )
                {
                    FleckMaker.ThrowAirPuffUp(Parent.DrawPos + new Vector3(0.0f, 0.5f), Parent.Map);
                    if (refuelable != null)
                        refuelable.Notify_UsedThisTick();
                    Parent.Push(Props.pushSpeed, Props.pushAccel, Props.pushPower);
                }
            }
            else
            {
                Parent.Brake(Props.pushAccel, Props.pushPower);
            }

            if (transporter != null 
                && refuelable != null 
                && parent.IsHashIntervalTick(GenTicks.TickRareInterval) 
                && refuelable.Props.fuelCapacity - refuelable.Fuel > 1f 
                && transporter.innerContainer.Any((t) => refuelable.Props.fuelFilter.Allows(t)))
            {
                refuelable.Refuel(
                    new List<Thing> {
                        transporter.innerContainer.First(
                            (t) => refuelable.Props.fuelFilter.Allows(t))
                    });
            }
        }
    }
}

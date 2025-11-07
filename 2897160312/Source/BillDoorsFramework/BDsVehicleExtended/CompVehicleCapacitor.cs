using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompVehicleCapacitor : VehicleComp
    {
        public new CompProperties_VehicleCapacitor Props => (CompProperties_VehicleCapacitor)props;

        public float Power => power;

        float power = 0;

        float capacity => Vehicle.statHandler.GetStatValue(VFStatDefOf.BDsVehiclePowerCapacitor);

        float powerGen => Vehicle.statHandler.GetStatValue(VFStatDefOf.BDsVehiclePowerRecharge);

        float fuelUsage => 1 / Vehicle.statHandler.GetStatValue(VFStatDefOf.BDsVehicleGeneratorFuelUseage);

        public bool ReactorOn;

        List<VehicleComponent> components = new List<VehicleComponent>();

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref power, "power");
            Scribe_Values.Look(ref ReactorOn, "ReactorOn");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            if (Vehicle.Drafted) { ReactorOn = true; }
            if (power < capacity && ReactorOn)
            {
                if (Vehicle.CompFueledTravel != null)
                {
                    if (Vehicle.CompFueledTravel.Fuel > 0)
                    {
                        if (power + powerGen > capacity)
                        {
                            Vehicle.CompFueledTravel.ConsumeFuel((capacity - power) * fuelUsage);
                            TryCharge(capacity - power);
                        }
                        else
                        {
                            Vehicle.CompFueledTravel.ConsumeFuel(powerGen * fuelUsage);
                            TryCharge(powerGen);
                        }
                        if (Vehicle.CompFueledTravel.Fuel <= 0)
                        {
                            ReactorOn = false;
                            Messages.Message("BDFV_ReactorOutOfFuel".Translate(parent.Label), parent, MessageTypeDefOf.NegativeEvent);
                        }
                    }
                }
                else
                {
                    TryCharge(powerGen);
                }
            }
        }

        public void TryCharge(float power)
        {
            if (power <= 0)
            {
                return;
            }
            this.power += power;
            if (this.power > capacity)
            {
                this.power = capacity;
            }
        }

        public bool TryDrain(float power)
        {
            if (power <= 0 || power > this.power)
            {
                return false;
            }
            this.power -= power;
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Gizmo_VehicleCapacitor gizmo = new Gizmo_VehicleCapacitor();
            gizmo.capacitor = this;
            yield return gizmo;
            Command_Action command_Action2 = new Command_Action();
            command_Action2.defaultLabel = (ReactorOn ? "BDFV_ReactorOn" : "BDFV_ReactorOff").Translate();
            command_Action2.defaultDesc = "BDFV_ReactorDesc".Translate(powerGen * 60, powerGen * fuelUsage * 60);
            command_Action2.icon = ContentFinder<Texture2D>.Get(ReactorOn ? Props.reactorOnGizmoIcon : Props.reactorOffGizmoIcon, reportFailure: false); ;
            command_Action2.action = delegate
            {
                ReactorOn = !ReactorOn;
            };
            command_Action2.activateSound = ReactorOn ? SoundDefOf.Designate_Cancel : SoundDefOf.Mouseover_ButtonToggle;
            yield return command_Action2;
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: full capacitor";
                command_Action.action = delegate
                {
                    power = capacity;
                };
                yield return command_Action;
            }
        }
    }

    public class CompProperties_VehicleCapacitor : CompProperties
    {
        public CompProperties_VehicleCapacitor()
        {
            this.compClass = typeof(CompVehicleCapacitor);
        }

        public float movingPowerDrain = 0;
        [NoTranslate]
        public string reactorOnGizmoIcon = "";
        [NoTranslate]
        public string reactorOffGizmoIcon = "";
    }

    [StaticConstructorOnStartup]
    public class Gizmo_VehicleCapacitor : Gizmo
    {
        public CompVehicleCapacitor capacitor;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_VehicleCapacitor()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, "BDFV_Capacitor".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = capacitor.Power / Mathf.Max(1f, capacitor.Vehicle.statHandler.GetStatValue(VFStatDefOf.BDsVehiclePowerCapacitor));
            Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect4, capacitor.Power.ToString("F2") + " / " + capacitor.Vehicle.statHandler.GetStatValue(VFStatDefOf.BDsVehiclePowerCapacitor).ToString("F2"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect2, "BDFV_CapacitorTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}

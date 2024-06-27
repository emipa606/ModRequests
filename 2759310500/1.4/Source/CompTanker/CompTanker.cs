using CompTanker.Compat;
using Multiplayer.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace CompTanker
{
    [HotSwappable]
    public class CompTanker : ThingComp
    {
        public bool drawOverlay = false;

        // Gizmos
        private Command_Action gizmoDebugFill;
        private Command_Action gizmoDebugEmpty;
        private Command_Toggle gizmoToggleDrain;
        private Command_Toggle gizmoToggleFill;

        // Exposed fields
        public double storedAmount = 0;
        public bool isDraining = false;
        public bool isFilling = false;
        public int contaminationLevel = 1;

        public double CapPercent => (storedAmount / Props.storageCap);
        public CompProperties_Tanker Props => (CompProperties_Tanker)props;
        private Command_Action GizmoDebugFill => gizmoDebugFill ??= new Command_Action
        {
            action = DebugFill,
            defaultLabel = "Dev: Fill",
        };
        private Command_Action GizmoDebugEmpty => gizmoDebugEmpty ??= new Command_Action
        {
            action = DebugEmpty,
            defaultLabel = "Dev: Empty",
        };
        private Command_Toggle GizmoToggleDrain => gizmoToggleDrain ??= new Command_Toggle
        {
            isActive = () => isDraining,
            toggleAction = ToggleDrain,
            defaultLabel = "RimefellerTankerToggleDrain".Translate(),
            defaultDesc = "RimefellerTankerToggleDrainDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("RimefellerTanker/UI/Drain"),
        };
        private Command_Toggle GizmoToggleFill => gizmoToggleFill ??= new Command_Toggle
        {
            isActive = () => isFilling,
            toggleAction = ToggleFill,
            defaultLabel = "RimefellerTankerToggleFill".Translate(),
            defaultDesc = "RimefellerTankerToggleFillDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("RimefellerTanker/UI/Fill"),
        };

        [SyncMethod]
        internal void ToggleFill()
        {
            isDraining = false;
            isFilling = !isFilling;
        }

        [SyncMethod]
        internal void ToggleDrain()
        {
            isFilling = false;
            isDraining = !isDraining;
        }

        [SyncMethod(debugOnly = true)]
        internal void DebugFill() => storedAmount = Props.storageCap;

        [SyncMethod(debugOnly = true)]
        internal void DebugEmpty() => storedAmount = 0;

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            switch (Props.contents)
            {
                case TankType.Fuel:
                case TankType.Oil:
                    RimefellerCompat.MarkForDrawing(parent.Map);
                    break;
                case TankType.Water:
                    BadHygieneCompat.MarkForDrawing(parent.Map);
                    break;
                case TankType.Helixien:
                    break;
                case TankType.Invalid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(Props.contents), Props.contents, "Invalid tanker contents");
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            switch (Props.contents)
            {
                case TankType.Fuel:
                case TankType.Oil:
                    RimefellerCompat.HandleTick(this);
                    break;
                case TankType.Water:
                    BadHygieneCompat.HandleTick(this);
                    break;
                case TankType.Helixien:
                    VanillaHelixienGasExpanded.HandleTick(this);
                    break;
                case TankType.Invalid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(Props.contents), Props.contents, "Invalid tanker contents");
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            yield return GizmoToggleDrain;
            yield return GizmoToggleFill;

            if (Prefs.DevMode)
            {
                yield return GizmoDebugEmpty;
                yield return GizmoDebugFill;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref storedAmount, nameof(storedAmount), 0);
            Scribe_Values.Look(ref isDraining, nameof(isDraining), false);
            Scribe_Values.Look(ref isFilling, nameof(isFilling), false);
            Scribe_Values.Look(ref contaminationLevel, nameof(contaminationLevel), 1);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!drawOverlay) return;

            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = ModResources.BarSize;
            r.fillPercent = (float)CapPercent;
            r.filledMat = ModResources.BarFilledMat;
            r.unfilledMat = ModResources.BarUnfilledMat;
            r.margin = 0.15f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);

            drawOverlay = false;
        }

        public override string CompInspectStringExtra()
        {
            if (!parent.Spawned) return string.Empty;

            var stringBuilder = new StringBuilder();

            var text = (Props.contents switch
            {
                TankType.Fuel => "TankerFuelStorage",
                TankType.Oil => "TankerOilStorage",
                TankType.Water => "TankerWaterStorage",
                TankType.Helixien => "TankerHelixienStorage",
                TankType.Invalid or _ => throw new ArgumentOutOfRangeException(nameof(Props.contents), Props.contents, "Invalid tanker contents"),
            }).Translate(storedAmount.ToString("0.0"), Props.storageCap);

            stringBuilder.AppendLine(text);
            if (isFilling)
                stringBuilder.AppendLine("RimefellerTankerFillingInspect".Translate());
            else if (isDraining)
                stringBuilder.AppendLine("RimefellerTankerDrainingInspect".Translate());

            if (Props.contents == TankType.Water)
            {
                switch (contaminationLevel)
                {
                    case 0:
                        stringBuilder.AppendLine("TreatedWater".Translate());
                        break;
                    case 1:
                        stringBuilder.AppendLine("UntreatedWater".Translate());
                        break;
                    case 2:
                        stringBuilder.AppendLine("ContaminatedWater".Translate());
                        break;
                }
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}

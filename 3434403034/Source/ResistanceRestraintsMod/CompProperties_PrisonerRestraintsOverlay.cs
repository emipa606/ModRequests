using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace ResistanceRestraintsMod
{
    public class CompProperties_PrisonerRestraintsOverlay : CompProperties
    {
        public List<OverlayData> overlays;
        public bool inheritParentColor = false;
        public bool onlyShowOverlayWhileInUse = false; // Overlay only appears when in use
        public bool dynamicColorChange = false; // New bool for enabling dynamic RGB color changes
        public int colorChangeInterval = 60; // Ticks per color change (1 second in RimWorld time)

        public CompProperties_PrisonerRestraintsOverlay()
        {
            compClass = typeof(CompPrisonerRestraintsOverlay);
        }
    }

    public class OverlayData
    {
        public string direction;
        public string texPath;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public float zOffset = 0f;
    }

    public class CompPrisonerRestraintsOverlay : ThingComp
    {
        private CompProperties_PrisonerRestraintsOverlay Props => (CompProperties_PrisonerRestraintsOverlay)props;
        private Dictionary<Rot4, (Graphic graphic, Vector3 size, float zOffset)> graphics = new Dictionary<Rot4, (Graphic, Vector3, float)>();

        private int ticksSinceLastColorChange = 0; // Tick counter
        private Color currentOverlayColor = Color.white; // Stores the current color

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (Props.overlays == null || Props.overlays.Count == 0)
                return;

            // Ensure graphic loading is done on the main thread
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                foreach (var overlay in Props.overlays)
                {
                    if (overlay.texPath.NullOrEmpty())
                        continue;

                    Rot4 rot = GetRotationFromString(overlay.direction);
                    graphics[rot] = new ValueTuple<Graphic, Vector3, float>(
                        GraphicDatabase.Get<Graphic_Single>(overlay.texPath, ShaderDatabase.Transparent),
                        overlay.size,
                        overlay.zOffset
                    );
                }
            });
        }


        public override void CompTick()
        {
            base.CompTick();

            if (Props.dynamicColorChange)
            {
                ticksSinceLastColorChange++;

                if (ticksSinceLastColorChange >= Props.colorChangeInterval)
                {
                    currentOverlayColor = GetDynamicRGBColor();
                    ticksSinceLastColorChange = 0;
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (parent.Destroyed || !graphics.ContainsKey(parent.Rotation)) return;

            // Check if overlay should only appear while in use
            if (Props.onlyShowOverlayWhileInUse && !IsBedInUse()) return;

            var overlayData = graphics[parent.Rotation];
            Vector3 drawLoc = parent.DrawPos;
            drawLoc.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            drawLoc.z += overlayData.Item3;

            Color overlayColor = Props.inheritParentColor ? GetParentColor() :
                (Props.dynamicColorChange ? currentOverlayColor : Color.white);

            Material overlayMaterial = MaterialPool.MatFrom(overlayData.Item1.path, ShaderDatabase.Transparent, overlayColor);

            Graphics.DrawMesh(
                MeshPool.plane10,
                Matrix4x4.TRS(drawLoc, Quaternion.identity, overlayData.Item2),
                overlayMaterial,
                0
            );
        }

        private bool IsBedInUse()
        {
            if (parent is Building_Bed bed)
            {
                return bed.CurOccupants.Any();
            }
            return false;
        }

        private Rot4 GetRotationFromString(string direction)
        {
            switch (direction.ToLower())
            {
                case "north": return Rot4.North;
                case "south": return Rot4.South;
                case "east": return Rot4.East;
                case "west": return Rot4.West;
                default: return Rot4.North;
            }
        }

        private Color GetParentColor()
        {
            return parent is ThingWithComps thingWithComps ? thingWithComps.DrawColor : Color.white;
        }

        private Color GetDynamicRGBColor()
        {
            float time = (float)Find.TickManager.TicksGame / 60f; // Convert ticks to seconds
            float r = (Mathf.Sin(time * 1.5f) + 1f) / 2f; // Sine wave for smooth transitions
            float g = (Mathf.Sin(time * 2f) + 1f) / 2f;
            float b = (Mathf.Sin(time * 2.5f) + 1f) / 2f;

            return new Color(r, g, b);
        }
    }
}
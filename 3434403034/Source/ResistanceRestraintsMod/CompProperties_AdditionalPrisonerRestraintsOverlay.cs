using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace ResistanceRestraintsMod
{
    public class CompProperties_AdditionalPrisonerRestraintsOverlay : CompProperties
    {
        public List<OverlayLayerData> overlayLayers;
        public bool adoptBaseColor = false;
        public bool showOnlyWhenEngaged = false;
        public bool enableColorCycling = false;
        public int colorCycleInterval = 60;

        public CompProperties_AdditionalPrisonerRestraintsOverlay()
        {
            compClass = typeof(CompAdditionalPrisonerRestraintsOverlay);
        }
    }

    public class OverlayLayerData
    {
        public string facingDirection;
        public string texturePath;
        public Vector3 dimensions = new Vector3(1f, 1f, 1f);
        public float depthOffset = 0f;
    }

    public class CompAdditionalPrisonerRestraintsOverlay : ThingComp
    {
        private CompProperties_AdditionalPrisonerRestraintsOverlay Props => (CompProperties_AdditionalPrisonerRestraintsOverlay)props;
        private Dictionary<Rot4, (Graphic graphic, Vector3 dimensions, float depthOffset)> overlayGraphics = new Dictionary<Rot4, (Graphic, Vector3, float)>();

        private int ticksSinceLastCycle = 0;
        private Color activeOverlayColor = Color.white;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (Props.overlayLayers == null || Props.overlayLayers.Count == 0)
                return;

            // Defer the graphic loading to the main thread
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                foreach (var overlay in Props.overlayLayers)
                {
                    if (overlay.texturePath.NullOrEmpty())
                        continue;

                    Rot4 rot = ConvertStringToRotation(overlay.facingDirection);
                    overlayGraphics[rot] = new ValueTuple<Graphic, Vector3, float>(
                        GraphicDatabase.Get<Graphic_Single>(overlay.texturePath, ShaderDatabase.Transparent),
                        overlay.dimensions,
                        overlay.depthOffset
                    );
                }
            });
        }


        public override void CompTick()
        {
            base.CompTick();

            if (Props.enableColorCycling)
            {
                ticksSinceLastCycle++;

                if (ticksSinceLastCycle >= Props.colorCycleInterval)
                {
                    activeOverlayColor = GenerateColorShift();
                    ticksSinceLastCycle = 0;
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (parent.Destroyed || !overlayGraphics.ContainsKey(parent.Rotation)) return;

            if (Props.showOnlyWhenEngaged && !IsOccupiedBed()) return;

            var overlayData = overlayGraphics[parent.Rotation];
            Vector3 drawPos = parent.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor() + 0.1f;
            drawPos.z += overlayData.Item3;

            Color finalOverlayColor = Props.adoptBaseColor ? RetrieveBaseColor() :
                (Props.enableColorCycling ? activeOverlayColor : Color.white);

            Material overlayMaterial = MaterialPool.MatFrom(overlayData.Item1.path, ShaderDatabase.Transparent, finalOverlayColor);

            Graphics.DrawMesh(
                MeshPool.plane10,
                Matrix4x4.TRS(drawPos, Quaternion.identity, overlayData.Item2),
                overlayMaterial,
                0
            );
        }

        private bool IsOccupiedBed()
        {
            if (parent is Building_Bed bed)
            {
                return bed.CurOccupants.Any();
            }
            return false;
        }

        private Rot4 ConvertStringToRotation(string direction)
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

        private Color RetrieveBaseColor()
        {
            return parent is ThingWithComps thingWithComps ? thingWithComps.DrawColor : Color.white;
        }

        private Color GenerateColorShift()
        {
            float time = (float)Find.TickManager.TicksGame / 60f;
            float r = (Mathf.Sin(time * 1.5f) + 1f) / 2f;
            float g = (Mathf.Sin(time * 2f) + 1f) / 2f;
            float b = (Mathf.Sin(time * 2.5f) + 1f) / 2f;

            return new Color(r, g, b);
        }
    }
}
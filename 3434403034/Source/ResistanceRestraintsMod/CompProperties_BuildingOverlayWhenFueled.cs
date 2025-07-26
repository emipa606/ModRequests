using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace ResistanceRestraintsMod
{
    public class CompProperties_BuildingOverlayWhenFueled : CompProperties
    {
        public List<OverlayData> overlays;
        public bool inheritParentColor = false;

        public CompProperties_BuildingOverlayWhenFueled()
        {
            compClass = typeof(CompBuildingOverlayWhenFueled);
        }
    }

    public class CompBuildingOverlayWhenFueled : ThingComp
    {
        private CompProperties_BuildingOverlayWhenFueled Props => (CompProperties_BuildingOverlayWhenFueled)props;
        private Dictionary<Rot4, Tuple<Graphic, Vector3, float>> graphics = new Dictionary<Rot4, Tuple<Graphic, Vector3, float>>();

        private CompRefuelable compRefuelable;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // Fetch refuelable component
            compRefuelable = parent.GetComp<CompRefuelable>();

            if (Props.overlays == null || Props.overlays.Count == 0)
                return;

            // Defer graphic loading to the main thread
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                foreach (var overlay in Props.overlays)
                {
                    if (overlay.texPath.NullOrEmpty())
                        continue;

                    Rot4 rot = GetRotationFromString(overlay.direction);
                    graphics[rot] = new Tuple<Graphic, Vector3, float>(
                        GraphicDatabase.Get<Graphic_Single>(overlay.texPath, ShaderDatabase.Transparent),
                        overlay.size,
                        overlay.zOffset
                    );
                }
            });
        }


        public override void PostDraw()
        {
            base.PostDraw();

            // Ensure the overlay is only drawn when the building is fueled
            if (compRefuelable == null || !compRefuelable.HasFuel || !graphics.ContainsKey(parent.Rotation))
                return;

            var overlayData = graphics[parent.Rotation];
            Vector3 drawLoc = parent.DrawPos;
            drawLoc.y = AltitudeLayer.Item.AltitudeFor(); // Ensures it's above the building but below pawns
            drawLoc.z += overlayData.Item3;

            Color overlayColor = Props.inheritParentColor ? GetParentColor() : Color.white;
            Material overlayMaterial = MaterialPool.MatFrom(overlayData.Item1.path, ShaderDatabase.Transparent, overlayColor);

            Graphics.DrawMesh(
                MeshPool.plane10,
                Matrix4x4.TRS(drawLoc, Quaternion.identity, overlayData.Item2),
                overlayMaterial,
                0
            );
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
    }
}

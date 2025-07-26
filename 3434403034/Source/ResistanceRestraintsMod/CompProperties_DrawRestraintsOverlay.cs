using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace ResistanceRestraintsMod
{
    public class CompProperties_DrawRestraintsOverlay : CompProperties
    {
        public string texNorth;
        public string texSouth;
        public string texEast;
        public string texWest;

        public CompProperties_DrawRestraintsOverlay()
        {
            compClass = typeof(CompDrawRestraintsOverlay);
        }
    }

    public class CompDrawRestraintsOverlay : ThingComp
    {
        private CompProperties_DrawRestraintsOverlay Props => (CompProperties_DrawRestraintsOverlay)props;
        private Graphic graphicNorth;
        private Graphic graphicSouth;
        private Graphic graphicEast;
        private Graphic graphicWest;

        public override void PostDraw()
        {
            base.PostDraw();

            if (parent.Destroyed) return;

            Vector3 drawLoc = parent.DrawPos;
            drawLoc.y = AltitudeLayer.MoteOverhead.AltitudeFor();

            Graphic overlayGraphic = GetGraphicForRotation(parent.Rotation);

            if (overlayGraphic != null)
            {
                Vector3 scale = parent.Rotation.IsHorizontal ? new Vector3(2f, 1f, 1f) : new Vector3(1f, 1f, 2f);

                // Shift East/West overlays forward in the Z-axis by 0.05
                if (parent.Rotation == Rot4.East || parent.Rotation == Rot4.West)
                {
                    drawLoc.z += 0.02f;
                }

                Graphics.DrawMesh(
                    MeshPool.plane10,
                    Matrix4x4.TRS(drawLoc, Quaternion.identity, scale),
                    overlayGraphic.MatSingle,
                    0
                );
            }
        }



        private Graphic GetGraphicForRotation(Rot4 rotation)
        {
            switch (rotation.AsInt)
            {
                case 0: return graphicNorth; // North
                case 1: return graphicEast;  // East
                case 2: return graphicSouth; // South
                case 3: return graphicWest;  // West
                default: return null;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                if (!Props.texNorth.NullOrEmpty()) graphicNorth = LoadGraphic(Props.texNorth);
                if (!Props.texSouth.NullOrEmpty()) graphicSouth = LoadGraphic(Props.texSouth);
                if (!Props.texEast.NullOrEmpty()) graphicEast = LoadGraphic(Props.texEast);
                if (!Props.texWest.NullOrEmpty()) graphicWest = LoadGraphic(Props.texWest);
            });
        }


        private Graphic LoadGraphic(string path)
        {
            return GraphicDatabase.Get<Graphic_Single>(path, ShaderDatabase.Transparent);
        }
    }
}


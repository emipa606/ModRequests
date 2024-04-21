using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    class StatueOfAnimalRenderer {
        private const float YOffset_Body = 0.0046875f;

        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Head = 0.02734375f;

        public void Render(StatueOfAnimalGraphicSet graphics, Vector3 rootLoc, float angle, Rot4 bodyFacing, bool portrait,float scale, bool packed, Graphic_Shadow shadowGraphic, Graphic_Shadow specialShadowGraphic,Thing thing) {
            Graphics_DrawMeshImpl_Patch.scaleX = scale;
            Graphics_DrawMeshImpl_Patch.scaleY = scale;

            if (!graphics.AllResolved) {
                graphics.ResolveAllGraphics();
            }
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Mesh mesh = graphics.nakedGraphic.MeshAt(bodyFacing);
            List<Material> list = graphics.MatsBodyBaseAt(bodyFacing, RotDrawMode.Fresh);
            Vector3 loc = rootLoc;
            loc.y += 0.0078125f;
            for (int i = 0; i < list.Count; i++) {
                GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, list[i], portrait);
                loc.y += 0.00390625f;
            }

            Vector3 vector = rootLoc;
            if (bodyFacing != Rot4.North) {
                vector.y += YOffset_Shell;
            } else {
                vector.y += YOffset_Head;
            }

            if (packed && graphics.packGraphic != null) {
                Graphics.DrawMesh(mesh, vector, quaternion, graphics.packGraphic.MatAt(bodyFacing, null), 0);
            }

            if (specialShadowGraphic != null) {
                specialShadowGraphic.Draw(vector, Rot4.North, thing, 0f);
            }
            if (shadowGraphic != null) {
                shadowGraphic.Draw(vector, Rot4.North, thing, 0f);
            }

            Graphics_DrawMeshImpl_Patch.Reset();
            //Log.Message("Render:" + rootLoc + "/" + angle + "/" + bodyFacing + "/" + portrait + "/" + scale +"/" + packed + "\n" + graphics.nakedGraphic.path + "," + graphics.nakedGraphic.Shader.name + "," + graphics.nakedGraphic.color);
        }
    }
}

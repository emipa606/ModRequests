using System.Drawing.Drawing2D;
using RimWorld;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace PawnStorages
{
    public class Building_PawnStorage : PSBuilding
    {
        private CompPawnStorage storageComp;

        public override bool ShouldUseAlternative => base.ShouldUseAlternative && !(storageComp?.StoredPawns.NullOrEmpty() ?? true);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            storageComp = this.TryGetComp<CompPawnStorage>();
            if(storageComp == null)
                Log.Warning($"{this} has null CompPawnStorage even though of type {nameof(Building_PawnStorage)}");
        }

        /*
        [TweakValue("STATUE ROTATION", 0, 4)]
        public static int ROTATION = 0;

        [TweakValue("STATUE FLIPPING", 0f, 100f)]
        public static bool FLIPBOOL = false;
        */

        public override void Print(SectionLayer layer)
        {
            var pawn = storageComp.StoredPawns.FirstOrDefault();
            if (storageComp.Props.showStoredPawn && pawn != null)
            {
                var pos = DrawPos;
                pos.y += Altitudes.AltInc;
                var rot = Rotation;
                var texture = PortraitsCache.Get(pawn, new Vector2(175f, 175f), rot, new Vector3(0f, 0f, 0.1f), 1.5f);

                MaterialRequest req2 = default(MaterialRequest);
                req2.mainTex = texture.GetGreyscale();
                req2.shader = Graphic.data?.shaderType?.Shader ?? ShaderDatabase.DefaultShader;
                req2.color = DrawColor;
                req2.colorTwo = DrawColorTwo;

                Mesh mesh = Graphic.MeshAt(rot);
                var mat = MaterialPool.MatFrom(req2);
                Vector3 s = new Vector3(1.3f, 1f, 1.3f);

                pos += Graphic.DrawOffset(rot);
                pos += StatueOffset;

                //Somehow this magically fixes the flipping issue, just keeping it this way.
                mesh.SetUVs(false);
                Printer_Mesh.PrintMesh(layer, Matrix4x4.TRS(pos, Graphic.QuatFromRot(rot), s), mesh, mat);
            }
            base.Print(layer);
        }
    }
}

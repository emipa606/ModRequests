using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnStorages
{
    public class Building_PawnDoor : Building_Door
    {
        public override void Draw()
        {
            base.Rotation = Building_Door.DoorRotationAt(base.Position, base.Map);
            float d = 0f + 0.45f * this.OpenPct;
            for (int i = 0; i < 2; i++)
            {
                Vector3 vector;
                Mesh mesh;
                if (i == 0)
                {
                    vector = new Vector3(0f, 0f, -1f);
                    mesh = MeshPool.plane10;
                }
                else
                {
                    vector = new Vector3(0f, 0f, 1f);
                    mesh = MeshPool.plane10Flip;
                }
                Rot4 rotation = base.Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                vector = rotation.AsQuat * vector;
                Vector3 vector2 = this.DrawPos;
                vector2.y = def.altitudeLayer.AltitudeFor(); //AltitudeLayer.DoorMoveable.AltitudeFor();
                vector2 += vector * d;
                Graphics.DrawMesh(mesh, vector2, base.Rotation.AsQuat, this.Graphic.MatAt(base.Rotation, null), 0);
                Graphic_Shadow shadowGraphic = this.Graphic.ShadowGraphic;
                if (shadowGraphic != null)
                {
                    shadowGraphic.DrawWorker(vector2, base.Rotation, this.def, this, 0f);
                }
            }
            base.Comps_PostDraw();
        }
	}
}

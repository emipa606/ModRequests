using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{

    public class ProjectileCE_Rotational : BulletCE
    {

        public float rotationAngle;

        RotationalProjExtension Extension => def.GetModExtension<RotationalProjExtension>();

        public virtual float Speed => landed ? 0 : Extension == null ? 1 : Extension.speed;

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (base.FlightTicks == 0 && launcher != null && launcher is Pawn)
            {
                return;
            }
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, Vector3.up);
            Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), DrawPos, rotation, def.DrawMatSingle, 0);
            if (!castShadow)
            {
                return;
            }
            Vector3 position = new Vector3(ExactPosition.x, def.Altitude - 0.01f, ExactPosition.z - Mathf.Max(0f, ExactPosition.y));

            Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), position, rotation, ShadowMaterial, 0);
        }

        public override void Tick()
        {
            rotationAngle += Speed;
            base.Tick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rotationAngle, "rotationAngle", 0);
        }
    }
}

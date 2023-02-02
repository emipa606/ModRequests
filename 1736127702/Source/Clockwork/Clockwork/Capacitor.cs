using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Clockwork
{
    [StaticConstructorOnStartup]
    public class ClockworkGraphics
    {
        private Dictionary<Thing, ThingOverlaysHandle> overlayHandles = new Dictionary<Thing, ThingOverlaysHandle>();

        public static readonly Material CapacitorBarFilledMat = MaterialPool.MatFrom("UI/Overlays/CapacitorBankBarFilled", ShaderDatabase.Transparent);
        public static readonly Material CapacitorBarUnfilledMat = MaterialPool.MatFrom("UI/Overlays/CapacitorBankBarUnfilled", ShaderDatabase.Transparent);
        //public static readonly Material RequiresSteamMat = MaterialPool.MatFrom("UI/Overlays/RequiresSteam", ShaderDatabase.MetaOverlay);

        public struct FadingBarRequest
        {
            public Vector3 center;

            public Vector2 size;

            public float fillPercent;

            public Material filledMat;

            public Material unfilledMat;

            public Rot4 rotation;

            public Vector2 preRotationOffset;
        }

        public static void DrawFadingBar(FadingBarRequest r)
        {
            Vector2 vector = r.preRotationOffset.RotatedBy(r.rotation.AsAngle);
            r.center += new Vector3(vector.x, 0f, vector.y);
            if (r.rotation == Rot4.South)
            {
                r.rotation = Rot4.North;
            }
            if (r.rotation == Rot4.West)
            {
                r.rotation = Rot4.East;
            }
            Vector3 s = new Vector3(r.size.x, 1f, r.size.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(r.center, r.rotation.AsQuat, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, r.unfilledMat, 0);
            if (r.fillPercent > 0.001f)
            {
                Color matTransparency = r.filledMat.color;
                matTransparency.a = r.fillPercent;
                r.filledMat.color = matTransparency;
                Graphics.DrawMesh(MeshPool.plane10, matrix, r.filledMat, 0);
            }
        }

        //public static void RenderPulsingOverlay(Thing thing, Material mat)
        //{
        //    Vector3 drawPos = thing.TrueCenter();
        //    float num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
        //    num = 0.3f + num * 0.7f;
        //    Material material = FadedMaterialPool.FadedVersionOf(mat, num);
        //    Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, material, 0);
        //}

    }

    public class Building_CapacitorBank : Building_Battery
    {

        private static readonly Vector2 BarSize = new Vector2(0.7f, 0.7f);

        public override void Draw()
        {
            CompPowerBattery comp = GetComp<CompPowerBattery>();
            ClockworkGraphics.FadingBarRequest r = default(ClockworkGraphics.FadingBarRequest);
            r.center = DrawPos + Vector3.up * 0.1f;
            r.size = BarSize;
            r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            r.filledMat = ClockworkGraphics.CapacitorBarFilledMat;
            r.unfilledMat = ClockworkGraphics.CapacitorBarUnfilledMat;
            r.rotation = Rotation;
            ClockworkGraphics.DrawFadingBar(r);
        }

        public override void Tick() { }
    }
}
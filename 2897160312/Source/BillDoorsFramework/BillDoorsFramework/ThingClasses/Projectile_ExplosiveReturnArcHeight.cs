using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class Projectile_ExplosiveReturnArcHeight : Projectile_Explosive
    {
        private static readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);
        ModExtension_ProjectileHeightOffset heightExt => def.GetModExtension<ModExtension_ProjectileHeightOffset>();
        Vector3 lastPos;
        private float ArcHeightFactor
        {
            get
            {
                float num = def.projectile.arcHeightFactor;
                float num2 = (destination - origin).MagnitudeHorizontalSquared();
                if (num * num > num2 * 0.2f * 0.2f)
                {
                    num = Mathf.Sqrt(num2) * 0.2f;
                }
                return num;
            }
        }

        private float HeightOffset
        {
            get
            {
                if (heightExt != null)
                {
                    return heightExt.heightOffset * DistanceCoveredFraction;
                }
                return 0;
            }
        }
        Quaternion adjustedRot => lastPos == Vector3.zero ? ExactRotation : Quaternion.LookRotation((DrawPos - lastPos).Yto0());
        public override Vector3 DrawPos => ExactPosition + new Vector3(0f, 0f, 1f) * (ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction) + HeightOffset);
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (def.projectile.shadowSize > 0f)
            {
                DrawShadow(base.DrawPos);
            }
            Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), DrawPos, adjustedRot, DrawMat, 0);
            Comps_PostDraw();
        }

        public override void Tick()
        {
            lastPos = DrawPos;
            base.Tick();
        }

        private void DrawShadow(Vector3 drawLoc)
        {
            if (!(shadowMaterial == null))
            {
                float num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction));
                Vector3 s = new Vector3(num, 1f, num);
                Vector3 vector = new Vector3(0f, -0.01f, 0f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawLoc + vector, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
            }
        }
    }

    public class ModExtension_ProjectileHeightOffset : DefModExtension
    {
        public float heightOffset;
    }
}

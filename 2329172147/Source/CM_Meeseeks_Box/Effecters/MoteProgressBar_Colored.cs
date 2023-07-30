using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public class MoteProgressBar_Colored : MoteDualAttached
    {
        public float progress;

        public float offsetZ;

        private Color UnfilledColor = new Color(0.3f, 0.3f, 0.3f, 0.65f);

        private Color FilledColor = new Color(0.9f, 0.85f, 0.2f, 0.65f);

        private Material UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);

        private Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f, 0.65f), ShaderDatabase.MetaOverlay);

        public void SetUnfilledColor(Color newColor)
        {
            if (newColor.r != UnfilledColor.r || newColor.g != UnfilledColor.g || newColor.b != UnfilledColor.b || newColor.a != UnfilledColor.a)
            {
                UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(newColor, ShaderDatabase.MetaOverlay);
                UnfilledColor = newColor;
            }
        }

        public void SetFilledColor(Color newColor)
        {
            if (newColor.r != FilledColor.r || newColor.g != FilledColor.g || newColor.b != FilledColor.b || newColor.a != FilledColor.a)
            {
                FilledMat = SolidColorMaterials.NewSolidColorMaterial(newColor, ShaderDatabase.MetaOverlay);
                FilledColor = newColor;
            }
        }

        public override void Draw()
        {
            UpdatePositionAndRotation();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = exactPosition;
                r.center.z += offsetZ;
                r.size = new Vector2(exactScale.x, exactScale.z);
                r.fillPercent = progress;
                r.filledMat = FilledMat;
                r.unfilledMat = UnfilledMat;
                r.margin = 0.12f;
                if (offsetZ >= -0.8f && offsetZ <= -0.3f && AnyThingWithQualityHere())
                {
                    r.center.z += 0.25f;
                }
                GenDraw.DrawFillableBar(r);
            }
        }

        private bool AnyThingWithQualityHere()
        {
            IntVec3 c = exactPosition.ToIntVec3();
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i].TryGetComp<CompQuality>() != null && (thingList[i].DrawPos - exactPosition).MagnitudeHorizontalSquared() < 0.0001f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

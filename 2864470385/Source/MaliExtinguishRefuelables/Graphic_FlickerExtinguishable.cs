using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MaliExtinguishRefuelables
{
    public class Graphic_FlickerExtinguishable : Graphic_Flicker
    {

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thingDef == null)
            {
                Log.ErrorOnce("Fire DrawWorker with null thingDef: " + loc, 3427324);
                return;
            }
            if (subGraphics == null)
            {
                Log.ErrorOnce("Graphic_Flicker has no subgraphics " + thingDef, 358773632);
                return;
            }
            int num = Find.TickManager.TicksGame;
            if (thing != null)
            {
                num += Mathf.Abs(thing.thingIDNumber ^ 0x80FD52);
            }
            int num2 = num / 15;
            int num3 = Mathf.Abs(num2 ^ ((thing?.thingIDNumber ?? 0) * 391)) % subGraphics.Length;
            float num4 = 1f;
            CompFireOverlayExtinguishableBase compFireOverlayExtinguishableBase = null;
            CompProperties_FireOverlayExtinguishable compProperties = thingDef.GetCompProperties<CompProperties_FireOverlayExtinguishable>();
            if (thing != null)
            {
                compFireOverlayExtinguishableBase = thing.TryGetComp<CompFireOverlayExtinguishableBase>();
                if (compFireOverlayExtinguishableBase != null)
                {
                    num4 = compFireOverlayExtinguishableBase.FireSize;
                }
                else
                {
                    compFireOverlayExtinguishableBase = thing.TryGetComp<CompDarklightOverlayExtinguishable>();
                    if (compFireOverlayExtinguishableBase != null)
                    {
                        num4 = compFireOverlayExtinguishableBase.FireSize;
                    }
                }
            }
            else if (compProperties != null)
            {
                num4 = compProperties.fireSize;
            }
            if (num3 < 0 || num3 >= subGraphics.Length)
            {
                Log.ErrorOnce("Fire drawing out of range: " + num3, 7453435);
                num3 = 0;
            }
            Graphic graphic = subGraphics[num3];
            float num5 = ((compFireOverlayExtinguishableBase == null) ? Mathf.Min(num4 / 1.2f, 1.2f) : num4);
            Vector3 vector = GenRadial.RadialPattern[num2 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius;
            vector *= 0.05f;
            Vector3 pos = loc + vector * num4;
            if (thing?.Graphic?.data != null)
            {
                pos += thing.Graphic.data.DrawOffsetForRot(rot);
            }
            if (compFireOverlayExtinguishableBase != null)
            {
                pos += compFireOverlayExtinguishableBase.Props.DrawOffsetForRot(rot);
            }
            Vector3 s = new Vector3(num5, 1f, num5);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
        }
    }
}

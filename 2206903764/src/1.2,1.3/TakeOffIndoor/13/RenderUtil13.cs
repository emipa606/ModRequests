using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace TakeOffIndoor
{
    public static class RenderUtil
    {
        public static int ver = 13;
        public static void NotifyApparelChanged(TakeOffComp comp)
        {
            comp.Pawn.apparel.Notify_ApparelChanged();
        }

        public static void NotifyApparelChanged(Pawn pawn)
        {
            pawn.apparel.Notify_ApparelChanged();
        }

        public static bool AnyRenderingPosition(ThingDef def)
        {
            ApparelProperties ap = def.apparel;
            return ap.hatRenderedFrontOfFace || ap.shellRenderedBehindHead || ap.forceRenderUnderHair || ap.hatRenderedAboveBody || ap.hatRenderedBehindHead;
        }

        public static bool HasWornGraphicPath(ApparelProperties ap)
        {
            return !ap.wornGraphicPath.NullOrEmpty() || !ap.wornGraphicPaths.NullOrEmpty();
        }
    }
}

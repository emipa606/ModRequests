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
        public static int ver = 12;
        public static void NotifyApparelChanged(Pawn pawn)
        {

            pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
            pawn.Drawer.renderer.graphics.SetApparelGraphicsDirty();
            PortraitsCache.SetDirty(pawn);
        }
        public static void NotifyApparelChanged(TakeOffComp comp)
        {
            comp.Pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
            comp.Pawn.Drawer.renderer.graphics.SetApparelGraphicsDirty();
            PortraitsCache.SetDirty(comp.Pawn);
        }

        public static bool AnyRenderingPosition(ThingDef def)
        {
            return def.apparel.hatRenderedFrontOfFace || def.apparel.shellRenderedBehindHead;
        }

        public static bool HasWornGraphicPath(ApparelProperties ap)
        {
            return !ap.wornGraphicPath.NullOrEmpty();
        }
    }
}

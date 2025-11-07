using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class PawnRenderNode_ApparelTexPathed : PawnRenderNode_Apparel
    {
        public PawnRenderNode_ApparelTexPathed(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public PawnRenderNode_ApparelTexPathed(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree, apparel)
        {
        }

        public PawnRenderNode_ApparelTexPathed(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) : base(pawn, props, tree, apparel, useHeadMesh)
        {
        }

        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            if (TryGetGraphicApparel(apparel, tree.pawn.story.bodyType, TexPathFor(pawn), out var rec))
            {
                yield return rec.graphic;
            }
        }

        public static bool TryGetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, string path, out ApparelGraphicRecord rec)
        {
            if (bodyType == null)
            {
                Log.Error("Getting apparel graphic with undefined body type.");
                bodyType = BodyTypeDefOf.Male;
            }
            Shader shader = ShaderDatabase.Cutout;
            path += "_" + bodyType.defName;
            if (apparel.StyleDef?.graphicData.shaderType != null)
            {
                shader = apparel.StyleDef.graphicData.shaderType.Shader;
            }
            else if ((apparel.StyleDef == null && apparel.def.apparel.useWornGraphicMask) || (apparel.StyleDef != null && apparel.StyleDef.UseWornGraphicMask))
            {
                shader = ShaderDatabase.CutoutComplex;
            }
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
            rec = new ApparelGraphicRecord(graphic, apparel);
            return true;
        }
    }
    public class PawnRenderNode_HeadgearTexPathed : PawnRenderNode_Apparel
    {
        public PawnRenderNode_HeadgearTexPathed(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public PawnRenderNode_HeadgearTexPathed(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree, apparel)
        {
        }

        public PawnRenderNode_HeadgearTexPathed(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) : base(pawn, props, tree, apparel, useHeadMesh)
        {
        }

        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            if (TryGetGraphicApparel(apparel, TexPathFor(pawn), out var rec))
            {
                yield return rec.graphic;
            }
        }

        public static bool TryGetGraphicApparel(Apparel apparel, string path, out ApparelGraphicRecord rec)
        {
            Shader shader = ShaderDatabase.Cutout;
            if (apparel.StyleDef?.graphicData.shaderType != null)
            {
                shader = apparel.StyleDef.graphicData.shaderType.Shader;
            }
            else if ((apparel.StyleDef == null && apparel.def.apparel.useWornGraphicMask) || (apparel.StyleDef != null && apparel.StyleDef.UseWornGraphicMask))
            {
                shader = ShaderDatabase.CutoutComplex;
            }
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
            rec = new ApparelGraphicRecord(graphic, apparel);
            return true;
        }
    }

    public class PawnRenderNodeSubWorker_HairDrawSize : PawnRenderSubWorker
    {
        public override void TransformScale(PawnRenderNode node, PawnDrawParms parms, ref Vector3 scale)
        {
            if (parms.pawn.story?.hairDef?.GetModExtension<DefModExtension_HairExt>() is DefModExtension_HairExt ext && ext.drawsize > 0)
            {
                scale *= ext.drawsize;
            }
        }
    }
}

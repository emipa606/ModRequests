using UnityEngine;
using Verse;

namespace StagzMerfolk;

public class PawnRenderNode_Fishtail : PawnRenderNode
{
    public PawnRenderNode_Fishtail(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props,
        tree)
    {
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        if (pawn.story?.furDef == null)
        {
            return null;
        }

        return GraphicDatabase.Get<Graphic_Multi>(pawn.story?.furDef.GetFurBodyGraphicPath(pawn),
            ShaderDatabase.CutoutComplex, Vector2.one, pawn.story?.SkinColor ?? Color.white, Props.color ?? ColorFor(pawn)
        );
    }

    public override Color ColorFor(Pawn pawn)
    {
        return pawn.story.HairColor;
    }
}
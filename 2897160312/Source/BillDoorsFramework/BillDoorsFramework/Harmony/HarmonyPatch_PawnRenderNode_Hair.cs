using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    //[StaticConstructorOnStartup]
    //[HarmonyPatch(typeof(PawnRenderNode_Hair), "MeshSetFor")]
    //public static class LongHairPatch
    //{
    //    public static bool Prefix(Pawn pawn, ref GraphicMeshSet __result)
    //    {
    //        if (pawn.story?.hairDef?.GetModExtension<DefModExtension_HairExt>() is DefModExtension_HairExt ext && ext.drawsize > 0)
    //        {
    //            __result = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn, ext.drawsize, ext.drawsize);
    //            return false;
    //        }
    //        return true;
    //    }
    //}

    public class DefModExtension_HairExt : DefModExtension
    {
        public float drawsize = -1;

        public string backHairTexPath;
        public float backHairDrawsize = 1;
        public ShaderTypeDef overrideShaderTypeDef;
    }

    public class PawnRenderNode_BackHair : PawnRenderNode
    {
        public PawnRenderNode_BackHair(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override GraphicMeshSet MeshSetFor(Pawn pawn)
        {
            if (pawn.story?.hairDef?.GetModExtension<DefModExtension_HairExt>() is DefModExtension_HairExt ext && ext.backHairTexPath != null)
            {
                return HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn, ext.backHairDrawsize, ext.backHairDrawsize);
            }
            return MeshPool.GetMeshSetForSize(Vector2.one);
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.story?.hairDef == null || pawn.story.hairDef.noGraphic || pawn.DevelopmentalStage.Baby() || pawn.DevelopmentalStage.Newborn())
            {
                return null;
            }
            if (pawn.story?.hairDef?.GetModExtension<DefModExtension_HairExt>() is DefModExtension_HairExt ext && ext.backHairTexPath != null)
            {
                return GraphicDatabase.Get<Graphic_Multi>(ext.backHairTexPath, ext.overrideShaderTypeDef?.Shader ?? ShaderDatabase.CutoutHair, Vector2.one * ext.backHairDrawsize, ColorFor(pawn));
            }
            return null;
        }
    }
}
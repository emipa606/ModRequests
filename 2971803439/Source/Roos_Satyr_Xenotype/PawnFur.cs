using HarmonyLib;
using UnityEngine;
using Verse;

namespace Roos_Satyr_Xenotype
{

    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("DrawPawnFur")]
    static class PawnRenderer_Prefix
    {
        static bool Prefix(ref PawnRenderer __instance, ref Pawn ___pawn, Vector3 shellLoc, Rot4 facing, Quaternion quat, PawnRenderFlags flags)
        {
            if (___pawn.genes.HasGene(RBSF_DefOf.RBM_UnguligradeLegs))
            {
                Mesh mesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn).MeshAt(facing);
                Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(___pawn.story.furDef.GetFurBodyGraphicPath(___pawn), ShaderDatabase.CutoutSkinOverlay, Vector2.one, ___pawn.story.HairColor, Color.white, null, ___pawn.story.bodyType.bodyNakedGraphicPath);
                Material mat2 = graphic.MatAt(facing, null);
                GenDraw.DrawMeshNowOrLater(mesh, shellLoc, quat, mat2, flags.FlagSet(PawnRenderFlags.DrawNow));
                return false;
            }
            return true;
        }
    }

}

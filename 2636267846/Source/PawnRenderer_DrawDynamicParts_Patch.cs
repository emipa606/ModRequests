using HarmonyLib;
using UnityEngine;
using Verse;

namespace CensoredNudity
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawDynamicParts")]
    static class PawnRenderer_DrawDynamicParts_Patch
    {
        public static void Postfix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            var rotation = Quaternion.AngleAxis(angle, Vector3.up);

            CensoredNudity.ShouldCensor(___pawn, out bool censorHair, out bool censorFace, out bool censorChest, out bool censorGroin, !flags.FlagSet(PawnRenderFlags.Headgear), !flags.FlagSet(PawnRenderFlags.Clothes));

            if ((censorHair || censorFace) && !flags.HasFlag(PawnRenderFlags.HeadStump))
            {
                var headMesh = MeshPool.humanlikeHeadSet.MeshAt(pawnRotation);
                var headLoc = rootLoc + __instance.BaseHeadOffsetAt(pawnRotation);

                if (censorHair)
                {
                    Graphics.DrawMesh(headMesh, headLoc, rotation, Materials.HairMat[(int)CensoredNudity.censorMode.Value][pawnRotation.AsInt], 0);
                }
                if (censorFace)
                {
                    Graphics.DrawMesh(headMesh, headLoc, rotation, Materials.FaceMat[(int)CensoredNudity.censorMode.Value][pawnRotation.AsInt], 0);
                }
            }

            if (censorChest || censorGroin)
            {
                var bodyMesh = __instance.GetBodyOverlayMeshSet().MeshAt(pawnRotation);

                if (censorChest)
                {
                    Graphics.DrawMesh(bodyMesh, rootLoc, rotation, Materials.ChestMat[(int)CensoredNudity.censorMode.Value][pawnRotation.AsInt], 0);
                }
                if (censorGroin)
                {
                    Graphics.DrawMesh(bodyMesh, rootLoc, rotation, Materials.GroinMat[(int)CensoredNudity.censorMode.Value][pawnRotation.AsInt], 0);
                }
            }
        }
    }
}

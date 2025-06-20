using HarmonyLib;
using UnityEngine;
using Verse;

namespace Therapy.Patches
{
    internal static class PawnRenderer_Patch
    {
        // To allow pawns to lay correctly on couches
        [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
        public class BodyAngle
        {
            [HarmonyPrefix]
            public static bool Replacement(ref Pawn ___pawn, ref float __result)
            {
                var couch = ___pawn?.CurrentCouch();
                if (couch == null) return true;

                __result = couch.Rotation.Opposite.AsAngle;
                return false;
            }
        }

        // To allow pawns to lay correctly on couches
        [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
        public class RenderPawnAt
        {
            [HarmonyPrefix]
            public static void Prefix(ref Pawn ___pawn, ref Vector3 drawLoc)
            {
                var couch = ___pawn?.CurrentCouch();
                if (couch == null) return;
                switch (couch.Rotation.AsInt)
                {
                    case 0:
                        drawLoc += new Vector3(0, 0, 0.3f);
                        break;
                    case 1:
                        drawLoc += new Vector3(0.3f, 0, 0);
                        break;
                    case 2:
                        drawLoc += new Vector3(0, 0, -0.3f);
                        break;
                    case 3:
                        drawLoc += new Vector3(-0.3f, 0, 0);
                        break;
                }
            }
        }
    }
}
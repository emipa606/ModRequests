using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Umbrellas {
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags) })]
    class RBHarmonyRearrangePawnGraphics {
        private static Dictionary<ApparelGraphicRecord, bool> resetValues = new Dictionary<ApparelGraphicRecord, bool>();
        static void Prefix(ref PawnRenderer __instance, Pawn ___pawn, PawnGraphicSet ___graphics) {
            foreach (ApparelGraphicRecord record in ___graphics.apparelGraphics) {
                if (UmbrellaDefMethods.UmbrellaDefs.Contains(record.sourceApparel.def)) {
                    resetValues.Add(record, record.sourceApparel.def.apparel.hatRenderedFrontOfFace);
                    record.sourceApparel.def.apparel.hatRenderedFrontOfFace = (___pawn.Rotation == Rot4.North) ? false: true;
                }
            }
            List<ApparelGraphicRecord> graphicsTemp = new List<ApparelGraphicRecord>();
            foreach (ApparelGraphicRecord record in ___graphics.apparelGraphics) {
                if (UmbrellaDefMethods.UmbrellaDefs.Contains(record.sourceApparel.def)) {
                    graphicsTemp.Add(record);
                }
            }
            foreach (ApparelGraphicRecord record in graphicsTemp) {
                ___graphics.apparelGraphics.Remove(record);
            }
            ___graphics.apparelGraphics.AddRange(graphicsTemp);
        }

        static void Postfix(Pawn ___pawn) {
            foreach (KeyValuePair<ApparelGraphicRecord, bool> entry in resetValues) {
                entry.Key.sourceApparel.def.apparel.hatRenderedFrontOfFace = entry.Value;
            }
            resetValues.Clear();
        }
    }
    /*[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool)})]
    class HarmonyRedrawUmbrellas {
        static void Postfix(ref PawnRenderer __instance, Vector3 rootLoc, float angle, bool portrait, bool headStump, Rot4 headFacing, Rot4 bodyFacing, RotDrawMode bodyDrawType, Pawn ___pawn, PawnGraphicSet ___graphics) {
            if (___pawn.AnimalOrWildMan()) return;
            // variables/code below copied from PawnRenderer.RenderPawnInternal to make sure variables are the same
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 b = new Vector3(); // this will be overwritten soon but it has to be initialized because the code won't compile if it thinks there's a chance b won't have a value
            try {
                b = quaternion * __instance.BaseHeadOffsetAt(headFacing);
            } catch (System.NullReferenceException) {
                return; // if this causes an error then likely the pawn is something from a mod, like Boats; for sure it isn't a human Pawn, so it can be safely skipped
            }
            Vector3 loc2 = rootLoc + b;
            loc2.y += 0.030303031f;
            Mesh mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North) {
                a.y += 7f / 264f;
                vector.y += 0.022727273f;
            }
            else {
                a.y += 0.022727273f;
                vector.y += 7f / 264f;
            }
            //END copied variables/code

            if (___graphics.headGraphic == null) return;
            bool HasUmbrella = false;
            bool DrawHair = true;
            foreach (Apparel apparel in ___pawn.apparel.WornApparel) {
                if (UmbrellaDefMethods.UmbrellaDefs.Contains(apparel.def)) {
                    HasUmbrella = true;
                    break;
                }
            }
            if (!HasUmbrella) return; // this method should be as simple as possible bc it's called every frame I think, and because it's probably better to not redraw meshes
            // next: check if need to draw hair, if so draw hair, last draw umbrella
            foreach (ApparelGraphicRecord apparel in ___graphics.apparelGraphics) {
                if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead) {
                    if (!(apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace) && !UmbrellaDefMethods.UmbrellaDefs.Contains(apparel.sourceApparel.def)) { // if it would normally block hair
                        DrawHair = false;
                    }
                }
            }

            if (DrawHair && !headStump && bodyDrawType != RotDrawMode.Dessicated) {
                GenDraw.DrawMeshNowOrLater(___graphics.HairMeshSet.MeshAt(headFacing), mat: ___graphics.HairMatAt(headFacing), loc: loc2, quat: quaternion, drawNow: portrait);
            }
            foreach (ApparelGraphicRecord apparel in ___graphics.apparelGraphics) {
                if (UmbrellaDefMethods.UmbrellaDefs.Contains(apparel.sourceApparel.def)) {
                    Material original = apparel.graphic.MatAt(bodyFacing);
                    original = OverrideMaterialIfNeeded(original, ___pawn, ___graphics);
                    //Graphics.DrawMesh(mesh, loc2, quaternion, original, 0);
                    loc2.y += 0.1f;
                    GenDraw.DrawMeshNowOrLater(mesh, loc2, quaternion, original, portrait); // this is possibly the wrong command
                }
            }
        }
        private static Material OverrideMaterialIfNeeded(Material original, Pawn pawn, PawnGraphicSet graphics) {
            Material baseMat = pawn.IsInvisible() ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return graphics.flasher.GetDamagedMat(baseMat);
        }
    }*/
}
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AppearanceClothes {
    [StaticConstructorOnStartup]
    class Main {
        static Main() {
            var harmony = new Harmony("com.tammybee.apparentclothes");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            AppearanceClothesPref.LoadPref();
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet))]
    [HarmonyPatch("ResolveAllGraphics")]
    public static class PawnGraphicSet_ResolveAllGraphics_Patch {

        [HarmonyPostfix]
        static void ApplyAppearanceBodyTypeDef(ref PawnGraphicSet __instance) {
            if (__instance.pawn.EnableAppearanceBody()) {
                CompAppearanceClothes comp = __instance.pawn.GetComp<CompAppearanceClothes>();
                if (comp != null && comp.ShowAppearanceClothes) {
                    BodyTypeDef bodyType = __instance.pawn.GetAppearanceBodyTypeDef();
                    if (__instance.pawn.story?.SkinColor != null) {
                        __instance.nakedGraphic = GetNakedGraphic(__instance.pawn, bodyType);
                    }
                    //__instance.rottingGraphic = GetRottingGraphic(__instance.pawn, bodyType);
                }
            }
        }

        public static Graphic GetNakedGraphic(Pawn pawn,BodyTypeDef bodyType) {
            return GraphicDatabase.Get<Graphic_Multi>(bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);
        }

        /*
        public static Graphic GetRottingGraphic(Pawn pawn, BodyTypeDef bodyType) {
            return GraphicDatabase.Get<Graphic_Multi>(bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, PawnGraphicSet.RottingColor);
        }
        */
    }

    [HarmonyPatch(typeof(PawnGraphicSet))]
    [HarmonyPatch("ResolveApparelGraphics")]
    public static class PawnGraphicSet_ResolveApparelGraphics_Patch {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> cis = new List<CodeInstruction>(instructions);

            MethodInfo m_WornApparel = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel");
            int pos = cis.FindIndex(c => c.opcode == OpCodes.Callvirt && c.operand != null && c.operand == m_WornApparel);
            cis[pos] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PawnGraphicSet_ResolveApparelGraphics_Patch), "GetWornApparel"));

            FieldInfo f_bodyType = AccessTools.Field(typeof(Pawn_StoryTracker), "bodyType");
            int pos2 = cis.FindIndex(c => c.opcode == OpCodes.Ldfld && c.operand != null && c.operand == f_bodyType);
            cis.Insert(pos2 + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PawnGraphicSet_ResolveApparelGraphics_Patch), "GetBodyTypeDef")));
            cis.RemoveRange(pos2 - 1,2);

            foreach (CodeInstruction ci in cis) {
                yield return ci;
            }
        }

        static List<Apparel> GetWornApparel(Pawn_ApparelTracker apparelTracker) {
            CompAppearanceClothes compAppearanceClothes = apparelTracker.pawn.GetComp<CompAppearanceClothes>();
            if (compAppearanceClothes == null || !compAppearanceClothes.ShowAppearanceClothes || compAppearanceClothes.AppearanceClothes == null) {
                //現在の装備を表示用画像に設定
                return apparelTracker.WornApparel;
            } else {
                //見た目装備を表示用画像に設定
                return compAppearanceClothes.AppearanceClothes.Where(t => t is Apparel).ToList().ConvertAll(t => t as Apparel);
            }
        }

        static BodyTypeDef GetBodyTypeDef(Pawn p) {
            return p.GetAppearanceBodyTypeDef();
        }

        /*
        [HarmonyPrefix]
        static bool Replace(ref PawnGraphicSet __instance) {
            __instance.ClearCache();
            __instance.apparelGraphics.Clear();

            if (__instance.pawn == null) {
                return false;
            }

            BodyTypeDef bodyType = __instance.pawn.GetAppearanceBodyTypeDef();
            CompAppearanceClothes compAppearanceClothes = __instance.pawn.GetComp<CompAppearanceClothes>();
            if (compAppearanceClothes == null || !compAppearanceClothes.ShowAppearanceClothes || compAppearanceClothes.AppearanceClothes == null) {
                //現在の装備を表示用画像に設定
                if (__instance.pawn.apparel != null) {
                    foreach (Apparel current in __instance.pawn.apparel.GetDirectlyHeldThings()) {
                        ApparelGraphicRecord item;
                        if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, bodyType, out item)) {
                            __instance.apparelGraphics.Add(item);
                        }
                    }
                }
            } else {
                //見た目装備を表示用画像に設定
                if (compAppearanceClothes.AppearanceClothes != null) {
                    for (int i=0;i < compAppearanceClothes.AppearanceClothes.Count;i++) {
                        Thing current = compAppearanceClothes.AppearanceClothes[i];
                        Apparel apparel = current as Apparel;
                        if (apparel != null && __instance.pawn.story != null) {
                            ApparelGraphicRecord item;
                            if (ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, bodyType, out item)) {
                                __instance.apparelGraphics.Add(item);
                            }
                        }
                    }
                }
            }

            return false;
        }
        */
    }

    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("BaseHeadOffsetAt")]
    public static class PawnRenderer_BaseHeadOffsetAt_Patch {

        [HarmonyPostfix]
        static void ApplyAppearanceBodyType(ref PawnRenderer __instance, Rot4 rotation, ref Vector3 __result) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            SetHeadOffset(pawn, rotation, ref __result);
        }

        static void SetHeadOffset(Pawn pawn, Rot4 rotation, ref Vector3 p) {
            if (pawn.EnableAppearanceBody()) {
                CompAppearanceClothes comp = pawn.GetComp<CompAppearanceClothes>();
                if (comp != null && comp.ShowAppearanceClothes) {
                    Vector2 headOffset = pawn.GetAppearanceBodyTypeDef().headOffset;
                    headOffset *= Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor);
                    switch (rotation.AsInt) {
                    case 0:
                        p = new Vector3(0f, 0f, headOffset.y);
                        break;
                    case 1:
                        p = new Vector3(headOffset.x, 0f, headOffset.y);
                        break;
                    case 2:
                        p = new Vector3(0f, 0f, headOffset.y);
                        break;
                    case 3:
                        p = new Vector3(-headOffset.x, 0f, headOffset.y);
                        break;
                    default:
                        p = Vector3.zero;
                        break;
                    }
                }
            }
        }
    }
}

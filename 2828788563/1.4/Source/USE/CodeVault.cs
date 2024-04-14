////using HarmonyLib;
////using System.Reflection;
////using UnityEngine;
////using USESetApparelDrawOrder;
////using Verse;

////[HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
////public static class RederPawnAtCustomDrawOrderPatch2
////{
////    //public static Vector3 shellLocField = Vector3.zero;
////    public static MethodInfo OverrideMaterialIfNeeded = AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded");

////    [HarmonyPrefix]
////    public static bool Prefix(PawnRenderer __instance, /*out List<ApparelGraphicRecord> __state,*/ ref Vector3 shellLoc, ref Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
////    {
////        //__state = null;
////        if (__instance.graphics.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
////        {
////            //__state = __instance.graphics.apparelGraphics.ListFullCopy();

////            shellLoc.y = utilityLoc.y;
////        }

////        return true;
////    }

////    //[HarmonyPostfix]
////    //public static void Postfix(PawnRenderer __instance, List<ApparelGraphicRecord> __state, Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
////    //{
////    //    //shellLocField = shellLoc;

////    //    //flags |= PawnRenderFlags.DrawNow;

////    //    if (__state != null)
////    //    {
////    //        //shellLoc.y = __state[0];
////    //        //utilityLoc.y = __state[1];

////    //        __instance.graphics.apparelGraphics = __state;
////    //    }

////    //    shellLoc.y += 0.000289575267f;

////    //    if (__instance.graphics.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
////    //    {
////    //        __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
////    //            x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

////    //        foreach (var apparelGraphicRecord in __instance.graphics.apparelGraphics)
////    //        {
////    //            if (!(apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.OnHead || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.StrappedHead))
////    //            {
////    //                // y값을 기존에 그려졌던 의류들을 덮어쓰기 위해서는 shellLoc으로 통일해야함. (utilityLoc)은 기존 이미지를 덮어 쓰기에는 y값이 너무 낮음.
////    //                var graphics = __instance.graphics;
////    //                Pawn pawn = graphics.pawn;
////    //                Mesh mesh3 = apparelGraphicRecord.graphic.MeshAt(bodyFacing);
////    //                Material material4 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
////    //                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

////    //                if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && !apparelGraphicRecord.sourceApparel.def.apparel.shellRenderedBehindHead)
////    //                {
////    //                    Material material = apparelGraphicRecord.graphic.MatAt(bodyFacing);
////    //                    //material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : (Material)OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
////    //                    Vector3 loc = shellLoc;
////    //                    if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
////    //                    {
////    //                        loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
////    //                    }
////    //                    if (apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead)
////    //                    {
////    //                        loc.y += 0.00289575267f;
////    //                    }

////    //                    loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;

////    //                    //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);

////    //                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
////    //                }
////    //                else if (PawnRenderer.RenderAsPack(apparelGraphicRecord.sourceApparel))
////    //                {
////    //                    Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
////    //                    //material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : (Material)OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
////    //                    if (apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData != null)
////    //                    {
////    //                        Vector2 vector = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(bodyFacing, pawn.story.bodyType);
////    //                        Vector2 vector2 = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(bodyFacing, pawn.story.bodyType);
////    //                        Vector3 loc = utilityLoc;
////    //                        if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
////    //                        {
////    //                            loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
////    //                        }
////    //                        //loc.y = shellLoc.y;
////    //                        //loc.y = ((bodyFacing == Rot4.South) ? utilityLoc.y : shellLoc.y + 0.0299575267f);
////    //                        loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
////    //                        //if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.Backpack)
////    //                        //{
////    //                        //    loc.y += ((bodyFacing == Rot4.South) ? 0f : 0.0389575271f);
////    //                        //}
////    //                        Matrix4x4 matrix = Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector.x, 0f, vector.y)) * Matrix4x4.Scale(new Vector3(vector2.x, 1f, vector2.y));
////    //                        GenDraw.DrawMeshNowOrLater(bodyMesh, matrix, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
////    //                    }
////    //                    else
////    //                    {
////    //                        Vector3 loc = shellLoc;
////    //                        if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
////    //                        {
////    //                            loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
////    //                        }
////    //                        loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
////    //                        //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);
////    //                        GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
////    //                    }
////    //                }
////    //                else
////    //                {
////    //                    Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
////    //                    //material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : (Material)OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
////    //                    Vector3 loc = shellLoc;
////    //                    if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
////    //                    {
////    //                        loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
////    //                    }

////    //                    loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
////    //                    //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);
////    //                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
////    //                }
////    //            }
////    //        }
////    //    }
////    //}
////}

////[HarmonyPostfix]
////public static void PostfixPatch(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
////{
////    __instance.graphics.ClearCache();
////    foreach (var apparelGraphic in __instance.graphics.apparelGraphics)
////    {
////        Quaternion quaternion = Quaternion.AngleAxis(__instance.BodyAngle(), Vector3.up);
////        GenDraw.DrawMeshNowOrLater(apparelGraphic.graphic.MeshAt(__instance.LayingFacing()), drawLoc, quaternion, apparelGraphic.graphic.MatAt(__instance.LayingFacing()), true);
////    }
////}

////[HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
////public static class RenderPawnAtCustomDrawOrderPatch7
////{
////    [HarmonyPostfix]
////    public static void Postfix(PawnRenderer __instance, ref Vector3 __result)
////    {
////        if (__instance.graphics.apparelGraphics.NullOrEmpty() == false && (__instance.graphics.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn))
////        {
////            ApparelGraphicRecord record = __instance.graphics.apparelGraphics.MaxBy(delegate (ApparelGraphicRecord apparelGraphicRecord)
////            {
////                return apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
////            });
////            if (record.sourceApparel != null && record.sourceApparel.GetComp<USEDrawOrderSetComp>() != null)
////            {
////                __result.y += record.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500.0f;
////            }
////        }
////    }
////}

////[HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
////public static class DrawEquipmentPatch1
////{
////    [HarmonyPrefix]
////    public static void PrefixPatch(PawnRenderer __instance, ref Vector3 rootLoc, Rot4 pawnRotation)
////    {
////        if (pawnRotation != Rot4.North)
////        {
////            rootLoc.y += 5.0f;
////        }
////    }
////}

////[HarmonyPatch(typeof(JobDriver), nameof(JobDriver.ModifyCarriedThingDrawPos))]
////public static class DrawCarriedThingPatch1
////{
////    [HarmonyPostfix]
////    public static void PrefixPatch(ref Vector3 drawPos, ref bool behind, ref bool flip)
////    {
////        if (behind == false)
////        {
////            drawPos.y += 5.0f;
////        }
////    }
////}

////[HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
////public static class DrawHeadHairPatch
////{
////    public static MethodInfo OverrideMaterialIfNeeded = AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded");
////    public static MethodInfo ShellFullyCoversHead_method = AccessTools.Method(typeof(PawnRenderer), "ShellFullyCoversHead");

////    [HarmonyPrefix]
////    public static void PrefixPatch(PawnRenderer __instance, /*out List<ApparelGraphicRecord> __state,*/ ref Vector3 rootLoc, ref Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, bool bodyDrawn)
////    {
////        //__state = null;

////        Pawn pawn = __instance.graphics.pawn;
////        if (pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
////        {
////            //__state = __instance.graphics.apparelGraphics.ListFullCopy();

////            //__instance.graphics.apparelGraphics.Clear();

////            //__state = new List<float>();
////            //__state.Add(rootLoc.y);
////            //__state.Add(headOffset.y);

////            //rootLoc.y = 0;
////            //headOffset.y = 0;
////            //__instance.graphics.apparelGraphics.Clear();
////            //if (!__state.NullOrEmpty())
////            //{
////            //    __instance.graphics.apparelGraphics.Add(new ApparelGraphicRecord());
////            //}
////        }
////    }

////    //[HarmonyPostfix]
////    //public static void PostfixPatch(PawnRenderer __instance, List<ApparelGraphicRecord> __state, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, bool bodyDrawn)
////    //{
////    //    Pawn pawn = __instance.graphics.pawn;
////    //    if (__state != null)
////    //    {
////    //        __instance.graphics.apparelGraphics = __state;
////    //    }

////    //    //flags |= PawnRenderFlags.DrawNow;

////    //    bool flag3 = pawn.CurrentBed() != null && !pawn.CurrentBed().def.building.bed_showSleeperBody;
////    //    bool flag4 = !flags.FlagSet(PawnRenderFlags.Portrait) && flag3;

////    //    if (flag4 && USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_DrawHeadApparelWhenInBed)
////    //    {
////    //        return;
////    //    }

////    //    if ((bool)ShellFullyCoversHead_method.Invoke(__instance, new object[] { flags }) && bodyDrawn)
////    //    {
////    //        return;
////    //    }
////    //    Vector3 onHeadLoc = rootLoc + headOffset;
////    //    onHeadLoc.y += 0.0289575271f;

////    //    foreach (ApparelGraphicRecord apparelRecord in __instance.graphics.apparelGraphics)
////    //    {
////    //        //if ((apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) || apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOfExtended.Neck) || apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOfExtended.Teeth) || apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOfExtended.Mouth)) && (!apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead && !apparelGraphicRecord.sourceApparel.def.apparel.shellRenderedBehindHead))



////    //        if ((apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover || apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.OnHead || apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.StrappedHead))
////    //        {
////    //            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
////    //            Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
////    //            Material material4 = apparelRecord.graphic.MatAt(bodyFacing);
////    //            material4 = (flags.FlagSet(PawnRenderFlags.Cache) ? material4 : (Material)OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material4, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
////    //            Vector3 loc3 = rootLoc + headOffset;
////    //            loc3.y += 0.0289575271f;
////    //            if (apparelRecord.sourceApparel.def.apparel.hatRenderedBehindHead)
////    //            {
////    //                loc3.y += 0.00221660212f;
////    //            }
////    //            else
////    //            {
////    //                loc3.y += ((bodyFacing == Rot4.North && !apparelRecord.sourceApparel.def.apparel.hatRenderedAboveBody) ? 0.00289575267f : 0.003185328f);
////    //            }

////    //            if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
////    //            {
////    //                loc3 += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelRecord, bodyFacing, loc3);
////    //            }


////    //            loc3.y += apparelRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
////    //            GenDraw.DrawMeshNowOrLater(mesh3, loc3, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
////    //        }
////    //        //if (!apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) && !apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) && !apparelGraphicRecord.sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes))
////    //        //{
////    //        //    Mesh bodyMesh = null;
////    //        //    if (pawn.RaceProps.Humanlike)
////    //        //    {
////    //        //        bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(bodyFacing);
////    //        //    }
////    //        //    else
////    //        //    {
////    //        //        bodyMesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
////    //        //    }

////    //        //    var graphics = __instance.graphics;
////    //        //    Mesh mesh3 = apparelGraphicRecord.graphic.MeshAt(bodyFacing);
////    //        //    Material material4 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
////    //        //    Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

////    //        //    Vector3 loc3 = RederPawnAtCustomDrawOrderPatch2.shellLocField;
////    //        //    if (apparelGraphicRecord.sourceApparel.def.apparel.hatRenderedBehindHead)
////    //        //    {
////    //        //        loc3.y += 0.0221660212f;
////    //        //    }
////    //        //    else
////    //        //    {
////    //        //        loc3.y += ((bodyFacing == Rot4.North && !apparelGraphicRecord.sourceApparel.def.apparel.hatRenderedAboveBody) ? 0.00289575267f : 0.03185328f);
////    //        //    }
////    //        //    loc3.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 5000.0f;
////    //        //    //Log.Message(loc3.ToString());

////    //        //    GenDraw.DrawMeshNowOrLater(bodyMesh, loc3, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
////    //        //}
////    //    }
////    //}
////}

//using HarmonyLib;
//using RimWorld;
//using System.Collections.Generic;
//using System.Reflection.Emit;
//using System.Reflection;
//using UnityEngine;
//using USESetApparelDrawOrder;
//using Verse;

//[HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
//public static class RenderPawnAtCustomDrawOrderPatch2
//{
//    //public static Vector3 shellLocField = Vector3.zero;
//    public static MethodInfo OverrideMaterialIfNeeded = AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded");

//    //[HarmonyPostfix]
//    //public static void Postfix(PawnRenderer instance, /*List<ApparelGraphicRecord> __state,*/ Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
//    //{
//    //    //shellLocField = shellLoc;

//    //    //flags |= PawnRenderFlags.DrawNow;

//    //    //if (__state != null)
//    //    //{
//    //    //    //shellLoc.y = __state[0];
//    //    //    //utilityLoc.y = __state[1];

//    //    //    instance.graphics.apparelGraphics = __state;
//    //    //}

//    //    shellLoc.y += 0.000289575267f;

//    //    if (instance.graphics.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
//    //    {
//    //        instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
//    //            x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

//    //        foreach (var apparelGraphicRecord in instance.graphics.apparelGraphics)
//    //        {
//    //            if (!(apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == RimWorld.ApparelLayerDefOf.Overhead || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == RimWorld.ApparelLayerDefOf.EyeCover || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.OnHead || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.StrappedHead))
//    //            {
//    //                // y값을 기존에 그려졌던 의류들을 덮어쓰기 위해서는 shellLoc으로 통일해야함. (utilityLoc)은 기존 이미지를 덮어 쓰기에는 y값이 너무 낮음.
//    //                var graphics = instance.graphics;
//    //                Pawn pawn = graphics.pawn;
//    //                Mesh mesh3 = apparelGraphicRecord.graphic.MeshAt(bodyFacing);
//    //                Material material4 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//    //                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

//    //                if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == RimWorld.ApparelLayerDefOf.Shell && !apparelGraphicRecord.sourceApparel.def.apparel.shellRenderedBehindHead)
//    //                {
//    //                    Material material = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//    //                    //material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : (Material)OverrideMaterialIfNeeded.Invoke(instance, new object[] { material, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
//    //                    Vector3 loc = shellLoc;
//    //                    if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
//    //                    {
//    //                        loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
//    //                    }
//    //                    if (apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead)
//    //                    {
//    //                        loc.y += 0.00289575267f;
//    //                    }

//    //                    loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;

//    //                    //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);

//    //                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
//    //                }
//    //                else if (PawnRenderer.RenderAsPack(apparelGraphicRecord.sourceApparel))
//    //                {
//    //                    Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//    //                    //material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : (Material)OverrideMaterialIfNeeded.Invoke(instance, new object[] { material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
//    //                    if (apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData != null)
//    //                    {
//    //                        Vector2 vector = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(bodyFacing, pawn.story.bodyType);
//    //                        Vector2 vector2 = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(bodyFacing, pawn.story.bodyType);
//    //                        Vector3 loc = utilityLoc;
//    //                        if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
//    //                        {
//    //                            loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
//    //                        }
//    //                        //loc.y = shellLoc.y;
//    //                        //loc.y = ((bodyFacing == Rot4.South) ? utilityLoc.y : shellLoc.y + 0.0299575267f);
//    //                        loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
//    //                        //if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOfExtended.Backpack)
//    //                        //{
//    //                        //    loc.y += ((bodyFacing == Rot4.South) ? 0f : 0.0389575271f);
//    //                        //}
//    //                        Matrix4x4 matrix = Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector.x, 0f, vector.y)) * Matrix4x4.Scale(new Vector3(vector2.x, 1f, vector2.y));
//    //                        GenDraw.DrawMeshNowOrLater(bodyMesh, matrix, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//    //                    }
//    //                    else
//    //                    {
//    //                        Vector3 loc = shellLoc;
//    //                        if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
//    //                        {
//    //                            loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
//    //                        }
//    //                        loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
//    //                        //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);
//    //                        GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//    //                    }
//    //                }
//    //                else
//    //                {
//    //                    Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//    //                    //material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : (Material)OverrideMaterialIfNeeded.Invoke(instance, new object[] { material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
//    //                    Vector3 loc = shellLoc;
//    //                    if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
//    //                    {
//    //                        loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
//    //                    }

//    //                    loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
//    //                    //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);
//    //                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    public static MethodInfo drawMeshNowOrLaterMethodMatrix = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[4]
//            {
//            typeof(Mesh),
//            typeof(Matrix4x4),
//            typeof(Material),
//            typeof(bool)
//            });
//    public static MethodInfo drawMeshNowOrLaterMethodVector3 = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
//    {
//            typeof(Mesh),
//            typeof(Vector3),
//            typeof(Quaternion),
//            typeof(Material),
//            typeof(bool)
//    });
//    public static MethodInfo unityEngineMatrix4x4Translate = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.Translate));
//    public static MethodInfo beltScaleAt = AccessTools.Method(typeof(WornGraphicData), nameof(WornGraphicData.BeltScaleAt));

//    public static Vector3 oldVector;

//    [HarmonyTranspiler]
//    [HarmonyPriority(Priority.Last)]
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
//    {
//        List<CodeInstruction> codes = instructions.ToList();
//        for (int i = 0; i < codes.Count; i++)
//        {
//            CodeInstruction code = codes[i];
//            if ((i + 2) < codes.Count && codes[i + 1].opcode == OpCodes.Ldloc_S && codes[i + 2].opcode == OpCodes.Ldloc_1)
//            {
//                yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
//                yield return code;
//            }
//            //else if ((i) > 2 && codes[i - 1].opcode == OpCodes.Stloc_S && codes[i - 2].opcode == OpCodes.Callvirt && codes[i - 2].Calls(beltScaleAt))
//            //{
//            //    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
//            //    yield return new CodeInstruction(OpCodes.Ldloc_3);
//            //    yield return new CodeInstruction(OpCodes.Ldarg_1);
//            //    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocSet"));
//            //    yield return code;
//            //    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
//            //    yield return new CodeInstruction(OpCodes.Ldloc_3);
//            //    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocReset"));
//            //}
//            //else if (codes[i].opcode == OpCodes.Call && codes[i].Calls(unityEngineMatrix4x4Translate))
//            //{
//            //    yield return code;
//            //    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
//            //    yield return new CodeInstruction(OpCodes.Ldloc_3);
//            //    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocReset"));
//            //}
//            else if ((i + 2) < codes.Count && codes[i + 1].opcode == OpCodes.Ldarg_1 && codes[i + 2].opcode == OpCodes.Ldloc_1)
//            {
//                yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Ldarg_1);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocSet"));
//                yield return code;
//                yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocReset"));
//            }
//            else if ((i) > 2 && codes[i - 1].opcode == OpCodes.Stloc_S && codes[i - 2].opcode == OpCodes.Callvirt && codes[i - 2].Calls(beltScaleAt))
//            {
//                //yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
//                //yield return new CodeInstruction(OpCodes.Ldloc_3);
//                //yield return new CodeInstruction(OpCodes.Ldarg_1);
//                //yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocSet"));

//                yield return codes[i];

//                yield return new CodeInstruction(OpCodes.Ldarg, 3);
//                yield return new CodeInstruction(OpCodes.Ldloc, 6);
//                yield return new CodeInstruction(OpCodes.Ldarg, 6);
//                yield return new CodeInstruction(OpCodes.Ldarg, 2);
//                yield return new CodeInstruction(OpCodes.Ldloc, 1);
//                yield return new CodeInstruction(OpCodes.Ldloc, 7);
//                yield return new CodeInstruction(OpCodes.Ldloc, 8);
//                yield return new CodeInstruction(OpCodes.Ldloc, 3);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "DrawPackApparel"));
//            }
//            //else if ((i > 4) && codes[i - 4].opcode == OpCodes.Ldloc_S && codes[i].opcode == OpCodes.Call && codes[i].Calls(drawMeshNowOrLaterMethodVector3))
//            //{
//            //    yield return code;
//            //    yield return new CodeInstruction(OpCodes.Ldarga_S, 3);
//            //    yield return new CodeInstruction(OpCodes.Ldloc_3);
//            //    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLocReset"));
//            //}
//            else if (i - 1 > 0 && code.opcode == OpCodes.Blt && codes[i - 1].opcode == OpCodes.Callvirt)
//            {
//                yield return new CodeInstruction(OpCodes.Ldarg_0);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Ldarg, 1);
//                yield return new CodeInstruction(OpCodes.Ldarg, 2);
//                yield return new CodeInstruction(OpCodes.Ldarg, 3);
//                yield return new CodeInstruction(OpCodes.Ldarg, 4);
//                yield return new CodeInstruction(OpCodes.Ldarg, 5);
//                yield return new CodeInstruction(OpCodes.Ldarg, 6);
//                yield return new CodeInstruction(OpCodes.Ldloc_1);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "DrawApparel"));
//                yield return code;
//            }
//            else
//            {
//                yield return code;
//            }
//            //if (i > 0 && code.opcode == OpCodes.Stloc_3)
//            //{
//            //    yield return code;
//            //    yield return new CodeInstruction(OpCodes.Ldarg_0);
//            //    yield return new CodeInstruction(OpCodes.Ldloc_3);
//            //    yield return new CodeInstruction(OpCodes.Ldarg, 1);
//            //    yield return new CodeInstruction(OpCodes.Ldarg, 2);
//            //    yield return new CodeInstruction(OpCodes.Ldarg, 3);
//            //    yield return new CodeInstruction(OpCodes.Ldarg, 4);
//            //    yield return new CodeInstruction(OpCodes.Ldarg, 5);
//            //    yield return new CodeInstruction(OpCodes.Ldarg, 6);
//            //    yield return new CodeInstruction(OpCodes.Ldloc_1);
//            //    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "DrawApparel"));
//            //}
//            //else
//            //{
//            //    yield return code;
//            //}
//        }
//    }

//    public static void DrawApparel(PawnRenderer instance, ApparelGraphicRecord apparelGraphicRecord, Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags, Quaternion quaternion)
//    {
//        if (apparelGraphicRecord.sourceApparel == null || apparelGraphicRecord.graphic == null)
//        {
//            return;
//        }
//        Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//        if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == RimWorld.ApparelLayerDefOf.Shell && !apparelGraphicRecord.sourceApparel.def.apparel.shellRenderedBehindHead)
//        {
//            return;
//        }

//        if (PawnRenderer.RenderAsPack(apparelGraphicRecord.sourceApparel))
//        {
//            return;
//        }

//        if (!(apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == RimWorld.ApparelLayerDefOf.Overhead || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == RimWorld.ApparelLayerDefOf.EyeCover || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == DefDatabase<ApparelLayerDef>.GetNamedSilentFail("OnHead") || apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == DefDatabase<ApparelLayerDef>.GetNamedSilentFail("StrappedHead")))
//        {
//            //material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : (Material)OverrideMaterialIfNeeded.Invoke(instance, new object[] { material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
//            Vector3 loc = shellLoc;
//            if (ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
//            {
//                loc += VFECoreCompatibility.VFECoreApparelDrawOffsetReturn(apparelGraphicRecord, bodyFacing, loc);
//            }

//            loc.y += apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500.0f;
//            //Log.Message(loc.y.ToString() + " " + apparelGraphicRecord.sourceApparel.def.defName);
//            //Log.Message(apparelGraphicRecord.sourceApparel.def.defName);
//            GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//        }
//    }


//    public static void DrawPackApparel(Mesh bodyMesh, Material material2, PawnRenderFlags flags, Vector3 utilityLoc, Quaternion quaternion, Vector2 vector, Vector2 vector2, ApparelGraphicRecord
//        apparelRecord)
//    {
//        utilityLoc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
//        //Log.Message(utilityLoc.y.ToString());
//        Matrix4x4 matrix = Matrix4x4.Translate(utilityLoc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector.x, 0f, vector.y)) * Matrix4x4.Scale(new Vector3(vector2.x, 1f, vector2.y));
//        GenDraw.DrawMeshNowOrLater(bodyMesh, matrix, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//    }
//    public static void ModifyLoc(ref Vector3 loc, ApparelGraphicRecord apparelRecord)
//    {
//        loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
//    }
//    public static void ModifyLocSet(ref Vector3 loc, ApparelGraphicRecord apparelRecord, Vector3 shellLoc)
//    {
//        oldVector = loc;
//        loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
//    }
//    public static void ModifyLocReset(ref Vector3 loc, ApparelGraphicRecord apparelRecord)
//    {
//        loc = oldVector;
//    }
//    public static void CheckY(Vector3 loc, ApparelGraphicRecord apparelRecord)
//    {
//        Log.Message(apparelRecord.sourceApparel.def.defName + " " + loc.y.ToString());
//    }
//}

//using HarmonyLib;
//using RimWorld;
//using System.Collections.Generic;
//using System.Reflection.Emit;
//using System.Reflection;
//using UnityEngine;
//using USESetApparelDrawOrder;
//using Verse;

//[HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
//public static class RenderPawnAtCustomDrawOrderPatch2
//{
//    public static MethodInfo drawMeshNowOrLaterMethodMatrix = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[4]
//            {
//            typeof(Mesh),
//            typeof(Matrix4x4),
//            typeof(Material),
//            typeof(bool)
//            });
//    public static MethodInfo drawMeshNowOrLaterMethodVector3 = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
//    {
//            typeof(Mesh),
//            typeof(Vector3),
//            typeof(Quaternion),
//            typeof(Material),
//            typeof(bool)
//    });
//    public static MethodInfo unityEngineMatrix4x4Translate = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.Translate));
//    public static MethodInfo beltScaleAt = AccessTools.Method(typeof(WornGraphicData), nameof(WornGraphicData.BeltScaleAt));

//    public static Vector3 oldVector;

//    [HarmonyTranspiler]
//    [HarmonyPriority(Priority.Last)]
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
//    {
//        List<CodeInstruction> codes = instructions.ToList();
//        for (int i = 0; i < codes.Count; i++)
//        {
//            if ((i + 6) < codes.Count && codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == drawMeshNowOrLaterMethodVector3 && codes[i].opcode == OpCodes.Ldloc_S)
//            {
//                yield return new CodeInstruction(OpCodes.Ldloc, 5);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
//            }
//            else if ((i + 2) < codes.Count && codes[i].opcode == OpCodes.Ldarg_2 && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Ldloc_1)
//            {
//                yield return new CodeInstruction(OpCodes.Ldarg_2);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
//            }
//            else if ((i + 6) < codes.Count && codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == drawMeshNowOrLaterMethodVector3 && codes[i].opcode == OpCodes.Ldarg_1)
//            {
//                yield return new CodeInstruction(OpCodes.Ldarg_1);
//                yield return new CodeInstruction(OpCodes.Ldloc_3);
//                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
//            }
//            else
//            {
//                yield return codes[i];
//            }
//        }
//    }

//    public static Vector3 ModifyLoc(Vector3 loc, ApparelGraphicRecord apparelRecord)
//    {
//        loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 10000f;
//        Log.Message(apparelRecord.sourceApparel.def.defName + loc.y.ToString());
//        return loc;
//    }
//}
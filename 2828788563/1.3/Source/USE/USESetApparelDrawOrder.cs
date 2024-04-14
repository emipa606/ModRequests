using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using USEFastDeepCloner;
using OversizedApparel;
using UnityEngine.SocialPlatforms;

namespace USESetApparelDrawOrder
{
    [StaticConstructorOnStartup]
    static class USESetApparelDrawOrder
    {
        static USESetApparelDrawOrder()
        {
            var harmony = new Harmony("USE.SetApparelDrawOrder");

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            DefsLoaded();
        }
        public static void DefsLoaded()
        {
            IEnumerable<ThingDef> things = (
                    from def in DefDatabase<ThingDef>.AllDefs
                    where def.IsApparel == true
                    select def
                );

            foreach (ThingDef thingDef in things)
            {
                thingDef.comps.Add(new USEDrawOrderSetCompProperties());

                //if (!thingDef.apparel.shellCoversHead)
                //{
                //    thingDef.apparel.shellRenderedBehindHead = true;
                //}
                //thingDef.apparel.hatRenderedFrontOfFace = true;
                //thingDef.apparel.hatRenderedAboveBody = true;
                //if (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) 
                //    || thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes))
                //{
                //    thingDef.apparel.layers.Add(ApparelLayerDefOf.Overhead);
                //}
                //else
                //{
                //    thingDef.apparel.layers.Add(ApparelLayerDefOf.Shell);
                //}

                // hatRenderedFrontOfFace가 true인 의류들은 렌더링에서 우선순위를 가지는 것으로 보임.
                // 수동 조절이 가능하도록 모든 의류에 우선순위 해제
                // 우선순위 해제 전, 뒤통수 그래픽이 제대로 표현되도록 hatRenderedAboveBody 를 true로 만듬.
                //if (thingDef.apparel.hatRenderedFrontOfFace == false)
                //{
                //    thingDef.apparel.hatRenderedAboveBody = true;
                //}
                //thingDef.apparel.shellCoversHead = false;

                //thingDef.apparel.layers.Add(ApparelLayerDefOf.Shell);

                //thingDef.graphicData.graphicClass = typeof(Graphic_Multi)
                //if (thingDef.apparel.shellCoversHead == false)
                //{
                //    thingDef.apparel.shellRenderedBehindHead = true;
                //}
            }

            things = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null);

            foreach (ThingDef thing in things)
            {
                thing.comps.Add(new USEDrawOrderSetCompPawnProperties());
            }


            //IEnumerable<ApparelLayerDef> things2 = (
            //        from def in DefDatabase<ApparelLayerDef>.AllDefs
            //        where def != null
            //        select def
            //    );

            //foreach (ApparelLayerDef thingDef in things2)
            //{
            //    thingDef.drawOrder = 0;
            //}
        }
    }

    public class USEDrawOrderSetCompPawnProperties : CompProperties
    {
        public USEDrawOrderSetCompPawnProperties()
        {
            this.compClass = typeof(USEDrawOrderSetCompPawn);
        }

        public USEDrawOrderSetCompPawnProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

    public class USEDrawOrderSetCompPawn : ThingComp
    {
        public bool isAutoSortEverytime = true;

        public USEDrawOrderSetCompPawnProperties Props => (USEDrawOrderSetCompPawnProperties)this.Props;

        public USEDrawOrderSetCompPawn()
        {
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isAutoSortEverytime, "USEisAutoSortEverytime");
        }
    }


    public class USEDrawOrderSetCompProperties : CompProperties
    {
        public USEDrawOrderSetCompProperties()
        {
            this.compClass = typeof(USEDrawOrderSetComp);
        }

        public USEDrawOrderSetCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

    public class USEDrawOrderSetComp : ThingComp
    {
        public float drawOrderEntered = 0;

        public USEDrawOrderSetCompProperties Props => (USEDrawOrderSetCompProperties)this.Props;

        public USEDrawOrderSetComp()
        {
        }

        //public override IEnumerable<Gizmo> CompGetGizmosExtra()
        //{
        //    Command_Action action = new Command_Action();
        //    action.action = delegate
        //    {
        //        drawOrderEntered--;
        //    };

        //    yield return action;
        //}

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref drawOrderEntered, "USEdrawOrderEntered");
        }
    }


    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveApparelGraphics))]
    public static class RenderPawnAtCustomDrawOrderPatch1
    {
        [HarmonyPostfix]
        public static void PostfixPatch(PawnGraphicSet __instance)
        {

            Pawn pawn = __instance.pawn;
            if (pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
            {
                __instance.ClearCache();

                // 머리 부분 오프셋을 높여줘서 shell 레이어에 옷 두개가 있으면 목 부분이 나타나는 것을 방지
                //if (__instance.headGraphic != null && __instance.headGraphic.data != null)
                //{
                //    if (__instance.headGraphic.data.drawOffsetEast.HasValue)
                //    {
                //        __instance.headGraphic.data.drawOffsetEast.Value.Set(0, 3.5f, 0);
                //    }
                //    if (__instance.headGraphic.data.drawOffsetNorth.HasValue)
                //    {
                //        __instance.headGraphic.data.drawOffsetNorth.Value.Set(0, 3.5f, 0);
                //    }
                //    if (__instance.headGraphic.data.drawOffsetSouth.HasValue)
                //    {
                //        __instance.headGraphic.data.drawOffsetSouth.Value.Set(0, 3.5f, 0);
                //    }
                //    if (__instance.headGraphic.data.drawOffsetWest.HasValue)
                //    {
                //        __instance.headGraphic.data.drawOffsetWest.Value.Set(0, 3.5f, 0);
                //    }
                //    __instance.headGraphic.data.drawOffset.y += 3.5f;
                //}
                if (pawn.RaceProps.Humanlike && !__instance.apparelGraphics.NullOrEmpty())
                {
                    if (__instance.apparelGraphics.All((x) => { return x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered == 0; }))
                    {
                        ITab_Pawn_SetApparelOrder tab = pawn.GetInspectTabs().ToList().Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

                        if (tab != null)
                        {
                            tab.AutoSort(ref __instance.apparelGraphics);
                        }
                    }


                    __instance.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                    x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

                    foreach (var apparelGraphic in __instance.apparelGraphics)
                    {
                        if (apparelGraphic.graphic != null)
                        {
                            apparelGraphic.graphic.data = new GraphicData();
                            if (apparelGraphic.graphic.data.drawOffsetEast.HasValue)
                            {
                                apparelGraphic.graphic.data.drawOffsetEast.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                            }
                            if (apparelGraphic.graphic.data.drawOffsetNorth.HasValue)
                            {
                                apparelGraphic.graphic.data.drawOffsetNorth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                            }
                            if (apparelGraphic.graphic.data.drawOffsetSouth.HasValue)
                            {
                                apparelGraphic.graphic.data.drawOffsetSouth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                            }
                            if (apparelGraphic.graphic.data.drawOffsetWest.HasValue)
                            {
                                apparelGraphic.graphic.data.drawOffsetWest.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                            }
                            //apparelGraphic.graphic.data.drawOffset.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100000, 0);
                            //apparelGraphic.graphic.data.drawOffset.z += apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                            apparelGraphic.graphic.data.drawOffset.y = (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500.0f;
                            //apparelGraphic.graphic.data.drawOffset.x += apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                        }
                    }

                    // 내림차순 정렬용. 내림차순으로 정렬하면 값이 커질수록 더 일찍 렌더링 됨.
                    //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_DescendingOrder == true)
                    //{
                    //    __instance.apparelGraphics.Reverse();
                    //}

                    __instance.ClearCache();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
    public static class RenderPawnAtCustomDrawOrderPatch3
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (__instance.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
            {
                apparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered = 0;
                apparel.Graphic.data = apparel.def.graphicData;
                apparel.Graphic.data.drawOffset.Set(0, 0, 0);
                //apparelGraphic.graphic.data.drawOffset.z += apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                //apparel.Graphic.data.drawOffset.y += apparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                //apparelGraphic.graphic.data.drawOffset.x += apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                if (apparel.Graphic.data.drawOffsetEast.HasValue)
                {
                    apparel.Graphic.data.drawOffsetEast.Value.Set(0, 0, 0);
                }
                if (apparel.Graphic.data.drawOffsetNorth.HasValue)
                {
                    apparel.Graphic.data.drawOffsetNorth.Value.Set(0, 0, 0);
                }
                if (apparel.Graphic.data.drawOffsetSouth.HasValue)
                {
                    apparel.Graphic.data.drawOffsetSouth.Value.Set(0, 0, 0);
                }
                if (apparel.Graphic.data.drawOffsetWest.HasValue)
                {
                    apparel.Graphic.data.drawOffsetWest.Value.Set(0, 0, 0);
                }
            }

        }

        [HarmonyPostfix]
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if ((__instance.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn) && __instance.pawn.GetComp<USEDrawOrderSetCompPawn>().isAutoSortEverytime)
            {
                ITab_Pawn_SetApparelOrder tab = __instance.pawn.GetInspectTabs().ToList().Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

                if (tab != null)
                {
                    tab.AutoSort(ref __instance.pawn.Drawer.renderer.graphics.apparelGraphics);
                }
            }
        }
    }

    //[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelChanged))]
    //public static class RenderPawnAtCustomDrawOrderPatch5
    //{
    //    [HarmonyPrefix]
    //    public static void Prefix(Pawn_ApparelTracker __instance)
    //    {
    //        if (__instance.pawn.GetComp<USEDrawOrderSetCompPawn>().isAutoSortEverytime)
    //        {
    //            ITab_Pawn_SetApparelOrder tab = __instance.pawn.GetInspectTabs().ToList().Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

    //            if (tab != null)
    //            {
    //                tab.AutoSort(ref __instance.pawn.Drawer.renderer.graphics.apparelGraphics);
    //            }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
    public static class RenderPawnAtCustomDrawOrderPatch6
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if ((__instance.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn )&& __instance.pawn.GetComp<USEDrawOrderSetCompPawn>().isAutoSortEverytime)
            {
                ITab_Pawn_SetApparelOrder tab = __instance.pawn.GetInspectTabs().ToList().Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

                if (tab != null)
                {
                    tab.AutoSort(ref __instance.pawn.Drawer.renderer.graphics.apparelGraphics);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
    public static class RederPawnAtCustomDrawOrderPatch2
    {
        [HarmonyPostfix]
        public static void Postfix(PawnRenderer __instance, Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
        {
            if (__instance.graphics.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
            {
                __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                    x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

                foreach (var apparelRecord in __instance.graphics.apparelGraphics)
                {
                    if (apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && !apparelRecord.sourceApparel.def.apparel.shellRenderedBehindHead)
                    {
                        var graphics = __instance.graphics;
                        Mesh mesh3 = apparelRecord.graphic.MeshAt(bodyFacing);
                        Material material4 = apparelRecord.graphic.MatAt(bodyFacing);
                        Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

                        Vector3 loc3 = shellLoc;
                        if (apparelRecord.sourceApparel.def.apparel.shellCoversHead)
                        {
                            loc3.y += apparelRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500.0f + __instance.BaseHeadOffsetAt(bodyFacing).y;
                        }

                        //Log.Message(loc3.ToString());

                        GenDraw.DrawMeshNowOrLater(bodyMesh, loc3, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
    public static class RederPawnAtCustomDrawOrderPatch4
    {
        [HarmonyPrefix]
        public static void Prefix(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            Pawn pawn = __instance.graphics.pawn;
            if (pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
            {
                __instance.graphics.ClearCache();

                // 머리 부분 오프셋을 높여줘서 shell 레이어에 옷 두개가 있으면 목 부분이 나타나는 것을 방지
                //if (__instance.graphics.headGraphic != null && __instance.graphics.headGraphic.data != null)
                //{
                //    if (__instance.graphics.headGraphic.data.drawOffsetEast.HasValue)
                //    {
                //        __instance.graphics.headGraphic.data.drawOffsetEast.Value.Set(0, 3.5f, 0);
                //    }
                //    if (__instance.graphics.headGraphic.data.drawOffsetNorth.HasValue)
                //    {
                //        __instance.graphics.headGraphic.data.drawOffsetNorth.Value.Set(0, 3.5f, 0);
                //    }
                //    if (__instance.graphics.headGraphic.data.drawOffsetSouth.HasValue)
                //    {
                //        __instance.graphics.headGraphic.data.drawOffsetSouth.Value.Set(0, 3.5f, 0);
                //    }
                //    if (__instance.graphics.headGraphic.data.drawOffsetWest.HasValue)
                //    {
                //        __instance.graphics.headGraphic.data.drawOffsetWest.Value.Set(0, 3.5f, 0);
                //    }
                //    __instance.graphics.headGraphic.data.drawOffset.y += 3.5f;
                //}


                if (pawn.RaceProps.Humanlike && !__instance.graphics.apparelGraphics.NullOrEmpty())
                {
                    if (__instance.graphics.apparelGraphics.All((x) => { return x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered == 0; }))
                    {
                        ITab_Pawn_SetApparelOrder tab = pawn.GetInspectTabs().ToList().Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

                        if (tab != null)
                        {
                            tab.AutoSort(ref __instance.graphics.apparelGraphics);
                        }
                    }

                    __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                    x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

                    foreach (var apparelGraphic in __instance.graphics.apparelGraphics)
                    {
                        apparelGraphic.graphic.data = new GraphicData();
                        if (apparelGraphic.graphic.data.drawOffsetEast.HasValue)
                        {
                            apparelGraphic.graphic.data.drawOffsetEast.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                        }
                        if (apparelGraphic.graphic.data.drawOffsetNorth.HasValue)
                        {
                            apparelGraphic.graphic.data.drawOffsetNorth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                        }
                        if (apparelGraphic.graphic.data.drawOffsetSouth.HasValue)
                        {
                            apparelGraphic.graphic.data.drawOffsetSouth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                        }
                        if (apparelGraphic.graphic.data.drawOffsetWest.HasValue)
                        {
                            apparelGraphic.graphic.data.drawOffsetWest.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                        }
                        //apparelGraphic.graphic.data.drawOffset.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                        apparelGraphic.graphic.data.drawOffset.y = (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500.0f;
                    }

                    // 내림차순 정렬용. 내림차순으로 정렬하면 값이 커질수록 더 일찍 렌더링 됨.
                    //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_DescendingOrder == true)
                    //{
                    //    __instance.graphics.apparelGraphics.Reverse();
                    //}

                    __instance.graphics.ClearCache();
                }
            
            }
        }
    }
        //[HarmonyPostfix]
        //public static void PostfixPatch(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        //{
        //    __instance.graphics.ClearCache();
        //    foreach (var apparelGraphic in __instance.graphics.apparelGraphics)
        //    {
        //        Quaternion quaternion = Quaternion.AngleAxis(__instance.BodyAngle(), Vector3.up);
        //        GenDraw.DrawMeshNowOrLater(apparelGraphic.graphic.MeshAt(__instance.LayingFacing()), drawLoc, quaternion, apparelGraphic.graphic.MatAt(__instance.LayingFacing()), true);
        //    }
        //}

    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class RenderPawnAtCustomDrawOrderPatch7
    {
        [HarmonyPostfix]
        public static void Postfix(PawnRenderer __instance, ref Vector3 __result)
        {
            if (__instance.graphics.apparelGraphics.NullOrEmpty() == false && (__instance.graphics.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn))
            {
                ApparelGraphicRecord record = __instance.graphics.apparelGraphics.MaxBy(delegate (ApparelGraphicRecord apparelGraphicRecord)
                {
                    return apparelGraphicRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                });
                if (record.sourceApparel != null && record.sourceApparel.GetComp<USEDrawOrderSetComp>() != null)
                {
                    __result.y += record.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500.0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class DrawEquipmentPatch1
    {
        [HarmonyPrefix]
        public static void PrefixPatch(PawnRenderer __instance, ref Vector3 rootLoc, Rot4 pawnRotation)
        {
            if (pawnRotation != Rot4.North)
            {
                rootLoc.y += 5.0f;
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.ModifyCarriedThingDrawPos))]
    public static class DrawCarriedThingPatch1
    {
        [HarmonyPostfix]
        public static void PrefixPatch(ref Vector3 drawPos, ref bool behind, ref bool flip)
        {
            if (behind == false)
            {
                drawPos.y += 5.0f;
            }
        }
    }

    //[HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    //public static class TestClass
    //{
    //    public static MethodInfo OverrideMaterialIfNeeded = AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded");

    //    [HarmonyPrefix]
    //    public static bool PostfixPatch(PawnRenderer __instance, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
    //    {
    //        __instance.graphics.ClearCache();
    //        Vector3 onHeadLoc = rootLoc + headOffset;
    //        Pawn pawn = __instance.graphics.pawn;
    //        foreach (var apparelRecord in __instance.graphics.apparelGraphics)
    //        {
    //            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
    //            //GenDraw.DrawMeshNowOrLater(apparelRecord.graphic.MeshAt(headFacing), rootLoc, quat, apparelRecord.graphic.MatAt(headFacing), true);

    //            Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
    //            if (!apparelRecord.sourceApparel.def.apparel.hatRenderedFrontOfFace)
    //            {
    //                Material material3 = apparelRecord.graphic.MatAt(bodyFacing);
    //                material3 = (flags.FlagSet(PawnRenderFlags.Cache) ? material3 : OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material3, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }) as Material);
    //                if ((apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ||
    //                    apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) &&
    //                    !apparelRecord.sourceApparel.def.apparel.forceRenderUnderHair)
    //                {
    //                    GenDraw.DrawMeshNowOrLater(mesh3, onHeadLoc, quat, material3, true);
    //                }
    //                else
    //                {
    //                    GenDraw.DrawMeshNowOrLater(mesh3, rootLoc, quat, material3, true);
    //                }
    //            }
    //            else
    //            {
    //                Material material4 = apparelRecord.graphic.MatAt(bodyFacing);
    //                material4 = (flags.FlagSet(PawnRenderFlags.Cache) ? material4 : OverrideMaterialIfNeeded.Invoke(__instance, new object[] { material4, pawn, flags.FlagSet(PawnRenderFlags.Portrait) }) as Material);
    //                Vector3 loc3 = rootLoc + headOffset;
    //                if (apparelRecord.sourceApparel.def.apparel.hatRenderedBehindHead)
    //                {
    //                    loc3.y += 0.0221660212f;
    //                }
    //                else
    //                {
    //                    loc3.y += ((bodyFacing == Rot4.North && !apparelRecord.sourceApparel.def.apparel.hatRenderedAboveBody) ? 0.00289575267f : 0.03185328f);
    //                }
    //                if ((apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ||
    //                    apparelRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) &&
    //                    !apparelRecord.sourceApparel.def.apparel.forceRenderUnderHair)
    //                {
    //                    GenDraw.DrawMeshNowOrLater(mesh3, loc3, quat, material4, true);
    //                }
    //                else
    //                {
    //                    GenDraw.DrawMeshNowOrLater(mesh3, rootLoc, quat, material4, true);
    //                }
    //                //GenDraw.DrawMeshNowOrLater(mesh3, loc3, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
    //            }
    //        }

    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(Verse.Dialog_InfoCard), nameof(Verse.Dialog_InfoCard.DoWindowContents))]
    public static class InfoTabInputAdd
    {
        [HarmonyPostfix]
        [HarmonyPriority(0)]
        public static void Postfix(Verse.Dialog_InfoCard __instance, Rect inRect)
        {
            Thing thing = AccessTools.Field(typeof(Dialog_InfoCard), "thing").GetValue(__instance) as Thing;

            if (thing != null)
            {
                if (thing.def.IsApparel == true)
                {
                    Rect rect = new Rect(inRect);
                    rect.y = inRect.height - 45f;
                    rect.x = inRect.width - 350f;
                    rect.width = 300f;
                    rect.height = 25f;
                    USEDrawOrderSetComp comp = thing.TryGetComp<USEDrawOrderSetComp>();
                    string buffer = comp.drawOrderEntered.ToString();
                    Widgets.TextFieldNumericLabeled(rect, "Draw Order", ref comp.drawOrderEntered, ref buffer, -99999, 99999);
                }
            }
        }
    }

    [HarmonyPatch(typeof(OversizedApparel.OversizedApparel), nameof(OversizedApparel.OversizedApparel.GetDrawOffset))]
    public static class GetDrawOffsetFromApparelInstancePatch
    {
        [HarmonyPostfix]
        public static void PostfixPatch(ref Vector3 __result, ApparelGraphicRecord apparelRecord, Rot4 rot)
        {
            __result = apparelRecord.graphic.DrawOffset(rot);
        }
    }

    //[HarmonyPatch(typeof(Verse.Graphic), "DrawMeshInt")]
    //public static class WornApparelSortPatchTest
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(Graphic __instance, Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
    //    {
    //        Graphics.DrawMesh(mesh, loc, quat, mat, (int)loc.y * 500);
    //    }
    //}

    //[HarmonyPatch(typeof(Thing), nameof(Thing.DrawPos), MethodType.Getter)]
    //public static class RederPawnAtCustomDrawOrderPatch4
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(Thing __instance, ref Vector3 __result)
    //    {
    //        if (__instance.def.IsApparel == true)
    //        {
    //            int drawOrder = __instance.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered;

    //            drawOrder = (new IntRange(0, 30)).RandomInRange;

    //            Log.Message(drawOrder.ToString());

    //            __result.y += (float)drawOrder * 500;

    //            Log.Message(__result.ToString());
    //        }
    //    }
    //}
}
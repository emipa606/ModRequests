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
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements.Experimental;
using HugsLib.Shell;
using System.Reflection.Emit;
using VFECore;
using System.Net.NetworkInformation;

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
            }

            things = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null);

            foreach (ThingDef thing in things)
            {
                thing.comps.Add(new USEDrawOrderSetCompPawnProperties());
            }
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
        public bool isAutoSortEverytime = false;

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
        public float originalOffset = 0;
        

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
            Scribe_Values.Look(ref originalOffset, "USEoriginalOffset");
        }
    }

    //[DefOf]
    //public static class BodyPartGroupDefOfExtended
    //{
    //    public static BodyPartGroupDef Neck;
    //    public static BodyPartGroupDef Mouth;
    //    public static BodyPartGroupDef Teeth;
    //}

    //[DefOf]
    //public static class ApparelLayerDefOfExtended
    //{
    //    public static ApparelLayerDef OnHead;
    //    public static ApparelLayerDef StrappedHead;
    //    public static ApparelLayerDef Backpack;
    //}

    public static class VFECoreCompatibility
    {
        public static Vector3 VFECoreApparelDrawOffsetReturn(ApparelGraphicRecord apparelRecord, Rot4 bodyFacing, Vector3 loc)
        {
            if (apparelRecord.sourceApparel != null && apparelRecord.sourceApparel.def != null && !apparelRecord.sourceApparel.def.modExtensions.NullOrEmpty())
            {
                VFECore.ApparelDrawPosExtension modDrawSetting = apparelRecord.sourceApparel.def.modExtensions.Find(x => x.GetType() == typeof(VFECore.ApparelDrawPosExtension)) as VFECore.ApparelDrawPosExtension;
                if (modDrawSetting != null)
                {
                    Vector3 returnLoc = Vector3.zero;
                    if (modDrawSetting.apparelDrawSettings != null)
                    {
                        returnLoc += modDrawSetting.apparelDrawSettings.GetDrawPosOffset(bodyFacing, Vector3.zero);
                    }
                    if (modDrawSetting.headgearDrawSettings != null)
                    {
                        returnLoc += modDrawSetting.headgearDrawSettings.GetDrawPosOffset(bodyFacing, Vector3.zero);
                    }
                    if (modDrawSetting.packPosDrawSettings != null)
                    {
                        returnLoc += modDrawSetting.packPosDrawSettings.GetDrawPosOffset(bodyFacing, Vector3.zero);
                    }
                    if (modDrawSetting.shellPosDrawSettings != null)
                    {
                        returnLoc += modDrawSetting.shellPosDrawSettings.GetDrawPosOffset(bodyFacing, Vector3.zero);
                    }

                    return returnLoc;
                }
            }
            return Vector3.zero;
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
                //instance.ClearCache();

                // 머리 부분 오프셋을 높여줘서 shell 레이어에 옷 두개가 있으면 목 부분이 나타나는 것을 방지
                //if (instance.headGraphic != null && instance.headGraphic.data != null)
                //{
                //    if (instance.headGraphic.data.drawOffsetEast.HasValue)
                //    {
                //        instance.headGraphic.data.drawOffsetEast.Value.Set(0, 3.5f, 0);
                //    }
                //    if (instance.headGraphic.data.drawOffsetNorth.HasValue)
                //    {
                //        instance.headGraphic.data.drawOffsetNorth.Value.Set(0, 3.5f, 0);
                //    }
                //    if (instance.headGraphic.data.drawOffsetSouth.HasValue)
                //    {
                //        instance.headGraphic.data.drawOffsetSouth.Value.Set(0, 3.5f, 0);
                //    }
                //    if (instance.headGraphic.data.drawOffsetWest.HasValue)
                //    {
                //        instance.headGraphic.data.drawOffsetWest.Value.Set(0, 3.5f, 0);
                //    }
                //    instance.headGraphic.data.drawOffset.y += 3.5f;
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

                    //foreach (var apparelGraphic in instance.apparelGraphics)
                    //{
                    //    if (apparelGraphic.graphic != null)
                    //    {
                    //        apparelGraphic.graphic.data = new GraphicData();
                    //        if (apparelGraphic.graphic.data.drawOffsetEast.HasValue)
                    //        {
                    //            apparelGraphic.graphic.data.drawOffsetEast.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                    //        }
                    //        if (apparelGraphic.graphic.data.drawOffsetNorth.HasValue)
                    //        {
                    //            apparelGraphic.graphic.data.drawOffsetNorth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                    //        }
                    //        if (apparelGraphic.graphic.data.drawOffsetSouth.HasValu2500e)
                    //        {
                    //            apparelGraphic.graphic.data.drawOffsetSouth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                    //        }
                    //        if (apparelGraphic.graphic.data.drawOffsetWest.HasValue)
                    //        {
                    //            apparelGraphic.graphic.data.drawOffsetWest.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500, 0);
                    //        }
                    //        //apparelGraphic.graphic.data.drawOffset.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100000, 0);
                    //        //apparelGraphic.graphic.data.drawOffset.z += apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                    //        apparelGraphic.graphic.data.drawOffset.y = (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 500.0f;
                    //        //apparelGraphic.graphic.data.drawOffset.x += apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered;
                    //    }
                    //}

                    // 내림차순 정렬용. 내림차순으로 정렬하면 값이 커질수록 더 일찍 렌더링 됨.
                    //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_DescendingOrder == true)
                    //{
                    //    instance.apparelGraphics.Reverse();
                    //}

                    //instance.ClearCache();
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
    //    public static void Prefix(Pawn_ApparelTracker instance)
    //    {
    //        if (instance.pawn.GetComp<USEDrawOrderSetCompPawn>().isAutoSortEverytime)
    //        {
    //            ITab_Pawn_SetApparelOrder tab = instance.pawn.GetInspectTabs().ToList().Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

    //            if (tab != null)
    //            {
    //                tab.AutoSort(ref instance.pawn.Drawer.renderer.graphics.apparelGraphics);
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
            if (__instance != null && __instance.pawn != null && (__instance.pawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn ) && __instance.pawn.GetComp<USEDrawOrderSetCompPawn>().isAutoSortEverytime)
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
    public static class RenderPawnAtCustomDrawOrderPatch2
    {
        public static MethodInfo drawMeshNowOrLaterMethodMatrix = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[4]
                {
                typeof(Mesh),
                typeof(Matrix4x4),
                typeof(Material),
                typeof(bool)
                });
        public static MethodInfo drawMeshNowOrLaterMethodVector3 = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
        {
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool)
        });
        public static MethodInfo unityEngineMatrix4x4Translate = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.Translate));
        public static MethodInfo beltScaleAt = AccessTools.Method(typeof(WornGraphicData), nameof(WornGraphicData.BeltScaleAt));

        public static Vector3 oldVector;

        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                //if ((i + 6) < codes.Count && codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == drawMeshNowOrLaterMethodVector3 && codes[i].opcode == OpCodes.Ldloc_S)
                if ((i + 6) < codes.Count && codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == drawMeshNowOrLaterMethodVector3 && codes[i].opcode == OpCodes.Ldloc_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc, 5);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
                }
                else if ((i + 2) < codes.Count && codes[i].opcode == OpCodes.Ldarg_2 && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Ldloc_1)
                {
                    //yield return codes[i];
                    //yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
                }
                else if ((i + 6) < codes.Count && codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == drawMeshNowOrLaterMethodVector3 && codes[i].opcode == OpCodes.Ldarg_1)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch2), "ModifyLoc"));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static Vector3 ModifyLoc(Vector3 loc, ApparelGraphicRecord apparelRecord, PawnRenderer instance)
        {
            loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;
            //Log.Message(apparelRecord.sourceApparel.def.defName + loc.y.ToString());

            //apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset = loc.y;

            //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_EnableFreeMode)
            //{
            //    int index = instance.graphics.apparelGraphics.IndexOf(apparelRecord);
            //    if (index >= 1 && index <= instance.graphics.apparelGraphics.Count - 1)
            //    {
            //        if (instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset > loc.y)
            //        {
            //            loc.y = instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset;
            //        }
            //        loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;
                    
            //        //Log.Message(apparelRecord.sourceApparel.ToString() + " " + loc.y.ToString());
            //        //Log.Message(instance.graphics.apparelGraphics[index - 1].sourceApparel.ToString() + " " + instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset);
            //    }
            //}

            return loc;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody")]
    public static class RenderPawnAtCustomDrawOrderPatch4
    {
        public static MethodInfo drawMeshNowOrLaterMethodMatrix = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[4]
                {
                typeof(Mesh),
                typeof(Matrix4x4),
                typeof(Material),
                typeof(bool)
                });
        public static MethodInfo drawMeshNowOrLaterMethodVector3 = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
        {
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool)
        });
        public static MethodInfo unityEngineMatrix4x4Translate = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.Translate));
        public static MethodInfo beltScaleAt = AccessTools.Method(typeof(WornGraphicData), nameof(WornGraphicData.BeltScaleAt));

        public static Vector3 oldVector;

        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if ((i + 6) < codes.Count && codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == drawMeshNowOrLaterMethodVector3 && codes[i].opcode == OpCodes.Ldloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Material>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnAtCustomDrawOrderPatch4), "ModifyLoc"));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static Vector3 ModifyLoc(Vector3 loc, PawnRenderer instance, Vector3 rootLoc, Material original, Rot4 facing)
        {
            List<ApparelGraphicRecord> apparelGraphicRecordList = instance.graphics.apparelGraphics;

            foreach (ApparelGraphicRecord record in apparelGraphicRecordList)
            {
                if (record.graphic.MatAt(facing) == original)
                {
                    if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_EnableFreeMode)
                    {
                        loc = rootLoc;
                        if (facing != Rot4.North)
                        {
                            loc.y += 3f / 148f;
                        }
                        else
                        {
                            loc.y += 0.0231660213f;
                        }
                    }

                    loc.y += record.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;

                    break;
                    

                    //record.sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset = loc.y;

                    //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_EnableFreeMode)
                    //{
                    //    int index = instance.graphics.apparelGraphics.IndexOf(record);
                    //    if (index >= 1 && index <= instance.graphics.apparelGraphics.Count - 1)
                    //    {
                    //        if (instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset > loc.y)
                    //        {
                    //            loc.y = instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset;
                    //        }
                    //        loc.y += record.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;

                    //        //Log.Message(record.sourceApparel.ToString() + " " + loc.y.ToString());
                    //        //Log.Message(instance.graphics.apparelGraphics[index - 1].sourceApparel.ToString() + " " + instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset);
                    //    }
                    //}
                }
            }
            return loc;
        }
    }

    [HarmonyPatch]
    public static class DrawHeadHairDrawApparelPatch
    {
        private static Type displayType;

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            MethodInfo methodInfo = typeof(PawnRenderer).GetNestedTypes(AccessTools.all).SelectMany((Type type) => type.GetMethods(AccessTools.all)).FirstOrDefault((MethodInfo x) => x.Name.Contains("<DrawHeadHair>") && x.Name.Contains("DrawApparel"));
            displayType = methodInfo.DeclaringType;
            return methodInfo;
        }

        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> codes = codeInstructions.ToList();
            AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
            {
            typeof(Mesh),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(Material),
            typeof(bool)
            });
            CodeInstruction getThis = codes.First((CodeInstruction ins) => ins.opcode == OpCodes.Ldfld);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i - 2 >= 0 && i + 8 < codes.Count && (codes[i - 1].opcode == OpCodes.Ldarg_0 && codes[i - 2].opcode == OpCodes.Ldloc_0 && codes[i].opcode == OpCodes.Ldfld && codes[i + 8].opcode == OpCodes.Call))
                {
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(displayType, "onHeadLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "onHeadLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "bodyFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawHeadHairDrawApparelPatch), nameof(DrawHeadHairDrawApparelPatch.AdjustYOffset)));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "onHeadLoc"));
                }
                else if (i + 8 < codes.Count && codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 3].opcode == OpCodes.Ldloc_2 && codes[i + 8].opcode == OpCodes.Call)
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "onHeadLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "bodyFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawHeadHairDrawApparelPatch), nameof(DrawHeadHairDrawApparelPatch.AdjustYOffset)));
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static void AdjustYOffset(ref Vector3 v, ApparelGraphicRecord apparelRecord, Vector3 baseOffset, Rot4 bodyFacing, PawnRenderer instance)
        {
            if (bodyFacing != Rot4.North)
            {
                v.y = baseOffset.y;
            }
            v.y += apparelRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;

            //apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset = v.y;

            //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_EnableFreeMode)
            //{
            //    int index = instance.graphics.apparelGraphics.IndexOf(apparelRecord);
            //    if (index >= 1 && index <= instance.graphics.apparelGraphics.Count - 1)
            //    {
            //        if (instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset > v.y)
            //        {
            //            v.y = instance.graphics.apparelGraphics[index - 1].sourceApparel.TryGetComp<USEDrawOrderSetComp>().originalOffset;
            //        }
            //        v.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;
            //    }
            //}

            //Log.Message("Head.ApparelDraw " + apparelRecord.sourceApparel.ToString() + " " + v.y.ToString());
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
                //instance.graphics.ClearCache();


                if (pawn.RaceProps.Humanlike && !__instance.graphics.apparelGraphics.NullOrEmpty())
                {
                    if (__instance.graphics.apparelGraphics.All((x) => { return x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered == 0; }))
                    {
                        ITab_Pawn_SetApparelOrder tab = pawn.GetInspectTabs()?.ToList()?.Find((x) => { return x.GetType() == typeof(ITab_Pawn_SetApparelOrder); }) as ITab_Pawn_SetApparelOrder;

                        if (tab != null)
                        {
                            tab.AutoSort(ref __instance.graphics.apparelGraphics);
                        }
                    }

                    __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                    x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

                    //instance.graphics.ClearCache();
                }
            
            }
        }
    }

    [HarmonyPatch(typeof(Verse.Graphic), nameof(Verse.Graphic.Draw))]
    public static class DrawPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static void Patch(ref Vector3 loc, Rot4 rot, Thing thing, float extraRotation = 0f)
        {
            Pawn pawn = thing as Pawn;

            if (pawn != null)
            {
                pawn.Drawer.renderer.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                    x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

                if (!pawn.Drawer.renderer.graphics.apparelGraphics.NullOrEmpty())
                {
                    ApparelGraphicRecord maxRecord = pawn.Drawer.renderer.graphics.apparelGraphics.Last();
                    if (maxRecord.sourceApparel != null && maxRecord.sourceApparel.GetComp<USEDrawOrderSetComp>() != null)
                    {
                        loc.y += maxRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;
                    }
                }
            }

            //Log.Message("Graphic.Draw " + thing.ToString() + " " + loc.y.ToString());
        }
    }

    [HarmonyPatch(typeof(Verse.PawnRenderer), "DrawEquipment")]
    public static class EquipmentDrawPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static void Patch(Verse.PawnRenderer __instance, ref Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

            if (!__instance.graphics.apparelGraphics.NullOrEmpty())
            {
                ApparelGraphicRecord maxRecord = __instance.graphics.apparelGraphics.Last();
                if (maxRecord.sourceApparel != null && maxRecord.sourceApparel.GetComp<USEDrawOrderSetComp>() != null)
                {
                    rootLoc.y += maxRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Verse.PawnRenderer), "BaseHeadOffsetAt")]
    public static class HeadOffsetPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        public static void Patch(Verse.PawnRenderer __instance, ref Vector3 __result)
        {
            __instance.graphics.apparelGraphics.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                x.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(y.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

            if (!__instance.graphics.apparelGraphics.NullOrEmpty())
            {
                ApparelGraphicRecord maxRecord = __instance.graphics.apparelGraphics.Last();
                if (maxRecord.sourceApparel != null && maxRecord.sourceApparel.GetComp<USEDrawOrderSetComp>() != null)
                {
                    __result.y += maxRecord.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Verse.Dialog_InfoCard), nameof(Verse.Dialog_InfoCard.DoWindowContents))]
    public static class InfoTabInputAdd
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
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
}
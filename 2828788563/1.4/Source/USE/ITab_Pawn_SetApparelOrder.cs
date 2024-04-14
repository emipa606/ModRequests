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

namespace USESetApparelDrawOrder
{
    [StaticConstructorOnStartup]
    public class ITab_Pawn_SetApparelOrder : ITab_Pawn_Gear
    {
        public int ReorderableGroup;
        private float scrollViewHeight;
        private Vector2 scrollPosition = Vector2.zero;
        public override bool IsVisible => SelPawn.RaceProps.intelligence == Intelligence.Humanlike;
        private Pawn SelPawnForGear
        {
            get
            {
                if (base.SelPawn != null)
                {
                    return base.SelPawn;
                }
                Corpse corpse = base.SelThing as Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }
                throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + base.SelThing);
            }
        }

        private bool CanControl
        {
            get
            {
                Pawn selPawnForGear = SelPawnForGear;
                if (selPawnForGear.Downed || selPawnForGear.InMentalState)
                {
                    return false;
                }
                if (selPawnForGear.Faction != Faction.OfPlayer && !selPawnForGear.IsPrisonerOfColony)
                {
                    return false;
                }
                if (selPawnForGear.IsPrisonerOfColony && selPawnForGear.Spawned && !selPawnForGear.Map.mapPawns.AnyFreeColonistSpawned)
                {
                    return false;
                }
                if (selPawnForGear.IsPrisonerOfColony && (PrisonBreakUtility.IsPrisonBreaking(selPawnForGear) || (selPawnForGear.CurJob != null && selPawnForGear.CurJob.exitMapOnArrival)))
                {
                    return false;
                }
                return true;
            }
        }

        private bool CanControlColonist
        {
            get
            {
                if (CanControl)
                {
                    return SelPawnForGear.IsColonistPlayerControlled;
                }
                return false;
            }
        }

        public ITab_Pawn_SetApparelOrder()
        {
            size = new Vector2(460f, 450f);
            labelKey = "App";
            tutorTag = "Gear";
        }

        private Rect DrawThingRow(ref float y, float width, ref List<ApparelGraphicRecord> graphicItem, bool inventory = false)
        {
            Rect rect = new Rect(0f, y, width, 28f);

            graphicItem.Sort((ApparelGraphicRecord apparel1, ApparelGraphicRecord apparel2) =>
                apparel1.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered.CompareTo(apparel2.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered));

            foreach (ApparelGraphicRecord apparelGraphic in graphicItem)
            {
                rect = new Rect(0f, y, width, 28f);
                Thing thing = apparelGraphic.sourceApparel;
                Widgets.InfoCardButton(rect.width - 24f, y, thing);
                // 그룹값이 먼저 나오고, 그 다음에 실제로 움직여야 할 대상이 존재하는 함수에 Reorderable 메소드 호출
                // 이 때, 새로운 Rect는 초기 Rect와 x, y값이 동일해야함 (가능하면 모든 값이 동일하도록). 여기서는 위의 rect변수가 우리가 움직이고자 하는 대상
                // 이므로 똑같은 값을 설정하였음. 이후, y값이 변하면서 Rect 초기값이 변형되는데, 그 전에 놓는것이 좋음.
                ReorderableWidget.Reorderable(ReorderableGroup, new Rect(0f, y, width, 28f));
                rect.width -= 24f;
                bool flag = false;
                if (CanControl && (inventory || CanControlColonist || (SelPawnForGear.Spawned && !SelPawnForGear.Map.IsPlayerHome)))
                {
                    Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                    bool flag2 = false;
                    if (SelPawnForGear.IsQuestLodger())
                    {
                        flag2 = inventory || !EquipmentUtility.QuestLodgerCanUnequip(thing, SelPawnForGear);
                    }
                    Apparel apparel;
                    bool flag3 = (apparel = thing as Apparel) != null && SelPawnForGear.apparel != null && SelPawnForGear.apparel.IsLocked(apparel);
                    flag = flag2 || flag3;
                    if (Mouse.IsOver(rect2))
                    {
                        if (flag3)
                        {
                            TooltipHandler.TipRegion(rect2, "DropThingLocked".Translate());
                        }
                        else if (flag2)
                        {
                            TooltipHandler.TipRegion(rect2, "DropThingLodger".Translate());
                        }
                        else
                        {
                            TooltipHandler.TipRegion(rect2, "DropThing".Translate());
                        }
                    }
                    Color color = (flag ? Color.grey : Color.white);
                    Color mouseoverColor = (flag ? color : GenUI.MouseoverColor);
                    rect.width -= 24f;
                }
                if (CanControlColonist)
                {
                    if (FoodUtility.WillIngestFromInventoryNow(SelPawnForGear, thing))
                    {
                        Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
                        TooltipHandler.TipRegionByKey(rect3, "ConsumeThing", thing.LabelNoCount, thing);
                    }
                    rect.width -= 24f;
                }
                Rect rect4 = rect;
                rect4.xMin = rect4.xMax - 60f;
                RimWorld.Planet.CaravanThingsTabUtility.DrawMass(thing, rect4);
                rect.width -= 60f;
                if (Mouse.IsOver(rect))
                {
                    GUI.color = HighlightColor;
                    GUI.DrawTexture(rect, TexUI.HighlightTex);
                }
                if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
                {
                    Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
                }
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = ThingLabelColor;
                Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
                string text = thing.LabelCap;
                Apparel apparel2 = thing as Apparel;
                if (apparel2 != null && SelPawn.outfits != null && SelPawnForGear.outfits.forcedHandler.IsForced(apparel2))
                {
                    text += ", " + "ApparelForcedLower".Translate();
                }
                if (flag)
                {
                    text += " (" + "ApparelLockedLower".Translate() + ")";
                }
                Text.WordWrap = false;
                Widgets.Label(rect5, text.Truncate(rect5.width));
                Text.WordWrap = true;
                if (Mouse.IsOver(rect))
                {
                    string text2 = thing.DescriptionDetailed;
                    if (thing.def.useHitPoints)
                    {
                        text2 = text2 + "\n" + thing.HitPoints + " / " + thing.MaxHitPoints;
                    }
                    TooltipHandler.TipRegion(rect, text2);
                }
                y += 28f;
            }

            return rect;
        }

        protected override void FillTab()
        {
            if ((Find.UIRoot as UIRoot_Play) != null && SelPawn != null && SelPawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
            {
                Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);
                Rect rect2 = new Rect(0f, scrollViewHeight, rect.width, 20f);
                Rect position = new Rect(rect.x, rect.y, rect.width, rect.height);
                Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
                Rect viewRect = new Rect(0f, 0f, position.width - 16f, size.y);

                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

                GUI.BeginGroup(viewRect);
                float curY = 0f;

                if (SelPawn != null && SelPawn.GetComp<USEDrawOrderSetCompPawn>() != null)
                {
                    Widgets.Checkbox(new Vector2(viewRect.width - 48f, curY), ref SelPawn.GetComp<USEDrawOrderSetCompPawn>().isAutoSortEverytime);
                    Widgets.ListSeparator(ref curY, viewRect.width, "Automatic Sort Everytime?".Translate());
                }
                Widgets.ListSeparator(ref curY, viewRect.width, "Equipment".Translate());

                List<ApparelGraphicRecord> graphicItem = SelPawn.Drawer.renderer.graphics.apparelGraphics.ListFullCopy();

                if (Widgets.ButtonText(rect2, "Auto Order"))
                {
                    AutoSort(ref graphicItem);
                }

                // 그룹 값을 얻기 위해서. action은 null이어도 상관이 없음. 하지만 null일 경우 대상을 재정렬하는것이 불가능해짐. 실험용으로 적합함.
                // 반드시 Reorder 메소드를 호출하기전에 그룹 값을 얻어야 함.
                if (Event.current.type == EventType.Repaint)
                // Better Workbench Managemet의 코드를 참고할 것.
                {
                    ReorderableGroup = ReorderableWidget.NewGroup(delegate (int from, int to) { drawOrderChanged(ref graphicItem, from, to); }, ReorderableDirection.Vertical, new Rect(0f, 0f, UI.screenWidth, UI.screenHeight));
                }

                //foreach (var item in graphicItem)
                //         {
                DrawThingRow(ref curY, viewRect.width, ref graphicItem);
                //}

                //        foreach (Apparel apparel in SelPawn.apparel.WornApparel)
                //        {
                //ThingDef newThingDef = new ThingDef();

                //foreach (var fields in USEFastDeepCloner.DeepCloner.GetFastDeepClonerFields(apparel.def.GetType()))
                //            {
                //	fields.SetValue(newThingDef, fields.GetValue(apparel.def));
                //            }

                //newThingDef.apparel.layers.Add(new ApparelLayerDef());

                //        }

                // 옷 순서가 바뀌면 즉시 모든 그래픽을 리셋하게 하는 것이 (현재로선) 유일한 즉시 텍스처 렌더링 반영 방법임.
                //SelPawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                //SelPawn.Drawer.renderer.graphics.ClearCache();
                //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.South, false);
                //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.South);
                //SelPawn.Drawer.renderer.graphics.ClearCache();
                //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.East, false);
                //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.East);
                //SelPawn.Drawer.renderer.graphics.ClearCache();
                //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.North, false);
                //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.North);
                //SelPawn.Drawer.renderer.graphics.ClearCache();
                //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.West, false);
                //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.West);
                //SelPawn.Drawer.renderer.graphics.ClearCache();
                //SelPawn.Drawer.renderer.graphics.ResolveApparelGraphics();

                //foreach (var apparelGraphic in SelPawn.Drawer.renderer.graphics.apparelGraphics)
                //         {
                //             apparelGraphic.graphic.data = new GraphicData();

                //             if (apparelGraphic.graphic.data.drawOffsetEast.HasValue)
                //             {
                //                 apparelGraphic.graphic.data.drawOffsetEast.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //             }
                //             if (apparelGraphic.graphic.data.drawOffsetNorth.HasValue)
                //             {
                //                 apparelGraphic.graphic.data.drawOffsetNorth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //             }
                //             if (apparelGraphic.graphic.data.drawOffsetSouth.HasValue)
                //             {
                //                 apparelGraphic.graphic.data.drawOffsetSouth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //             }
                //             if (apparelGraphic.graphic.data.drawOffsetWest.HasValue)
                //             {
                //                 apparelGraphic.graphic.data.drawOffsetWest.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //             }
                //             apparelGraphic.graphic.data.drawOffset.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //         }

                if (Event.current.type == EventType.Layout)
                {
                    if (curY + 70f > 450f)
                    {
                        size.y = Mathf.Min(curY + 70f, (float)(UI.screenHeight - 35) - 165f - 30f);
                    }
                    else
                    {
                        size.y = 450f;
                    }
                    scrollViewHeight = curY + 20f;

                }
                GUI.EndGroup();
                Widgets.EndScrollView();
            }

        }

        public void AutoSort(ref List<ApparelGraphicRecord> graphicItem)
        {
            if ((Find.UIRoot as UIRoot_Play) != null && SelPawn != null && (SelPawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn))
            {
                if (graphicItem.NullOrEmpty())
                {
                    return;
                }
                graphicItem.Sort((ApparelGraphicRecord x, ApparelGraphicRecord y) =>
                    x.sourceApparel.def.apparel.LastLayer.drawOrder.CompareTo(y.sourceApparel.def.apparel.LastLayer.drawOrder));

                //if (USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_DescendingOrder == true)
                //{
                //    graphicItem.Reverse();
                //}

                for (int i = 0; i < graphicItem.Count; i++)
                {
                    drawOrderChanged(ref graphicItem, 10000, 10000);
                }
            }
        }

        public void drawOrderChanged(ref List<ApparelGraphicRecord> graphicItem, int from, int to)
        {
            if ((Find.UIRoot as UIRoot_Play) != null && SelPawn != null && SelPawn.IsColonist || USESetApparelDrawOrderModSettings.USESetApparelDrawOrderSettings_WorkEveryPawn)
            {
                //Log.Warning("=======================");
                //Log.Warning("to : " + to.ToString() + " from : " + from.ToString());
                //Log.Warning("=======================");

                if (graphicItem.NullOrEmpty() == true)
                {
                    //Log.Warning("Garphic item is Null");
                    return;
                }
                int pre_to = to;
                int pre_from = from;
                int beforeRemoveCount = graphicItem.Count;
                if (to >= graphicItem.Count)
                {
                    to = graphicItem.Count - 1;
                }
                else if (to < 0)
                {
                    to = 0;
                }
                if (from >= graphicItem.Count)
                {
                    from = graphicItem.Count - 1;
                }
                else if (from < 0)
                {
                    from = 0;
                }
                //if ((to + 1) < graphicItem.Count)
                //{
                //	graphicItem[from].sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered =
                //	graphicItem[to + 1].sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered + 1;
                //}
                //else if ((to - 1) >= 0)
                //{
                //	graphicItem[from].sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered =
                //	graphicItem[to - 1].sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered - 1;
                //}
                var temp = graphicItem.ElementAt(from);
                graphicItem.RemoveAt(from);
                if (pre_to >= beforeRemoveCount)
                {
                    graphicItem.Add(temp);
                }
                else if (pre_to >= to && pre_to > from)
                {
                    to--;
                    graphicItem.Insert(to, temp);
                }
                else
                {
                    graphicItem.Insert(to, temp);
                }
                int i = 0;
                foreach (ApparelGraphicRecord t in graphicItem)
                {
                    i++;
                    //Log.Warning(i.ToString() + " : " + t.sourceApparel.ToString());
                    t.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered = i;
                }



                //Log.Warning("=======================");
                //Log.Warning("to : " + to.ToString() + " from : " + from.ToString());
                //Log.Warning("=======================");

                // 옷 순서가 바뀌면 즉시 모든 그래픽을 리셋하게 하는 것이 (현재로선) 유일한 즉시 텍스처 렌더링 반영 방법임.
                if (SelThing != null && SelPawn != null && SelPawn.Drawer != null && SelPawn.Drawer.renderer != null && SelPawn.Drawer.renderer.graphics != null)
                {
                    SelPawn.Drawer.renderer.graphics.ClearCache();
                    SelPawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                    ////SelPawn.Drawer.renderer.graphics.ClearCache();
                    //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.South);
                    //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.South);
                    ////SelPawn.Drawer.renderer.graphics.ClearCache();
                    //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.East);
                    //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.East);
                    ////SelPawn.Drawer.renderer.graphics.ClearCache();
                    //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.North);
                    //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.North);
                    ////SelPawn.Drawer.renderer.graphics.ClearCache();
                    //SelPawn.Drawer.renderer.graphics.MatsBodyBaseAt(Rot4.West);
                    //SelPawn.Drawer.renderer.graphics.HairMatAt(Rot4.West);
                    ////SelPawn.Drawer.renderer.graphics.ClearCache();
                    SelPawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                }

                //foreach (var apparelGraphic in SelPawn.Drawer.renderer.graphics.apparelGraphics)
                //{
                //    apparelGraphic.graphic.data = new GraphicData();

                //    if (apparelGraphic.graphic.data.drawOffsetEast.HasValue)
                //    {
                //        apparelGraphic.graphic.data.drawOffsetEast.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //    }
                //    if (apparelGraphic.graphic.data.drawOffsetNorth.HasValue)
                //    {
                //        apparelGraphic.graphic.data.drawOffsetNorth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //    }
                //    if (apparelGraphic.graphic.data.drawOffsetSouth.HasValue)
                //    {
                //        apparelGraphic.graphic.data.drawOffsetSouth.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //    }
                //    if (apparelGraphic.graphic.data.drawOffsetWest.HasValue)
                //    {
                //        apparelGraphic.graphic.data.drawOffsetWest.Value.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //    }
                //    apparelGraphic.graphic.data.drawOffset.Set(0, (float)apparelGraphic.sourceApparel.GetComp<USEDrawOrderSetComp>().drawOrderEntered / 100, 0);
                //}

                //if (SelThing != null && SelPawn != null && SelPawn.drafter != null && SelPawn.Drafted == true)
                //{
                //    SelPawn.drafter.Drafted = true;
                //}
                //else if (SelThing != null && SelPawn != null && SelPawn.drafter != null)
                //{
                //    SelPawn.drafter.Drafted = false;
                //}
            }
        }
    }
}

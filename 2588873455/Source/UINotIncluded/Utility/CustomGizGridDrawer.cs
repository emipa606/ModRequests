using System;
using System.Collections.Generic;
using System.Linq;
using UINotIncluded;
using UnityEngine;

namespace Verse
{
    public static class CustomGizmoGridDrawer
    {
        public static Dictionary<KeyCode, Gizmo> drawnHotKeys = new Dictionary<KeyCode, Gizmo>();
        public static Func<Gizmo, bool> customActivator;
        private static float heightDrawn;
        private static int heightDrawnFrame;
        public static readonly Vector2 GizmoSpacing = new Vector2(5f, 5f);
        private static readonly float defaultSize = 80f;
        private static readonly List<List<Gizmo>> gizmoGroups = new List<List<Gizmo>>();
        private static readonly List<Gizmo> firstGizmos = new List<Gizmo>();
        private static readonly List<Gizmo> tmpAllGizmos = new List<Gizmo>();
        private static readonly Func<Gizmo, Gizmo, int> SortByOrder = (lhs, rhs) => lhs.Order.CompareTo(rhs.Order);

        public static float HeightDrawnRecently => Time.frameCount > heightDrawnFrame + 2 ? 0.0f : heightDrawn;

        public static void DrawGizmoGrid(
            IEnumerable<Gizmo> gizmos,
            int rows,
            float startX,
            out Gizmo mouseoverGizmo,
            bool leftoversOnRight = false,
            Func<Gizmo, bool> customActivatorFunc = null,
            Func<Gizmo, bool> highlightFunc = null,
            Func<Gizmo, bool> lowlightFunc = null)
        {
            if (Event.current.type == EventType.Layout)
            {
                mouseoverGizmo = null;
            }
            else
            {
                tmpAllGizmos.Clear();
                tmpAllGizmos.AddRange(gizmos);
                tmpAllGizmos.SortStable(SortByOrder);
                gizmoGroups.Clear();

                var buttonSize = defaultSize / rows - GizmoSpacing.y * (rows - 1f) / rows;
                var rowLength = (int)Math.Ceiling((float)gizmos.Count() / rows);
                var fullRows = gizmos.Count() % rows == 0;

                var curX = startX;
                var startY = UI.screenHeight - 15f - buttonSize * rows - GizmoSpacing.y * (rows - 1);

                var startYrows = new int[rows];
                var simpleButtons = buttonSize < 50;

                if (Settings.TabsOnBottom) startY -= UIManager.ExtendedBarHeight;

                for (var i = 0; i < rows; i++)
                    startYrows[i] = (int)Math.Floor(startY + i * (buttonSize + GizmoSpacing.y));

                for (var index1 = 0; index1 < tmpAllGizmos.Count; ++index1)
                {
                    var tmpAllGizmo = tmpAllGizmos[index1];
                    var flag = false;
                    for (var index2 = 0; index2 < gizmoGroups.Count; ++index2)
                        if (gizmoGroups[index2][0].GroupsWith(tmpAllGizmo))
                        {
                            flag = true;
                            gizmoGroups[index2].Add(tmpAllGizmo);
                            gizmoGroups[index2][0].MergeWith(tmpAllGizmo);
                            break;
                        }

                    if (!flag)
                    {
                        var gizmoList = SimplePool<List<Gizmo>>.Get();
                        gizmoList.Add(tmpAllGizmo);
                        gizmoGroups.Add(gizmoList);
                    }
                }

                firstGizmos.Clear();

                for (var index3 = 0; index3 < gizmoGroups.Count; ++index3)
                {
                    var gizmoGroup = gizmoGroups[index3];
                    var gizmo = (Gizmo)null;
                    for (var index4 = 0; index4 < gizmoGroup.Count; ++index4)
                        if (!gizmoGroup[index4].Disabled)
                        {
                            gizmo = gizmoGroup[index4];
                            break;
                        }

                    if (gizmo == null)
                    {
                        gizmo = gizmoGroup.FirstOrDefault();
                    }
                    else if (gizmo is Command_Toggle commandToggle23)
                    {
                        if (!commandToggle23.activateIfAmbiguous && !commandToggle23.isActive())
                            for (var index5 = 0; index5 < gizmoGroup.Count; ++index5)
                                if (gizmoGroup[index5] is Command_Toggle commandToggle24 && !commandToggle24.Disabled &&
                                    commandToggle24.isActive())
                                {
                                    gizmo = gizmoGroup[index5];
                                    break;
                                }

                        if (commandToggle23.activateIfAmbiguous && commandToggle23.isActive())
                            for (var index6 = 0; index6 < gizmoGroup.Count; ++index6)
                                if (gizmoGroup[index6] is Command_Toggle commandToggle25 && !commandToggle25.Disabled &&
                                    !commandToggle25.isActive())
                                {
                                    gizmo = gizmoGroup[index6];
                                    break;
                                }
                    }

                    if (gizmo != null) firstGizmos.Add(gizmo);
                }

                customActivator = customActivatorFunc;
                Text.Font = GameFont.Tiny;

                mouseoverGizmo = null;
                var interactedGiz = (Gizmo)null;
                var interactedEvent = (Event)null;
                var floatMenuGiz = (Gizmo)null;

                var curRow = 1;
                for (var index = 0; index < firstGizmos.Count; ++index)
                {
                    var firstGizmo = firstGizmos[index];
                    if (firstGizmo.Visible)
                    {
                        var buttonSpace = new Rect(curX, startYrows[curRow - 1], buttonSize, buttonSize);

                        heightDrawnFrame = Time.frameCount;
                        heightDrawn = (float)UI.screenHeight - startYrows[curRow - 1];
                        var result = ((Designator)firstGizmo).DoCustomGuizmoOnGUI(buttonSpace, new GizmoRenderParms
                            {
                                highLight = highlightFunc != null && highlightFunc(firstGizmo),
                                lowLight = lowlightFunc != null && lowlightFunc(firstGizmo)
                            },
                            simpleButtons);
                        ProcessGizmoState(firstGizmo, result, ref mouseoverGizmo);
                        GenUI.AbsorbClicksInRect(buttonSpace);

                        if ((index + 1) % rowLength == 0)
                        {
                            curX = startX;
                            curRow++;
                            if (curRow == rows && !leftoversOnRight && !fullRows)
                                curX += buttonSize +
                                        GizmoSpacing
                                            .x; // TODO: Dirty fix. Only works with 2 rows. Make it work on n rows.
                        }
                        else
                        {
                            curX += buttonSize + GizmoSpacing.x;
                        }
                    }
                }

                if (interactedGiz != null)
                {
                    var matchingGroup = FindMatchingGroup(interactedGiz);
                    for (var index = 0; index < matchingGroup.Count; ++index)
                    {
                        var other = matchingGroup[index];
                        if (other != interactedGiz && !other.Disabled && interactedGiz.InheritInteractionsFrom(other))
                            other.ProcessInput(interactedEvent);
                    }

                    interactedGiz.ProcessInput(interactedEvent);
                    Event.current.Use();
                }
                else if (floatMenuGiz != null)
                {
                    var floatMenuOptionList = new List<FloatMenuOption>();
                    foreach (var clickFloatMenuOption in floatMenuGiz.RightClickFloatMenuOptions)
                        floatMenuOptionList.Add(clickFloatMenuOption);
                    var matchingGroup = FindMatchingGroup(floatMenuGiz);
                    for (var index7 = 0; index7 < matchingGroup.Count; ++index7)
                    {
                        var other = matchingGroup[index7];
                        if (other != floatMenuGiz && !other.Disabled &&
                            floatMenuGiz.InheritFloatMenuInteractionsFrom(other))
                            foreach (var clickFloatMenuOption in other.RightClickFloatMenuOptions)
                            {
                                var floatMenuOption = (FloatMenuOption)null;
                                for (var index8 = 0; index8 < floatMenuOptionList.Count; ++index8)
                                    if (floatMenuOptionList[index8].Label == clickFloatMenuOption.Label)
                                    {
                                        floatMenuOption = floatMenuOptionList[index8];
                                        break;
                                    }

                                if (floatMenuOption == null)
                                {
                                    floatMenuOptionList.Add(clickFloatMenuOption);
                                }
                                else if (!clickFloatMenuOption.Disabled)
                                {
                                    if (!floatMenuOption.Disabled)
                                    {
                                        var prevAction = floatMenuOption.action;
                                        var localOptionAction = clickFloatMenuOption.action;
                                        floatMenuOption.action = () =>
                                        {
                                            prevAction();
                                            localOptionAction();
                                        };
                                    }
                                    else if (floatMenuOption.Disabled)
                                    {
                                        floatMenuOptionList[floatMenuOptionList.IndexOf(floatMenuOption)] =
                                            clickFloatMenuOption;
                                    }
                                }
                            }
                    }

                    Event.current.Use();
                    if (floatMenuOptionList.Any())
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptionList));
                }

                for (var index = 0; index < gizmoGroups.Count; ++index)
                {
                    gizmoGroups[index].Clear();
                    SimplePool<List<Gizmo>>.Return(gizmoGroups[index]);
                }

                gizmoGroups.Clear();
                firstGizmos.Clear();
                tmpAllGizmos.Clear();

                void ProcessGizmoState(Gizmo giz, GizmoResult result, ref Gizmo mouseoverGiz)
                {
                    if (result.State == GizmoState.Interacted || (result.State == GizmoState.OpenedFloatMenu &&
                                                                  giz.RightClickFloatMenuOptions.FirstOrDefault() ==
                                                                  null))
                    {
                        interactedEvent = result.InteractEvent;
                        interactedGiz = giz;
                    }
                    else if (result.State == GizmoState.OpenedFloatMenu)
                    {
                        floatMenuGiz = giz;
                    }

                    if (result.State < GizmoState.Mouseover)
                        return;
                    mouseoverGiz = giz;
                }
            }

            List<Gizmo> FindMatchingGroup(Gizmo toMatch)
            {
                for (var index = 0; index < gizmoGroups.Count; ++index)
                    if (gizmoGroups[index].Contains(toMatch))
                        return gizmoGroups[index];
                return null;
            }
        }

        public static void Clean()
        {
            GizmoGridDrawer.drawnHotKeys.Clear();
            drawnHotKeys.Clear();
        }

        public static float CalculateWidth(IEnumerable<Gizmo> gizmos, int rows)
        {
            float amountGizmos = gizmos.Count();
            var buttonSize = defaultSize / rows - GizmoSpacing.y * (rows - 1f) / rows;
            var rowLength = (float)Math.Ceiling(amountGizmos / rows);

            return rowLength * buttonSize + GizmoSpacing.y * rowLength;
        }
    }
}
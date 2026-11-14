using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget
{
    internal static class CustomLists
    {
        private static readonly float buttonspace_heigth = 40f;
        private static readonly float contraction = 4f;

        public static void Draggable<T>(string name, Rect inRect, List<T> elements, Func<T, string> getLabel,
            DragManager<T> manager, bool drawTittle = true)
        {
            if (drawTittle)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), name);
                Text.Anchor = TextAnchor.UpperLeft;
                inRect.y += 30f;
                inRect.height -= 30f;
            }

            Widgets.DrawMenuSection(inRect);

            var scroll = ScrollManager.GetInstance(name.GetHashCode());
            var scrollviewRect = new Rect(inRect).ContractedBy(1f);
            var scrollviewInRect = new Rect(0, 0, scrollviewRect.width - 21f,
                buttonspace_heigth * elements.Count() + contraction * 2);

            Widgets.BeginScrollView(scrollviewRect, ref scroll.pos, scrollviewInRect);

            var scrollbarWidth = 0f;
            if (scrollviewInRect.height > scrollviewRect.height) scrollbarWidth = 15f;

            var widgetSpace = new Rect(0f, 0f, scrollviewRect.width - scrollbarWidth, buttonspace_heigth);
            manager.ManageList(name, elements);

            var n = 0;
            foreach (var element in elements)
            {
                var innerSpace = widgetSpace.ContractedBy(contraction);
                var current = new DragElement { pos = n, listname = name, size = innerSpace.size };
                var shouldDrawButton = true;

                var spaceBefore = new Rect(innerSpace.x, widgetSpace.y - 13f, innerSpace.width, widgetSpace.height);
                var spaceAfter = new Rect(innerSpace.x, widgetSpace.y + 13f, innerSpace.width, widgetSpace.height);

                if (DragMemory.Dragging)
                {
                    if (DragMemory.Dragged?.listname == name && DragMemory.Dragged?.pos == n) shouldDrawButton = false;
                    var beforeDragged = DragMemory.Dragged?.pos > n;
                    if (Mouse.IsOver(spaceBefore))
                    {
                        Widgets.DrawLineHorizontal(innerSpace.x, widgetSpace.y, widgetSpace.width);
                        DragMemory.hoveringOver = current;
                    }
                    else if (Mouse.IsOver(spaceAfter))
                    {
                        Widgets.DrawLineHorizontal(innerSpace.x, widgetSpace.y + buttonspace_heigth, widgetSpace.width);
                        DragMemory.hoveringOver = new DragElement
                            { pos = n + 1, listname = name, size = innerSpace.size };
                    }
                }

                var hasOnClick = manager.HasOnClick(current);
                var buttonResult =
                    CustomButtons.DraggableButton(innerSpace, getLabel(element), ConfigActionIcon: hasOnClick);

                if (hasOnClick && buttonResult == Widgets.DraggableResult.Pressed) manager.OnClick(current);
                else if (shouldDrawButton && buttonResult == Widgets.DraggableResult.Dragged)
                    manager.DraggStart(current);

                widgetSpace.y += buttonspace_heigth;
                n++;
            }

            if (DragMemory.Dragging && !DragMemory.Hovering &&
                Mouse.IsOver(new Rect(0f, 0f, scrollviewRect.width, scrollviewRect.height)))
            {
                var innerSpace = widgetSpace.ContractedBy(contraction);
                Widgets.DrawLineHorizontal(innerSpace.x, widgetSpace.y, widgetSpace.width);
                DragMemory.hoveringOver = new DragElement { pos = n, listname = name, size = innerSpace.size };
            }

            Widgets.EndScrollView();
        }
    }
}
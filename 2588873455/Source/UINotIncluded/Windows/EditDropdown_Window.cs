using System;
using System.Collections.Generic;
using System.Linq;
using UINotIncluded.Widget;
using UINotIncluded.Widget.Configs;
using UnityEngine;
using Verse;

namespace UINotIncluded.Windows
{
    public class EditDropdown_Window : EditMainButton_Window
    {
        private List<ElementConfig> cacheAvaibleElements;

        public EditDropdown_Window(DropdownMenuConfig config) : base(config)
        {
        }

        public override Vector2 InitialSize => new Vector2(786f, 600f);

        public override void PreOpen()
        {
            base.PreOpen();
            WidgetManager.SelectGetter(WidgetManager.BUTTONS);
        }

        public override void DoWindowContents(Rect rect)
        {
            var firstHalf = new Rect(rect.x, rect.y, 314f, rect.height);
            var secondHalf = new Rect(firstHalf.xMax + 36, rect.y, 400f, rect.height);

            secondHalf.height -= CloseButSize.y;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(secondHalf.x, secondHalf.y, secondHalf.width, 35f), "Edit Dropdown");
            Text.Font = GameFont.Small;
            secondHalf.yMin += 45f;
            var list = new Listing_Standard();
            list.Begin(secondHalf);

            list.CheckboxLabeled("Match label size", ref ((DropdownMenuConfig)config).matchLabelSize,
                "If on, the dropdown will match the width of its button.");

            if (!((DropdownMenuConfig)config).matchLabelSize)
            {
                list.Label(string.Format("Width ({0}px)", Math.Round(((DropdownMenuConfig)config).width).ToString()));
                ((DropdownMenuConfig)config).width = list.Slider(((DropdownMenuConfig)config).width, 50f, 400f);
            }

            list.Label(string.Format("Spacing ({0}px)", Math.Round(((DropdownMenuConfig)config).spacing).ToString()));
            ((DropdownMenuConfig)config).spacing = list.Slider(((DropdownMenuConfig)config).spacing, 0f, 10f);

            list.End();

            var optionsHeight = list.MaxColumnHeightSeen;
            var dragableRect = new Rect(secondHalf.x, secondHalf.y + optionsHeight, secondHalf.width,
                secondHalf.height - optionsHeight);
            DoElementDraggables(dragableRect);

            base.DoWindowContents(firstHalf);
        }

        private void DoElementDraggables(Rect rect)
        {
            var columnWidth = rect.width / 2;
            var curY = rect.y;

            if (cacheAvaibleElements == null)
                cacheAvaibleElements = WidgetManager.AvailableSelectedWidgets(true)
                    .Where(e => e.GetType() != typeof(DropdownMenuConfig)).ToList();

            var manager = new DragManager<ElementConfig>(
                () => cacheAvaibleElements = null,
                element => { return element.SettingLabel; },
                element =>
                {
                    if (element.Configurable) return element.Worker.OpenConfigWindow;
                    return null;
                });

            var selectTypeRect = new Rect(rect.x, curY, columnWidth, 30f);
            if (Widgets.ButtonText(selectTypeRect.ContractedBy(2f), WidgetManager.SelectedGetterName))
            {
                var options = new List<FloatMenuOption>();
                foreach (var widgetGetter in WidgetManager.buttonGetters.Keys)
                    options.Add(new FloatMenuOption(widgetGetter, () =>
                    {
                        WidgetManager.SelectGetter(widgetGetter);
                        cacheAvaibleElements = null;
                    }));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            DragMemory.hoveringOver = null;

            CustomLists.Draggable("AvailableDrop",
                new Rect(rect.x, curY + selectTypeRect.height, columnWidth, rect.height - selectTypeRect.height)
                    .ContractedBy(3f), cacheAvaibleElements, element => { return element.SettingLabel; }, manager,
                false);
            CustomLists.Draggable("Dropdown",
                new Rect(rect.x + columnWidth, curY, columnWidth, rect.height).ContractedBy(3f),
                ((DropdownMenuConfig)config).elements, element => { return element.SettingLabel; }, manager);

            manager.Update();
        }
    }
}
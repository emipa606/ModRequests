using UINotIncluded.Widget.Configs;
using UINotIncluded.Windows;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget.Workers
{
    public class Dropdown_Worker : Button_Worker
    {
        public Dropdown_Worker(ButtonConfig config) : base(config)
        {
        }

        public override void InterfaceTryActivate()
        {
            DropdownMenu_Window.config = (DropdownMenuConfig)config;
            base.InterfaceTryActivate();
        }

        public override void OnGUI(Rect rect)
        {
            ((DropdownMenuConfig)config).lastX = rect.x;
            ((DropdownMenuConfig)config).lastY = rect.y;
            ((DropdownMenuConfig)config).lastWidth = rect.width;
            base.OnGUI(rect);
        }

        public override void OpenConfigWindow()
        {
            Find.WindowStack.Add(new EditDropdown_Window((DropdownMenuConfig)config));
        }
    }
}
using UINotIncluded.Widget;
using UnityEngine;
using Verse;

namespace UINotIncluded
{
    public static class UIManager
    {
        public static readonly float archButtonWidth = ExtendedToolbar.Height;

        private static bool tabsOnTop = Settings.TabsOnTop;
        private static readonly JobDesignatorBar JobsBar = new JobDesignatorBar();
        public static float ExtendedBarHeight => ExtendedToolbar.Height;
        public static float ExtendedBarWidth => ExtendedToolbar.Width;
        public static float ResourceGap => Settings.TabsOnTop ? ExtendedToolbar.Height : 0f;


        public static void BarsOnGUI()
        {
            ExtendedToolbar.ExtendedToolbarOnGUI(Settings.TopBarElements,
                new Rect(0f, 0f, UI.screenWidth, ExtendedToolbar.Height));
            ExtendedToolbar.ExtendedToolbarOnGUI(Settings.BottomBarElements,
                new Rect(0f, UI.screenHeight - ExtendedToolbar.Height + 1f, UI.screenWidth, ExtendedToolbar.Height));
        }

        public static void Before_MainUIOnGUI()
        {
            if (tabsOnTop != Settings.TabsOnTop)
            {
                tabsOnTop = Settings.TabsOnTop;
                Find.ColonistBar.MarkColonistsDirty();
            }
        }

        public static void MainUIOnGUI()
        {
            if (Settings.useDesignatorBar && Find.MainTabsRoot.OpenTab == null) JobsBar.JobDesignatorBarOnGUI();
        }

        public static void After_MainUIOnGUI()
        {
        }
    }
}
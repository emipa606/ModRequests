using HarmonyLib;
using Verse;

namespace UINotIncluded.Patches
{
    [HarmonyPatch(typeof(DebugWindowsOpener), "DrawButtons")]
    internal class DebugWindowsOpener_Patch
    {
        private static bool opened = true;

        public static bool Prefix(WidgetRow ___widgetRow)
        {
            if (!opened)
            {
                ___widgetRow.Init(0f, 0f);
                if (___widgetRow.ButtonIcon(ModTextures.chevronRight, "Show Dev Toolbar")) opened = true;
            }

            return opened;
        }

        public static void Postfix(WidgetRow ___widgetRow)
        {
            if (opened && ___widgetRow.ButtonIcon(ModTextures.chevronLeft, "Hide Dev Toolbar")) opened = false;
        }
    }
}


namespace StylishRim
{
    internal class HarmonyPatch_ColonistBar
    {
        [HarmonyLib.HarmonyPriority(404)]
        public static void ColonistBarOnGUI_Prefix()
        {
            StylishAdjuster.adjustPortrait = true;
        }
        [HarmonyLib.HarmonyPriority(404)]
        public static void ColonistBarOnGUI_Postfix()
        {
            StylishAdjuster.adjustPortrait = false;
        }
    }
}

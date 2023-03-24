namespace AnaerobicDigester.Harmony
{
    public class HarmonyBase
    {
        private static HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("com.helixenadditions.patch");

        public static void ApplyPatches()
        {
            harmonyInstance.PatchAll();
        }
    }
}
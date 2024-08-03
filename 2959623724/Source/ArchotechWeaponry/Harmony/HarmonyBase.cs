namespace ArchotechWeaponry.Harmony
{
    public class HarmonyBase
    {
        private static HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("com.archotechweaponry.patch");

        public static void ApplyPatches()
        {
            harmonyInstance.PatchAll();
        }
    }
}
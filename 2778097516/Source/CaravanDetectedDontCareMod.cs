using Verse;
using HarmonyLib;

namespace CaravanDetectedDontCare
{
    public class CaravanDetectedDontCareMod : Mod
    {
        public const string PACKAGE_ID = "caravandetecteddontcare.1trickPwnyta";
        public const string PACKAGE_NAME = "Caravan Detected? Don't Care";

        public CaravanDetectedDontCareMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}

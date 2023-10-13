using Verse;
using HarmonyLib;

namespace BuriedAlive
{
    public class BuriedAliveMod : Mod
    {
        public const string PACKAGE_ID = "buriedalive.1trickPonyta";
        public const string PACKAGE_NAME = "Buried Alive!";

        public BuriedAliveMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}

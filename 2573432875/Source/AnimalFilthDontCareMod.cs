using Verse;
using HarmonyLib;

namespace AnimalFilthDontCare
{
    public class AnimalFilthDontCareMod : Mod
    {
        public const string PACKAGE_ID = "animalfilthdontcare.1trickPonyta";
        public const string PACKAGE_NAME = "Animal Filth? Don't Care";

        public AnimalFilthDontCareMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}

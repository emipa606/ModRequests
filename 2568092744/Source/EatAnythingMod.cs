using Verse;
using RimWorld;

namespace EatAnything
{
    public class EatAnythingMod : Mod
    {
        public const string PACKAGE_ID = "eatanything.1trickPonyta";
        public const string PACKAGE_NAME = "Eat Anything";

        public EatAnythingMod(ModContentPack content) : base(content)
        {
            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}

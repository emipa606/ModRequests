using HarmonyLib;
using Verse;

namespace ImperialFunctionality
{
    public class ImperialFunctionalityMod : Mod
    {
        public ImperialFunctionalityMod(ModContentPack pack) : base(pack)
        {
			new Harmony("ImperialFunctionalityMod").PatchAll();
        }
    }
}

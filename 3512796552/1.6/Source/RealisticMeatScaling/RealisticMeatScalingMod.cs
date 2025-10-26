using System.Reflection;
using HarmonyLib;
using Verse;

namespace RealisticMeatScaling
{
    public class MeatMod : Mod
    {
        public MeatMod(ModContentPack content) : base(content)
        {
            var h = new Harmony("com.ocarina.realistic.meat.scaling");
            h.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

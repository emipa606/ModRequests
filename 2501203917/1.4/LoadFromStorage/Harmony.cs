using System.Reflection;
using HarmonyLib;
using Verse;

namespace LoadFromStorage
{
    [StaticConstructorOnStartup]
    internal static class Patch
    {
        static Patch()
        {
            var har = new Harmony("LoadFromStorage");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

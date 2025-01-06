using System.Reflection;
using Verse;
using HarmonyLib;

namespace LoadFromStockpileZone
{
    [StaticConstructorOnStartup]
    internal static class Patch
    {
        static Patch()
        {
            var har = new Harmony("LoadFromStockpilezone");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

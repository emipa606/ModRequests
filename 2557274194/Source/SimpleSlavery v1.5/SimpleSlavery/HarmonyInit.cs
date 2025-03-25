using HarmonyLib;
using System.Reflection;
using Verse;

namespace SimpleSlaveryCollars
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit() => new Harmony("TRIBeagle.simpleslaverycollars").PatchAll(Assembly.GetExecutingAssembly());
    }
}


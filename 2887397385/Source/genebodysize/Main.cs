using System.Reflection;
using HarmonyLib;
using Verse;

namespace genebodysize;
[StaticConstructorOnStartup]
public static class Main
{
    static Main()
    {
        new Harmony("mute.genebodysize").PatchAll(Assembly.GetExecutingAssembly());
    }
}
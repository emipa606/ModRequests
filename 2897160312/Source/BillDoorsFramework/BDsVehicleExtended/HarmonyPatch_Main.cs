using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using Verse;

namespace BillDoorsFramework
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        static PatchMain()
        {
            var instance = new Harmony("BD_HarmonyPatchesVF");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

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
        private static Harmony harmonyInstance;

        internal static Harmony HarmonyInstance
        {
            get
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new Harmony("BD_HarmonyPatchesVanilla");
                }
                return harmonyInstance;
            }
        }

        static PatchMain()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

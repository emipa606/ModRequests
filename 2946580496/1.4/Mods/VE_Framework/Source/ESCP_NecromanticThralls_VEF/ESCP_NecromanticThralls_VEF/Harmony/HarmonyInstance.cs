using HarmonyLib;
using Verse;
using System.Reflection;

namespace ESCP_NecromanticThralls_VEF
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ESCP_NecromanticThralls_VEF");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

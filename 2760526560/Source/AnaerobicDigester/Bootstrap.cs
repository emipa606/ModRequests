using AnaerobicDigester.Harmony;
using Verse;

namespace AnaerobicDigester
{
    [StaticConstructorOnStartup]
    public class Bootstrap
    {
        static Bootstrap()
        {
            HarmonyBase.ApplyPatches();
        }
    }
}
using ArchotechWeaponry.Harmony;
using Verse;

namespace ArchotechWeaponry
{
    [StaticConstructorOnStartup]
    public static class Bootstrap
    {
        static Bootstrap()
        {
            HarmonyBase.ApplyPatches();
            Log.Message("[ArchotechWeaponry] Done intialization");
        }    
    }
}
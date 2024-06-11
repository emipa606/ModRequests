using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace Reload
{
    [StaticConstructorOnStartup]
    public static class Compatibility
    {
        public static bool RunAndGun => ModsConfig.IsActive("roolo.RunAndGun") || ModLister.HasActiveModWithName("RunAndGun");

        public static bool Achtung => ModsConfig.IsActive("brrainz.achtung") || ModLister.HasActiveModWithName("Achtung!");
    }
}

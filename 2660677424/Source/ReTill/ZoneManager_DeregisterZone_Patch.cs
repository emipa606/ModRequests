using HarmonyLib;
using RimWorld;
using Verse;

namespace ReTill
{
    [HarmonyPatch(typeof(ZoneManager), nameof(ZoneManager.DeregisterZone))]
    static class ZoneManager_DeregisterZone_Patch
    {
        public static void Postfix(Zone oldZone)
        {
            if (oldZone is Zone_Growing growing)
            {
                AutoReTillZone.Forget(growing);
            }
        }
    }
}

namespace EasyLoadTransports
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    using System;

    [StaticConstructorOnStartup]

    public class EasyLoadTransports : Mod
    {
        public EasyLoadTransports(ModContentPack pack) : base(pack)
        {
            new Harmony("rimworld.scott2521.EasyLoadTransports").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Dialog_LoadTransporters), "TryAccept")]
    public static class Dialog_LoadTransporters_TryAccept_Patch
    {
        public static bool Prefix(Dialog_LoadTransporters __instance)
        {
            if (GenHostility.AnyHostileActiveThreatToPlayer(__instance.map) is false)
            {
                if (__instance.DebugTryLoadInstantly())
                {
                    __instance.Close();
                    return false;
                }
            }
            return true;
        }
    }
}
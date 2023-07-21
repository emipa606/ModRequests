using HarmonyLib;
using RimWorld;
using Verse;

namespace Renamon
{
    [HarmonyPatch(typeof(Ideo), "MemberNamePlural", MethodType.Getter)]
    public static class Ideo_MemberNamePlural_Fix
    {
        public static void Prefix(Ideo __instance)
        {
            if (__instance.memberName is null)
            {
                __instance.memberName = "CaravanMembers".Translate().ToLower();
            }
        }
    }
}

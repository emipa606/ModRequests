using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ShardMods
{
    [HarmonyPatch(typeof(Pawn), "Notify_Teleported")]
    class RangePatch_TeleportPatch
    { 
        static void Postfix(Pawn __instance)
        {
            if (__instance.IsColonistPlayerControlled)
                if (__instance.Map.fogGrid.IsFogged(__instance.Position))
                    __instance.Map.fogGrid.FloodUnfogAdjacent(__instance.Position, false);
        }
    }
}

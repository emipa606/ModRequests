using HarmonyLib;
using RimWorld;
using System;
using System.CodeDom;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class MapPawnsForMedEvac
    {
        [HarmonyPatch(typeof(MapPawns), "PlayerEjectablePodHolder")]
        static class PlayerEjectablePodHolder_PostFix
        {
            [HarmonyPostfix]
            static void PostFix(Thing thing, ref IThingHolder __result)
            {
                if (thing is FlyShipLeaving_MedEvac evac && evac.innerContainer.Any)
                {
                    Skyfaller skyfaller = evac.innerContainer[0] as Skyfaller;
                    __result = skyfaller;
                }
            }
        }
    }
}

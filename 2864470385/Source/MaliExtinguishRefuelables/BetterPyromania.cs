using System.Reflection;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Linq;
using Verse.Sound;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;
using BetterPyromania;

namespace MaliExtinguishRefuelables
{
    class BetterPyromaniaPatch
    {

        //im a goddamn butcher lmao
        //       public static bool Prefix(Need_Pyromania __instance, ref bool __result, IntVec3 center, Map map)
        //       {
        //           if (map == null)
        //           {
        //               return true;
        //           }
        //           Room room = center.GetRoom(map);
        //           bool flag = false;
        //           foreach (IntVec3 item in GenRadial.RadialCellsAround(center, 8f, useCenter: true))
        //           {
        //               if (!item.InBounds(map) || item.Fogged(map))
        //               {
        //                   continue;
        //               }
        //               Room room2 = item.GetRoom(map);
        //               if (room2 != null && room2 == room)
        //               {
        //                   (bool, bool) tuple = BetterPyromania_PyroUtility.IsFireAt(item, map);
        //                   var (flag2, _) = tuple;
        //                   if (tuple.Item2)
        //                   {
        //                       __instance.Notify_ObservedUncontainedFire();
        //                       __result = true;
        //                       return false;
        //                   }
        //                   if (flag2)
        //                   {
        //                       flag = true;
        //                   }
        //               }
        //           }
        //           if (flag)
        //           {
        //               __instance.Notify_ObservedContainedFire();
        //               __result = true;
        //               return false;
        //           }
        //           return true;
        //       }
    }

}

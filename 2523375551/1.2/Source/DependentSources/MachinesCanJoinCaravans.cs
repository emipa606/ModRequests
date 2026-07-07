using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.HarmonyPatches;

namespace RedScare
{
    [HarmonyPatch(typeof(MachinesCannotJoinCaravans), "Postfix")]
    public static class RedMachinesCanJoinCaravans
    {
        [HarmonyPrefix]
        [HarmonyPriority(300)]
        public static bool Prefix(ref List<Pawn> __0)
        {
            //Log.Message("PREFIX MACHINES CAN JOIN CARAVANS");
            //foreach(var pawn in __0)
            //    if(CompMachine.cachedMachines.TryGetValue(pawn.Drawer.renderer, out CompMachine value))
            //    {
            //        Log.Message($"{value.parent.GetType().Name} {value.Props.hoursActive >= 24000}");
            //        
            //    }
            __0 = __0.Where(pawn => !CompMachine.cachedMachines.TryGetValue(pawn.Drawer.renderer, out CompMachine value) || (value.Props.hoursActive >= 24000)).ToList();
            return false;
        }
    }
}

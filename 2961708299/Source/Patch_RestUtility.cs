using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.LoftBed {
    [HarmonyPatch(typeof(RestUtility), "CurrentBed")]
	[HarmonyPatch(new Type[] { typeof(Pawn), typeof(int?) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
    static class Patch_CurrentBed
    {
        static void Postfix(ref Building_Bed __result, Pawn p, ref int? sleepingSlot)
        {
            if ( __result != null || p == null || p.Map == null )
                return;

            List<Thing> things = p.Position.GetThingList(p.Map);
            List<Building_Bed> beds = new List<Building_Bed>();
            for (int i = 0; i < things.Count; i++){
                if( things[i] is Building_Bed bed ){
                    beds.Add(bed);
                }
            }

            if( beds.Count < 2 )
                return;

            foreach( Building_Bed bed in beds ){
                List<Pawn> owners = bed.OwnersForReading;
                for (int i = 0; i < owners.Count; i++){
                    if( owners[i] == p ){
                        __result = bed;
                        sleepingSlot = i;
                    }
                }
            }

            // keep original result?
        }
    }
}

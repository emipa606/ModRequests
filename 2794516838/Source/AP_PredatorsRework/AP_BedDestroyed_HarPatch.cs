using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using Verse.AI;
using System.Reflection;

namespace AP_PredatorsRework
{
    [HarmonyPatch(typeof(TaleUtility), "Notify_PawnDied")]
    public static class AP_BedDestroyed_HarPatch
    {
        [HarmonyPostfix]
        public static void Fix(Pawn victim)
        {
            if (victim.RaceProps.predator && victim.ownership.OwnedBed != null)
            {
                victim.ownership.OwnedBed.DeSpawn();
            }
        }
    }

    
}

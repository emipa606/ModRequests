using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VillageStandalone
{
    public class VillageStandaloneModExtension : DefModExtension
    {
        public bool RemoveSoakingWet = false;
        public bool RemoveSleptOutside = false;
        public bool RemoveSleptInCold = false;
        public bool RemoveSleptInHeat = false;
        public bool RemoveSleptInBarracks = false;
        public bool RemoveSleepDisturbed = false;
        public bool RemoveRemoveToxicFallout = false;
        //public bool RemoveSunlightSensitivity_Mild = false;
        public bool ideologyVillageStandaloneAssignmentAllowed = false;
        public HediffDef customHediff = null;
    }
}
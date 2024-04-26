using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace WallStuff
{
    class WallStuffSettings : ModSettings
    {
        internal static List<ThingCountExposable> listOfSpawnableThings = null;
        internal static float heaterPower = 21f;
        internal static float coolerPower = -15f;
        internal static int repairPowerUsage = 10;
        internal static int repairRateHours = 120;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref listOfSpawnableThings, "listOfSpawnableThings");
            Scribe_Values.Look(ref heaterPower, "heaterPower", 21f);
            Scribe_Values.Look(ref coolerPower, "coolerPower", -15f);
            Scribe_Values.Look(ref repairPowerUsage, "repairPowerUsage", 10);
            Scribe_Values.Look(ref repairRateHours, "repairRateHours", 24);

            // Remove all null entries in the oreDictionary
            // This is most likely due to removing a mod, which will trigger a game reset
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<ThingCountExposable> dict = new List<ThingCountExposable>();
                bool warning = false;
                for (int i = 0; i < listOfSpawnableThings.Count; i++)
                {
                    if (listOfSpawnableThings[i] != null)
                    {
                        dict.Add(new ThingCountExposable(listOfSpawnableThings[i].thingDef, listOfSpawnableThings[i].count));
                    }
                    else if (!warning)
                    {
                        warning = true;
                    }
                }
                listOfSpawnableThings = dict;
            }
        }
    }
}

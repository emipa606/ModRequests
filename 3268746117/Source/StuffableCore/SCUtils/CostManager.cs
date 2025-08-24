using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCUtils
{
    public static class CostManager
    {
        public static void UpdateCost(ThingDef thingDef, int defaultStuffCost, bool usingAltCost = false)
        {
            int newStuffCostCount;
            if (usingAltCost)
                newStuffCostCount = defaultStuffCost;
            else
                newStuffCostCount = CalculateCost(thingDef);
            thingDef.costStuffCount = newStuffCostCount > 0 ? newStuffCostCount : defaultStuffCost;
        }

        private static int CalculateCost(ThingDef thingDef)
        {
            if (thingDef.costList.NullOrEmpty())
                return 0;
            List<ThingDef> filter = new List<ThingDef>() 
            { 
                ThingDefOf.Steel 
            };
            if (thingDef.IsRangedWeapon)
                filter.Add(ThingDefOf.WoodLog);
            int newStuffCostCount = thingDef.costStuffCount;
            int filterSize = filter.Count;
            for (int i = 0; i < filterSize; i++)
            {
                ThingDef filterThingDef = filter[i];
                int index = thingDef.costList.FindIndex(j => filterThingDef.Equals(j.thingDef));
                if (index >= 0)
                {
                    ThingDefCountClass itemCost = thingDef.costList[index];
                    newStuffCostCount += itemCost.count;
                    thingDef.costList.RemoveAt(index);
                }
            }

            return newStuffCostCount;
        }
    }
}

using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class Wish_Equipment : Wish_Item
    {
        protected override int CountMatch()
        {
            int count = 0;
            List<ThingWithComps> equipment = new List<ThingWithComps>();
            if (pawn.equipment?.AllEquipmentListForReading != null) equipment.AddRange(pawn.equipment.AllEquipmentListForReading);
            if(pawn.apparel?.WornApparel != null) equipment.AddRange(pawn.apparel.WornApparel);
            for (int i = 0; i < ThingsWanted.Count; i++)
            {
                count += AdjustForSpecifiedCount(ThingMatching(equipment, ThingsWanted[i]), ThingsWanted[i].needAmount);
            }
            return count;
        }

    }
}

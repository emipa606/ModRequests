using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UniqueItems
{
    [DefOf]
    public static class UniqueItemsDefOf
    {
        public static KeyBindingDef DM_ShowAllUniqueItems;
        public static ThoughtDef DM_CraftedUniqueItem;
        public static ThoughtDef DM_LostCraftedUniqueItem;
    }
}

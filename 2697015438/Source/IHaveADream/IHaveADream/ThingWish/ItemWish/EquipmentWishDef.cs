using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class EquipmentWishDef : ItemWishDef
    {

        protected override void CompleteInfo()
        {
            for (int i = cachedItems.Count - 1; i >= 0 ; i--)
            {
                if (IsEquipable(cachedItems[i].def)) continue;
                Log.Error("HDream : ItemWishInfo of def " + cachedItems[i].def.defName + " isn't equipable, it's mandatory for thing in EquipmentWishDef. It was removed from the pool.");
                cachedItems.RemoveAt(i);
            }
            base.CompleteInfo();
        }

        protected override bool FastSearchMatch(ThingDef thing)
        {
            return base.FastSearchMatch(thing) && IsEquipable(thing);
        }

        protected virtual bool IsEquipable(ThingDef tDef)
        {
            return (tDef.comps != null && tDef.comps.Any(prop => prop.compClass == typeof(CompEquippable)))
                || typeof(Apparel).IsAssignableFrom(tDef.thingClass);
        }
    }
}

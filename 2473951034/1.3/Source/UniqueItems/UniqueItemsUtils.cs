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
    public static class UniqueItemsUtils
    {
        public static bool DestroyedOrNullOrHaveNoHolder(this Thing t)
        {
            if (t != null && t.ParentHolder is null && t.holdingOwner is null && t.Map is null)
            {
                return true;
            }
            return t?.Destroyed ?? true;
        }

        public static bool IsRare(this ThingDef def)
        {
            return UniqueItemsSettings.uniqueThingsMaxCount[def.defName] > 1;
        }

        public static string GetRareOrUniqueLabel(this ThingDef def)
        {
            var props = def?.GetCompProperties<CompProperties_UniqueItem>();
            if (props != null)
            {
                if (def.IsRare())
                {
                    return props.labelRarePrefix + def.label;
                }
                return props.labelUniquePrefix + def.label;
            }
            return "";
        }
        public static string GetRareOrUniqueLabelCap(this ThingDef def)
        {
            return GetRareOrUniqueLabel(def).CapitalizeFirst();
        }

        public static Thing GetUniqueThingFrom(Pawn p)
        {
            var apparels = p.apparel?.WornApparel;
            if (apparels != null)
            {
                for (int num = apparels.Count - 1; num >= 0; num--)
                {
                    var apparel = apparels[num];
                    var comp = apparel.TryGetComp<CompUniqueItem>();
                    if (comp != null)
                    {
                        return apparel;
                    }
                }
            }
            var equipments = p.equipment?.AllEquipmentListForReading;
            if (equipments != null)
            {
                for (int num = equipments.Count - 1; num >= 0; num--)
                {
                    var equipment = equipments[num];
                    var comp = equipment.TryGetComp<CompUniqueItem>();
                    if (comp != null)
                    {
                        return equipment;
                    }
                }
            }
            return null;
        }
    }
}

using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HDream
{
    public abstract class Wish_Item : Wish_ThingPossession<ItemWishInfo>
    {
        public new ItemWishDef Def => (ItemWishDef)def;
        protected override List<ItemWishInfo> GetThingsFromDef() => Def.Items;
        protected override ThingDef GetThingDef(ItemWishInfo thing) => thing.def;
        protected override LookMode ExposeLookModeT() => LookMode.Deep;

        protected virtual bool ThingMatching(Thing thing, ItemWishInfo match)
        {
            if(match.def != thing.def) return false;
            if (match.fromRessource != null
                    && (thing.Stuff == null || !match.fromRessource.Contains(thing.Stuff))) return false;
            if (match.neededComp != null
                && ((match.neededComp.Contains(typeof(CompQuality))
                        && (thing.TryGetComp<CompQuality>() == null || thing.TryGetComp<CompQuality>().Quality < match.minQuality))
                    || (match.neededComp.Contains(typeof(CompArt))
                        && (thing.TryGetComp<CompArt>() == null || !thing.TryGetComp<CompArt>().Active)))) return false;
            if (match.neededStats != null) for (int j = 0; j < match.neededStats.Count; j++)
                {
                    if (thing.GetStatValue(match.neededStats[j].def) < match.neededStats[j].minValue) return false;
                }
            return true;
        }
        protected override int ThingMatching(IEnumerable<Thing> things, ItemWishInfo match)
        {
            if (things.EnumerableNullOrEmpty()) return 0;
            int count = 0;
            for (int i = 0; i < things.Count(); i++)
            {
                if(ThingMatching(things.ElementAt(i), match))
                {
                    count += things.ElementAt(i).stackCount;
                    if (count >= match.needAmount) return count;
                }
            }
            return count;
        }

        public override string FormateText(string text)
        {
            if (Def.roomRole != null) text = text.Replace(Def.role_Key, Def.roomRole.label);
            return base.FormateText(text);
        }
        protected override string FormateListThing(List<ItemWishInfo> things)
        {
            if (things.NullOrEmpty()) return base.FormateListThing(things);
            string listingDefaultParam = "";
            string listingOwnParam = "";
            string elemSeparator = ", ";
            string bbSeparator = "\n";

            for (int i = 0; i < things.Count; i++)
            {
                if (SimilareToDefault(things[i]))
                {
                    if(listingDefaultParam == "") listingDefaultParam += ParamFor(things[i], elemSeparator) + " : \n";
                    listingDefaultParam += things[i].def.label + elemSeparator;
                }
                else
                {
                    listingOwnParam += things[i].def.label + ParamFor(things[i], elemSeparator) + bbSeparator;
                }
            }
            if (listingDefaultParam.EndsWith(elemSeparator)) listingDefaultParam = listingDefaultParam.Substring(0, listingDefaultParam.Length - elemSeparator.Length);
            if (listingOwnParam.EndsWith(bbSeparator)) listingOwnParam = listingOwnParam.Substring(0, listingOwnParam.Length - bbSeparator.Length);

            if (listingDefaultParam != "") 
            {
                if (listingOwnParam != "") listingDefaultParam += "\n\n" + listingOwnParam;
            }
            else listingDefaultParam += listingOwnParam;
            return listingDefaultParam;
        }
        private string ParamFor(ItemWishInfo info, string elemSeparator)
        {
            string neededSeparator = " // ";
            string minSymbole = " >= ";

            string paramDesc = "(x" + info.needAmount + ") (";
            if (!info.neededComp.NullOrEmpty()) for (int j = 0; j < info.neededComp.Count; j++)
                {
                    paramDesc += info.neededComp[j].Name.Substring("Comp".Length);
                    if (info.neededComp[j] == typeof(CompQuality)) paramDesc += minSymbole + info.minQuality.GetLabel();
                    paramDesc += (j != info.neededComp.Count - 1) ? elemSeparator : neededSeparator;
                }
            if (!info.neededStats.NullOrEmpty()) for (int j = 0; j < info.neededStats.Count; j++)
                {
                    paramDesc += info.neededStats[j].def.LabelCap + minSymbole + info.neededStats[j].minValue;
                    paramDesc += (j != info.neededStats.Count - 1) ? elemSeparator : neededSeparator;
                }
            if (!info.fromRessource.NullOrEmpty()) for (int j = 0; j < info.fromRessource.Count; j++)
                {
                    paramDesc += info.fromRessource[j].LabelAsStuff;
                    paramDesc += (j != info.fromRessource.Count - 1) ? elemSeparator : neededSeparator;
                }
            if (paramDesc.EndsWith(neededSeparator)) paramDesc = paramDesc.Substring(0, paramDesc.Length - neededSeparator.Length);
            paramDesc += ")";
            return paramDesc;
        }

        protected bool SimilareToDefault(ItemWishInfo info)
        {
            return Def.findPossibleWant && (Def.includedThing == null 
                                            || !Def.includedThing.Any(inc => inc.def == info.def)
                                            || (info.fromRessource.Count == Def.fromRessource.Count
                                                && !info.fromRessource.Any(def => !Def.fromRessource.Contains(def))
                                                && info.neededComp.Count == Def.neededComp.Count
                                                && !info.neededComp.Any(comp => !Def.neededComp.Contains(comp))
                                                && info.neededStats.Count == Def.neededStats.Count
                                                && !info.neededStats.Any(neededStats => !Def.neededStats.Any(defstat => defstat.def == neededStats.def && defstat.minValue == neededStats.minValue))
                                                && info.needAmount == Def.specificAmount
                                                && info.minQuality == Def.minQuality));
        }

    }
}

using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HDream
{
    public class ItemWishDef : ThingWishDef
    {
        public List<ItemWishInfo> includedThing;

        public QualityCategory minQuality = QualityCategory.Awful;
        public List<ItemNeededStat> neededStats;

        public List<ThingDef> fromRessource;

        public WishItemStuffSetting stuffSetting;

        public RoomRoleDef roomRole;
        public bool shoulBeRoomOwner = false;

        public List<ThingCategoryDef> fromCategory;

        protected List<ItemWishInfo> cachedItems = null;

        public string role_Key = "{Role}";

        public List<ItemWishInfo> Items
        {
            get
            {
                if (cachedItems == null)
                {
                    cachedItems = includedThing ?? new List<ItemWishInfo>();
                    if(stuffSetting != null)
                    {
                        if (fromRessource == null) fromRessource = new List<ThingDef>();
                        fromRessource.AddRange(stuffSetting.MatchingStuffs());
                        if (fromRessource.Count == 0) Log.Error("HDream : default stuffSetting for " + defName + " don't contain any matching stuff");
                        fromRessource.RemoveDuplicates();
                    }
                    CompleteInfo();
                    if (findPossibleWant) CacheData(SearchedDef);
                }
                return cachedItems;
            }
        }

        protected override bool FastSearchMatch(ThingDef thing)
        {
            return base.FastSearchMatch(thing) && (includedThing == null || includedThing.Find(info => info.def == thing) == null);
        }

        protected override bool LongSearchMatch(ThingDef thing)
        {
            if (!base.LongSearchMatch(thing)) return false;
            if (!fromCategory.NullOrEmpty())
            {
                if (thing.thingCategories.NullOrEmpty() || !thing.thingCategories.Exists(cat => fromCategory.Contains(cat))) 
                    return false;
            }
            if (!fromRessource.NullOrEmpty())
            {
                if (!fromRessource.Exists(ress => ress.stuffProps.CanMake(thing))) return false;
            }
            if (!neededStats.NullOrEmpty())
            {
                if (thing.statBases.NullOrEmpty()) return false;
                QualityCategory quality;
                List<ThingDef> stuffs;
                for (int i = 0; i < neededStats.Count; i++)
                {
                    if (!thing.statBases.StatListContains(neededStats[i].def)) return false;
                    quality = thing.HasComp(typeof(CompQuality)) ? QualityCategory.Legendary : QualityCategory.Normal;
                    if (thing.MadeFromStuff)
                    {
                        stuffs = fromRessource.NullOrEmpty() ? 
                            DefDatabase<ThingDef>.AllDefsListForReading.FindAll(def => def.IsStuff && def.stuffProps.CanMake(thing)) :
                            fromRessource.FindAll(def => def.stuffProps.CanMake(thing));
                        for (int j = 0; j < stuffs.Count; j++)
                        {
                            if(neededStats[i].def.Worker.GetValue(StatRequest.For(thing, stuffs[i], quality)) < neededStats[i].minValue) return false;
                        }
                    }
                    else if(neededStats[i].def.Worker.GetValue(StatRequest.For(thing, null, quality)) < neededStats[i].minValue) return false;
                }
            }
            return true;
        }

        protected virtual void CompleteInfo()
        {
            for (int i = 0; i < cachedItems.Count; i++)
            {
                if (cachedItems[i].useDefaultParam)
                {
                    if (cachedItems[i].needAmount == -1) cachedItems[i].needAmount = specificAmount;
                    if (cachedItems[i].minQuality == (QualityCategory)100) cachedItems[i].minQuality = minQuality;
                    if (cachedItems[i].neededComp == null) cachedItems[i].neededComp = neededComp;
                    if (cachedItems[i].neededStats == null) cachedItems[i].neededStats = neededStats;
                    if (cachedItems[i].fromRessource == null) cachedItems[i].fromRessource = fromRessource;
                }
            }
        }
        protected override void CacheData(List<ThingDef> defs)
        {
            if (defs.NullOrEmpty()) 
            { 
                Log.Message("HDream : no def found to complete infos for " + defName);
                return;
            }
            for (int i = 0; i < defs.Count; i++)
            {
                cachedItems.Add(new ItemWishInfo
                {
                    def = defs[i],
                    needAmount = specificAmount,
                    minQuality = minQuality,
                    neededComp = neededComp,
                    neededStats = neededStats,
                    fromRessource = fromRessource
                });
            }
        }
    }
}

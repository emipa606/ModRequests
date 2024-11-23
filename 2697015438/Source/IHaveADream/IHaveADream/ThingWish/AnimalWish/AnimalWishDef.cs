using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace HDream
{
    public class AnimalWishDef : ThingWishDef
    {

        public List<AnimalWishInfo> includedAnimal;

        public List<ThingDef> leathers;
        public FloatRange? petnessRange;
        public FloatRange? wildnessRange;
        public FloatRange? baseBodySizeRange;
        public FloatRange? trainabilityRange;
        public FloatRange? lifeExpectancyRange;
        public FloatRange? baseMarketValueRange;

        public bool shouldBeNuzzler = false;
        public bool shouldBePredator = false;
        public bool shouldBePack = false;


        public bool shouldBeExotic = false;

        public bool shouldBeBonded = false;
        public List<int> includedStage;


        private List<AnimalWishInfo> cachedAnimals = null;

        public List<AnimalWishInfo> Animals
        {
            get
            {
                if (cachedAnimals == null)
                {
                    cachedAnimals = includedAnimal ?? new List<AnimalWishInfo>();
                    CompleteInfo();
                    if (findPossibleWant) CacheData(SearchedDef);
                }
                return cachedAnimals;
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
                cachedAnimals.Add(new AnimalWishInfo
                {
                    animal = defs[i],
                    amount = specificAmount,
                    shouldBeBonded = shouldBeBonded,
                    includedStage = includedStage
                });
            }
        }

        protected override bool FastSearchMatch(ThingDef thing)
        {
            return base.FastSearchMatch(thing)
                && thing.race != null
                && thing.race.Animal
                && (!shouldBePredator || thing.race.predator)
                && (!shouldBePack || thing.race.packAnimal)
                && (!shouldBeNuzzler || thing.race.nuzzleMtbHours > 0)
                && (leathers == null || leathers.Contains(thing.race.leatherDef))
                && (petnessRange?.Includes(thing.race.petness) ?? true)
                && (wildnessRange?.Includes(thing.race.wildness) ?? true)
                && (baseBodySizeRange?.Includes(thing.race.baseBodySize) ?? true)
                && (trainabilityRange?.Includes(thing.race.trainability.index) ?? true)
                && (lifeExpectancyRange?.Includes(thing.race.lifeExpectancy) ?? true)
                && (baseMarketValueRange?.Includes(thing.BaseMarketValue) ?? true);
        }

        protected virtual void CompleteInfo()
        {
            for (int i = 0; i < cachedAnimals.Count; i++)
            {
                if (cachedAnimals[i].amount == -1) cachedAnimals[i].amount = specificAmount;
            }
        }

    }
}

using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace HDream
{
    public abstract class ThingWishDef : WishDef
    {

        public List<ThingDef> excludedThing;

        public bool findPossibleWant = false;

        public List<Type> neededComp;

        public int specificAmount = 1;
        public bool countAmountPerInfo = false;


        public string countRule_Key = "{CountRule}";

        protected abstract void CacheData(List<ThingDef> defs);

        protected virtual List<ThingDef> SearchedDef
        {
            get
            {
                List<ThingDef> thingDefs = new List<ThingDef>();
                List<ThingDef> allThing = DefDatabase<ThingDef>.AllDefsListForReading;

                for (int i = 0; i < allThing.Count; i++)
                {
                    if (FastSearchMatch(allThing[i]) && LongSearchMatch(allThing[i])) thingDefs.Add(allThing[i]);
                }
                return thingDefs;
            }
        }
        protected virtual bool FastSearchMatch(ThingDef thing)
        {
            return (excludedThing.NullOrEmpty() || !excludedThing.Contains(thing));
        }

        protected virtual bool LongSearchMatch(ThingDef thing)
        {
            if (!neededComp.NullOrEmpty())
            {
                if (thing.comps.NullOrEmpty() || thing.comps.FindAll(comp => neededComp.Contains(comp.compClass)).Count < neededComp.Count) 
                    return false;
            }
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            List<string> enume = new List<string>();
            if (neededComp != null)
            {
                neededComp.RemoveAll(delegate (Type type) {
                    if (type != typeof(ThingComp) && typeof(ThingComp).IsAssignableFrom(type)) return false;
                    enume.Add("HDream : wrong type (" + type.ToString() + ") added in neededComp, it was removed. You should only add type inherited from ThingComp");
                    return true;
                });
            }
            foreach (string error in enume.AsEnumerable()) yield return error;
        }


    }
}

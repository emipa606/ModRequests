using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCCaching
{
    public static class CacheUtil
    {

        public static bool AddToCache(string key, ThingDef value, Dictionary<string, List<ThingDef>> cache)
        {
            if (cache == null)
                cache = new Dictionary<string, List<ThingDef>>();
            List<ThingDef> newList = new List<ThingDef>();
            newList.Add(value);
            if(cache.TryGetValue(key, out List<ThingDef> oldValues))
                oldValues.ForEach(x => newList.Add(x));
            cache.SetOrAdd(key, newList);
            return true;
        }

        public static bool AddToCache(string key, List<ThingDef> value, Dictionary<string, List<ThingDef>> cache)
        {
            if (cache == null)
                cache = new Dictionary<string, List<ThingDef>>();
            cache.SetOrAdd(key, value);
            return true;
        }


        public static bool AddToCache(string key, ThingDef value, Dictionary<string, ThingDef> cache)
        {
            if (cache == null)
                cache = new Dictionary<string, ThingDef>();
            cache.SetOrAdd(key, value);
            return true;
        }

        public static bool GetFromCache(string key, out ThingDef value, Dictionary<string, ThingDef> cache)
        {
            if (cache == null)
            {
                cache = new Dictionary<string, ThingDef>();
                value = null;
                return false;
            }
            return cache.TryGetValue(key, out value);
        }

        public static bool GetFromCache(string key, out List<ThingDef> value, Dictionary<string, List<ThingDef>> cache)
        {
            if (cache == null)
            {
                cache = new Dictionary<string, List<ThingDef>>();
                value = null;
                return false;
            }
            return cache.TryGetValue(key, out value);
        }

        public static bool GetFromCache(string key, out List<string> value, Dictionary<string, List<ThingDef>> cache)
        {
            if (cache == null)
            {
                cache = new Dictionary<string, List<ThingDef>>();
                value = null;
                return false;
            }
            bool found = cache.TryGetValue(key, out List<ThingDef> newValue) && newValue != null;
            if(found)
                value = newValue.ToDictionary(i => i.defName, i => newValue).Keys.ToList();
            else
                value = null;
            return found;
        }
    }
}

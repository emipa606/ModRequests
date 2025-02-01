using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;

namespace LostTechnology
{

    public static class dataUtility
    {
        public static readonly Dictionary<Site, siteData> dic_site = new Dictionary<Site, siteData>();
        public static siteData GetData(Site key)
        {
            if (!dic_site.ContainsKey(key)) dic_site[key] = new siteData();
            dic_site[key].setParent(key);
            return dic_site[key];
        }
    }

}

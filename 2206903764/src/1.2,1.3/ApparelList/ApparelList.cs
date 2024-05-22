using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using System.Reflection.Emit;
using AlienRace;
using Garam_RaceAddon;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace TakeOffIndoor
{
    public class BodyPartGroupComparere : IComparer<BodyPartGroupDef>

    {
        public int Compare(BodyPartGroupDef x, BodyPartGroupDef y)
        {
            if (x.listOrder != y.listOrder)
            {
                return y.listOrder - x.listOrder;
            }
            if (x.index != y.index)
            {
                return x.index - y.index;
            }
            return x.shortHash - y.shortHash;
        }
    }

    public class ApparelLayerDefComparer : IComparer<ApparelLayerDef>
    {

        public int Compare(ApparelLayerDef x, ApparelLayerDef y)
        {
            if (x.drawOrder != y.drawOrder)
            {
                return x.drawOrder - y.drawOrder;
            }
            if (x.index != y.index)
            {
                return x.index - y.index;
            }
            return x.shortHash - y.shortHash;
        }
    }

    public class ApparelGroup
    {
        public SortedSet<BodyPartGroupDef> bodyPartGroupSorted;
        public BodyPartGroupDef firstBodyPartGroup;
        public BodyPartGroupDef maxBodyGroup;
        public SortedSet<ApparelLayerDef> layerListSorted;
        public ApparelLayerDef lastLayer;

        public int layerHash;
        public int bodyHash;
        public string hashKey;

        public ApparelGroup(ThingDef ap)
        {

            firstBodyPartGroup = ap.apparel.bodyPartGroups[0];

            bodyPartGroupSorted = GetSortedBodyList(ap.apparel.bodyPartGroups);

            maxBodyGroup = bodyPartGroupSorted.ToList()[0];

            layerListSorted = GetSortedApparelList(ap.apparel.layers);
            lastLayer = ap.apparel.LastLayer;

            layerHash = GetHash(layerListSorted);
            bodyHash = GetHash(bodyPartGroupSorted);

            hashKey = layerHash.ToString() + bodyHash.ToString();
        }

        public static SortedSet<ApparelLayerDef> GetSortedApparelList(List<ApparelLayerDef> layers)
        {
            SortedSet<ApparelLayerDef> ret = new SortedSet<ApparelLayerDef>(new ApparelLayerDefComparer());

            foreach (ApparelLayerDef ald in layers)
            {
                ret.Add(ald);
            }

            return ret;
        }

        public static SortedSet<BodyPartGroupDef> GetSortedBodyList(List<BodyPartGroupDef> bodies)
        {
            SortedSet<BodyPartGroupDef> ret = new SortedSet<BodyPartGroupDef>(new BodyPartGroupComparere());

            foreach (BodyPartGroupDef bpg in bodies)
            {
                ret.Add(bpg);
            }

            return ret;
        }


        public static int GetHash(SortedSet<ApparelLayerDef> list)
        {
            string str = "";
            foreach (ApparelLayerDef def in list)
            {
                str += def.defName;
            }
            //debug.WriteLine(str+ ":"+str.GetHashCode().ToString());
            return str.GetHashCode();
        }
        public static int GetHash(SortedSet<BodyPartGroupDef> list)
        {
            string str = "";
            foreach (BodyPartGroupDef def in list)
            {
                str += def.defName;
            }
            return str.GetHashCode();
        }

        public static string GetHash(List<ApparelLayerDef> layers, List<BodyPartGroupDef> bodies)
        {
            return GetHash(GetSortedApparelList(layers)).ToString() + GetHash(GetSortedBodyList(bodies)).ToString();
        }

    }

    public class ApparelListRecord
    {
        public ThingDef thing;
        public ApparelGroup group;

        public ApparelListRecord(ThingDef ap)
        {
            thing = ap;

            group = new ApparelGroup(ap);
        }

        public override string ToString()
        {
            string ret = "";
            bool first = true;
            foreach (ApparelLayerDef ald in group.layerListSorted)
            {
                if (first)
                {
                    ret += ald.defName;
                    first = false;
                }
                else
                {
                    ret += "," + ald.defName;
                }
            }
            ret += "\t";

            first = true;
            foreach (BodyPartGroupDef bpg in group.bodyPartGroupSorted)
            {
                if (first)
                {
                    ret += bpg.defName;
                    first = false;
                }
                else
                {
                    ret += "," + bpg.defName;
                }
            }

            ret += "\t" + thing.defName + "\t" + thing.label;

            return ret;
        }

        public bool IsInvisible
        {
            get
            {
                return ApparelList.IsInvisible(thing);
            }
            set
            {
                ApparelList.SetInvisible(thing, value);
            }
        }
    }
    public class ApparelList
    {
        public static List<ApparelListRecord> list = new List<ApparelListRecord>();

        public static Dictionary<ThingDef, ApparelListRecord> apparelToRecord = new Dictionary<ThingDef, ApparelListRecord>();
        public static Dictionary<string, ApparelListRecord> defNameToRecord = new Dictionary<string, ApparelListRecord>();
        public static Dictionary<ThingDef, bool> apparelToInvisible = new Dictionary<ThingDef, bool>();

        public static void MakeDic(List<string> defList)
        {
            IEnumerable<ThingDef> apparels = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel);
            foreach (ThingDef ap in apparels)
            {
                ApparelListRecord ald = new ApparelListRecord(ap);
                list.Add(ald) ;
                apparelToRecord.Add(ap, ald);
                defNameToRecord.Add(ap.defName, ald);
                apparelToInvisible.Add(ap, false);
            }

            foreach(string defName in defList)
            {
                if(defNameToRecord.TryGetValue(defName, out ApparelListRecord ald))
                {
                    ald.IsInvisible = true;
                }
            }
        }

        public static bool IsInvisible(ThingDef ap)
        {
            if(apparelToInvisible.TryGetValue(ap,out bool invisible))
            {
                return invisible;
            }
            return false;
        }
        public static void SetInvisible(ThingDef ap,bool invisible)
        {
            if (apparelToInvisible.ContainsKey(ap))
            {
                apparelToInvisible[ap] = invisible;
            }
        }

        public static List<string> InvisibleDefNameList
        {
            get
            {
                List<string> defNameList = new List<string>();
                foreach(ApparelListRecord ald in list.Where(x => x.IsInvisible))
                {
                    defNameList.Add(ald.thing.defName);
                }

                return defNameList;
            }
        }
    }
}

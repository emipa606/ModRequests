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

    public class ApparelGroupComparer : IComparer<ApparelGroup>
    {
        public static int _Compare(ApparelGroup x, ApparelGroup y, List<BodyPartGroupDef> xBody, List<BodyPartGroupDef> yBody)
        {
            if (x.layerHash != y.layerHash)
            {
                if (x.layerListSorted.Count != y.layerListSorted.Count)
                {
                    return y.layerListSorted.Count - x.layerListSorted.Count;
                }
                if (x.lastLayer.drawOrder != y.lastLayer.drawOrder)
                {
                    return y.lastLayer.drawOrder - x.lastLayer.drawOrder;
                }
                if (x.lastLayer.index != y.lastLayer.index)
                {
                    return x.lastLayer.index - y.lastLayer.index;
                }
            }

            if (x.bodyHash != y.bodyHash)
            {
                if (xBody.Count != yBody.Count)
                {
                    return yBody.Count - xBody.Count;
                }

                if (x.maxBodyGroup.listOrder != y.maxBodyGroup.listOrder)
                {
                    return x.maxBodyGroup.listOrder - y.maxBodyGroup.listOrder;
                }
                if (x.maxBodyGroup.index != y.maxBodyGroup.index)
                {
                    return x.maxBodyGroup.index - y.maxBodyGroup.index;
                }

                return x.bodyHash - y.bodyHash;
            }
            else
            {
                for (int i = 0; i < x.layerListSorted.Count; i++)
                {
                    ApparelLayerDef a = x.layerListSorted.ToArray()[i], b = y.layerListSorted.ToArray()[i];
                    if (a != b)
                    {
                        if (a.drawOrder != b.drawOrder)
                        {
                            return b.drawOrder - a.drawOrder;
                        }
                    }
                }
                return x.layerHash - y.layerHash;
            }

        }
        public int Compare(ApparelGroup x, ApparelGroup y)
        {
            return _Compare(x, y, x.bodyPartGroupSorted.ToList(), y.bodyPartGroupSorted.ToList());
        }
    }

    public class ApparelGroup
    {
        public SortedSet<ApparelLayerDef> layerListSorted;
        public ApparelLayerDef lastLayer;

        public SortedSet<BodyPartGroupDef> bodyPartGroupSorted;
        public BodyPartGroupDef firstBodyPartGroup;
        public BodyPartGroupDef maxBodyGroup;

        public int layerHash;
        public int bodyHash;
        public string hashKey;

        public static SortedSet<ApparelGroup> groupList=new SortedSet<ApparelGroup>(new ApparelGroupComparer());
        public static Dictionary<string, ApparelGroup> hashToGroup = new Dictionary<string, ApparelGroup>();

        public ApparelGroup(ThingDef ap)
        {

            firstBodyPartGroup = ap.apparel.bodyPartGroups.FirstOrDefault();

            bodyPartGroupSorted = GetSortedBodyList(ap.apparel.bodyPartGroups);

            maxBodyGroup = bodyPartGroupSorted.ToList()[0];

            layerListSorted = GetSortedApparelList(ap.apparel.layers);
            lastLayer = ap.apparel.LastLayer;

            layerHash = GetHash(layerListSorted);
            bodyHash = GetHash(bodyPartGroupSorted);

            hashKey = layerHash.ToString() + bodyHash.ToString();

            if (!hashToGroup.ContainsKey(hashKey))
            {
                hashToGroup.Add(hashKey, this);
                groupList.Add(this);
            }
        }
        public ApparelGroup() { }

        public static ApparelGroup NewOrContains(ThingDef ap) //インスタンスを無駄に作らない様にするため
        {
            SortedSet<ApparelLayerDef> sl = GetSortedApparelList(ap.apparel.layers);
            SortedSet<BodyPartGroupDef> sb = GetSortedBodyList(ap.apparel.bodyPartGroups);
            int lh = GetHash(sl);
            int bh = GetHash(sb);
            string hk = lh.ToString() + bh.ToString();

            if (!hashToGroup.ContainsKey(hk))
            {
                ApparelGroup ag = new ApparelGroup();
                if (ap.apparel.bodyPartGroups.Count == 0)
                {
                    //debug.WriteLine("noc-3-1-1 nb:"+ap.defName);
                    ag.firstBodyPartGroup = BodyPartGroupDefOf.Torso; //無ければ胴体扱いにする
                    ag.bodyPartGroupSorted = sb;
                    ag.maxBodyGroup = BodyPartGroupDefOf.Torso;
                }
                else
                {
                    ag.firstBodyPartGroup = ap.apparel.bodyPartGroups[0];
                    ag.bodyPartGroupSorted = sb;
                    ag.maxBodyGroup = sb.ToList()[0];
                }
                ag.layerListSorted = sl;
                ag.lastLayer = ap.apparel.LastLayer;

                ag.layerHash = lh;
                ag.bodyHash = bh;
                ag.hashKey = hk;

                hashToGroup.Add(hk, ag);
                groupList.Add(ag);
                return ag;
            }
            else
            {
                return hashToGroup[hk];
            }
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

        public override string ToString()
        {
            return ToString(",","\t");
        }
        public string ToString(string delimiter = ",", string delimiter2 = "\t", bool bracketsLayer = false, bool bracketsBody = false)
        {
            string ret = "";
            bool first = true;
            if (bracketsLayer) ret += "(";
            foreach (ApparelLayerDef ald in layerListSorted)
            {
                if (first)
                {
                    ret += ald.label;
                    first = false;
                }
                else
                {
                    ret += delimiter + ald.label;
                }
            }
            if (bracketsLayer) ret += ")";
            ret += delimiter2;

            if (bracketsBody) ret += "(";
            first = true;
            foreach (BodyPartGroupDef bpg in bodyPartGroupSorted)
            {
                if (first)
                {
                    ret += bpg.label;
                    first = false;
                }
                else
                {
                    ret += delimiter + bpg.label;
                }
            }
            if (bracketsBody) ret += ")";

            return ret;
        }
    }

    public class ApparelListRecord
    {
        public ThingDef thing;
        public ApparelGroup group;
        public Texture tex;

        public ApparelListRecord(ThingDef ap)
        {
            thing = ap;

            //group = new ApparelGroup(ap);
            group = ApparelGroup.NewOrContains(ap);
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
                if(ApparelList.apparelToVisible.TryGetValue(thing,out VisibleMode visi))
                {
                    return visi == VisibleMode.Invisible;
                }
                return false;
            }
        }
        public bool IsNever
        {
            get
            {
                if (ApparelList.apparelToVisible.TryGetValue(thing, out VisibleMode visi))
                {
                    return visi == VisibleMode.Never;
                }
                return false;
            }
        }
        public bool IsVisible
        {
            get
            {
                if (ApparelList.apparelToVisible.TryGetValue(thing, out VisibleMode visi))
                {
                    return visi == VisibleMode.Visible;
                }
                return false;
            }
        }
        public bool IsForce
        {
            get
            {
                if (ApparelList.apparelToVisible.TryGetValue(thing, out VisibleMode visi))
                {
                    return visi == VisibleMode.Force;
                }
                return false;
            }
        }

        public void SetInvisible()
        {
            ApparelList.SetVisible(thing,VisibleMode.Invisible);
        }
        public void SetNever()
        {
            ApparelList.SetVisible(thing, VisibleMode.Never);
        }
        public void SetVisible()
        {
            ApparelList.SetVisible(thing, VisibleMode.Visible);
        }
        public void SetForce()
        {
            ApparelList.SetVisible(thing, VisibleMode.Force);
        }

        public VisibleMode Visible
        {
            get
            {
                if (CustomApparelLayerDictionary.DefsModeDic.TryGetValue(thing, out VisibleMode visi))
                {
                    return visi;
                }
                else
                {
                    return VisibleMode.None;
                }
            }
            set
            {
                ApparelList.SetVisible(thing, value);
            }
        }

        public VisibleMode VisibleDrafted
        {
            get
            {
                if (ApparelList.apparelToVisible.TryGetValue(thing, out VisibleMode visi))
                {
                    return visi;
                }
                else
                {
                    return VisibleMode.None;
                }
            }
            set
            {
                ApparelList.SetVisible(thing, value);
            }
        }



    }
    public class ApparelList
    {
        public static List<ApparelListRecord> list = new List<ApparelListRecord>();

        public static Dictionary<ThingDef, ApparelListRecord> apparelToRecord = new Dictionary<ThingDef, ApparelListRecord>();
        public static Dictionary<string, ApparelListRecord> defNameToRecord = new Dictionary<string, ApparelListRecord>();
        public static Dictionary<ThingDef, VisibleMode> apparelToVisible = new Dictionary<ThingDef, VisibleMode>(); //0なし 1非表示 2強制表示

        static bool texGot=false;

        public static int Count
        {
            get
            {
                return list.Count;
            }
        }
        //public static void MakeDic(Dictionary<string,VisibleMode> defList)
        //{
        //    IEnumerable<ThingDef> apparels = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel);
        //    foreach (ThingDef ap in apparels)
        //    {
        //        ApparelListRecord alr = new ApparelListRecord(ap);
        //        list.Add(alr) ;
        //        apparelToRecord.Add(ap, alr);
        //        defNameToRecord.Add(ap.defName, alr);
        //        apparelToVisible.Add(ap, 0);
        //    }

        //    if (defList != null)
        //    {
        //        foreach (KeyValuePair<string, VisibleMode> d in defList)
        //        {
        //            if (defNameToRecord.TryGetValue(d.Key, out ApparelListRecord alr))
        //            {
        //                    alr.Visible = d.Value;
        //            }
        //        }
        //    }
        //}
        public static void MakeDic()
        {
            IEnumerable<ThingDef> apparels = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel);
            foreach (ThingDef ap in apparels)
            {
                ApparelListRecord alr = new ApparelListRecord(ap);
                list.Add(alr);
                apparelToRecord.Add(ap, alr);
                defNameToRecord.Add(ap.defName, alr);
                apparelToVisible.Add(ap, 0);
            }

        }

        public static void MakeTexListOnce()
        {
            if (!texGot)
            {
                foreach(ApparelListRecord alr in list)
                {
                    alr.tex = alr.thing.graphic.MatAt(alr.thing.defaultPlacingRot, null).mainTexture;
                }

                texGot = true;
            }
        }

        public static void SetVisible(ThingDef ap, VisibleMode visible)
        {
            CustomApparelLayerDictionary.DefsModeDic[ap] = visible;
        }

    }
}

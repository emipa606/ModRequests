using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace UnderWhere
{

    public class UWPawn_ApparelTracker : IThingHolder
    {
        public UWPawn_ApparelTracker(Pawn pawn)
        {
            this.pawn = pawn;
            this.wornApparel = new ThingOwner<Apparel>(this);
        }
        public IThingHolder ParentHolder
        {
            get
            {
                return this.pawn;
            }
        }
        public List<Apparel> WornApparel
        {
            get
            {
                return this.wornApparel.InnerListForReading;
            }
        }
        public int WornApparelCount
        {
            get
            {
                return this.wornApparel.Count;
            }
        }
        public bool CheckUnderwear
        {
            get
            {
                if (this.pawn.gender == Gender.None)
                {
                    return false;
                }
                bool flag;
                bool flag2;
                this.HasBasicApparel(out flag, out flag2);
                if (!flag)
                {
                    bool flag3 = false;
                    foreach (BodyPartRecord bodyPartRecord in this.pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null))
                    {
                        if (bodyPartRecord.IsInGroup(BodyPartGroupDefOf.Legs))
                        {
                            flag3 = true;
                            break;
                        }
                    }
                    if (!flag3)
                    {
                        flag = true;
                    }
                }
                if (this.pawn.gender == Gender.Male)
                {
                    return !flag;
                }
                return this.pawn.gender == Gender.Female && (!flag || !flag2);
            }
        }
        public bool Contains(Thing apparel)
        {
            return this.wornApparel.Contains(apparel);
        }
        public void HasBasicApparel(out bool hasUndies, out bool hasBra)
        {
            hasBra = false;
            hasUndies = false;
            for (int i = 0; i < this.wornApparel.Count; i++)
            {
                Apparel apparel = this.wornApparel[i];

                for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
                {
                    if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
                    {
                        List<string> list = apparel.def.apparel.tags;
                        if ((list != null) && (list.Count > 0))
                        {
                            foreach (string tag in list)
                            {
                                if (tag.Equals("OnlyGender_female"))
                                {
                                    hasBra = true;
                                }
                            }
                        }
                    }
                    if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Legs)
                    {
                        hasUndies = true;
                    }
                    if (hasBra && hasUndies)
                    {
                        return;
                    }
                }
            }
        }
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.wornApparel;
        }
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
        public Pawn pawn;
        private ThingOwner<Apparel> wornApparel;
        private static List<Apparel> tmpApparelList = new List<Apparel>();
        private static List<Apparel> tmpApparel = new List<Apparel>();
    }
}

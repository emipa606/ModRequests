using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HDream
{
    public class Wish_Hediff : WishWithComp
    {

        public HediffWishDef Def => (HediffWishDef)def;

        public uint AmountNeeded => (uint)Def.amountNeeded;

        protected List<HediffWishInfo> hediffsNeeded = new List<HediffWishInfo>();
        public List<HediffWishInfo> HediffsNeeded => hediffsNeeded;


        protected List<HediffDef> hediffToGetRid;
        public List<HediffDef> HediffToGetRid => hediffToGetRid;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref doAtTick, "lastTick", 0);
            Scribe_Values.Look(ref actualCount, "hediffCount", 0);
            Scribe_Collections.Look(ref hediffsNeeded, "hediffsNeeded", LookMode.Deep);
            Scribe_Collections.Look(ref hediffToGetRid, "hediffToGetRid");
        }
        public override void PostMake()
        {
            base.PostMake();
            List<HediffWishInfo> hediffWishInfos = PrimeHediffInfo();

            if (hediffWishInfos.Count == 0 && hediffToGetRid.NullOrEmpty())
            {
                MakeFailed();
                return;
            }
            if (Def.wantSpecific)
            {
                if (def.tryPreventSimilare)
                {
                    List<HediffDef> similareHediff = new List<HediffDef>();
                    List<Wish> wishes = pawn.wishes().wishes;
                    for (int i = 0; i < wishes.Count; i++)
                    {
                        if (wishes[i].def == def)
                        {
                            if (hediffWishInfos.Where(info => info.def == (wishes[i] as Wish_Hediff).HediffsNeeded[0].def).Any())
                                    similareHediff.Add((wishes[i] as Wish_Hediff).HediffsNeeded[0].def);
                            HediffRidRemoveFromSpecificWish(wishes[i] as Wish_Hediff);
                        }
                    }
                    if (similareHediff.Count < hediffWishInfos.Count) for (int i = 0; i < similareHediff.Count; i++) hediffWishInfos.RemoveAll(info => info.def == similareHediff[i]);
                }
                if (hediffWishInfos.Count > 0)
                {
                    if (!hediffToGetRid.NullOrEmpty())
                    {
                        if (Rand.Value < 0.5f)
                        {
                            hediffsNeeded.Add(hediffWishInfos[Mathf.FloorToInt(Rand.Value * hediffWishInfos.Count)]);
                            HediffRidNegate();
                        }
                        else
                        {
                            HediffRidKeepOnly(Mathf.FloorToInt(Rand.Value * hediffToGetRid.Count));
                            hediffsNeeded = null;
                        }
                    }
                    else hediffsNeeded.Add(hediffWishInfos[Mathf.FloorToInt(Rand.Value * hediffWishInfos.Count)]);
                }
                else HediffRidKeepOnly(Mathf.FloorToInt(Rand.Value * hediffToGetRid.Count));
            }
            else hediffsNeeded = hediffWishInfos;

            if(!Def.countPreWishProgress) actualCount = CountMatch();
        }
        protected virtual void HediffRidKeepOnly(int index)
        {
            HediffDef hed = hediffToGetRid[index];
            hediffToGetRid.Clear();
            hediffToGetRid.Add(hed);
        }
        protected virtual void HediffRidRemoveFromSpecificWish(Wish_Hediff wish)
        {
            hediffToGetRid.Remove(wish.HediffToGetRid[0]);
        }
        protected virtual void HediffRidNegate()
        {
            hediffToGetRid = null;
        }
        protected virtual List<HediffWishInfo> PrimeHediffInfo()
        {
            return new List<HediffWishInfo>(Def.CompleteInfos);
        }
        public override void Tick()
        {
            base.Tick();
            TickToResolve();
        }

        protected override void CheckResolve()
        {
            base.CheckResolve();
            int newCount = CountMatch();
            CountProgressStep(ref actualCount, newCount);
            if (newCount >= AmountNeeded) OnFulfill();
        }
        protected override int CountMatch()
        {
            int count = 0;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;

            if (hediffsNeeded != null)
            {
                for (int i = 0; i < hediffsNeeded.Count; i++)
                {
                    for (int j = 0; j < hediffs.Count; j++)
                    {
                        if (MatchingNeededAtIndex(i, hediffs[j]))
                        {
                            count++;
                            if (!hediffsNeeded[i].countAllMatch) break;
                        }
                    }
                }
            }
            if (hediffToGetRid != null)
            {
                for (int i = 0; i < hediffToGetRid.Count; i++)
                {
                    if(DidGetRidAtIndex(i, hediffs.FindAll(hed => hed.def == hediffToGetRid[i])))
                    {
                        count++;
                        if (!hediffsNeeded[i].countAllMatch) break;
                    }
                }
            }
            return count;
        }
        protected virtual bool MatchingNeededAtIndex(int index, Hediff hediff)
        {
            return hediff.def == hediffsNeeded[index].def
                && hediff.Severity >= hediffsNeeded[index].severityToReach
                && hediff.CurStageIndex >= hediffsNeeded[index].stageToReach;
        }
        protected virtual bool DidGetRidAtIndex(int index, List<Hediff> matchingHediffDef)
        {
            return matchingHediffDef.NullOrEmpty();
        }
        public override string FormateText(string text)
        {
            text = text.Replace(Def.amount_Key, AmountNeeded.ToString());
            text = FormateList(text, Def.listing_Separator);
            return base.FormateText(text);
        }
        protected virtual string FormateList(string text, string separator)
        {
            string listing = "";
            if(hediffsNeeded != null) for (int i = 0; i < hediffsNeeded.Count; i++)
            {
                listing += hediffsNeeded[i].def.label;
                if (i != hediffsNeeded.Count - 1) listing += separator;
            }
            text = text.Replace(Def.covetedObjects_Key, listing);
            listing = "";
            if (HediffToGetRid != null) for (int i = 0; i < HediffToGetRid.Count; i++)
            {
                listing += HediffToGetRid[i].label;
                if (i != HediffToGetRid.Count - 1) listing += separator;
            }
            text = text.Replace(Def.toRemove_Key, listing);
            return text;
        }

        public override string DescriptionToFulfill
        {
            get
            {
                return base.DescriptionToFulfill + (AmountNeeded > 1 ? " (" + actualCount.ToString() + "/" + AmountNeeded.ToString() + ")" : "");
                
            }
        }
    }
}

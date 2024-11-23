using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HDream
{
    public class WishComp_Master : WishComp
    {

        public WishCompProperties_Master Props => (WishCompProperties_Master)props;

        private List<WishDef> SubWishDef => Props.subWish;

        public List<WishWithComp> subWishes = new List<WishWithComp>();
        public List<WishComp_Sub> subComps = new List<WishComp_Sub>();

        private int fulfillCount = 0; 

        private int doAtTick = 0;
        public const int waitForTick = 50;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref fulfillCount, "fulfillCount", 0);
            Scribe_Values.Look(ref doAtTick, "doAtTick", 0);
            Scribe_Collections.Look(ref subWishes, "subWishes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                for (int i = 0; i < subWishes.Count; i++)
                {
                    subWishes[i].pawn = parent.pawn;
                }
            }
        }
        public override void CompPostMake()
        {
            base.CompPostMake();
            if(parent.GetType() != typeof(WishWithComp))
            {
                Log.Error("HDream : There a WishComp_Master on " + parent.def.defName + ", it should only be used on an empty wish WishWithComp");
                parent.MakeFailed();
                return;
            }
            if (SubWishDef.NullOrEmpty()) 
            {
                Log.Error("HDream : Not enougth SubWish for " + parent.def.defName + " comp WishComp_Master, it need at least 1 subWish to be revelant");
                parent.MakeFailed();
                return;
            }
            for (int i = 0; i < SubWishDef.Count; i++)
            {
                if (!typeof(WishWithComp).IsAssignableFrom(SubWishDef[i].wishClass))
                {
                    Log.Error("HDream : " + parent.def.defName + "'s SubWish (" + SubWishDef[i].defName + ")'s wishClass isn't a WishWithComp ");
                    parent.MakeFailed();
                    return;
                }
                if (SubWishDef[i].comps != null && SubWishDef[i].comps.FindAll(comp => typeof(WishCompProperties_Sub).IsAssignableFrom(comp.GetType())).Count > 1)
                {
                    Log.Error("HDream : " + parent.def.defName + "'s SubWish (" + SubWishDef[i].defName + ") have more than 1 WishCompProperties_Sub");
                    parent.MakeFailed();
                    return;
                }
                WishWithComp subWish = (WishWithComp)Activator.CreateInstance(SubWishDef[i].wishClass);
                subWish.def = SubWishDef[i];
                subWish.pawn = parent.pawn;
                subWish.PostMake();
                if (subWish.makeFailed)
                {
                    Log.Error("HDream : " + parent.def.defName + "'s SubWish (" + SubWishDef[i].defName + ") make failed");
                    parent.MakeFailed();
                    return;
                }
                if (subWish.comps == null) subWish.comps = new List<WishComp>();
                WishComp_Sub wishComp = (WishComp_Sub)subWish.comps.Find(comp => typeof(WishComp_Sub).IsAssignableFrom(comp.GetType()));
                if (wishComp == null)
                {
                    wishComp = (WishComp_Sub)Activator.CreateInstance(typeof(WishComp_Sub));
                    wishComp.props = new WishCompProperties_Sub();
                    wishComp.parent = subWish;
                    subWish.comps.Add(wishComp);
                    wishComp.CompPostMake();
                }
                subWishes.Add(subWish);
            }
        }
        public override void CompPostAdd()
        {
            base.CompPostAdd();
            for (int i = 0; i < subWishes.Count; i++)
            {
                subWishes[i].PostAdd();
            }
        }

        public override void CompPostTick()
        {
            base.CompPostTick();
            for (int i = 0; i < subWishes.Count; i++) subWishes[i].Tick();
            for (int i = 0; i < subWishes.Count; i++) subWishes[i].PostTick();

            if (Find.TickManager.TicksGame < doAtTick) return;
            doAtTick = Find.TickManager.TicksGame + waitForTick;

            int count = 0;
            for (int i = 0; i < subWishes.Count; i++)
            {
                if (((WishComp_Sub)subWishes[i].comps.Find(comp => typeof(WishComp_Sub).IsAssignableFrom(comp.GetType()))).fulfilled) count++;
            }

            if (count >= subWishes.Count
                || (Props.subFulfillNeeded != -1 && count >= Props.subFulfillNeeded))
            {
                parent.OnFulfill();
                return;
            }
            if(count != fulfillCount)
            {
                parent.ChangeProgress(count - fulfillCount);
                fulfillCount = count;
            }
        }

        public override string CompDescription()
        {

            string desc = parent.pawn.wishes().wishes.Contains(parent) ? base.CompDescription() + (string)TranslationKey.WISHCOMP_MASTER_FULFILL.Translate() : "";
            for (int i = 0; i < subWishes.Count; i++)
            {
                desc += "\n\n";
                if (!parent.pawn.wishes().wishes.Contains(parent)) desc += "    ";
                desc += ((WishComp_Sub)subWishes[i].comps.Find(comp => typeof(WishComp_Sub).IsAssignableFrom(comp.GetType()))).fulfilled ? "[+] " : "[-] ";
                desc += subWishes[i].DescriptionToFulfill;
            }
            return desc;
        }
    }
}

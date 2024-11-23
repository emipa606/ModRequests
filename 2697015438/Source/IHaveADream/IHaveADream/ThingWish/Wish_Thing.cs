using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HDream
{
    public abstract class Wish_Thing<T>: WishWithComp
    {
        public ThingWishDef Def => (ThingWishDef)def;

        private List<T> thingsWanted = new List<T>();

        public List<T> ThingsWanted => thingsWanted;

        protected abstract LookMode ExposeLookModeT();
        protected abstract ThingDef GetThingDef(T thing);
        protected abstract List<T> GetThingsFromDef();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref thingsWanted, "thingsWanted", ExposeLookModeT());
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                int oldCount = thingsWanted.Count;
                for (int i = oldCount - 1; i >= 0; i--)
                {
                    if (GetThingDef(thingsWanted[i]) == null) thingsWanted.RemoveAt(i);
                }
                if (thingsWanted.Count == 0)
                {
                    DoRemove();
                    Log.Message("HDream : " + Def.defName + " was removed from " + pawn.LabelShort + " after resolving ref, it was not longer fulfillable.");
                }
                if (oldCount > thingsWanted.Count)
                {
                    Log.Message("HDream : " + Def.defName + " from " + pawn.LabelShort + ", " + (oldCount - thingsWanted.Count).ToString() + " def were removed from this wish possibility after resolving ref.");
                }
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            List<T> items = GetThingsFromDef().ListFullCopyOrNull();

            if (items.NullOrEmpty())
            {
                MakeFailed();
                return;
            }

            if (def.wantSpecific)
            {
                if (def.tryPreventSimilare)
                {
                    List<Wish> wishes = pawn.wishes().wishes.FindAll(wish => wish.def == def && wish != this);
                    if(!wishes.NullOrEmpty())
                    {
                        ThingDef sThing;
                        for (int i = 0; i < wishes.Count; i++)
                        {
                            sThing = (wishes[i] as Wish_Thing<T>).GetThingDef((wishes[i] as Wish_Thing<T>).ThingsWanted[0]);
                            if (sThing != null) items.RemoveAll(item => GetThingDef(item) == sThing);
                        }
                    }
                    if (items.NullOrEmpty())
                    {
                        MakeFailed();
                        return;
                    }
                }
                thingsWanted.Add(items[Mathf.FloorToInt(Rand.Value * items.Count)]);
            }
            else thingsWanted = items;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            SortList(thingsWanted);
        }

        public override List<Texture2D> DreamIcon()
        {
            List<Texture2D> icons = new List<Texture2D>();
            for (int i = 0; i < thingsWanted.Count; i++)
            {
                if (GetThingDef(thingsWanted[i]).uiIcon != null
                    && GetThingDef(thingsWanted[i]).uiIcon != BaseContent.BadTex) 
                        icons.Add(GetThingDef(thingsWanted[i]).uiIcon);
            }
            return icons;
        }

        public override string FormateText(string text)
        {
            text = text.Replace(Def.amount_Key, Def.amountNeeded.ToString());
            text = text.Replace(Def.countRule_Key, (Def.countAmountPerInfo ? TranslationKey.WISH_THING_PER_INFO.Translate() : TranslationKey.WISH_THING_PER_UNIT.Translate()));
            text = text.Replace(def.covetedObjects_Key, FormateListThing(thingsWanted));
            return base.FormateText(text);
        }


        protected virtual string FormateListThing(List<T> things)
        {
            string listing = "";
            for (int i = 0; i < things.Count; i++)
            {
                listing += GetThingDef(things[i]).label;
                if (i != things.Count - 1) listing += def.listing_Separator;
            }
            return listing;
        }


        protected virtual void SortList(List<T> list) { }
    }
}

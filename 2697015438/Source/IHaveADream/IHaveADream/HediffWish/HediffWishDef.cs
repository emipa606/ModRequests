using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class HediffWishDef : WishDef
    {
        public List<HediffWishInfo> hediffInfos;

        public string toRemove_Key = "{toRemoves}";

        protected List<HediffWishInfo> completeInfos;
        public virtual List<HediffWishInfo> CompleteInfos 
        {
            get 
            {
                if (completeInfos == null) completeInfos = hediffInfos ?? new List<HediffWishInfo>();
                return completeInfos;
            } 
        }

        public override void PostLoad()
        {
            base.PostLoad();
            if (hediffInfos == null) return;
            for (int i = 0; i < hediffInfos.Count - 1; i++)
            {
                for (int j = i + 1; j < hediffInfos.Count; j++)
                {
                    if(hediffInfos[i].def == hediffInfos[j].def)
                    {
                        Log.Error("HDream : Same HediffDef was found multiple time in HediffWishDef => heddifInfos, heddifInfos should not countain similare HediffDef");
                        return;
                    }
                }
            }
        }
    }
}

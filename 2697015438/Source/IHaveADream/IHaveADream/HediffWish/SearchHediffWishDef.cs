using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class SearchHediffWishDef : HediffWishDef
    {
        public List<HediffDef> excludedHediff;

        public int searchedStageToReach = -1;
        public float searchedSeverityToReach = -1;

        public bool tryFindHediff = true;

        public bool canBeBadHediff = true;
        public bool canBeGoodHediff = true;

        public bool getRidOfFound = false;
        protected virtual List<HediffDef> SearchedDef
        {
            get
            {
                List<HediffDef> hediffDefs = new List<HediffDef>();
                List<HediffDef> allHediff = DefDatabase<HediffDef>.AllDefsListForReading;
                for (int i = 0; i < allHediff.Count; i++)
                {
                    if ((!excludedHediff.NullOrEmpty() && !excludedHediff.Contains(allHediff[i]))
                        || (!canBeBadHediff && allHediff[i].isBad)
                        || (!canBeGoodHediff && !allHediff[i].isBad)) continue;
                    hediffDefs.Add(allHediff[i]);
                }
                return hediffDefs;
            } 
        }

        public override List<HediffWishInfo> CompleteInfos 
        {
            get
            {
                if (completeInfos == null)
                {
                    completeInfos = base.CompleteInfos;
                    if(tryFindHediff) CompleteHediffInfo(SearchedDef);
                }
                return completeInfos;
            }
        }
        protected void CompleteHediffInfo(List<HediffDef> defs)
        {
            if (defs.NullOrEmpty()) Log.Message("HDream : no def to complete heddifInfos for " + defName);
            for (int i = 0; i < defs.Count; i++)
            {
                if (hediffInfos != null && hediffInfos.Where(info => info.def == defs[i]).Any()) continue;
                completeInfos.Add(new HediffWishInfo
                {
                    def = defs[i],
                    stageToReach = searchedStageToReach,
                    severityToReach = searchedSeverityToReach
                });
            }
        }
    }
}

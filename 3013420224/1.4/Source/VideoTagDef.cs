using RimWorld;
using System;
using System.Linq;
using Verse;

namespace NoobertNebulous
{
    public class VideoTagDef : Def
    {
        public float subscriberChanceMultiplier = 1f;
        public float viewRevenueMultiplier = 1f;
        public float unsubscribeChance;
        public float workToMakeVideoMultiplier = 1f;
        public SkillDef baseViewsPerDayMinimumSkill;
        public SkillDef baseViewsPerDayMaximumSkill;
        public float baseViewsPerDayMinimumMultiplier = 1f;
        public float baseViewsPerDayMaximumMultiplier = 1f;
        public Type baseViewsPerDayMaximumMultiplierWorker;
        private VideoTagWorker _BaseViewsPerDayMaximumMultiplierWorker;
        public VideoTagWorker BaseViewsPerDayMinimumMultiplierWorker
        {
            get
            {
                if (_BaseViewsPerDayMaximumMultiplierWorker == null)
                {
                    _BaseViewsPerDayMaximumMultiplierWorker = (VideoTagWorker)Activator.CreateInstance(baseViewsPerDayMaximumMultiplierWorker);
                }
                return _BaseViewsPerDayMaximumMultiplierWorker;
            }
        }

        public Type subscriberChanceMultiplierWorker;
        private VideoTagWorker _subscriberChanceMultiplierWorker;
        public VideoTagWorker SubscriberChanceMultiplierWorker
        {
            get
            {
                if (_subscriberChanceMultiplierWorker == null)
                {
                    _subscriberChanceMultiplierWorker = (VideoTagWorker)Activator.CreateInstance(subscriberChanceMultiplierWorker);
                }
                return _subscriberChanceMultiplierWorker;
            }
        }
    }

    public abstract class VideoTagWorker
    {
        public abstract float GetMultiplier(Pawn creator);
    }

    public class ResearchProjectsWorker : VideoTagWorker
    {
        public override float GetMultiplier(Pawn creator)
        {
            var projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(x => x.IsFinished).ToList();
            return projects.Count * 0.05f;
        }
    }

    public class BeautyWorker : VideoTagWorker
    {
        public override float GetMultiplier(Pawn creator)
        {
            var beauty = creator.GetStatValue(StatDefOf.PawnBeauty);
            if (beauty <= 0)
            {
                return 1f;
            }
            //Log.Message("Beauty: " + beauty);
            return beauty;
        }
    }
}

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace NoobertNebulous
{
    public class Video : IExposable
    {
        public const float BaseViewsPerDayMinimum = 100f;
        public const float BaseViewsPerDayMaximum = 1000f;
        public const float BaseSubscriberChance = 0.01f;
        public const float BaseViewRevenue = 0.003f;
        public const float BaseWorkToMakeVideo = 7500f;
        public const float BaseUnsubscribeChance = 0.00f;

        public string name;
        public Pawn creator;
        public int creationDateTick;
        public int viewCount;
        public int viewCountPrevTime;
        public int subscriberViewCount;
        public int subscriberViewCountPrevQuadrum;
        public float revenue;
        public float revenueNew;

        public float TotalRevenue => revenue + revenueNew;

        public List<VideoTagDef> tags = new List<VideoTagDef>();

        public float ViewsPerDayMinimum
        {
            get
            {
                var value = BaseViewsPerDayMinimum;
                foreach (var tag in tags)
                {
                    value *= tag.baseViewsPerDayMinimumMultiplier;
                    if (tag.baseViewsPerDayMinimumSkill != null)
                    {
                        var skill = creator.skills.GetSkill(tag.baseViewsPerDayMinimumSkill);
                        value *= skill.Level / 5f;
                    }
                }
                return value;
            }
        }
        public float ViewsPerDayMaximum
        {
            get
            {
                var value = BaseViewsPerDayMaximum;
                foreach (var tag in tags)
                {
                    value *= tag.baseViewsPerDayMaximumMultiplier;
                    if (tag.baseViewsPerDayMaximumSkill != null)
                    {
                        var skill = creator.skills.GetSkill(tag.baseViewsPerDayMaximumSkill);
                        value *= skill.Level / 5f;
                    }
                    if (tag.baseViewsPerDayMaximumMultiplierWorker != null)
                    {
                        value *= tag.BaseViewsPerDayMinimumMultiplierWorker.GetMultiplier(creator);
                    }
                }
                return value;
            }
        }
        public float SubscriberChance
        {
            get
            {
                var value = BaseSubscriberChance;
                foreach (var tag in tags)
                {
                    value *= tag.subscriberChanceMultiplier;
                    if (tag.subscriberChanceMultiplierWorker != null)
                    {
                        value *= tag.SubscriberChanceMultiplierWorker.GetMultiplier(creator);
                    }
                }
                return value;
            }
        }
        public float ViewRevenueMininum
        {
            get
            {
                var viewMinimum = ViewsPerDayMinimum;
                var value = BaseViewRevenue;
                foreach (var tag in tags)
                {
                    value *= tag.viewRevenueMultiplier;
                }
                return viewMinimum * value;

            }
        }
        public float ViewRevenueMaximum
        {
            get
            {
                var viewMaximum = ViewsPerDayMaximum;
                var value = BaseViewRevenue;
                foreach (var tag in tags)
                {
                    value *= tag.viewRevenueMultiplier;
                }
                return viewMaximum * value;
            }
        }

        public float ViewRevenue
        {
            get
            {
                var value = BaseViewRevenue;
                foreach (var tag in tags)
                {
                    value *= tag.viewRevenueMultiplier;
                }
                return value;
            }
        }

        public int WorkToMakeVideo
        {
            get
            {
                var workToMake = BaseWorkToMakeVideo;
                foreach (var tag in tags)
                {
                    workToMake *= tag.workToMakeVideoMultiplier;
                }
                workToMake *= 1f - (creator.skills.GetSkill(SkillDefOf.Social).Level * 0.03f);
                return (int)workToMake;
            }
        }
        public float UnsubscribeChance
        {
            get
            {
                var unsubscribeChance = BaseUnsubscribeChance;
                foreach (var tag in tags)
                {
                    unsubscribeChance += tag.unsubscribeChance;
                }
                return unsubscribeChance;
            }
        }

        public SimpleCurve DiminishingViewsMultiplierPerYear = new SimpleCurve
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(1f, 1f),
            new CurvePoint(2f, 0.75f),
            new CurvePoint(3f, 0.5f),
            new CurvePoint(10f, 0.01f)
        };

        public int prevCalculationTick;

        public void CalculateData()
        {
            //Log.Message("----------------------------");
            var tickInterval = (Find.TickManager.TicksGame - prevCalculationTick) / (float)GenDate.TicksPerDay;
            //Log.Message(name + " - " + string.Join(", ", tags.Select(x => x.label)) + " - calculating views, subscribers");
            //Log.Message("Tick interval multiplier: " + tickInterval);
            var channel = creator.GetVidtubeChannel();
            int videoAgeYears = (Find.TickManager.TicksGame - creationDateTick) / GenDate.TicksPerYear;
            var newViews = (int)((new FloatRange(ViewsPerDayMinimum, ViewsPerDayMaximum).RandomInRange *
                DiminishingViewsMultiplierPerYear.Evaluate(videoAgeYears)) * tickInterval);
            //Log.Message("New views: " + newViews);
            viewCount += newViews;
            if (channel.subscribers > subscriberViewCount)
            {
                var newSubscriberViews = (int)((channel.subscribers - subscriberViewCount) * 0.7f);
                var subscribersLeft = (int)(newSubscriberViews * UnsubscribeChance);
                channel.subscribers = Mathf.Max(0, channel.subscribers - subscribersLeft);
                subscriberViewCount += newSubscriberViews;
                viewCount += newSubscriberViews;
                //Log.Message("New subscriber views: " + newSubscriberViews);
                //Log.Message("Subscribers left: " + subscribersLeft);
            }

            var newSubscribers = (int)(newViews * SubscriberChance);
            //Log.Message("New subscribers: " + newSubscribers);
            channel.subscribers += newSubscribers;

            var gainedRevenue = (viewCount - viewCountPrevTime) * ViewRevenue;
            this.revenueNew += gainedRevenue;
            viewCountPrevTime = viewCount;

            //Log.Message("Gained revenue: " + gainedRevenue + ", total revenue this quadrum: " + this.revenueNew);
            //Log.Message("Total views: " + viewCount + ", total subscribers: " + channel.subscribers + ", video age in years: " + videoAgeYears);
            prevCalculationTick = Find.TickManager.TicksGame;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref creator, "creator");
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref viewCount, "viewCount");
            Scribe_Values.Look(ref subscriberViewCount, "subscriberViewCount");
            Scribe_Values.Look(ref revenue, "revenue");
            Scribe_Values.Look(ref revenueNew, "revenueNew");
            Scribe_Values.Look(ref creationDateTick, "creationDateTick");
            Scribe_Values.Look(ref prevCalculationTick, "prevCalculationTick");

            Scribe_Values.Look(ref viewCountPrevTime, "viewCountPrevQuadrum");
            Scribe_Values.Look(ref subscriberViewCountPrevQuadrum, "subscriberViewCountPrevQuadrum");
            Scribe_Collections.Look(ref tags, "tags", LookMode.Def);
        }
    }
}

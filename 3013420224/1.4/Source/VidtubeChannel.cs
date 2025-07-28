using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace NoobertNebulous
{
    public class VidtubeChannel : IExposable
    {
        public Pawn author;
        public string name;
        public int subscribers;
        public List<Video> videos = new List<Video>();

        public Quadrum checkedQuadrum = Quadrum.Undefined;
        public int videoReleasedPrevQuadrum;
        public int viewsPrevQuadrum;
        public int subscribersPrevQuadrum;

        public void AddVideo(Video video)
        {
            video.creationDateTick = Find.TickManager.TicksGame;
            video.prevCalculationTick = Find.TickManager.TicksGame;
            videos.Add(video);
        }

        public void TickOnDay()
        {
            foreach (var video in videos)
            {
                video.CalculateData();
            }
        }

        public void RemoveVideo(Video video)
        {
            if (checkedQuadrum != Quadrum.Undefined)
            {
                videoReleasedPrevQuadrum--;
                viewsPrevQuadrum -= video.viewCountPrevTime;
                subscribersPrevQuadrum -= video.subscriberViewCountPrevQuadrum;
            }
            videos.Remove(video);
        }

        public void GainRevenue(Quadrum checkedQuadrum)
        {
            this.checkedQuadrum = checkedQuadrum;
            var newVideosReleased = this.videos.Count - this.videoReleasedPrevQuadrum;
            int totalViews = this.videos.Sum(x => x.viewCount);
            var newViews = totalViews - this.viewsPrevQuadrum;
            var newSubscribers = Mathf.Max(0, this.subscribers - this.subscribersPrevQuadrum);
            var revenueThisQuadrum = 0f;
            foreach (var video in videos)
            {
                revenueThisQuadrum += video.revenueNew;
                video.revenue += video.revenueNew;
                video.revenueNew = 0f;
            }

            var mostProfitableVideo = this.videos.MaxBy(x => x.revenue);
            revenueThisQuadrum = Mathf.CeilToInt(revenueThisQuadrum);
            List<Thing> thingsToLaunch = new List<Thing>();


            this.videoReleasedPrevQuadrum = this.videos.Count;
            this.viewsPrevQuadrum = totalViews;
            this.subscribersPrevQuadrum = this.subscribers;

            var revenueReceived = revenueThisQuadrum;
            while (revenueThisQuadrum > 0)
            {
                Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
                var curAmount = Mathf.Min(thing.def.stackLimit, revenueThisQuadrum);
                thing.stackCount = (int)(curAmount);
                revenueThisQuadrum -= curAmount;
                thingsToLaunch.Add(thing);
            }
            var localMap = author.MapHeld ?? Find.AnyPlayerHomeMap;
            foreach (var thing in thingsToLaunch)
            {
                var cell = DropCellFinder.TradeDropSpot(localMap);
                TradeUtility.SpawnDropPod(cell, localMap, thing);
            }

            Find.LetterStack.ReceiveLetter("NN.VidtubeSummaryTitle".Translate(author.Named("PAWN")),
                "NN.VidtubeSummaryDesc".Translate(author.Named("PAWN"), this.videos.Count, newVideosReleased, string.Format("{0:n0}", totalViews),
                string.Format("{0:n0}", newViews), string.Format("{0:n0}", newSubscribers), revenueReceived.ToStringMoney(), mostProfitableVideo.name), LetterDefOf.PositiveEvent, thingsToLaunch);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref author, "author");
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref subscribers, "subscribers");
            Scribe_Collections.Look(ref videos, "videos", LookMode.Deep);

            Scribe_Values.Look(ref checkedQuadrum, "checkedQuadrum", Quadrum.Undefined);
            Scribe_Values.Look(ref videoReleasedPrevQuadrum, "videoReleasedPrevQuadrum");
            Scribe_Values.Look(ref viewsPrevQuadrum, "totalViewsPrevQuadrum");
            Scribe_Values.Look(ref subscribersPrevQuadrum, "totalSubscribersPrevQuadrum");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                videos ??= new List<Video>();
            }
        }
    }
}

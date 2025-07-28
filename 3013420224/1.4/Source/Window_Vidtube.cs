using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace NoobertNebulous
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public class Window_Vidtube : Window
    {
        public Pawn pawn;
        public CompStreamingConsole console;
        public VidtubeChannel channel;
        public override Vector2 InitialSize => new Vector2(1000f, 700f);
        public static readonly Texture2D SubscriberIcon = ContentFinder<Texture2D>.Get("UI/Icons/SubscriberIcon");

        public Window_Vidtube(Pawn pawn, CompStreamingConsole compStreamingConsole)
        {
            this.pawn = pawn;
            this.console = compStreamingConsole;
            this.channel = pawn.GetVidtubeChannel();
            foreach (var video in this.channel.videos)
            {
                video.CalculateData();
            }
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        private Vector2 scrollPos;
        private float scrollHeight = 99999999;

        public override void DoWindowContents(Rect inRect)
        {
            var pos = new Vector2(15, 0);
            var channelTitleRect = new Rect(pos.x, pos.y, 300, 32);
            Text.Font = GameFont.Medium;
            Widgets.Label(channelTitleRect, channel.name);
            pos.y += 32 + 10;
            Text.Font = GameFont.Small;
            pos.x += 5;

            Text.Anchor = TextAnchor.MiddleLeft;

            var subscriberCountRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(subscriberCountRect, "NN.Subscribers".Translate());
            var subscriberCountIconRect = new Rect(subscriberCountRect.xMax, subscriberCountRect.y, 20, 20);
            GUI.DrawTexture(subscriberCountIconRect, SubscriberIcon);
            subscriberCountRect = new Rect(subscriberCountIconRect.xMax + 5, subscriberCountRect.y, 150, 24);
            Widgets.Label(subscriberCountRect, string.Format("{0:n0}", channel.subscribers));
            pos.y += 24;

            var totalViewsCountRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(totalViewsCountRect, "NN.TotalViews".Translate());
            totalViewsCountRect = new Rect(totalViewsCountRect.xMax, totalViewsCountRect.y, 150, 24);
            Widgets.Label(totalViewsCountRect, string.Format("{0:n0}", channel.videos.Sum(x => x.viewCount)));
            pos.y += 24;

            var totalRevenueCountRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(totalRevenueCountRect, "NN.TotalRevenue".Translate());
            totalRevenueCountRect = new Rect(totalRevenueCountRect.xMax, totalRevenueCountRect.y, 150, 24);
            Widgets.Label(totalRevenueCountRect, channel.videos.Sum(x => x.TotalRevenue).ToStringMoney());
            pos.y += 24;

            var videoCountRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(videoCountRect, "NN.Videos".Translate());
            videoCountRect = new Rect(videoCountRect.xMax, videoCountRect.y, 150, 24);
            Widgets.Label(videoCountRect, string.Format("{0} / {1}", channel.videos.Count, NoobertNebulousSettings.maxVideoLimit));
            pos.y += 24;

            pos.y += 24;
            pos.x = 0;
            Text.Anchor = TextAnchor.LowerLeft;
            Text.Font = GameFont.Medium;
            var videosHeaderRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(videosHeaderRect, "NN.Videos".Translate());
            Text.Font = GameFont.Small;
            pos.x += 265;

            var tagsHeaderRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(tagsHeaderRect, "NN.Tags".Translate());
            pos.x += 315;

            var viewsHeaderRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(viewsHeaderRect, "NN.Views".Translate());
            pos.x += 90;

            var revenueHeaderRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(revenueHeaderRect, "NN.Revenue".Translate());
            pos.x += 100;

            var publishDateHeaderRect = new Rect(pos.x, pos.y, 100, 24);
            Widgets.Label(publishDateHeaderRect, "NN.PublishDate".Translate());

            pos.y += 24;
            pos.x = 0;
            float y = pos.y + 6;
            Color color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineHorizontal(pos.x, y, inRect.width);
            GUI.color = color;

            pos.y += 15;

            var totalRect = new Rect(pos.x, pos.y, inRect.width, 380);
            var viewRect = new Rect(pos.x, pos.y, totalRect.width - 16, scrollHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            scrollHeight = 0;
            var entryHeight = 42;

            Widgets.BeginScrollView(totalRect, ref scrollPos, viewRect);
            Video videoToRemove = null;
            for (var i = 0; i < channel.videos.Count; i++)
            {
                var video = channel.videos[i];
                Text.Anchor = TextAnchor.MiddleLeft;
                var tagSelectionRect = new Rect(pos.x + 250 + 15, pos.y + 8, 300, 32);
                var rect = GenUI.DrawElementStack(tagSelectionRect, 24, video.tags, delegate (Rect r, VideoTagDef tag)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    var labelRect = new Rect(r.x, r.y, r.width, r.height);
                    Color color2 = GUI.color;
                    GUI.color = UIHelper.StackElementBackground;
                    GUI.DrawTexture(labelRect, BaseContent.WhiteTex);
                    GUI.color = color2;

                    if (Mouse.IsOver(labelRect))
                    {
                        Widgets.DrawHighlight(labelRect);
                    }

                    Widgets.Label(labelRect, tag.LabelCap);
                    GUI.color = Color.white;
                    if (Mouse.IsOver(labelRect))
                    {
                        var trLocal = tag;
                        var tooltip = tag.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + tag.description;
                        TooltipHandler.TipRegion(tip: new TipSignal(() => tooltip, tag.LabelCap.GetHashCode()), rect: labelRect);
                    }
                }, (VideoTagDef tag) => Text.CalcSize(tag.LabelCap).x + 10);
                Text.Anchor = TextAnchor.MiddleLeft;
                var titleWidth = 250 - 15;
                var titleSize = Text.CalcHeight(video.name, titleWidth);
                var rowHeight = Mathf.Max(rect.height + 8 + 5, entryHeight, titleSize);

                var titleRect = new Rect(pos.x + 15, pos.y, titleWidth, rowHeight);
                Widgets.Label(titleRect, video.name);
                var videoRect = new Rect(pos.x, pos.y, viewRect.width, rowHeight);
                if (i % 2 == 0)
                {
                    Widgets.DrawLightHighlight(videoRect);
                }

                var viewsRect = new Rect(tagSelectionRect.xMax + 15, pos.y, 85, rowHeight);
                Widgets.Label(viewsRect, string.Format("{0:n0}", video.viewCount));

                var revenueRect = new Rect(viewsRect.xMax + 5, pos.y, 85, rowHeight);
                Widgets.Label(revenueRect, video.TotalRevenue.ToStringMoney());

                var publishDateRect = new Rect(revenueRect.xMax + 15, pos.y, 150, rowHeight);
                Vector2 locForDates = QuestUtility.GetLocForDates();
                Widgets.Label(publishDateRect, GenDate.DateFullStringAt(GenDate.TickGameToAbs(video.creationDateTick), locForDates)) ;

                if (Mouse.IsOver(videoRect))
                {
                    var size = videoRect.height - 24;
                    var removeVideoRect = new Rect(publishDateRect.xMax, pos.y + (size / 2f), 24, 24);
                    if (Widgets.ButtonImage(removeVideoRect, TexButton.DeleteX))
                    {
                        videoToRemove = video;
                    }
                }

                pos.y += rowHeight;
                scrollHeight += rowHeight;
            }
            if (videoToRemove != null)
            {
                channel.RemoveVideo(videoToRemove);
                videoToRemove = null;
            }
            Widgets.EndScrollView();



            Text.Anchor = TextAnchor.UpperLeft;
            var recordNewVideoRect = new Rect(inRect.width - 150, inRect.height - 60, 150, 24);
            if (channel.videos.Count >= NoobertNebulousSettings.maxVideoLimit)
            {
                GUI.color = Color.grey;
            }
            if (Widgets.ButtonText(recordNewVideoRect, "NN.RecordNewVideo".Translate(), active: channel.videos.Count 
                < NoobertNebulousSettings.maxVideoLimit))
            {
                Find.WindowStack.Add(new Window_RecordVideo(pawn, console));
            }
            GUI.color = Color.white;

            var closeButtonRect = new Rect((inRect.width / 2f) - 60, inRect.height - 32, 120, 32);
            if (Widgets.ButtonText(closeButtonRect, "Close".Translate()))
            {
                Close();
            }
        }
    }
}

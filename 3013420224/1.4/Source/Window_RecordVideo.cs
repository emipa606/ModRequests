using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace NoobertNebulous
{
    [HotSwappable]
    public class Window_RecordVideo : Window
    {
        public Pawn pawn;
        public CompStreamingConsole compStreamingConsole;
        public Video video;

        public override Vector2 InitialSize => new Vector2(850f, 330f);

        public Window_RecordVideo(Pawn pawn, CompStreamingConsole compStreamingConsole) 
        {
            this.pawn = pawn;
            this.compStreamingConsole = compStreamingConsole;
            this.video = Utils.CreateVideo(pawn, Utils.GenerateTextFromRule(Utils.videoNameRulePack));
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var pos = new Vector2(0, 0);
            var createVideoRect = new Rect(pos.x, pos.y, 300, 32);
            Text.Font = GameFont.Medium;
            Widgets.Label(createVideoRect, "NN.CreateVideo".Translate());
            pos.y += 32 + 10;
            Text.Font = GameFont.Small;
            pos.x += 5;

            Text.Anchor = TextAnchor.MiddleLeft;

            Text.Font = GameFont.Tiny;
            pos.x = 0;
            Text.Anchor = TextAnchor.LowerLeft;

            var basicsRect = new Rect(pos.x, pos.y, 150, 24);
            Widgets.Label(basicsRect, "NN.Basic".Translate());

            pos.y += 18;
            pos.x = 0;
            float y = pos.y + 6;
            Color color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineHorizontal(pos.x, y, inRect.width);
            GUI.color = color;
            pos.x = 15;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            pos.y += 15;
            var videoTitleRect = new Rect(pos.x, pos.y, 150, 32);
            Widgets.Label(videoTitleRect, "NN.VideoTitle".Translate());

            var videoTitleInputRect = new Rect(200, pos.y, 450, 32);
            video.name = Widgets.TextField(videoTitleInputRect, video.name);

            var randomizeRect = new Rect(videoTitleInputRect.xMax + 15, pos.y, 150, 32);
            if (Widgets.ButtonText(randomizeRect, "Randomize".Translate()))
            {
                video.name = Utils.GenerateTextFromRule(Utils.videoNameRulePack);
            }

            pos.y += 32 + 15;
            var tagsRect = new Rect(pos.x, pos.y, 150, 32);
            Widgets.Label(tagsRect, "NN.Tags".Translate());
            var tagSelectionRect = new Rect(200, pos.y + 4, 450, 32);

            VideoTagDef tagToDelete = null;
            var rect = GenUI.DrawElementStack(tagSelectionRect, 24f, video.tags, delegate (Rect r, VideoTagDef tag)
            {
                Text.Anchor = TextAnchor.MiddleCenter;

                var labelRect = new Rect(r.x, r.y, r.width - 24, r.height);
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

                var deleteTagRect = new Rect(labelRect.xMax, labelRect.y, 24, 24).ContractedBy(4);
                if (Widgets.ButtonImage(deleteTagRect, TexButton.DeleteX))
                {
                    tagToDelete = tag;
                }
                if (Mouse.IsOver(deleteTagRect))
                {
                    var tooltip = "NN.DeleteTag".Translate();
                    TooltipHandler.TipRegion(tip: new TipSignal(() => tooltip, tag.LabelCap.GetHashCode()), rect: deleteTagRect);
                }

            }, (VideoTagDef tag) => Text.CalcSize(tag.LabelCap).x + 10 + 24);

            if (tagToDelete != null)
            {
                video.tags.Remove(tagToDelete);
                tagToDelete = null;
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            if (video.tags.Count >= 3)
            {
                GUI.color = Color.grey;
            }
            var addTagsRect = new Rect(tagSelectionRect.xMax + 15, pos.y, 150, 32);

            if (Widgets.ButtonText(addTagsRect, "NN.AddTag".Translate(), active: video.tags.Count < 3))
            {
                var availableTags = DefDatabase<VideoTagDef>.AllDefsListForReading.Where(x => video.tags.Contains(x) is false).ToList();
                Find.WindowStack.Add(new Window_SelectItem<VideoTagDef>(availableTags.First(), availableTags, 
                    delegate (VideoTagDef x)
                    {
                        video.tags.Add(x);
                    }, tooltipGetter: delegate(VideoTagDef x)
                    {
                        return x.description;
                    }));
            }

            GUI.color = Color.white;
            pos.y += rect.height;
            Text.Anchor = TextAnchor.UpperLeft;

            Text.Font = GameFont.Tiny;
            pos.x = 0;
            Text.Anchor = TextAnchor.LowerLeft;

            var statisticsRect = new Rect(pos.x, pos.y, 150, 24);
            Widgets.Label(statisticsRect, "NN.Statistics".Translate());

            pos.y += 18;
            pos.x = 0;
            y = pos.y + 6;
            color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineHorizontal(pos.x, y, inRect.width);
            GUI.color = color;
            pos.x = 15;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            pos.y += 15;

            var viewsPerDayRect = new Rect(pos.x, pos.y, 185, 24);
            Widgets.Label(viewsPerDayRect, "NN.ViewsPerDay".Translate());
            viewsPerDayRect = new Rect(viewsPerDayRect.xMax, viewsPerDayRect.y, 150, 24);
            Widgets.Label(viewsPerDayRect, video.ViewsPerDayMinimum + "~" + video.ViewsPerDayMaximum);

            var viewRevenuePerDayRect = new Rect(400, pos.y, 185, 24);
            Widgets.Label(viewRevenuePerDayRect, "NN.ViewRevenuePerDay".Translate());
            viewRevenuePerDayRect = new Rect(viewRevenuePerDayRect.xMax, viewsPerDayRect.y, 150, 24);
            Widgets.Label(viewRevenuePerDayRect, video.ViewRevenueMininum.ToStringMoney() + "~" + video.ViewRevenueMaximum.ToStringMoney());
            pos.y += 24;

            var subscriberChanceRect = new Rect(pos.x, pos.y, 185, 24);
            Widgets.Label(subscriberChanceRect, "NN.SubscriberChance".Translate());
            subscriberChanceRect = new Rect(subscriberChanceRect.xMax, subscriberChanceRect.y, 150, 24);
            Widgets.Label(subscriberChanceRect, video.SubscriberChance.ToStringPercent());

            var workToMakeRect = new Rect(400, pos.y, 185, 24);
            Widgets.Label(workToMakeRect, "NN.WorkToMake".Translate());
            workToMakeRect = new Rect(workToMakeRect.xMax, workToMakeRect.y, 150, 24);
            Widgets.Label(workToMakeRect, video.WorkToMakeVideo.ToStringTicksToPeriod());
            pos.y += 24 + 15;

            Text.Anchor = TextAnchor.UpperLeft;

            var cancelRect = new Rect(pos.x, pos.y, 150, 32);
            if (Widgets.ButtonText(cancelRect, "Cancel".Translate()))
            {
                this.Close();
            }

            var startRecordingRect = new Rect(inRect.width - 150, pos.y, 150, 32);
            if (Widgets.ButtonText(startRecordingRect, "NN.StartRecording".Translate()))
            {
                compStreamingConsole.videoToRecord = video;
                Job job = JobMaker.MakeJob(NN_DefOf.NN_RecordVideo, compStreamingConsole.parent);
                pawn.jobs.TryTakeOrderedJob(job);
                this.Close();
                var window = Find.WindowStack.WindowOfType<Window_Vidtube>();
                if (window != null)
                {
                    window.Close();
                }
            }
        }

    }
}

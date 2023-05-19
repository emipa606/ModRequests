using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace WorksController
{
    public class MainTabWindow_WorksControllerConfigMenu : MainTabWindow
    {
        protected virtual float ExtraBottomSpace
        {
            get
            {
                return 53f; //53f
            }
        }

        protected virtual float ExtraTopSpace
        {
            get
            {
                return 53f; //0f
            }
        }

        protected WC_WorkGiverTableDef WorkGiverTableDef => WC_WorkGiverTableDefOf.WC_Configure;

        protected override float Margin
        {
            get
            {
                return 6f;
            }
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                if (this.table == null)
                {
                    return Vector2.zero;
                }
                return new Vector2(this.table.Size.x + this.Margin * 2f, this.table.Size.y + this.ExtraBottomSpace + this.ExtraTopSpace + this.Margin * 2f);
            }
        }

        protected virtual IEnumerable<WC_DataObject> Datas
        {
            get
            {
                return WC_DataContext.GetDataObjects();
            }
        }

        public override void PostOpen()
        {
            if (this.table == null)
            {
                this.table = this.CreateTable();
            }
            this.SetDirty();
            WC_DataContext.m_WCOpend = true;
            WC_DataContext.UpdateStatusDisplayInfo(true);
        }

        public override void PostClose()
        {
            base.PostClose();
            WC_DataContext.m_WCOpend = false;
        }

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);
            this.DoTopControllPane(rect);
            this.table.WC_WorkGiverTableOnGUI(new Vector2(rect.x, rect.y + this.ExtraTopSpace));
        }

        private void DoTopControllPane(Rect rect)
        {
            // ModEnable
            float marginLeft = Margin;
            float marginTop = Margin;
            Vector2 vector = new Vector2(rect.x + (float)marginLeft, rect.y + (float)marginTop);
            Rect rect2 = new Rect(vector.x, vector.y, 24f, 24f);
            bool flag = WC_DataContext.m_ModEnable;
            Widgets.Checkbox(vector, ref WC_DataContext.m_ModEnable, 32f, false, true, null, null);
            if (Mouse.IsOver(rect2))
            {
                string tip = null;
                if (!tip.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(rect2, tip);
                }
            }
            if (WC_DataContext.m_ModEnable != flag)
            {
                WC_DataContext.NotifyHeaderValueUpdated();
            }


            // ModEnableDesc
            Rect rect3 = new Rect(rect.x + marginLeft + rect2.width + 10f, rect.y + marginTop, 800f, Mathf.Min(rect.height, 30f));
            if (Mouse.IsOver(rect3))
            {
                GUI.DrawTexture(rect3, TexUI.HighlightTex);
            }
            Rect rect4 = rect3;
            rect4.xMin += 3f;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Widgets.Label(rect4, "WC_ModEnableDesc".Translate());
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            //ModRestoreSaveButton
            if (Current.ProgramState == ProgramState.Playing)
            {
                Rect rectr1 = new Rect(rect.x + rect.width - 200f, rect.y + marginTop, 180f, Mathf.Min(rect.height, 30f));
                Rect rectr2 = rectr1;
                rectr2.xMin += 3f;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.WordWrap = false;
                string buttonText = String.Format("{0}/{1}", "LoadGameButton".Translate(), "SaveGameButton".Translate());
                if (Widgets.ButtonText(rectr2, buttonText))
                {
                    Find.MainTabsRoot.EscapeCurrentTab(false);
                    Find.WindowStack.Add(new WC_PersistableSettingDialog());
                }
                Text.WordWrap = true;
                Text.Anchor = TextAnchor.UpperLeft;
            }


            Widgets.DrawLineHorizontal(rect.x, rect.y + rect3.height + marginTop + 8, rect.width);

            marginTop = rect.height - ExtraBottomSpace + 15;

            // ModWarningDesc
            Rect rect5 = new Rect(rect.x + marginLeft, rect.y + marginTop, 576f, Mathf.Min(rect.height, 30f));
            if (Mouse.IsOver(rect5))
            {
                GUI.DrawTexture(rect5, TexUI.HighlightTex);
            }
            Rect rect6 = rect5;
            //rect6.xMin += 3f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Widgets.Label(rect6, "WC_ModWarningDesc".Translate());
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;


            // ModStopCurJobAndEval
            Rect rect7 = new Rect(rect.x + marginLeft + rect5.width, rect.y + marginTop, 400f, Mathf.Min(rect.height, 30f));
            if (Mouse.IsOver(rect7))
            {
                GUI.DrawTexture(rect7, TexUI.HighlightTex);
            }
            Rect rect8 = rect7;
            //rect8.xMin += 3f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            if (Widgets.ButtonText(rect8, "WC_ModStopCurJobAndReeval".Translate()))
            {
                StopCurJobAndReeval();
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;


        }

        private void StopCurJobAndReeval()
        {
            StringBuilder sb = new StringBuilder();
            bool result = false;
            foreach (Map map in Find.Maps.Where(x=>x.mapPawns.AllPawns.Any(y=>y.Faction == Faction.OfPlayer || y.IsQuestLodger())))
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns.Where(x => x.Faction == Faction.OfPlayer || x.IsQuestLodger()))
                {
                    if (pawn.jobs != null && pawn.jobs.AllJobs().Any())
                    {
                        Job job = pawn.CurJob;
                        if (job.workGiverDef != null && WorksControllerUtil.WorkGiver_ShouldSkip(job.workGiverDef, pawn))
                        {
                            result = true;
                            string pawnLabel = pawn.LabelCap;
                            string pawnJobLabel = job.def.LabelCap;
                            string workLabel = job.workGiverDef.LabelCap;
                            pawn.jobs.EndCurrentOrQueuedJob(job, JobCondition.InterruptForced);
                            sb.AppendLine(String.Format("{0} -> {1} {2}", pawnLabel, workLabel, "WC_WorkStopped".Translate()));
                        }
                    }
                }
            }
            if (result)
            {
                Messages.Message(sb.ToString(), MessageTypeDefOf.NeutralEvent, true);
            }
            else
            {
                Messages.Message("WC_NoChangePawnsWork".Translate(), MessageTypeDefOf.NeutralEvent, true);
            }
        }

        public void Notify_PawnsChanged()
        {
            this.SetDirty();
        }

        public override void Notify_ResolutionChanged()
        {
            this.table = this.CreateTable();
            base.Notify_ResolutionChanged();
        }

        private WC_WorkGiverTable CreateTable()
        {
            return (WC_WorkGiverTable)Activator.CreateInstance(this.WorkGiverTableDef.workerClass, new object[]
            {
                this.WorkGiverTableDef,
                new Func<IEnumerable<WC_DataObject>>(() => this.Datas),
                UI.screenWidth - (int)(this.Margin * 2f),
                (int)((float)(UI.screenHeight - 35) - this.ExtraBottomSpace - this.ExtraTopSpace - this.Margin * 2f)
            });
        }

        protected void SetDirty()
        {
            this.table.SetDirty();
            this.SetInitialSizeAndPosition();
        }

        private WC_WorkGiverTable table;

    }
}

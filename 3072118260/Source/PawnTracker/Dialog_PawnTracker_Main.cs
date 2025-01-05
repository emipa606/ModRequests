using RimWorld;
using RimWorld.Planet;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static PawnTrackerMain.PawnTrackerUtility;

namespace PawnTrackerMain {
    public class Dialog_PawnTracker_Main : Window {
        private readonly QuickSearchFilter searchFilter = new QuickSearchFilter();
        private Vector2 scrollPosition = Vector2.zero;
        private string searchText = "";
        private PawnTrackerTab tab;
        public bool showDeadMissing = false;
        public bool showPrisoners = false;
        
        // Used by ContentRects *and* the diplay rects for both tabs
        internal float DefaultContentsY => 0f + tabHeight; // this is where the contents should START to be drawn
        internal float ContentsY = 0f; // this gets updated as we fill contents
        private float EmptyContentsHeight => pix + pad + 20f;
        private bool colorCodeEvents;
        private float ContentsHeight; // The height for the rect that is scrolled (not always all in view)
        private readonly float tabHeight = 35f; // Height for the tabs at the top -- helps us offset the rest of the contents in a dynamic way, were this to change
        private float RectEffectiveMinY => tabHeight + 10f; // The top of content-related rects should always be [some padding] below the tab height
        //

        // Specific to Pawn Menu view
        private readonly float pawnGridSearchHeight = 30f; // The height for the text box/checkboxes on the pawn grid; helps tell us where to start drawing the contents w/o overlapping
        public List<Pawn> DisplayPawns => MatchingPawns();
        public List<DeadPawnComp> DisplayDeadPawns => MatchingDeadPawns();
        internal float pix = 100f; // Height for the lil pawn tiles
        internal float pad = 10f; // used for Pawn Menu
        internal float drawPawnsX = -1;
        internal float drawPawnsY = -1;
        //

        public Dialog_PawnTracker_Main()
        {
            ContentsHeight = EmptyContentsHeight;
        }

        private List<Pawn> MatchingPawns()
        {
            List<Pawn> tracked_pawns = GetComp().TrackedPawns;
            foreach (Pawn p in tracked_pawns)
            {
                p.PawnTrackerComp().UpdatePawnStatus();
            }

            if (tracked_pawns == null)
            {
                Log.Error("TrackedPawns is null");
                return null;
            }

            tracked_pawns = tracked_pawns.Where(p => 
                p != null 
                && p.RaceProps.Humanlike 
                && (p.PawnTrackerComp().everWasColonist 
                    || p.PawnTrackerComp().pawnStatus == PawnTrackerUtility.PawnStatus.Colonist 
                    || p.PawnTrackerComp().pawnStatus == PawnTrackerUtility.PawnStatus.Prisoner
                )
            ).ToList();

            if (searchFilter == null)
            {
                tracked_pawns = tracked_pawns.Where(p => p != null).ToList();
                return tracked_pawns.Distinct().ToList();
            }
            tracked_pawns = tracked_pawns.Where(p => searchFilter.Matches(p.Name.ToString())).ToList();
            return tracked_pawns.Distinct().ToList();
        }

        private List<DeadPawnComp> MatchingDeadPawns()
        {
            List<DeadPawnComp> tracked_pawns = GetComp().DeadTrackedPawns.Values.ToList();
            if (tracked_pawns == null)
            {
                Log.Error("TrackedPawns is null");
                return null;
            }

            if (searchFilter == null)
            {
                return tracked_pawns.Distinct().ToList();
            }

            tracked_pawns = tracked_pawns.Where(c => searchFilter.Matches(c.Name)).ToList();
            return tracked_pawns.Distinct().ToList();
        }

        private void Setup()
        {
            this.forcePause = false;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside = false;
            this.soundAppear = SoundDefOf.InfoCard_Open;
            this.soundClose = SoundDefOf.InfoCard_Close;
            this.resizeable = true;
            this.draggable = true;
        }

        protected override void SetInitialSizeAndPosition()
        {
            Vector2 initialSize = new Vector2(700f, 700f);
            this.windowRect = new Rect((float) (((double) UI.screenWidth - (double) initialSize.x) / 2.0), (float) (((double) UI.screenHeight - (double) initialSize.y) / 2.0), initialSize.x, initialSize.y);
            this.windowRect = this.windowRect.Rounded();
        }

        public void SetCurrentTab(PawnTrackerTab tab) => this.tab = tab;

        public override void DoWindowContents(Rect rect)
        {
            List<TabRecord> tabs = new List<TabRecord>();
            tabs.Add(new TabRecord("Pawn Menu", (Action) (() => this.SetCurrentTab(PawnTrackerTab.PawnGrid)), this.tab == PawnTrackerTab.PawnGrid));
            
            tabs.Add(new TabRecord("Colony Overview", (Action) (() => this.SetCurrentTab(PawnTrackerTab.EventTimeline)), this.tab == PawnTrackerTab.EventTimeline));

            Rect tabRect = new Rect(0f, tabHeight, rect.width, tabHeight);

            TabDrawer.DrawTabs<TabRecord>(tabRect, tabs);

            if (this.tab == PawnTrackerTab.PawnGrid)
            {
                searchText = Widgets.TextField(new Rect(0, RectEffectiveMinY, 200, pawnGridSearchHeight), searchText);
                searchFilter.Text = searchText;
                Text.Anchor = TextAnchor.MiddleRight;
                
                float prisonerLabelWidth = Text.CalcSize("Show prisoners").x + 40f;
                float deadMissingLabelWidth = Text.CalcSize("Show dead and missing pawns").x + 40f;

                float checkboxSpacing = 25f;
                float deadMissingCheckboxX = rect.width - deadMissingLabelWidth - 10f; // 10f for some right padding
                float prisonerCheckboxX = deadMissingCheckboxX - prisonerLabelWidth - checkboxSpacing;
                
                Widgets.CheckboxLabeled(new Rect(prisonerCheckboxX, RectEffectiveMinY, prisonerLabelWidth, pawnGridSearchHeight), "Show prisoners", ref this.showPrisoners);
                Widgets.CheckboxLabeled(new Rect(deadMissingCheckboxX, RectEffectiveMinY, deadMissingLabelWidth, pawnGridSearchHeight), "Show dead and missing pawns", ref this.showDeadMissing);
                Text.Anchor = TextAnchor.UpperLeft;

                Rect scrollExtentsRect = new Rect(pad, RectEffectiveMinY + pawnGridSearchHeight, rect.width - 5f, rect.height - 90);
                Rect pawnGridRect = new Rect(0f, RectEffectiveMinY + pawnGridSearchHeight, rect.width - 25f, ContentsHeight);
                
                Widgets.BeginScrollView(scrollExtentsRect, ref this.scrollPosition, pawnGridRect);
                ContentsHeight = EmptyContentsHeight;
                FillPawnGrid(pawnGridRect);
                Widgets.EndScrollView();
            }
            else if (this.tab == PawnTrackerTab.EventTimeline)
            {
                Rect scrollExtentsRect = new Rect(pad, RectEffectiveMinY, rect.width - 5f, rect.height - 90);
                Rect eventOutlineRect = new Rect(0f, RectEffectiveMinY, rect.width - 25f, ContentsHeight);

                Widgets.BeginScrollView(scrollExtentsRect, ref this.scrollPosition, eventOutlineRect);
                ContentsHeight = EmptyContentsHeight;
                DoGameEventOutline(eventOutlineRect);

                Widgets.EndScrollView();
                Text.Anchor = TextAnchor.MiddleRight;
                float colorCodeLabelWidth = Text.CalcSize("Color code").x + 40f;
                float colorCodeCheckboxX = rect.width - colorCodeLabelWidth - 5f; // 10f for some right padding
                
                Widgets.CheckboxLabeled(new Rect(colorCodeCheckboxX, rect.height - 30f, colorCodeLabelWidth, 30f), "Color code", ref this.colorCodeEvents);
                Text.Anchor = TextAnchor.UpperLeft;
            }   
            return;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Setup();
            this.ContentsY = DefaultContentsY;
            foreach (Pawn pawn in PawnTrackerUtility.GetPawns(colonistStatus: PawnTrackerUtility.CSO.Any, deadOrAlive: PawnTrackerUtility.DOAO.Any))
            {
                if (pawn == null)
                    {continue;}
                if (pawn.PawnTrackerComp() == null)
                    {continue;}
                pawn.PawnTrackerComp().UpdateVarious();
            }
        }

        protected List<DeadPawnComp> SortDeadPawns(List<DeadPawnComp> subset)
        {
            List<DeadPawnComp> outList = subset.OrderBy(p => p.tickOfDeath).ToList();
            return outList;
        }

        protected List<Pawn> SortPawns(List<Pawn> subset, Type eventType)
        {
            List<Pawn> pawnList = subset.OrderBy(p =>
            {
                var sortEvents = p.PawnTrackerComp().GetDocumentedEvents(e => e.GetType() == eventType);
                if (sortEvents.Any())
                {
                    return sortEvents.Max(e => e.ticks);
                }
                return int.MinValue;
            }).ToList();

            return pawnList;
        }

        protected List<Pawn> SortPawns(List<Pawn> subset, DocumentedEventDef docEventDef)
        {
            List<Pawn> pawnList = subset.OrderBy(p =>
            {
                var sortEvents = p.PawnTrackerComp().GetDocumentedEvents(e => e.def == docEventDef);
                if (sortEvents.Any())
                {
                    return sortEvents.Max(e => e.ticks);
                }
                return int.MinValue;
            }).ToList();

            return pawnList;
        }

        public List<Pawn> ConcatenatedPawnList(params List<Pawn>[] pawnLists)
        {
            List<Pawn> outList = new List<Pawn>();
            foreach (List<Pawn> subset in pawnLists)
                outList.AddRange(subset);
            return outList.Where(p => p != null).ToList();
        }

        protected void FillPawnGrid(Rect rect)
        {
            // Push things down by pawnGridSearchHeight so it doesn't overlap the search bar
            this.ContentsY += pawnGridSearchHeight + 10f;;

            Widgets.ListSeparator(ref this.ContentsY, rect.width, "Current Colonists");
            DrawPawns(rect, SortPawns(DisplayPawns.Where(
                p => p != null && 
                p.PawnTrackerComp().shouldBeHere == true 
                && p.PawnTrackerComp().documentedEvents != null
                && p.PawnTrackerComp().documentedEvents.Any()
                && p.PawnTrackerComp().pawnStatus == PawnStatus.Colonist
                && !p.PawnTrackerComp().EventAfterEvent(e => e is JoinEvent, e => e.eventSubType == EventSubType.Capture)
                && (!p.Dead || p.PawnTrackerComp().ResurrectAfterDeath())
            ).ToList(), typeof(JoinEvent)));

            if (showDeadMissing)
            {
                Widgets.ListSeparator(ref this.ContentsY, rect.width, "Past Colonists");
                DrawPawns(rect, SortPawns(
                    DisplayPawns.Where(p => 
                        p != null
                        && p.PawnTrackerComp().documentedEvents != null 
                        && p.PawnTrackerComp().documentedEvents.Any()
                        && p.PawnTrackerComp().shouldBeHere == false 
                        && (
                            p.PawnTrackerComp().pawnStatus == PawnStatus.Colonist 
                            || p.PawnTrackerComp().everWasColonist
                        ) // They are, or ever WERE, a colonist
                        && !p.PawnTrackerComp().EventAfterEvent(e => e is JoinEvent, e => e.eventSubType == EventSubType.Capture)
                        && (!p.Dead || p.PawnTrackerComp().ResurrectAfterDeath()) // They're NOT dead
                    ).ToList(), typeof(JoinEvent)), newline: false);
                
                DrawPawns(rect, SortDeadPawns(
                    DisplayDeadPawns.Where(c => 
                        c.shouldBeHere == false && 
                        (
                            c.pawnStatus == PawnStatus.Colonist
                            || c.everWasColonist == true
                        )// They are, or ever WERE, a colonist
                        
                    ).ToList()),
                    startX: this.drawPawnsX);
            }

            if (showPrisoners)
            {
                Widgets.ListSeparator(ref this.ContentsY, rect.width, "Current Prisoners");
                DrawPawns(rect, SortPawns(
                    DisplayPawns.Where(p => 
                        p != null
                        && p.PawnTrackerComp().documentedEvents != null
                        && p.PawnTrackerComp().documentedEvents.Any()
                        && p.PawnTrackerComp().shouldBeHere
                        && p.PawnTrackerComp().pawnStatus == PawnStatus.Prisoner
                    ).ToList(), DocumentedEventDefOf.Captured));
                
                if (showDeadMissing)
                {
                    Widgets.ListSeparator(ref this.ContentsY, rect.width, "Past Prisoners");
                    DrawPawns(rect, SortPawns(
                        DisplayPawns.Where(p => 
                            p != null
                            && p.PawnTrackerComp().documentedEvents != null
                            && p.PawnTrackerComp().documentedEvents.Any()
                            && !p.PawnTrackerComp().shouldBeHere
                            && p.PawnTrackerComp().pawnStatus == PawnStatus.Prisoner
                            && (!p.Dead || p.PawnTrackerComp().ResurrectAfterDeath())
                        ).ToList(), DocumentedEventDefOf.Captured), newline: false);
                    
                    DrawPawns(rect, SortDeadPawns(
                        DisplayDeadPawns.Where(c => 
                        c.shouldBeHere == false && 
                        c.pawnStatus == PawnStatus.Prisoner)
                    .ToList()),
                    startX: this.drawPawnsX);
                }
            }
            this.ContentsY = this.DefaultContentsY;
        }

        public void DrawPawns(Rect rect, List<Pawn> pawns, float gridPix = -1f, float startX = -1f, float startY = -1f, bool newline = true)
        {
            if (startX == -1f)
                startX = this.pad;
            if (startY != -1f)
                this.ContentsY = startY;
            
            if (gridPix != -1)
                pix = gridPix;

            float pawnRectHeight = 0f;

            float x = startX;
            foreach (Pawn pawn in pawns)
            {
                if (pawn == null)
                    continue;
                if (pawn.PawnTrackerComp().pawnStatus != PawnTrackerUtility.PawnStatus.Colonist && pawn.PawnTrackerComp().pawnStatus != PawnTrackerUtility.PawnStatus.Prisoner)
                    continue;

                float textHeight = 60f; // default height for text
                string pawnName = pawn.Name.ToStringFull;

                // Check if the name would fit within the width
                Vector2 nameSize = Text.CalcSize(pawnName);
                float nameRatio = nameSize.x / pix;
                
                // If name goes on three lines or more, force to 2
                if (nameRatio > 2)
                {
                    try
                    {
                        NameTriple tripleName = (NameTriple) pawn.Name;
                        if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last)
                            pawnName = $"'{tripleName.Nick}' {tripleName.Last}";
                        else
                            pawnName = $"{tripleName.First} {tripleName.Last}";
                    }
                    catch {}
                }
                // If the name fits on one line, force to 2
                else if (nameRatio <= 1f)
                {
                    try
                    {
                        NameTriple tripleName = (NameTriple) pawn.Name;
                        //Make sure if first-and-middle on same line is doable...
                        if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last && (Text.CalcSize($"{tripleName.First} '{tripleName.Nick}'").x / pix <= 1))
                            pawnName = $"{tripleName.First} '{tripleName.Nick}'\n{tripleName.Last}";
                        else if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last)
                            pawnName = $"'{tripleName.Nick}'\n{tripleName.Last}";
                        else
                            pawnName = $"{tripleName.First}\n{tripleName.Last}";
                    }
                    catch {}
                }
                Rect pawnRect = new Rect(x, this.ContentsY, pix, pix + 40);
                pawnRectHeight = pawnRect.height;
                float nameY = pawnRect.yMax - 60;
                float textWidth = pix;
                //Get updated name size for what we've split to multiple lines
                nameSize = Text.CalcSize(pawnName);
                //If the name still wouldn't fit in the box, move the text up a line --> This wouldn't be the right approach if a single part of the name is what's going wide...
                //NEVER MIND --> if the name still wouldn't fit, widen the bounds of the name (may or may not be a good idea)
                /*if (nameSize.x > pix)
                {
                    textWidth += pix - nameSize.x;
                }*/
                if (Mouse.IsOver(pawnRect))
                {
                    Widgets.DrawBoxSolid(pawnRect, new Color(1f, 1f, 1f, 0.3f));
                    if (Widgets.ButtonInvisible(pawnRect))
                    {
                        Find.WindowStack.Add(new Dialog_PawnTracker_Details(pawn, showAllEvents: true));
                    }
                }
                Vector2 size = new Vector2(pix, pix);
                Rot4 south = Rot4.South;
                GUI.DrawTexture(new Rect(pawnRect.x, pawnRect.y, pix, pix), PortraitsCache.Get(pawn, size, south));

                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(new Rect(pawnRect.x, nameY, textWidth, textHeight), pawnName);

                Text.Anchor = TextAnchor.UpperLeft;
                x += pix + pad;
                if (x + pix >= rect.width + pad && !(pawn == pawns.Last()))
                {
                    x = pad;
                    this.ContentsY += pawnRectHeight + pad;
                    ContentsHeight += pawnRectHeight + pad;
                }
            }
            if (pawns != null && pawns.Any() && newline)
            {
                this.ContentsY += pawnRectHeight + pad + 10f;
                ContentsHeight += pawnRectHeight + pad + 10f;
            }  

            this.drawPawnsX = x;
        }

        public void DrawPawns(Rect rect, List<DeadPawnComp> pawns, float gridPix = -1, float startX = -1f, float startY = -1f, bool newline = true)
        {
            if (startX == -1f)
                startX = this.pad;
            if (startY != -1f)
                this.ContentsY = startY;
            
            if (gridPix != -1)
                pix = gridPix;

            float pawnRectHeight = 0f;

            float x = startX;
            foreach (DeadPawnComp comp in pawns)
            {
                if (comp.pawnStatus != PawnTrackerUtility.PawnStatus.Colonist && comp.pawnStatus != PawnTrackerUtility.PawnStatus.Prisoner)
                    continue;

                float textHeight = 60f; // default height for text
                string pawnName = comp.Name;

                // Check if the name would fit within the width
                Vector2 nameSize = Text.CalcSize(pawnName);
                float nameRatio = nameSize.x / pix;
                
                // If name goes on three lines or more, force to 2
                if (nameRatio > 2)
                {
                    try
                    {
                        NameTriple tripleName = comp.PawnTripleName;
                        if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last)
                            pawnName = $"'{tripleName.Nick}' {tripleName.Last}";
                        else
                            pawnName = $"{tripleName.First} {tripleName.Last}";
                    }
                    catch {}
                }
                // If the name fits on one line, force to 2
                else if (nameRatio <= 1f)
                {
                    try
                    {
                        NameTriple tripleName = comp.PawnTripleName;
                        //Make sure if first-and-middle on same line is doable...
                        if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last && (Text.CalcSize($"{tripleName.First} '{tripleName.Nick}'").x / pix <= 1))
                            pawnName = $"{tripleName.First} '{tripleName.Nick}'\n{tripleName.Last}";
                        else if (tripleName.Nick != tripleName.First && tripleName.Nick != tripleName.Last)
                            pawnName = $"'{tripleName.Nick}'\n{tripleName.Last}";
                        else
                            pawnName = $"{tripleName.First}\n{tripleName.Last}";
                    }
                    catch {}
                }
                Rect pawnRect = new Rect(x, this.ContentsY, pix, pix + 40);
                pawnRectHeight = pawnRect.height;
                float nameY = pawnRect.yMax - 60;
                float textWidth = pix;
                //Get updated name size for what we've split to multiple lines
                nameSize = Text.CalcSize(pawnName);
                //If the name still wouldn't fit in the box, move the text up a line --> This wouldn't be the right approach if a single part of the name is what's going wide...
                //NEVER MIND --> if the name still wouldn't fit, widen the bounds of the name (may or may not be a good idea)
                /*if (nameSize.x > pix)
                {
                    textWidth += pix - nameSize.x;
                }*/
                if (Mouse.IsOver(pawnRect))
                {
                    Widgets.DrawBoxSolid(pawnRect, new Color(1f, 1f, 1f, 0.3f));
                    if (Widgets.ButtonInvisible(pawnRect))
                    {
                        Find.WindowStack.Add(new Dialog_PawnTracker_Details_Dead(comp));
                    }
                }

                //Try get portrait from comp filepath; if doesn't exist, try from pawn name...
                Texture2D portrait = PortraitManager.GetSavedPortrait(comp.PortraitPath);
                if (portrait == null)
                {
                    portrait = PortraitManager.GetSavedPortraitFromPawnName(comp.Name);
                }

                GUI.DrawTexture(new Rect(pawnRect.x, pawnRect.y, pix, pix), portrait);


                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(new Rect(pawnRect.x, nameY, textWidth, textHeight), pawnName);

                Text.Anchor = TextAnchor.UpperLeft;
                x += pix + pad;
                if (x + pix >= rect.width + pad && !(comp == pawns.Last()))
                {
                    x = pad;
                    this.ContentsY += pawnRectHeight + pad;
                    ContentsHeight += pawnRectHeight + pad;
                }
            }
            if (pawns != null && pawns.Any() && newline)
            {
                this.ContentsY += pawnRectHeight + pad + 10f;
                ContentsHeight += pawnRectHeight + pad + 10f;
            }    

            this.drawPawnsX = x;
        }


        public void DoGameEventOutline(Rect rect)
        {
            List<DocumentedEventDef> combs = new List<DocumentedEventDef>() {DocumentedEventDefOf.Captured, DocumentedEventDefOf.Rescued};
            float x = 0f;
            float y = RectEffectiveMinY;
            string date = null;
            ContentsHeight = EmptyContentsHeight;

            List<EventBasics> events = AllCoreEvents();
            List<EventBasics> documented = new();
            
            foreach (EventBasics docEvent in events)
            {
                DrawGameEventOutlineRow(rect, ref date, ref x, ref y, ref documented, docEvent);
            }
            
            y += 25f;
            ContentsHeight += 25f;
        }

        public void DrawGameEventOutlineRow(Rect rect, ref string date, ref float x, ref float y, ref List<EventBasics> documented, EventBasics docEvent, float inset = 0f)
        {
            if (documented.Contains(docEvent))
                return;
                
            string detailString = "";
            string hourString = null;
            int lines = 1;

            float detailsLineRectHeight = 0;
            
            if (date != $"Day {docEvent.sortDay}, Year {docEvent.sortYear}")
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                GUIStyle dateStyle = new GUIStyle();
                dateStyle.font = CHGameComp.BerlinSansDemibold;
                dateStyle.fontSize = 16; // Adjust as necessary
                dateStyle.normal.textColor = Color.white;
                Rect dateHeaderRect = new Rect(x, y, rect.width, 20f);
                string dateHeader = $"Day {docEvent.sortDay}, Year {docEvent.sortYear}";
                date = dateHeader;
                
                Widgets.DrawBoxSolid(dateHeaderRect, new Color(1f, 1f, 1f, 0.3f));
                GUI.Label(dateHeaderRect, dateHeader, dateStyle);
                y += 20f;
                this.ContentsHeight += 20f;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            detailString = docEvent.description;

            if (detailString == null)
            {
                LogPTMDev.Error("detailString is null");
            }
            else if (docEvent.thing is DocumentedEvent de1 && de1.def != DocumentedEventDefOf.Kidnapped && de1.associatedEvent != null && (de1.associatedEvent is GameRaidEvent || de1.associatedEvent is GameManhunterEvent))
            {
                if (detailString.Contains("unknown causes"))
                    detailString += GameEventInjuriesDescription(de1.associatedEvent);
                else
                {
                    detailString += " during";
                    if (de1.associatedEvent is GameRaidEvent gre1)
                        detailString += $" the recent raid by {gre1.raidersFaction}";
                    else if (de1.associatedEvent is GameManhunterEvent gme1)
                        detailString += $" the recent manhunter {gme1.animalDef.label} attack";
                }
                //if (de1 is DeathEvent && (de1.associatedEvent is GameRaidEvent || de1.associatedEvent is GameManhunterEvent))
                //    detailString += " following";
                //else

                
            }
            else if (docEvent.thing is DeadPawnEvent dpe && dpe.associatedEvent != null && (dpe.associatedEvent is GameRaidEvent || dpe.associatedEvent is GameManhunterEvent))
            {
                if (detailString.Contains("unknown causes"))
                    detailString += GameEventInjuriesDescription(dpe.associatedEvent);
                else
                {
                    if (dpe.associatedEvent is GameRaidEvent gre1)
                        detailString += $" following the recent raid by {gre1.raidersFaction}";
                    else if (dpe.associatedEvent is GameManhunterEvent gme1)
                        detailString += $" following the recent manhunter {gme1.animalDef.label} attack";
                }
                
            }

            float x_pos = inset == 0 ? x + 55f + inset : x + 35f + inset;
            float width = rect.width - 70f - inset;

            hourString = $"({docEvent.sortHour}h)";
                
            Vector2 textWidth = Text.CalcSize(detailString);
            double textRatio = (double)textWidth.x / width;
            lines = (int)Math.Ceiling(textRatio);

            detailsLineRectHeight = 25f * lines;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect hourRect = new Rect(x + inset, y, 50f, detailsLineRectHeight);
            Widgets.Label(hourRect, hourString);

            this.ContentsHeight += detailsLineRectHeight; 
            Rect detailsRect = new Rect(x_pos, y, width, detailsLineRectHeight);
            Widgets.Label(detailsRect, detailString);
            if (colorCodeEvents)
            {
                Rect lineBackground = new Rect(x, y, rect.width, 25f * lines).ContractedBy(3f);
                bool color = false;
                Color lineColor = new Color();

                

                if (docEvent.thing is JoinEvent || (docEvent.thing is DeadPawnEvent dpe1 && dpe1.eventType == "Join"))
                {
                    lineColor = GetComp().joinColor;
                    color = true;
                }
                else if (docEvent.def == DocumentedEventDefOf.Kidnapped)
                {
                    lineColor = GetComp().missingColor;
                    color = true;
                }
                else if (docEvent.thing is DeathEvent || (docEvent.thing is DeadPawnEvent dpe2 && dpe2.eventType == "Death"))
                {
                    lineColor = GetComp().deathColor;
                    color = true;
                }
                else if (docEvent.thing is GameEvent)
                {
                    lineColor = GetComp().gameEventColor;
                    color = true;
                }
                else if (docEvent.thing is LifeEvent le)
                {
                    switch (le.category)
                    {
                        case LifeEventCategory.Positive:
                            lineColor = GetComp().lifeColorPositive;
                            color = true;
                            break;
                        case LifeEventCategory.Neutral:
                            lineColor = GetComp().lifeColorNeutral;
                            color = true;
                            break;
                        case LifeEventCategory.Negative:
                            lineColor = GetComp().lifeColorNegative;
                            color = true;
                            break;
                        default: 
                            break;
                    }                            
                }
                else if (docEvent.thing is DeadPawnEvent dpe3 && dpe3.eventType == "Misc")
                {
                    switch (dpe3.lifeEventCategory)
                    {
                        case LifeEventCategory.Positive:
                            lineColor = GetComp().lifeColorPositive;
                            color = true;
                            break;
                        case LifeEventCategory.Neutral:
                            lineColor = GetComp().lifeColorNeutral;
                            color = true;
                            break;
                        case LifeEventCategory.Negative:
                            lineColor = GetComp().lifeColorNegative;
                            color = true;
                            break;
                        default: 
                            break;
                    }                            
                }
                if (color)
                    Widgets.DrawBoxSolid(lineBackground, lineColor);
            }
            y += detailsLineRectHeight;
            Widgets.DrawLineHorizontal(0, y, rect.width);    
            y += 3f;   
            this.ContentsHeight += 3f;
            Text.Anchor = TextAnchor.UpperLeft;

            documented.Add(docEvent);

            foreach (EventBasics subEvent in docEvent.subEvents)
            {
                DrawGameEventOutlineRow(rect, ref date, ref x, ref y, ref documented, subEvent, inset: 55f);
            }    
        }
    
        public bool RaidRelated(EventBasics docEvent, GameRaidEvent lastRaid)
        {
            if (lastRaid != null && (docEvent.thing is DeadPawnEvent || docEvent.thing is DeathEvent || (docEvent.thing is LeaveEvent leaveEvent && leaveEvent.def == DocumentedEventDefOf.Kidnapped)))
            {
                if (docEvent.thing is DeathEvent deathEvent1 && deathEvent1.ticks > lastRaid.tickStart && deathEvent1.ticks < lastRaid.tickEnd)
                    return true;

                if (docEvent.thing is DeathEvent deathEvent2 && deathEvent2.Description().Contains(lastRaid.raidersFaction.ToString()))
                    return true;

                if (docEvent.thing is DeadPawnEvent deadPawnEvent1 && deadPawnEvent1.ticks > lastRaid.tickStart && deadPawnEvent1.ticks < lastRaid.tickEnd)
                    return true;

                if (docEvent.thing is DeadPawnEvent deadPawnEvent2 && deadPawnEvent2.description.Contains(lastRaid.raidersFaction.ToString()))
                    return true;

                if (docEvent.thing is LeaveEvent leaveEvent1 && leaveEvent1.kidnappers == lastRaid.raidersFaction)
                    return true;
                
                return false;
            }
            return false;
        }

        public bool ManhunterRelated(EventBasics docEvent, GameManhunterEvent lastManhunter)
        {
            if (lastManhunter != null && (docEvent.thing is DeadPawnEvent || docEvent.thing is DeathEvent))
            {
                if (docEvent.thing is DeathEvent deathEvent2 && deathEvent2.Description().Contains(lastManhunter.animalDef.label))
                    return true;

                if (docEvent.thing is DeadPawnEvent deadPawnEvent2 && deadPawnEvent2.description.Contains(lastManhunter.animalDef.label))
                    return true;
                
                return false;
            }
            return false;
        }


        public enum PawnTrackerTab : byte
        {
            PawnGrid,
            EventTimeline,
        }
    }

    
}
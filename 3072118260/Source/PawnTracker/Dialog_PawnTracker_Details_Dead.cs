using RimWorld;
using RimWorld.Planet;
using System;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static PawnTrackerMain.PawnTrackerUtility;

namespace PawnTrackerMain
{
    public class Dialog_PawnTracker_Details_Dead : Window {
        private DeadPawnComp comp;
        private bool showLifeEvents = true;
        private bool colorCodeEvents = true;
        
        private List<DeadPawnEvent> DocumentEvents => EventsToDocument();
       
        private Vector2 scrollPosition = Vector2.zero;
        private float EmptyDetailsHeight = 0f;
        private float DetailsRectHeight;

        private void Setup()
        {
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.soundAppear = SoundDefOf.InfoCard_Open;
            this.soundClose = SoundDefOf.InfoCard_Close;
            this.resizeable = true;
        }

        public Dialog_PawnTracker_Details_Dead(DeadPawnComp comp, bool showLifeEvents = true) 
        {
            this.comp = comp;
            this.showLifeEvents = showLifeEvents;
        }

        public List<DeadPawnEvent> EventsToDocument()
        {
            return showLifeEvents ? comp.documentedEvents : comp.documentedEvents.Where(d => d.eventType != "Misc").ToList();
        }

        public override void DoWindowContents(Rect rect)
        {
            DoEventOutline(rect);
            return;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Setup();
            comp.OrderPawnHistory();            
        }

        public void DoEventOutline(Rect rect)
        {
            List<DocumentedEventDef> combs = new List<DocumentedEventDef>() {DocumentedEventDefOf.Captured, DocumentedEventDefOf.Rescued};
            float x = 0f;
            float y = 5f;
            string date = null;
            int index = 0;
            string detailString = null;
            string hourString = null;
            int lines = 1;

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.font = CHGameComp.BerlinSansDemibold;
            nameStyle.fontSize = 19; // Adjust as necessary
            nameStyle.normal.textColor = Color.white;
            Rect nameHeaderRect = new Rect(x, y, rect.width, 20f);
            string nameHeader = $"{PawnName(comp.PawnTripleName)} (Dead)";
            
            GUI.Label(nameHeaderRect, nameHeader, nameStyle);

            float showLifeEventsLabelWidth = Text.CalcSize("Show life events").x + 40f;
            float showLifeEventsCheckboxX = rect.width - showLifeEventsLabelWidth - 5f; // 10f for some right padding
            
            Widgets.CheckboxLabeled(new Rect(showLifeEventsCheckboxX, 5f, showLifeEventsLabelWidth, 30f), "Show life events", ref this.showLifeEvents);
            Text.Anchor = TextAnchor.UpperLeft;
            y += 25f;
            
            Rect listRect = new Rect(x, y, rect.width - 20f, DetailsRectHeight); 
            DetailsRectHeight = EmptyDetailsHeight;
            Rect outRect = new Rect(x, y, rect.width, rect.height - 74f);

            Widgets.BeginScrollView(outRect, ref this.scrollPosition, listRect);

            foreach (DeadPawnEvent docEvent in DocumentEvents)
            {
                float detailsLineRectHeight = 0;
                if (combs.Contains(docEvent.def) && index-1 >= 0 && DocumentEvents[index - 1].def is PawnTrackerMain.ArriveEventDef)
                {
                    index++;
                    continue;
                }
                
                if (date != $"Day {docEvent.dayOfYear}, Year {docEvent.year}")
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    GUIStyle dateStyle = new GUIStyle();
                    dateStyle.font = CHGameComp.BerlinSansDemibold;
                    dateStyle.fontSize = 16; // Adjust as necessary
                    dateStyle.normal.textColor = Color.white;
                    Rect dateHeaderRect = new Rect(x, y, rect.width, 20f);
                    string dateHeader = $"Day {docEvent.dayOfYear}, Year {docEvent.year}";
                    date = dateHeader;
                    
                    Widgets.DrawBoxSolid(dateHeaderRect, new Color(1f, 1f, 1f, 0.3f));
                    GUI.Label(dateHeaderRect, dateHeader, dateStyle);
                    y += 20f;
                    DetailsRectHeight += 20f;
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                

                if (DocumentEvents[index].def is DeathEventDef)
                {
                    // If it's an unknown death but associated with an event....change how we present it
                    if (DocumentEvents[index].def == DocumentedEventDefOf.UnknownDeath && DocumentEvents[index].associatedEvent != null)
                    {
                        var assoc = DocumentEvents[index].associatedEvent;
                        detailString = $"{comp.PawnLabelShort} {GameEventInjuriesDescription(assoc)}";
                    }
                    else
                    {
                        detailString = $"{comp.PawnLabelShort} {docEvent.dinfo}";    
                    }
                    
                }
                else if (docEvent.def is ArriveEventDef && index + 1 < DocumentEvents.Count() && combs.Contains(DocumentEvents[index + 1].def))
                {
                    detailString = $"{comp.PawnLabelShort} {DocumentEvents[index + 1].description} after {comp.Pronoun} {docEvent.description}";
                }    
                else
                {
                    detailString = $"{comp.PawnLabelShort} {docEvent.description}";
                }
                hourString = $"({docEvent.hour}h)";

                Vector2 textWidth = Text.CalcSize(detailString);
                double textRatio = (double)textWidth.x / (rect.width - 70f);
                lines = (int)Math.Ceiling(textRatio);
                
                detailsLineRectHeight = 25f * lines;

                Text.Anchor = TextAnchor.MiddleLeft;
                Rect hourRect = new Rect(x, y, 50f, detailsLineRectHeight);
                Widgets.Label(hourRect, hourString);

                DetailsRectHeight += detailsLineRectHeight; 

                Rect detailsRect = new Rect(x + 55f, y, rect.width - 70f, detailsLineRectHeight);
                Widgets.Label(detailsRect, detailString);

                if (colorCodeEvents)
                {
                    Rect lineBackground = new Rect(x, y, rect.width, detailsLineRectHeight).ContractedBy(3f);
                    bool color = false;
                    Color lineColor = new Color();

                    if (docEvent.eventType == "Join")
                    {
                        lineColor = GetComp().joinColor;
                        color = true;
                    }
                    else if (docEvent.def == DocumentedEventDefOf.Kidnapped)
                    {
                        lineColor = GetComp().missingColor;
                        color = true;
                    }
                    else if (docEvent.def is DeathEventDef)
                    {
                        lineColor = GetComp().deathColor;
                        color = true;
                    }
                    else if (docEvent.def is LifeEventDef le)
                    {
                        switch (le.Category)
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
                DetailsRectHeight += 3f;
                index++;   
                Text.Anchor = TextAnchor.UpperLeft;
            }
            Widgets.EndScrollView();

            float deleteButtonWidth = Text.CalcSize("Permanently\nuntrack").x + 40f;
            if (GetComp().DeadTrackedPawns.ContainsKey(comp.Name) && (comp.pawnStatus == PawnTrackerUtility.PawnStatus.Colonist || comp.pawnStatus == PawnTrackerUtility.PawnStatus.Prisoner))
            {
                if (Widgets.ButtonText(new Rect(10f, rect.height - 50f, deleteButtonWidth, 50f), "Permanently\nuntrack"))
                {
                    Find.WindowStack.Add(new Dialog_ConfirmManualUntrack_Dead(comp.Name));
                }
            }

            Text.Anchor = TextAnchor.MiddleRight;
            float colorCodeLabelWidth = Text.CalcSize("Color code").x + 40f;
            float colorCodeCheckboxX = rect.width - colorCodeLabelWidth - 5f; // 10f for some right padding
            
            Widgets.CheckboxLabeled(new Rect(colorCodeCheckboxX, rect.height - 30f, colorCodeLabelWidth, 30f), "Color code", ref this.colorCodeEvents);
            Text.Anchor = TextAnchor.UpperLeft;
            y += 25f;
            DetailsRectHeight += 25f;
        }
    }    
}
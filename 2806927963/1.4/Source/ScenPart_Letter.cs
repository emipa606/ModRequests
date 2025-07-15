using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.ScenarioLetters
{
	public class ScenPart_Letter : ScenPart
	{
        // saved fields
        public string title = "Letter_Initial_Title".Translate();
		public string body = "Letter_Initial_Body".Translate();
		public float occursOnDay;
		public bool letterSent = false;

		// temporary fields
		public string occursOnDayBuffer;
		public Vector2 textScrollbarPosition = new Vector2();

		public override string Summary(Scenario scen)
		{
			Log.Debug("called ScenPart_Letter.Summary()");
			//return "ScenPart_PawnsAreNaked".Translate(context.ToStringHuman(), chance.ToStringPercent()).CapitalizeFirst();
			return "Letter_Summary_Description".Translate(title, Math.Floor(occursOnDay));
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref title, "title", "Letter_Initial_Title".Translate());
			Scribe_Values.Look(ref body, "body", "Letter_Initial_Body".Translate());
			Scribe_Values.Look(ref occursOnDay, "occursOnDay");
			Scribe_Values.Look(ref letterSent, "letterSent", false);
		}

        public override void Tick()
        {
			Log.DebugOnce("at least ScenPart_Letter.Tick() is being called regularly");
			// todo: show the dialog when the time is right
			if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
			{
				if (!letterSent)
				{
					Log.DebugOnce("letter was not sent yet: '" + title + "'", "letter startup check " + title);
					if (Find.TickManager.TicksGame >= occursOnDay * 60000f)
                    {
						Log.Debug("target date reached for '" + title + "'. Sending letter.");
						// alternative presentations confirmation dialog, letter and message...
						//Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DanielRenner: Cravings - test dialog injected at tick " + Find.TickManager.TicksGame, delegate { return; })); // this has a go back and confirm buttons
						Find.LetterStack.ReceiveLetter(title, body, LetterDefOf.NeutralEvent, RimWorld.Planet.GlobalTargetInfo.Invalid, null);
						//Messages.Message("DanielRenner: Cravings - test message injected at tick " + Find.TickManager.TicksGame, MessageTypeDefOf.CautionInput); // messages are too hidden...

						// make sure not to send that same letter again...
						this.letterSent = true;
						
					}
					else
                    {
						Log.DebugOnce("target date for '" + title + "' is not reached yet: day "+ occursOnDay);
					}
				}
				else
				{
					Log.DebugOnce("letter was already sent: '" + title + "'", "letter startup check " + title);
				}

			}
		}

		private const float TotalHeightInRows = HeightBodyInRows + HeightTitleInRows + HeightDayFieldInRows;
		private const float HeightTitleInRows = 1f;
		private const float HeightBodyInRows = 5f;
		private const float HeightDayFieldInRows = 1f;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			// todo: labels for the text fields
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * TotalHeightInRows);
			Rect rectTitle = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height * HeightTitleInRows  / TotalHeightInRows);
			Rect rectBody = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * HeightTitleInRows / TotalHeightInRows, scenPartRect.width, scenPartRect.height * HeightBodyInRows / TotalHeightInRows);
			Rect rectDate = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * (HeightTitleInRows + HeightBodyInRows) / TotalHeightInRows, scenPartRect.width, scenPartRect.height * HeightDayFieldInRows / TotalHeightInRows);

			title = Widgets.TextArea(rectTitle, title);
			body = Widgets.TextAreaScrollable(rectBody, body, ref textScrollbarPosition);
			Widgets.TextFieldNumericLabeled(rectDate, "Letter_Occurs_On_Day".Translate(), ref occursOnDay, ref occursOnDayBuffer);
		}
	}

}

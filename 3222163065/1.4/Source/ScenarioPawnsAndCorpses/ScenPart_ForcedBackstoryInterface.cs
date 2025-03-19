using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// Base class for scenparts adding backstories
	/// </summary>
	public abstract class ScenPart_ForcedBackstoryInterface: ScenPart_PawnModifier
	{
		/// <summary>
		/// The backstory def this scenpart will add
		/// </summary>
		private BackstoryDef backstoryDef;

		/// <summary>
		/// The backstory title this scenpart will add. This is only to make choosing backstories easier in handwritten scenarios; the scenpart will translate this to the backstoryDef.
		/// </summary>
		protected string backstoryTitle;

        /// <summary>
        /// Get the backstory def this scenpart will add; if the handwritten scenario specified the title, this will get the translated backstory def.
        /// </summary>
        protected BackstoryDef BackstoryDef
        {
            get
            {
                if (this.backstoryDef == null && this.backstoryTitle != null)
                {
					this.backstoryDef = DefDatabase<BackstoryDef>.AllDefs.Where(b => b.title.Equals(this.backstoryTitle, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                return this.backstoryDef;
            }
        }

        /// <summary>
        /// Get the pawn context for the pawns this backstory will be added to
        /// </summary>
        /// <returns>the pawn context of this scenpart</returns>
        public PawnGenerationContext GetContext()
        {
			return this.context;
        }

		/// <summary>
		/// Get the backstory this scenpart is adding
		/// </summary>
		/// <returns>the backstory of this scenpart</returns>
		//public BackstoryDef GetBackstory()
		//{
		//	return DefDatabase<BackstoryDef>.GetNamedSilentFail(this.BackstoryDef);
		//}

		/// <summary>
		/// Get the backstory slot this scenpart can add
		/// </summary>
		/// <returns>the backstory slot this scenpart can add</returns>
		public abstract BackstorySlot GetBackstorySlot();

		protected IEnumerable<BackstoryDef> GetAllBackstories()
		{
			return DefDatabase<BackstoryDef>.AllDefs.Where(b => b.slot == this.GetBackstorySlot()).GroupBy(b => b.title).Select(x => x.First()).OrderBy(b => b.title);
		}

		/// <summary>
		/// Get the chance the backstory will be added to the pawn of the appropriate backstory
		/// </summary>
		/// <returns>the chance the backstory will be applied</returns>
		public float GetChance()
        {
			return this.chance;
        }


		/// <summary>
		/// Save backstory info: the def and title if applicable
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref this.backstoryDef, "backstoryDef");
			Scribe_Values.Look(ref this.backstoryTitle, "backstoryTitle");
		}

		/// <summary>
		/// Setup the UI for the part in the scenario edit interface
		/// </summary>
		/// <param name="listing">The list host for the scen part</param>
		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
			if (Widgets.ButtonText(scenPartRect.TopPart(0.333f), this.BackstoryDef.title))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();

				foreach (BackstoryDef item in this.GetAllBackstories().OrderBy(b => b.title))
				{
					list.Add(new FloatMenuOption(item.title, delegate
					{
						this.backstoryDef = item;
					}));

				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			this.DoPawnModifierEditInterface(scenPartRect.BottomPart(0.666f));
		}

		/// <summary>
		/// Get the summary for this scenpart
		/// </summary>
		/// <param name="scen">unused</param>
		/// <returns>The string representation of this backstory scenpart</returns>
		public override string Summary(Scenario scen)
		{
			return "ScenarioPawnsAndCorpses.ScenPart_PawnsHaveBackstory".Translate(
				this.context.ToStringHuman(),
				this.chance.ToStringPercent(),
				this.GetBackstorySlot().ToString(),
				this.BackstoryDef.title).CapitalizeFirst();
		}

		/// <summary>
		/// Setup defaults for a new scenpart
		/// </summary>
		public override void Randomize()
		{
			base.Randomize();
			IEnumerable<BackstoryDef> backstories = this.GetAllBackstories();
			this.backstoryDef = backstories.ElementAt(Rand.Range(0, backstories.Count()));
		}

		/// <summary>
		/// Handle the Notify_EarlyNewPawnGenerating notification if applicable
		/// </summary>
		/// <param name="pawn">The pawn being generated</param>
		/// <param name="context">The pawn's context</param>
		public void Notify_EarlyNewPawnGenerating(Pawn pawn, PawnGenerationContext context)
		{
			if (this.context.Includes(context) && (!hideOffMap || context != PawnGenerationContext.PlayerStarter) && pawn.RaceProps.Humanlike)
			{
				this.ModifyPawnEarlyGenerate(pawn);
			}
		}

		/// <summary>
		/// Add the scenpart's backstory to the provided pawn
		/// </summary>
		/// <param name="pawn">The pawn being generated</param>
		protected void ModifyPawnEarlyGenerate(Pawn pawn)
		{
			if (this.GetBackstorySlot() == BackstorySlot.Childhood)
			{
				pawn.story.Childhood= this.BackstoryDef;
			}
			else
			{
				pawn.story.Adulthood = this.BackstoryDef;
			}
		}
	}
}

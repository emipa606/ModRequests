using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// ScenPart that enables the player arriving with corpses
	/// </summary>
	public class ScenPart_StartingCorpse : ScenPart_CorpseyCorpseyCorpseCorpse
	{
		/// <summary>
		/// Get the summary for this scenpart
		/// </summary>
		/// <param name="scen">unused</param>
		/// <returns>The string representation of this starting corpse scenpart</returns>
		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
		}

		/// <summary>
		/// Get the summary list entries (ie scenario overview list) for this scenpart
		/// </summary>
		/// <param name="tag">tag to match</param>
		/// <returns>smmary list entries</returns>
		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "PlayerStartsWith")
			{
				yield return this.CurrentCorpseLabel().CapitalizeFirst() + " x" + count;
			}
		}

		/// <summary>
		/// Try to merge two scenparts of the same pawndef togther
		/// </summary>
		/// <param name="other">The other scenpart to review merge conditions</param>
		/// <returns>whether the two scenparts can be merged</returns>
		public override bool TryMerge(ScenPart other)
		{
			ScenPart_StartingCorpse scenPart_StartingCorpse = other as ScenPart_StartingCorpse;
			if (scenPart_StartingCorpse != null && scenPart_StartingCorpse.pawnKindDef == this.pawnKindDef)
			{
				this.count += scenPart_StartingCorpse.count;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Create the corpses that will be added to the player starting things
		/// </summary>
		/// <returns>the created corpses</returns>
		public override IEnumerable<Thing> PlayerStartingThings()
		{
			return this.GetCorpses();
		}
	}
}
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// ScenPart that enables the scattering of corpses nearby the player
	/// </summary>
	public class ScenPart_ScatteredCorpseNearPlayerStart : ScenPart_ScatteredCorpse
	{
		/// <summary>
		/// Tag defining near player map scatter
		/// </summary>
		public const string PlayerStartsWithTag = "PlayerStartsWith";

		/// <summary>
		/// Whether the corpses should be scattered near the player start location. Scatter near player = true.
		/// </summary>
		protected override bool NearPlayerStart => true;

		/// <summary>
		/// Get the summary for this scenpart
		/// </summary>
		/// <param name="scen">unused</param>
		/// <returns>The string representation of this corpse scatter scenpart</returns>
		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, ScenPart_ScatteredCorpseNearPlayerStart.PlayerStartsWithTag, ScenPart_StartingThing_Defined.PlayerStartWithIntro);
		}

		/// <summary>
		/// Get the summary list entries (ie scenario overview list) for this scenpart
		/// </summary>
		/// <param name="tag">tag to match</param>
		/// <returns>summary list entries</returns>
		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == ScenPart_ScatteredCorpseNearPlayerStart.PlayerStartsWithTag)
			{
				yield return this.CurrentCorpseLabel().CapitalizeFirst() + " x" + count;
			}
		}
	}
}

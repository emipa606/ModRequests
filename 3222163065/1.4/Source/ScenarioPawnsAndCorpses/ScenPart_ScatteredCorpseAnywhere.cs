using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// ScenPart that enables the scattering of corpses anywhere on the map
	/// </summary>
	public class ScenPart_ScatteredCorpseAnywhere : ScenPart_ScatteredCorpse
	{
		/// <summary>
		/// Tag defining full map scatter
		/// </summary>
		public const string MapScatteredWithTag = "MapScatteredWith";

		/// <summary>
		/// Whether the corpses should be scattered near the player start location. Scatter anywhere = false.
		/// </summary>
		protected override bool NearPlayerStart => false;

		/// <summary>
		/// Get the summary for this scenpart
		/// </summary>
		/// <param name="scen">unused</param>
		/// <returns>The string representation of this corpse scatter scenpart</returns>
		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, ScenPart_ScatteredCorpseAnywhere.MapScatteredWithTag, "ScenPart_MapScatteredWith".Translate());
		}

		/// <summary>
		/// Get the summary list entries (ie scenario overview list) for this scenpart
		/// </summary>
		/// <param name="tag">tag to match</param>
		/// <returns>smmary list entries</returns>
		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == ScenPart_ScatteredCorpseAnywhere.MapScatteredWithTag)
			{
				yield return this.CurrentCorpseLabel().CapitalizeFirst() + " x" + count;
			}
		}
	}
}
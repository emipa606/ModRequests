using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// Base class for ScenParts that generate and scatter corpses
	/// </summary>
	public abstract class ScenPart_ScatteredCorpse : ScenPart_CorpseyCorpseyCorpseCorpse
	{
		/// <summary>
		/// Whether to allow corpses under roofed areas. This is probably always goodness as true.
		/// </summary>
		public bool allowRoofed = true;

		/// <summary>
		/// Whether the corpses should be scattered near the player start location.
		/// </summary>
		protected abstract bool NearPlayerStart { get; }

		/// <summary>
		/// Generate corpses into the map.
		/// </summary>
		/// <param name="map">The starting map</param>
		public override void GenerateIntoMap(Map map)
		{
			if (Find.GameInitData != null)
			{
				GenStep_ScatterCorpses genStep_ScatterThings = new GenStep_ScatterCorpses(this.GetCorpses(false));
				genStep_ScatterThings.nearPlayerStart = this.NearPlayerStart;
				genStep_ScatterThings.allowFoggedPositions = !this.NearPlayerStart;
				genStep_ScatterThings.spotMustBeStandable = true;
				genStep_ScatterThings.minSpacing = 1f;
				genStep_ScatterThings.allowRoofed = this.allowRoofed;
				genStep_ScatterThings.Generate(map, default(GenStepParams));
			}
		}

		/// <summary>
		/// Save corpse scatter info... which is pretty much only the allowRoofed field which is currently always true
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref this.allowRoofed, "allowRoofed", defaultValue: false);
		}
	}
}
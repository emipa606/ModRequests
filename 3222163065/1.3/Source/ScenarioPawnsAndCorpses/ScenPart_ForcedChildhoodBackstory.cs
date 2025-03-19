using RimWorld;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// Scenpart allowing a childhood backstory to be added to pawns
	/// </summary>
	public class ScenPart_ForcedChildhoodBackstory : ScenPart_ForcedBackstoryInterface
	{
		/// <summary>
		/// The backstory slot this scenpart can add
		/// </summary>
		private const BackstorySlot backstorySlot = BackstorySlot.Childhood;

		/// <summary>
		/// Get the backstory slot this scenpart can add
		/// </summary>
		/// <returns>the backstory slot this scenpart can add</returns>
		public override BackstorySlot GetBackstorySlot()
        {
			return ScenPart_ForcedChildhoodBackstory.backstorySlot;
		}

		/// <summary>
		/// Whether the backstory this scenpart is adding can coexist with other backstories from other scenparts
		/// </summary>
		/// <param name="other">A scenpart to review</param>
		/// <returns>Whether this scenpart can coexist with the other</returns>
		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_ForcedChildhoodBackstory scenPart_ForcedChildhoodBackstory = other as ScenPart_ForcedChildhoodBackstory;
			if (scenPart_ForcedChildhoodBackstory != null && this.BackstoryDef == scenPart_ForcedChildhoodBackstory.BackstoryDef && context.OverlapsWith(scenPart_ForcedChildhoodBackstory.context))
			{
				return false;
			}

			return true;
		}
	}
}

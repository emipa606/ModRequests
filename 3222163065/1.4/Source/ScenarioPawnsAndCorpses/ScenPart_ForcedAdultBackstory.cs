using RimWorld;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// Scenpart allowing an adult backstory to be added to pawns
	/// </summary>
	public class ScenPart_ForcedAdultBackstory : ScenPart_ForcedBackstoryInterface
	{
		/// <summary>
		/// The backstory slot this scenpart can add
		/// </summary>
		private const BackstorySlot backstorySlot = BackstorySlot.Adulthood;

		/// <summary>
		/// Get the backstory slot this scenpart can add
		/// </summary>
		/// <returns>the backstory slot this scenpart can add</returns>
		public override BackstorySlot GetBackstorySlot()
		{
			return ScenPart_ForcedAdultBackstory.backstorySlot;
		}

		/// <summary>
		/// Whether the backstory this scenpart is adding can coexist with other backstories from other scenparts
		/// </summary>
		/// <param name="other">A scenpart to review</param>
		/// <returns>Whether this scenpart can coexist with the other</returns>
		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_ForcedAdultBackstory scenPart_ForcedAdultBackstory = other as ScenPart_ForcedAdultBackstory;
			if (scenPart_ForcedAdultBackstory != null && this.BackstoryDef == scenPart_ForcedAdultBackstory.BackstoryDef && context.OverlapsWith(scenPart_ForcedAdultBackstory.context))
			{
				return false;
			}

			return true;
		}
	}
}

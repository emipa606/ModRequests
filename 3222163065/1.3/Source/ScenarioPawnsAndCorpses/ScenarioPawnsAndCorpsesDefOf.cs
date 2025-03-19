using RimWorld;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
    /// Defs of the various scenparts added in this mod to make them searchable
    /// </summary>
    [DefOf]
    public static class ScenarioPawnsAndCorpsesDefOf
    {
        /// <summary>
        /// ScenPart to have colonists start with corpses, including coming down from drop pods
        /// </summary>
        public static ScenPartDef ScenPart_StartingCorpse;

        /// <summary>
        /// ScenPart to have colonists start with corpses nearby
        /// </summary>
        public static ScenPartDef ScenPart_ScatteredCorpseNearPlayerStart;

        /// <summary>
        /// ScenPart to have corpses around the map
        /// </summary>
        public static ScenPartDef ScenPart_ScatteredCorpseAnywhere;

        /// <summary>
        /// ScenPart to have colonists start with a specific childhood backstory
        /// </summary>
        public static ScenPartDef ScenPart_ForcedChildhoodBackstory;

        /// <summary>
        /// ScenPart to have colonists start with a specific adult backstory
        /// </summary>
        public static ScenPartDef ScenPart_ForcedAdultBackstory;

        /// <summary>
        /// ScenPart to have colonists start with a specific implant
        /// </summary>
        public static ScenPartDef ScenPart_ForcedPart;
    }
}

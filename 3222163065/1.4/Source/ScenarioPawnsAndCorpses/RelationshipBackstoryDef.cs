using RimWorld;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
    /// This class allows new BackgroundDefs with relationship settings to be specified in XML
    /// </summary>
    class RelationshipBackstoryDef : BackstoryDef
    {
#pragma warning disable CS0649
        #region XML

        /// <summary>
        /// XML option: likelihood of pawn having relationships.
        /// </summary>
        public RelationshipOverrides relationshipOverrides = new RelationshipOverrides();
        #endregion
#pragma warning restore CS0649
    }
}
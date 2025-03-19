using Verse;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
    /// Provides relationship overrides for backgrounds
    /// </summary>
    class RelationshipOverrides
    {
        /// <summary>
        /// Chance to have a child
        /// </summary>
        public float haveChildChance = 1f;

        /// <summary>
        /// Chance to have an ex-lover
        /// </summary>
        public float haveExLoverChance = 1f;

        /// <summary>
        /// Chance to have an ex-spouse
        /// </summary>
        public float haveExSpouseChance = 1f;

        /// <summary>
        /// Chance to have a fiance
        /// </summary>
        public float haveFianceChance = 1f;

        /// <summary>
        /// Chance to have a lover
        /// </summary>
        public float haveLoverChance = 1f;

        /// <summary>
        /// Chance to have a parent
        /// </summary>
        public float haveParentChance = 1f;

        /// <summary>
        /// Chance to have a sibling
        /// </summary>
        public float haveSiblingChance = 1f;

        /// <summary>
        /// Chance to have a spouse
        /// </summary>
        public float haveSpouseChance = 1f;

        /// <summary>
        /// The different overridable relationships
        /// </summary>
        public enum RelationType
        {
            Child,
            ExLover,
            ExSpouse,
            Fiance,
            Lover,
            Parent,
            Sibling,
            Spouse
        }

        /// <summary>
        /// The chance to have a relationship
        /// </summary>
        /// <param name="relationType">The relationship type</param>
        /// <returns>The chance to have a relationship</returns>
        public float GetRelationChance(RelationType relationType)
        {
            switch (relationType)
            {
                case RelationType.Child:
                    return this.haveChildChance;
                case RelationType.ExLover:
                    return this.haveExLoverChance;
                case RelationType.ExSpouse:
                    return this.haveExSpouseChance;
                case RelationType.Fiance:
                    return this.haveFianceChance;
                case RelationType.Lover:
                    return this.haveLoverChance;
                case RelationType.Parent:
                    return this.haveParentChance;
                case RelationType.Sibling:
                    return this.haveSiblingChance;
                case RelationType.Spouse:
                    return this.haveSpouseChance;
                default:
                    Log.Error($"[{this.GetType().Namespace}] Unknown relation type: {relationType}");
                    return 0f;
            }
        }
    }
}

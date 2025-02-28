namespace EvolvedOrgansRedux {
    public sealed class Singleton {
        public System.Collections.Generic.List<System.Tuple<Verse.RecipeDef, Verse.BodyPartDef>> bodyPartsToDelete = new System.Collections.Generic.List<System.Tuple<Verse.RecipeDef, Verse.BodyPartDef>>();

        public System.Collections.Generic.List<string> forbiddenMods = new System.Collections.Generic.List<string>() {
                "Android tiers",
                "Android tiers - TX Series"
            };
        public float ManipulationLimbCore { get; set; }
        public float MovingLimbCore { get; set; }
        public string NameOfThisMod { get; set; }
        public System.Collections.Generic.List<Verse.RecipeDef> AddLowershouldersToRecipedef { get; set; }
        public System.Collections.Generic.List<Verse.RecipeDef> AddLeftchestcavityToRecipedef { get; set; }
        public System.Collections.Generic.List<Verse.RecipeDef> AddRightchestcavityToRecipedef { get; set; }
        private static readonly Singleton instance = new Singleton();
        static Singleton() { }
        private Singleton() {
            AddLowershouldersToRecipedef = new System.Collections.Generic.List<Verse.RecipeDef>();
            AddLeftchestcavityToRecipedef = new System.Collections.Generic.List<Verse.RecipeDef>();
            AddRightchestcavityToRecipedef = new System.Collections.Generic.List<Verse.RecipeDef>();
        }
        public static Singleton Instance { get { return instance; } }
        public void calculateLimbCores() {
            ManipulationLimbCore = 0;
            MovingLimbCore = 0;
            foreach (Verse.BodyPartRecord bpr in Verse.DefDatabase<Verse.BodyDef>.GetNamed("Human").AllParts) {
                foreach (Verse.BodyPartTagDef bptd in bpr.def.tags) {
                    if (bptd.defName == RimWorld.BodyPartTagDefOf.ManipulationLimbCore.defName) {
                        ManipulationLimbCore++;
                    } else if (bptd.defName == RimWorld.BodyPartTagDefOf.MovingLimbCore.defName) {
                        MovingLimbCore++;
                    }
                }
            }
            ManipulationLimbCore /= 2f;
            MovingLimbCore /= 2f;
        }
    }
}

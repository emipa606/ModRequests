namespace EvolvedOrgansRedux {
    public class AddAdditionalBodypartsAfterResearch : Verse.ResearchMod {
        public Verse.ResearchProjectDef researchProjectDef;
        public Verse.RecipeDef recipeDef;
        public System.Collections.Generic.List<Verse.BodyPartDef> bodyPartDefs;
        public override void Apply() {
            //Need to check up on the Recipes added by BodyPartAffinity and see how that shit works out with this research. 
            if (researchProjectDef.IsFinished) {
                foreach (Verse.BodyPartDef bpd in bodyPartDefs) {
                    //Add the BodyPartDef from the list to the recipe
                    if (!recipeDef.appliedOnFixedBodyParts.Contains(bpd)) {
                        recipeDef.appliedOnFixedBodyParts.Add(bpd);
                        Singleton.Instance.bodyPartsToDelete.Add(new System.Tuple<Verse.RecipeDef, Verse.BodyPartDef>(recipeDef, bpd));
                    }
                    //Check if a non human race has a BodyPartDef that serves the same purpose as one of those in the list but with a different defName. (TailBone instead of Tail)
                    foreach (Verse.ThingDef def in Verse.DefDatabase<Verse.ThingDef>.AllDefs) {
                        if (def.race?.Humanlike == true && !def.defName.Equals("Human")) {
                            foreach (Verse.BodyPartRecord bpr in def.race.body.AllParts) {
                                if (bpr.def.defName.ToLower().Contains(bpd.defName.ToLower())) {
                                    if (!recipeDef.appliedOnFixedBodyParts.Contains(bpr.def)) {
                                        recipeDef.appliedOnFixedBodyParts.Add(bpr.def);
                                        Singleton.Instance.bodyPartsToDelete.Add(new System.Tuple<Verse.RecipeDef, Verse.BodyPartDef>(recipeDef, bpr.def));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            recipeDef.ClearCachedData();
            recipeDef.ResolveReferences();
        }
    }
    public class BodyPartAffiniyLowershoulders : Verse.ResearchMod {
        public Verse.ResearchProjectDef researchProjectDef;
        public override void Apply() {
            if (researchProjectDef.IsFinished) {
                foreach (Verse.RecipeDef rd in Singleton.Instance.AddLowershouldersToRecipedef) {
                    rd.appliedOnFixedBodyParts.Add(DefOf.LowerShoulder);
                }
            }
        }
    }
    public class BodyPartAffiniyLeftchestcavity : Verse.ResearchMod {
        public Verse.ResearchProjectDef researchProjectDef;
        public override void Apply() {
            if (researchProjectDef.IsFinished) {
                foreach (Verse.RecipeDef rd in Singleton.Instance.AddLeftchestcavityToRecipedef) {
                    rd.appliedOnFixedBodyParts.Add(DefOf.BodyCavity1);
                }
            }
        }
    }
    public class BodyPartAffiniyRightchestcavity : Verse.ResearchMod {
        public Verse.ResearchProjectDef researchProjectDef;
        public override void Apply() {
            if (researchProjectDef.IsFinished) {
                foreach (Verse.RecipeDef rd in Singleton.Instance.AddRightchestcavityToRecipedef) {
                    rd.appliedOnFixedBodyParts.Add(DefOf.BodyCavity2);
                }
            }
        }
    }
}

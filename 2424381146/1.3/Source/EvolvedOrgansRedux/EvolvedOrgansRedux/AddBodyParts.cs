namespace EvolvedOrgansRedux {
    public class AddBodyParts {
        public AddBodyParts(Verse.RaceProperties raceProperties, string nameOfThisMod) {
            System.Collections.Generic.List<Verse.BodyPartRecord> HumanParts = new System.Collections.Generic.List<Verse.BodyPartRecord>(Verse.DefDatabase<Verse.BodyDef>.GetNamed("Human").corePart.parts);
            //Get all BodyParts this mods adds to humans
            foreach (Verse.BodyPartRecord bpr in Verse.DefDatabase<Verse.BodyDef>.GetNamed("Human").corePart.parts) {
                if (bpr.def.modContentPack.Name != nameOfThisMod)
                    HumanParts.Remove(bpr);
            }
            //Add the BodyParts this mod adds to the other race's body, if a similar body part already exists, change the tags.
            foreach (Verse.BodyPartRecord bpr in HumanParts) {
                if (!raceProperties.body.AllParts.Exists(e => e.def.defName.ToLower().Contains(bpr.def.defName.ToLower()))) {
                    raceProperties.body.corePart.parts.Add(CopyBodyPartRecord(bpr, raceProperties));
                } else {
                    foreach (Verse.BodyPartTagDef bptd in bpr.def.tags) {
                        if (!raceProperties.body.AllParts.Find(e => e.def.defName.ToLower().Contains(bpr.def.defName.ToLower())).def.tags.Contains(bptd)) {
                            raceProperties.body.AllParts.Find(e => e.def.defName.ToLower().Contains(bpr.def.defName.ToLower())).def.tags.Add(bptd);
                        }

                    }
                }
            }
            Verse.DefDatabase<Verse.BodyDef>.GetNamed(raceProperties.body.defName).AllParts.Clear();
            Verse.DefDatabase<Verse.BodyDef>.GetNamed(raceProperties.body.defName).ResolveReferences();
        }
        private Verse.BodyPartRecord CopyBodyPartRecord(Verse.BodyPartRecord bpr, Verse.RaceProperties raceProperties) {
            return new Verse.BodyPartRecord {
                body = raceProperties.body,
                coverage = bpr.coverage,
                coverageAbs = bpr.coverageAbs,
                coverageAbsWithChildren = bpr.coverageAbsWithChildren,
                customLabel = bpr.customLabel,
                def = bpr.def,
                depth = bpr.depth,
                groups = bpr.groups,
                height = bpr.height,
                parent = Verse.DefDatabase<Verse.BodyDef>.GetNamed(raceProperties.body.defName).corePart,
                untranslatedCustomLabel = bpr.untranslatedCustomLabel
            };
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EvolvedOrgansRedux {
    public sealed class Singleton {
        private static readonly Singleton instance = new();
        public Settings Settings { get; set; } = LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>();
        public Dictionary<BodyPartTagDef, int> BodyPartTagsToRecalculate { get; set; } = [];
        public List<Finished_EVOR_Research_AddGroupsAndTags> BodyPartsToReset { get; set; } = [];
        public List<string> ForbiddenMods { get; set; } = [
                "Android tiers",
                "Android tiers - TX Series",
                "Android Tiers Reforged"
            ];
        static Singleton() {}
        private Singleton() {}
        public static Singleton Instance { get { return instance; } }
        public void FillListsOfBodyPartTagsToRecalculate() {
            BodyPartTagsToRecalculate.Clear();
            AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.ManipulationLimbCore);
            AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.ManipulationLimbSegment);
            AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.ManipulationLimbDigit);
           //AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.MovingLimbCore);
           //AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.MovingLimbSegment);
           //AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.MovingLimbDigit);
           //AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.SightSource);
           //AddBodyPartTagDefToDictionary(RimWorld.BodyPartTagDefOf.HearingSource);
        }
        public void AddBodyPartTagDefToDictionary(BodyPartTagDef bodyPartTagDef) {
            BodyPartTagsToRecalculate.Add(bodyPartTagDef, DefDatabase<BodyDef>.GetNamed("Human").AllParts.
                Where(e => e.def.tags.Contains(bodyPartTagDef) &&
                e.def.modContentPack == Settings.Mod.Content)
                .Count());
        }
    }
}

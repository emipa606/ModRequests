using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace EvolvedOrgansRedux {
    [StaticConstructorOnStartup]
    public static class AddEVORBodyPartsToRecipesIfResearchIsNotRequired {
        static AddEVORBodyPartsToRecipesIfResearchIsNotRequired() {
            if (!Settings.RequireResearchProject) {
                foreach (BodyPartDef bodyPartDefToAdd in Singleton.Instance.Settings.Mod.Content.AllDefs.Where(e =>
                    e.GetType() == typeof(BodyPartDef) &&
                    e.HasModExtension<EVOR_DefModExtension_BodyPartDefToGetSurgeryDefsFromToModify>()).Cast<BodyPartDef>()) {
                    BodyPartAffinity.AddEVORBodyPartsToRecipes(bodyPartDefToAdd.GetModExtension<EVOR_DefModExtension_BodyPartDefToGetSurgeryDefsFromToModify>().BodyPartDefToGetSurgeryDefsFromToModify, bodyPartDefToAdd);
                }
            }
            BodyPartAffinity.AddCapModsToHediffs(BodyPartDefOf.Lung, PawnCapacityDefOf.Breathing, 0.1f);
            BodyPartAffinity.AddCapModsToHediffs(BodyPartDefOf.Heart, PawnCapacityDefOf.BloodPumping, 0.1f);
            Singleton.Instance.FillListsOfBodyPartTagsToRecalculate();

        }
    }
    public class GameComponent(Game game) : Verse.GameComponent {
        public override void StartedNewGame() {
            base.StartedNewGame();
            if (Settings.RequireResearchProject)
                ResetBodyDefsAndResearchDefsOnNewGame();
            if (Singleton.Instance.Settings.ChoicesOfWorkbenches.Count > 1 && Settings.ChosenWorkbench == Singleton.Instance.Settings.ChoicesOfWorkbenches[0]) {
                string label = "EvolvedOrgansRedux";
                string text = "ChoicesOfWorkbenchesPartOneSingle".Translate(Singleton.Instance.Settings.ChoicesOfWorkbenches[1]);
                //ToDo
                //if (Singleton.Instance.Settings.ChoicesOfWorkbenches.Count > 2 && Settings.ChosenWorkbench == Singleton.Instance.Settings.ChoicesOfWorkbenches[0]) {
                //    text = "ChoicesOfWorkbenchesPartOneMultiple".Translate();
                //    for (int i = 1; i < Singleton.Instance.Settings.ChoicesOfWorkbenches.Count; i++) {
                //        text += "\n\n" + Singleton.Instance.Settings.ChoicesOfWorkbenches[i];
                //    }
                //    text += "ChoicesOfWorkbenchesPartTwoMultiple".Translate();
                //}
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent);
            }
            if (Settings.ShowResearchMessageAtNewGame && Settings.RequireResearchProject)
                Find.LetterStack.ReceiveLetter("ShowResearchMessageAtNewGameLetterName".Translate(),
                    "ShowResearchMessageAtNewGameLetterContent".Translate(),
                    LetterDefOf.NeutralEvent);
        }
        public override void FinalizeInit() {
            base.FinalizeInit();
            LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().WriteSettings();
        }
        //Any BodyParts that have been added to RecipeDefs or Tags/Groups that have been added to BodyParts have to be manually reset on new games
        //Else you can research the additional eyes on a playthrough and start a new one and the added groups, tags and stuff still sticks.
        private void ResetBodyDefsAndResearchDefsOnNewGame() {
            BodyDef bodyDef = DefDatabase<BodyDef>.GetNamed("Human");
            List<RecipeDef> list = [];
            foreach (Finished_EVOR_Research_AddGroupsAndTags defModExtension in Singleton.Instance.BodyPartsToReset) {
                BodyPartDef bodyPartDef = defModExtension.BodyPart;
                bodyPartDef.tags.Clear();
                foreach (BodyPartTagDef bodyPartTagDef in Singleton.Instance.BodyPartTagsToRecalculate.Keys)
                    if (bodyDef.cachedPartsByTag.ContainsKey(bodyPartTagDef))
                        bodyDef.cachedPartsByTag[bodyPartTagDef].RemoveAll(x => x.def == bodyPartDef);
                foreach (BodyPartRecord bpr in bodyDef.GetPartsWithDef(bodyPartDef))
                    bpr.groups.Clear();
                foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs.Where(x => x.appliedOnFixedBodyParts.Contains(bodyPartDef)))
                    recipeDef.appliedOnFixedBodyParts.Remove(bodyPartDef);
            }
            foreach (Finished_EVOR_Research_AddGroupsAndTags r in Singleton.Instance.BodyPartsToReset)
                r.AlreadyApplied = false;
        }
    }
}
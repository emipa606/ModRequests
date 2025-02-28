namespace EvolvedOrgansRedux {
    [Verse.StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            Singleton.Instance.calculateLimbCores();
            HarmonyLib.Harmony harmony = null;
            try {
                harmony = new HarmonyLib.Harmony("EvolvedOrgansRedux");
                harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
            } catch (System.Exception e) {
                string errorMessage = "EvolvedOrgansRedux Error: Step one";
                errorMessage += "\n" + e;
                Verse.Log.Error(errorMessage);
            }
            Singleton.Instance.NameOfThisMod = harmony.Id;
            try {
                new AddDefsForEachModdedRace(harmony.Id);
            } catch (System.Exception e) {
                string errorMessage = "EvolvedOrgansRedux Error: Step two";
                errorMessage += "\n" + e;
                Verse.Log.Error(errorMessage);
            }
            try {
                if (Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().BodyPartAffinity) {
                    new BodyPartAffinity(harmony.Id);
                }
            } catch (System.Exception e) {
                string errorMessage = "EvolvedOrgansRedux Error: Step three";
                errorMessage += "\n" + e;
                Verse.Log.Error(errorMessage);
            }
        }
    }
    public class GameComponent : Verse.GameComponent {
        public GameComponent(Verse.Game game) { }
        public override void StartedNewGame() {
            if (!Verse.DefDatabase<Verse.ResearchProjectDef>.GetNamed("EVOR_Research_Limbs3").IsFinished) {
                foreach (System.Tuple<Verse.RecipeDef, Verse.BodyPartDef> t in Singleton.Instance.bodyPartsToDelete) {
                    t.Item1.appliedOnFixedBodyParts.Remove(t.Item2);
                    t.Item1.ClearCachedData();
                    t.Item1.ResolveReferences();
                }
            }
            Singleton.Instance.bodyPartsToDelete.Clear();
            if (Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().CombatibilitySwitchEorVersionMidSave) {
                string label = "EvolvedOrgansRedux";
                string text = "The setting for switching from another version of EvolvedOrgansRedux to this one is still active." +
                    "\nPlease disable the setting, save and restart the game." +
                    "\nThis setting can cause issues with other mods that also add bodyparts.";
                Verse.Find.LetterStack.ReceiveLetter(label, text, RimWorld.LetterDefOf.NeutralEvent);
            }
            if (Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches.Count > 1 && Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().ChosenWorkbench == Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches[0]) {
                string label = "EvolvedOrgansRedux";
                string text = "EvolvedOrgansRedux has detected that you have the mod " +
                    Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches[1] +
                    " active. In the settings menu you can choose the workbench of that mod to reduce the amount of workbenches and ressources.";
                if (Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches.Count > 2 && Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().ChosenWorkbench == Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches[0]) {
                    text = "EvolvedOrgansRedux has detected that you have the mods ";
                    for (int i = 1; i < Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches.Count; i++) {
                        text += "\n\n" + Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().workbenches[i];
                    }
                    text += "\n\nenabled." +
                        "\nIn the settings menu you can choose a workbench of those mods to reduce the amount of workbenches and ressources.";
                }
                Verse.Find.LetterStack.ReceiveLetter(label, text, RimWorld.LetterDefOf.NeutralEvent);
            }
        }
        public override void FinalizeInit() {
        }
    }
}
using System.Reflection;
using HarmonyLib;
using Verse;

namespace EvolvedOrgansRedux {
    [StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            Harmony harmony = null;
            try {
                harmony = new Harmony("EvolvedOrgansRedux");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            } catch (System.Exception e) {
                string errorMessage = "EvolvedOrgansRedux Error: Step one";
                errorMessage += "\n" + e;
                Verse.Log.Error(errorMessage);
            }
            try {
                new AddDefsForEachModdedRace(harmony.Id);
            } catch (System.Exception e) {
                string errorMessage = "EvolvedOrgansRedux Error: Step two";
                errorMessage += "\n" + e;
                Verse.Log.Error(errorMessage);
            }
            try {
                if (LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().BodyPartAffinity) {
                    new BodyPartAffinity(harmony.Id);
                }
            } catch (System.Exception e) {
                string errorMessage = "EvolvedOrgansRedux Error: Step three";
                errorMessage += "\n" + e;
                Verse.Log.Error(errorMessage);
            }
        }
    }
}
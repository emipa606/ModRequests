using System.Linq;

namespace EvolvedOrgansRedux {
    [HarmonyLib.HarmonyPatch(typeof(RimWorld.AgeInjuryUtility))]
    [HarmonyLib.HarmonyPatch("RandomHediffsToGainOnBirthday", HarmonyLib.MethodType.Normal)]
    [HarmonyLib.HarmonyPatch(new System.Type[] { typeof(Verse.Pawn), typeof(int) })]
    static class PreventAgeHediffsIfImmortal_Patch {
        [HarmonyLib.HarmonyPostfix]
        public static void PreventAgeInjuries_Postfix(Verse.Pawn pawn, ref System.Collections.Generic.IEnumerable<Verse.HediffGiver_Birthday> __result) {
            if (pawn.health.hediffSet.HasHediff(Verse.HediffDef.Named("EVOR_Hediff_AbdominalAndChestcavity_RasVacoule"))) {
                Verse.Log.Message("Pawn is immortal because of Ras Vacoule.");
                __result = System.Linq.Enumerable.Empty<Verse.HediffGiver_Birthday>();
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Verse.PawnCapacityUtility))]
    [HarmonyLib.HarmonyPatch("CalculateLimbEfficiency", HarmonyLib.MethodType.Normal)]
    [HarmonyLib.HarmonyPatch(new System.Type[] { typeof(Verse.HediffSet), typeof(Verse.BodyPartTagDef), typeof(Verse.BodyPartTagDef), typeof(Verse.BodyPartTagDef),
        typeof(float), typeof(float), typeof(System.Collections.Generic.List<Verse.PawnCapacityUtility.CapacityImpactor>) },
        new HarmonyLib.ArgumentType[] { HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal,
            HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Out, HarmonyLib.ArgumentType.Normal })]
    public static class ManipulationFix {
        [HarmonyLib.HarmonyPostfix]
        public static void ManipulationFix_Patch(Verse.HediffSet diffSet, Verse.BodyPartTagDef limbCoreTag, ref float __result) {
            //Bodyparts can be tagged with ManipulationLimbCore. That way they are influenced by body part efficency.
            //Rimworld divides the manipulation gained by body parts tagged with ManipulationLimbCore by the amount of body parts tagged with ManipulationLimbCore.
            //Rimworld usually only has Left Arm and Right Arm tagged with ManipulationLimbCore. That's why the Bionic Arm doesn't actually give 125% manipulation but just 112% -> It gets divided by 2.
            //This mod adds another 5 parts that are influenced by effiency, and to get the gained manipulation back to basegame a bit of math has to be used.
            //((Amount of Manipulation - Basestat) * (Amount of tagged BodyPartRecords / 2)) + Basestat
            if (diffSet.pawn.def.modContentPack == null || !Singleton.Instance.forbiddenMods.Contains(diffSet.pawn.def.modContentPack.Name)) {
                if (limbCoreTag.defName == RimWorld.BodyPartTagDefOf.ManipulationLimbCore.defName) {
                    __result = ((__result - 1f) * Singleton.Instance.ManipulationLimbCore) + 1f;
                }
                if (limbCoreTag.defName == RimWorld.BodyPartTagDefOf.MovingLimbCore.defName) {
                    __result = ((__result - 1f) * Singleton.Instance.MovingLimbCore) + 1f;
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RimWorld.Recipe_RemoveBodyPart))]
    [HarmonyLib.HarmonyPatch("GetPartsToApplyOn", HarmonyLib.MethodType.Normal)]
    [HarmonyLib.HarmonyPatch(new System.Type[] { typeof(Verse.Pawn), typeof(Verse.RecipeDef) })]
    public static class Recipe_RemoveBodyPart {
        [HarmonyLib.HarmonyPostfix]
        public static void Recipe_RemoveBodyPart_Postfix(ref System.Collections.Generic.IEnumerable<Verse.BodyPartRecord> __result) {
            //Remove the Recipe_RemoveBodyPart for the bodyparts added by this mod.
            //I would rather use my custom Recipe_RemoveImplant.
            __result = __result.Where(bodypartrecord => bodypartrecord.def != DefOf.LowerShoulder && bodypartrecord.def != DefOf.Back &&
                bodypartrecord.def != DefOf.BodyCavity1 && bodypartrecord.def != DefOf.BodyCavity2 && bodypartrecord.def != DefOf.BodyCavityA
                && !bodypartrecord.def.defName.ToLower().Contains("tail"));
        }
    }
}
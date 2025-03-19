using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
    /// Harmony integration, allowing injection of code before/after methods.
    /// </summary>
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        /// <summary>
        /// Type decoration to add to the injected methods
        /// </summary>
        private static readonly Type patchType = typeof(HarmonyPatches);

        /// <summary>
        /// Static constructor to load up injections on startup
        /// </summary>
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.kayesh.scenariopawnsandcorpses");

            //PawnRelationWorker_*.GenerationChance
            try
            {
                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Child), nameof(PawnRelationWorker_Child.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(Child_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExLover), nameof(PawnRelationWorker_ExLover.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(ExLover_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_ExSpouse), nameof(PawnRelationWorker_ExSpouse.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(ExSpouse_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Fiance), nameof(PawnRelationWorker_Fiance.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(Fiance_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Lover.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(Lover_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Parent), nameof(PawnRelationWorker_Parent.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(Parent_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Sibling), nameof(PawnRelationWorker_Sibling.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(Sibling_GenerationChance_Postfix)));

                harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Spouse), nameof(PawnRelationWorker_Spouse.GenerationChance)),
                    postfix: new HarmonyMethod(patchType, nameof(Spouse_GenerationChance_Postfix)));


                // PawnGenerator.GenerateRandomAge_Prefix
                //harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "GenerateRandomAge"),
                //    prefix: new HarmonyMethod(patchType, nameof(GenerateRandomAge_Prefix)));

                // PawnGenerator.TryGenerateNewPawnInternal_Transpiler

                harmony.Patch(AccessTools.Method(typeof(PawnGenerator), name: "TryGenerateNewPawnInternal"), transpiler: new HarmonyMethod(patchType, nameof(TryGenerateNewPawnInternal_Transpiler)));

                // Page_ScenarioEditor.CheckAllPartsCompatible_PostFix

                harmony.Patch(
                    AccessTools.Method(GenTypes.GetTypeInAnyAssembly(typeName: "Page_ScenarioEditor"), name: "CheckAllPartsCompatible", new[] { typeof(Scenario) }),
                    postfix: new HarmonyMethod(patchType, nameof(CheckAllPartsCompatible_PostFix)));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
}

        /// <summary>
        /// Library function: gets the relationship overrides from the backstory slot.
        /// </summary>
        /// <param name="pawn">Pawn to get the relationship overrides from</param>
        /// <param name="slot">Which backstory to get: adult or child</param>
        /// <returns>Relationship overrides</returns>
        private static RelationshipOverrides GetRelationshipOverrides(Pawn pawn, BackstorySlot slot)
        {
            string backstoryIdString = pawn.story.GetBackstory(slot)?.identifier;
            RelationshipOverrides relationshopOverrides = null;

            if (!string.IsNullOrWhiteSpace(backstoryIdString))
            {
                relationshopOverrides = DefDatabase<RelationshipBackstoryDef>.GetNamedSilentFail(backstoryIdString)?.relationshipOverrides;
            }

            return relationshopOverrides;
        }

        /// <summary>
        /// Library function: gets the generation chance to have a type of relationship between two pawns.
        /// </summary>
        /// <param name="firstPawn">First pawn of the relationship</param>
        /// <param name="secondPawn">Second pawn of the relationship</param>
        /// <param name="firstToSecond">The relationship type</param>
        /// <returns>The chance for the relationship</returns>
        private static float GetGenerationChance(Pawn firstPawn, Pawn secondPawn, RelationshipOverrides.RelationType firstToSecond)
        {
            return HarmonyPatches.GetGenerationChance(firstPawn, secondPawn, firstToSecond, firstToSecond);
        }


        /// <summary>
        /// Library function: gets the generation chance to have a type of relationship between two pawns.
        /// </summary>
        /// <param name="firstPawn">First pawn of the relationship</param>
        /// <param name="secondPawn">Second pawn of the relationship</param>
        /// <param name="firstToSecond">The relationship type from the first to the second pawn</param>
        /// <param name="secondToFirst">The relationship type from the second to the first pawn</param>
        /// <returns>The chance for the relationship</returns>
        private static float GetGenerationChance(Pawn firstPawn, Pawn secondPawn, RelationshipOverrides.RelationType firstToSecond, RelationshipOverrides.RelationType secondToFirst)
        {
            if (firstPawn == null || secondPawn == null || firstPawn == secondPawn)
            {
                return 0f;
            }

            float result = 1f;

            result *= (HarmonyPatches.GetRelationshipOverrides(firstPawn, BackstorySlot.Childhood)?.GetRelationChance(firstToSecond) ?? 1f);
            result *= (HarmonyPatches.GetRelationshipOverrides(firstPawn, BackstorySlot.Adulthood)?.GetRelationChance(firstToSecond) ?? 1f);
            result *= (HarmonyPatches.GetRelationshipOverrides(secondPawn, BackstorySlot.Childhood)?.GetRelationChance(secondToFirst) ?? 1f);
            result *= (HarmonyPatches.GetRelationshipOverrides(secondPawn, BackstorySlot.Adulthood)?.GetRelationChance(secondToFirst) ?? 1f);

            return result;
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: child
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void Child_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.Child, RelationshipOverrides.RelationType.Parent);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: ex-lover
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void ExLover_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.ExLover);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: ex-spouse
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void ExSpouse_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.ExSpouse);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: fiance
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void Fiance_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.Fiance);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: lover
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void Lover_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.Lover);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: parent
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void Parent_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.Parent, RelationshipOverrides.RelationType.Child);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: sibling
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void Sibling_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.Sibling);
        }

        /// <summary>
        /// Get the generation chance of a relation between two pawns: spouse
        /// </summary>
        /// <param name="__result">The chance for the relationship</param>
        /// <param name="generated">The pawn under consideration</param>
        /// <param name="other">The pawn to potentially have the relation with</param>
        /// <param name="request">The pawn generation info</param>
        private static void Spouse_GenerationChance_Postfix(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            __result *= HarmonyPatches.GetGenerationChance(generated, other, RelationshipOverrides.RelationType.Spouse);
        }

        //private static void GenerateRandomAge_Prefix(Pawn pawn, ref PawnGenerationRequest request)
        //{
        //    ScenarioHelper.HandleAge(pawn, ref request);
        //}

        /// <summary>
        /// Modify the flow of TryGenerateNewPawnInternal, allowing:
        /// 1. The relationship setting to be skipped
        /// 2. A notification call into the backstory scenparts to allow forced backstories
        /// 3. The relationship setting to be called after the forced backstories have been added
        /// </summary>
        /// <param name="instructions">The code instructions of TryGenerateNewPawnInternal</param>
        /// <returns>the modified code instructions of TryGenerateNewPawnInternal</returns>
        public static IEnumerable<CodeInstruction> TryGenerateNewPawnInternal_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 		// if (!request.AllowedDevelopmentalStages.Newborn() && request.CanGeneratePawnRelations)
		    //IL_0329: ldarg.0
		    //IL_032a: call instance valuetype Verse.DevelopmentalStage Verse.PawnGenerationRequest::get_AllowedDevelopmentalStages()
		    //IL_032f: call bool Verse.DevelopmentalStageExtensions::Newborn(valuetype Verse.DevelopmentalStage)
		    //IL_0334: brtrue.s IL_0345
		    //IL_0336: ldarg.0
		    //IL_0337: call instance bool Verse.PawnGenerationRequest::get_CanGeneratePawnRelations()
		    //IL_033c: brfalse.s IL_0345
            MethodInfo allowedDevelopmentStages = AccessTools.PropertyGetter(typeof(PawnGenerationRequest), nameof(PawnGenerationRequest.AllowedDevelopmentalStages));
            MethodInfo newbornInfo = AccessTools.Method(typeof(DevelopmentalStageExtensions), nameof(DevelopmentalStageExtensions.Newborn));
            MethodInfo canGeneratePawnRelations = AccessTools.PropertyGetter(typeof(PawnGenerationRequest), nameof(PawnGenerationRequest.CanGeneratePawnRelations));

            //      PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, request.FixedLastName, faction2.def, request.ForceNoBackstory);
            //      call void RimWorld.PawnBioAndNameGenerator::GiveAppropriateBioAndNameTo(class Verse.Pawn, string, class RimWorld.FactionDef, bool)
            MethodInfo giveAppropriateBioAndNameToInfo = AccessTools.Method(GenTypes.GetTypeInAnyAssembly(typeName: "PawnBioAndNameGenerator"), name: "GiveAppropriateBioAndNameTo");

            //      GenerateTraits(pawn, request);
            //      call void Verse.PawnGenerator::GenerateTraits(class Verse.Pawn, valuetype Verse.PawnGenerationRequest)
            MethodInfo generateTraitsInfo = AccessTools.Method(GenTypes.GetTypeInAnyAssembly(typeName: "PawnGenerator"), name: "GenerateTraits");

            // GenerateRandomAge(pawn, request);
            // call void Verse.PawnGenerator::GenerateRandomAge(class Verse.Pawn, valuetype Verse.PawnGenerationRequest)
            MethodInfo generateRandomAgeInfo = AccessTools.Method(GenTypes.GetTypeInAnyAssembly(typeName: "PawnGenerator"), name: "GenerateRandomAge");

            bool getRandomAgeReplaced = false;

            //bool newbornCheckSkipped = false;
            bool earlyNewPawnGeneratingNotifyInjected = false;
            //bool generateRelationsInjected = false;
            //int instructionCount = instructions.Count();
            CodeInstruction currentInstruction = null;

            for (int i = 0; i < instructions.Count(); i++)
            {
                //try
                //{
                //    Log.Message($"Review + {instructions.ElementAt(i)}; {!ModsConfig.IsActive("erdelf.HumanoidAlienRaces")} and {!newbornCheckSkipped} and {i + 6 < instructionCount} and {instructions.ElementAt(i + 1).OperandIs(allowedDevelopmentStages)} and {instructions.ElementAt(i + 2).OperandIs(newbornInfo)} amd {instructions.ElementAt(i + 5).OperandIs(canGeneratePawnRelations)}");

                //}
                //catch (Exception ex)
                //{
                //    Log.Message($"HERE Exception {ex} : instruction count = {instructionCount}, i = {i}");
                //}

                //if (!ModsConfig.IsActive("erdelf.HumanoidAlienRaces") &&
                //    !newbornCheckSkipped &&
                //    i + 5 < instructionCount &&
                //    instructions.ElementAt(i + 1).OperandIs(allowedDevelopmentStages) &&
                //    instructions.ElementAt(i + 2).OperandIs(newbornInfo) && 
                //    instructions.ElementAt(i + 5).OperandIs(canGeneratePawnRelations))
                //{
                //    Log.Warning("SKIP Start");
                //    // Skip the relationship settings
                //    newbornCheckSkipped = true;
                //    i+=2;
                //    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                //}
                //else
                //{
                    // Sneak in a EarlyNewPawnGeneratingHelper notification to edit the pawn early. This will be just after the backgrounds are set so that we override them.
                    CodeInstruction previousInstruction = currentInstruction;
                    currentInstruction = instructions.ElementAt(i);

                    if (!earlyNewPawnGeneratingNotifyInjected && previousInstruction?.OperandIs(giveAppropriateBioAndNameToInfo) == true)
                    {
                        //Log.Warning("HERE postbit");
                        earlyNewPawnGeneratingNotifyInjected = true;

                        foreach (CodeInstruction injectedInstruction in HarmonyPatches.HandleEarlyNewPawnGeneratingHelper_InTryGenerateNewPawn())
                        {
                            yield return injectedInstruction;
                        }
                    }
                    else if (!getRandomAgeReplaced && i+2 < instructions.Count() && instructions.ElementAt(i+1).OperandIs(generateRandomAgeInfo))
                    {
                        getRandomAgeReplaced = true;

                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ScenarioHelper), nameof(ScenarioHelper.HandleAge)));

                        // ldloc.0
                        yield return new CodeInstruction(OpCodes.Ldloc_0);

                        // ldarg.0
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                    }

                    //else if (!ModsConfig.IsActive("erdelf.HumanoidAlienRaces") && !generateRelationsInjected && previousInstruction?.OperandIs(generateTraitsInfo) == true)
                    //{
                    //    Log.Warning("Inject start");
                    //    //Add the relationship settings back in
                    //    generateRelationsInjected = true;

                    //    foreach (CodeInstruction injectedInstruction in HarmonyPatches.GenerateRelations_InTryGenerateNewPawn())
                    //    {
                    //        yield return injectedInstruction;
                    //    }
                    //}

                    yield return currentInstruction;
                }
            //}
        }

        /// <summary>
        /// Creates and returns the instructions to call ScenarioHelper.HandleEarlyNewPawnGenerating
        /// </summary>
        /// <returns>the code instructions necessary to jump into ScenarioHelper.HandleEarlyNewPawnGenerating</returns>
        private static IEnumerable<CodeInstruction> HandleEarlyNewPawnGeneratingHelper_InTryGenerateNewPawn()
        {

            // HandleEarlyNewPawnGenerating(pawn, request)

            // ldloc.0
            yield return new CodeInstruction(OpCodes.Ldloc_0);

            // ldarg.0
            yield return new CodeInstruction(OpCodes.Ldarg_0);

            // call instance valuetype RimWorld.PawnGenerationContext Verse.PawnGenerationRequest::get_Context()
            yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(PawnGenerationRequest), nameof(PawnGenerationRequest.Context)));

            // callvirt instance void ScenarioPawnsAndCorpses.ScenarioHelper::HandleEarlyNewPawnGenerating(class Verse.Pawn, valuetype RimWorld.PawnGenerationContext)
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ScenarioHelper), nameof(ScenarioHelper.HandleEarlyNewPawnGenerating)));
        }

        /// <summary>
        /// Creates and returns the instructions to call PawnGenerator.GenerateRelations
        /// </summary>
        /// <returns>the code instructions necessary to jump into PawnGenerator.GenerateRelations</returns>
        private static IEnumerable<CodeInstruction> GenerateRelations_InTryGenerateNewPawn()
        {
            // ldloc.0
            yield return new CodeInstruction(OpCodes.Ldloc_0);

            // ldarg.0
            yield return new CodeInstruction(OpCodes.Ldarg_0);

            //call void Verse.PawnGenerator::GeneratePawnRelations(class Verse.Pawn, valuetype Verse.PawnGenerationRequest&)
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.GenerateRelations)));
        }

        /// <summary>
        /// Delegate method for PawnGenerator.PawnRelations
        /// </summary>
        /// <param name="pawn">The pawn to review</param>
        /// <param name="request">The pawn generation request</param>
        public delegate void PawnGeneratorPawnRelations(Pawn pawn, ref PawnGenerationRequest request);

        /// <summary>
        /// Perform the standard flow for generating relations
        /// </summary>
        /// <param name="pawn">The pawn to review</param>
        /// <param name="request">The pawn generation request</param>
        public static void GenerateRelations(Pawn pawn, ref PawnGenerationRequest request)
        {
            try
            {
                if (!request.AllowedDevelopmentalStages.Newborn() && request.CanGeneratePawnRelations)
                {
                    AccessTools.MethodDelegate<PawnGeneratorPawnRelations>(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"))(pawn, ref request);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        /// <summary>
        /// Ensure that backstories don't conflict with each other; specifically that they don't go over 100% chance for each slot
        /// </summary>
        /// <param name="__result">Whether the check passed; no need to check on failure</param>
        /// <param name="scen">The scenario containing the scenparts</param>
        private static void CheckAllPartsCompatible_PostFix(ref bool __result, Scenario scen)
        {
            if (__result)
            {
                Dictionary<BackstorySlot, Dictionary<PawnGenerationContext, int>> backstoryChanceSum = new Dictionary<BackstorySlot, Dictionary<PawnGenerationContext, int>>();

                foreach (ScenPart_ForcedBackstoryInterface allPart in scen.AllParts.Where(p => p is ScenPart_ForcedBackstoryInterface))
                {
                    if (!backstoryChanceSum.ContainsKey(allPart.GetBackstorySlot()))
                    {
                        backstoryChanceSum.Add(allPart.GetBackstorySlot(), new Dictionary<PawnGenerationContext, int>());
                    }

                    if (!backstoryChanceSum[allPart.GetBackstorySlot()].ContainsKey(allPart.GetContext()))
                    {
                        backstoryChanceSum[allPart.GetBackstorySlot()].Add(allPart.GetContext(), (int)Math.Round(allPart.GetChance() * 100, 0));
                    }
                    else
                    {
                        backstoryChanceSum[allPart.GetBackstorySlot()][allPart.GetContext()] += (int)Math.Round(allPart.GetChance() * 100, 0);
                    }
                }

                foreach (BackstorySlot slot in backstoryChanceSum.Keys)
                {
                    foreach (PawnGenerationContext context in backstoryChanceSum[slot].Keys)
                    {
                        if (backstoryChanceSum[slot][context] > 100)
                        {
                            Messages.Message("ScenarioPawnsAndCorpses.ScenPart_TooManyBackstories".Translate(context.ToString(), slot.ToString(), backstoryChanceSum[slot][context]), MessageTypeDefOf.RejectInput, historical: false);
                            __result = false;
                        }
                    }
                }
            }
        }
    }
}

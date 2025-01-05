using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using PawnTrackerMain.CommonPatches;
using System.Drawing;


namespace PawnTackerMain
{
    internal class PawnTrackerModBase : ModBase
    {
        public static object settings;
        public override string ModIdentifier => "PawnTrackerUtility";
        private Harmony harmony;
        
        public override void Initialize()
        {
            base.Initialize();
            harmony = new Harmony("rimworld.mod.PawnTrackerMain");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            PawnTrackerMain.PawnTracker_SpecialInjector.Inject();
            PatchCommon();
            PatchHospitality();
            PawnTrackerMain.VEEPatches.Patches.ApplyDynamicPatches(harmony);
        }
        
        
        private void PatchCommon()
        {
            // Get the assembly in which patches are declared (obvs)
            var assembly = Assembly.GetExecutingAssembly();
            HashSet<string> methodsToIgnore = new HashSet<string>
            {
                "ReceiveLetter", 
                "GeneratePawn",
                "AddHediff",
                "IndividualThoughtToAdd"
            };

            foreach (var type in assembly.GetTypes())
            {
                if (type.Namespace == "PawnTrackerMain.CommonPatches")
                {
                    var ogMethods = HarmonyMethodExtensions.GetFromType(type);
                    if (ogMethods == null)
                    {
                        Log.Error("ogMethod null");
                        return;
                    }

                    foreach (var original in ogMethods)
                    {
                        if (methodsToIgnore.Contains(original.methodName) ||  original.methodType == MethodType.Constructor)
                        {
                            continue;
                        }

                        if (original.methodName == null)
                        {
                            Log.Error($"methodName null in HarmonyMethod for {original}");
                            continue;
                        }

                        MethodInfo originalMethod = AccessTools.Method(original.declaringType, original.methodName);
                        if (originalMethod == null)
                        {
                            Log.Message($"OriginalMethod is null for {original.methodName} of type {original.declaringType}");
                            continue;
                        }
                        var prefix = type.GetMethod("Prefix");
                        var postfix = type.GetMethod("Postfix");
                        
                        var harmonyPrefix = prefix != null ? new HarmonyMethod(prefix) : null;
                        var harmonyPostfix = postfix != null ? new HarmonyMethod(postfix) : null;

                        //Log.Message($"Patching {original.methodName}");
                        harmony.Patch(originalMethod,
                            prefix != null ? new HarmonyMethod(prefix) : null,
                            postfix != null ? new HarmonyMethod(postfix) : null);  
                            
                    }
                }
            }

            //Log.Message("Patch LetterStack_Patch");
            //Patch LetterStack_Patch
            MethodInfo targetMethod = AccessTools.Method(typeof(LetterStack), "ReceiveLetter", new Type[] { typeof(Letter), typeof(string) });
            MethodInfo postfixMethodInfo = typeof(LetterStack_Patch).GetMethod(nameof(LetterStack_Patch.Postfix), BindingFlags.Static | BindingFlags.Public);
            HarmonyMethod postfixHarmonyMethod = new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(targetMethod, postfix: postfixHarmonyMethod);

            //Log.Message("Patching LetterStack_Patch2");
            //Patch LetterStack_Patch2
            targetMethod = AccessTools.Method(typeof(LetterStack), "ReceiveLetter", new Type[] {typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string)});
            postfixMethodInfo = typeof(LetterStack_Patch2).GetMethod(nameof(LetterStack_Patch2.Postfix), BindingFlags.Static | BindingFlags.Public);
            postfixHarmonyMethod = new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(targetMethod, postfix: postfixHarmonyMethod);

            //Log.Message("Patching PawnGenerator_GeneratePawn_Patch1");
            //Patch PawnGenerator_GeneratePawn_Patch1
            targetMethod = AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] {typeof(PawnKindDef), typeof(Faction)});
            postfixMethodInfo = typeof(PawnGenerator_GeneratePawn_Patch1).GetMethod(nameof(PawnGenerator_GeneratePawn_Patch1.Postfix), BindingFlags.Static | BindingFlags.Public);
            postfixHarmonyMethod = new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(targetMethod, postfix: postfixHarmonyMethod);

            //Log.Message("Patching PawnGenerator_GeneratePawn_Patch2");
            //Patch PawnGenerator_GeneratePawn_Patch2
            targetMethod = AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] {typeof(PawnGenerationRequest)});
            postfixMethodInfo = typeof(PawnGenerator_GeneratePawn_Patch2).GetMethod(nameof(PawnGenerator_GeneratePawn_Patch2.Postfix), BindingFlags.Static | BindingFlags.Public);
            postfixHarmonyMethod = new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(targetMethod, postfix: postfixHarmonyMethod);

            //Log.Message("Patching Pawn_HealthTracker_Patch");
            //Patch Pawn_HealthTracker_Patch
            targetMethod = AccessTools.Method(typeof(Pawn_HealthTracker), "AddHediff", new Type[] {typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)});
            postfixMethodInfo = typeof(Pawn_HealthTracker_Patch).GetMethod(nameof(Pawn_HealthTracker_Patch.Postfix), BindingFlags.Static | BindingFlags.Public);
            postfixHarmonyMethod = new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(targetMethod, postfix: postfixHarmonyMethod);

            //Log.Message("Patching IndividualThoughtToAdd");
            //Patch IndividualThoughtToAdd
            ConstructorInfo targetConstructor = AccessTools.Constructor(typeof(IndividualThoughtToAdd), new Type[] { typeof(ThoughtDef), typeof(Pawn), typeof(Pawn), typeof(float), typeof(float) });
            if (targetConstructor == null)
            {
                Log.Error("Target constructor not found.");
                return;
            }
            postfixMethodInfo = typeof(IndividualThoughtToAdd_Patch).GetMethod(nameof(IndividualThoughtToAdd_Patch.Postfix), BindingFlags.Static | BindingFlags.Public);
            postfixHarmonyMethod = new HarmonyMethod(postfixMethodInfo);
            harmony.Patch(targetConstructor, postfix: postfixHarmonyMethod);

            Log.Message("Common patches applied successfully");
        }

        private void PatchHospitality()
        {
            if (ModsConfig.IsActive("Orion.Hospitality")) {
                
                Type guestUtilityType = AccessTools.TypeByName("Hospitality.Utilities.GuestUtility");
                if (guestUtilityType != null) {
                    try {
                        // Hospitality is loaded, apply Hospitality-specific patches
                        PawnTrackerMain.HospitalityPatches.Patches.ApplyPatches(harmony);
                        Log.Message("Hospitality-specific patches applied successfully.");
                        return;
                    }
                    catch (Exception ex) {
                        Log.Error($"Failed to apply Hospitality-specific patches: {ex}");
                        return;
                    }
                }
                else {
                    Log.Error("Hospitality.Utilities.GuestUtility type not found. Hospitality-specific patches will not be applied.");
                    return;
                }
            }
            else {
                Log.Message("Hospitality mod not active, skipping Hospitality-specific patches.");
            }
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            //foreach (var map in Find.Maps) map.GetMapComponent().FirstTimeLoaded();
        }
    }

    internal class PawnTrackerMainSettings : ModBase
    {
        public override string ModIdentifier {
            get { return "PawnTrackerMainSettings"; }
        }
        
        public PawnTrackerMainSettings() {}

        public static SettingHandle<int> checkArriveDisappearTicks;
        public static SettingHandle<int> checkDeadPawnsTicks;
        public static SettingHandle<int> updatePawnStatusTicks;
        public static SettingHandle<int> updateEventsTicks;
        public static SettingHandle<int> updatePawnRelationshipsTicks;
        
        public override void DefsLoaded() {
            checkArriveDisappearTicks = Settings.GetHandle<int>(
                "checkArriveDisappearTicks", 
                "Check for arrived/departed pawns (ticks)", 
                "", 
                100);
            checkDeadPawnsTicks = Settings.GetHandle<int>(
                "checkDeadPawnsTicks", 
                "Check for & address dead pawns (ticks)", 
                "", 
                100);
            updatePawnStatusTicks = Settings.GetHandle<int>(
                "updatePawnStatusTicks", 
                "Re-check if pawn is colonist, prisoner, etc. (ticks)", 
                "", 
                300);
            updateEventsTicks = Settings.GetHandle<int>(
                "updateEventsTicks", 
                "Clean up certain events (ticks)", 
                "", 
                300);
            updatePawnRelationshipsTicks = Settings.GetHandle<int>(
                "updatePawnRelationshipsTicks", 
                "Check for changes in pawns' relationships (ticks)", 
                "", 
                300);
        }
       
    }
}
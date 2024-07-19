using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            MainProcesses.DefInjection();
            MainProcesses.HarmonyPatch();
        }

        static class MainProcesses
        {
            public static void DefInjection()
            {
                DefInjectionPawn();
                DefInjectionFrame();
            }

            static void DefInjectionPawn()
            {
                foreach (ThingDef raceDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false))
                {
                    CompProperties_FinishingTouchPawn prop = new CompProperties_FinishingTouchPawn();
                    prop.isFinishingTouchTarget = raceDef.race?.Humanlike ?? false;
                    //Log.Message($"@@@{raceDef.LabelCap}({raceDef.defName}): isFinishingTouchTarget={prop.isFinishingTouchTarget}");

                    raceDef.comps.Add(prop);
                }
            }
            static void DefInjectionFrame()
            {
                foreach (ThingDef buildingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.building != null))
                {
                    ThingDef frameDef = DefDatabase<ThingDef>.GetNamedSilentFail($"Frame_{buildingDef.defName}");
                    if (frameDef == null)
                    {
                        continue;
                    }
                    CompProperties_FinishingTouchFrame prop = new CompProperties_FinishingTouchFrame();
                    prop.isFinishingTouchTarget = buildingDef.HasComp(typeof(CompQuality));//frameDef.HasComp(typeof(CompQuality));

                    //Log.Message($"@@@{frameDef.LabelCap}({frameDef.defName}): isFinishingTouchTarget={prop.isFinishingTouchTarget}");

                    frameDef.comps.Add(prop);
                }
            }

            public static void HarmonyPatch()
            {
                Harmony harmony = new Harmony("miyamiya.FinishingTouch");
                HarmonyPatch_Automatic(harmony);
                HarmonyPatch_Manually(harmony);
            }

            static void HarmonyPatch_Automatic(Harmony harmony)
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            static void HarmonyPatch_Manually(Harmony harmony)
            {
                // ==== Vanilla Patch ====
                //Construction Toil
                Type c__DisplayClass4_0 = AccessTools.TypeByName("RimWorld.JobDriver_ConstructFinishFrame+<>c__DisplayClass4_0");
                MethodInfo b__1 = AccessTools.Method(c__DisplayClass4_0, "<MakeNewToils>b__1");
                harmony.Patch(b__1, null, null, new HarmonyMethod(typeof(JobDriver_ConstructFinishFrame_DisplayClass4_0_Patch), nameof(JobDriver_ConstructFinishFrame_DisplayClass4_0_Patch.MakeNewToils_b__1_Transpiler), new Type[] { typeof(IEnumerable<CodeInstruction>), typeof(ILGenerator) }));

                //  ==== Mod Patches ====
                //Replace Stuff Compatible
                if (ModsConfig.ActiveModsInLoadOrder.Any(x => x.PackageId.ToLower() == "Uuugggg.ReplaceStuff".ToLower()))
                {
                    Type typeThingDefGenerator_ReplaceFrame = AccessTools.TypeByName("Replace_Stuff.ThingDefGenerator_ReplaceFrame");
                    if (typeThingDefGenerator_ReplaceFrame != null)
                    {
                        MethodInfo methodNewReplaceFrameDef_Thing = AccessTools.Method(typeThingDefGenerator_ReplaceFrame, "NewReplaceFrameDef_Thing");
                        if (methodNewReplaceFrameDef_Thing != null)
                        {
                            //Log.Message("Replace Stuff Enabled!");
                            harmony.Patch(methodNewReplaceFrameDef_Thing, null, new HarmonyMethod(typeof(ThingDefGenerator_ReplaceFrame_Patch), nameof(ThingDefGenerator_ReplaceFrame_Patch.NewReplaceFrameDef_Thing_Postfix), new Type[] { typeof(ThingDef), typeof(ThingDef).MakeByRefType() }));
                        }
                    }
                }
            }
        }
    }
}

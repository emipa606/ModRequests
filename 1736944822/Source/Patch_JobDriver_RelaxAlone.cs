using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit; // for OpCodes in Harmony Transpiler

/***************** Patch two parts of JobDriver_RelaxAlone: ***********************/
/*                   One of the toils, and MakeToils itself                       */
namespace LWM.PrayerSpot {
    /* Patch RimWorld/JobDriver_RelaxAlone's MakeNewToils
     * Change it to specify facing when on a PrayerSpot
     *
     * The line in the code is something like this:
     *  relax.initAction = delegate()
     *  {
     *    this.faceDir = ((!this.job.def.faceDir.IsValid) ? Rot4.Random : this.job.def.faceDir);
     *  };
     * We want to replace Rot4.Random with LWM.PrayerSpot.Patch_JobDriver_RelaxAlone_Toil_Delegate.faceDir(pawn)
     *
     * Of course, it's in an anonymous delegate function, but...
     *     it's no longer inside an anonymouse Iterator class! The update to 1.1 made some things easier!
     *     (Interesting read on how it's handled:  https://blogs.msdn.microsoft.com/oldnewthing/20060802-00/?p=30263/)
     */
     // Harmony can directly find the delegate, if we have the IL name:
    [HarmonyPatch(typeof(RimWorld.JobDriver_RelaxAlone), "<MakeNewToils>b__6_1")]
    public static class Patch_JobDriver_RelaxAlone_Toil_Delegate {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var callToRandomDirection=HarmonyLib.AccessTools.Method("Verse.Rot4:get_Random");
            // Should never fail.
            if (callToRandomDirection == null) Log.Error("LWM.PrayerSpot: Failed to find Verse.Rot4:get_Random");
            //  RimWorld.JobDriver_RelaxAlone RimWorld.JobDriver_RelaxAlone/'<MakeNewToils>c__Iterator0'::$this
            foreach (var code in instructions) {
                //Log.Message("Code: "+code.opcode+": "+code.operand);
                if (code.opcode==OpCodes.Call && (MethodInfo)code.operand==callToRandomDirection) {
                    //Log.Message("Patching to remove Rot4.Random!");
                    // Replace with our call:
                    //   first put pawn on stack:
                    var c=new CodeInstruction(OpCodes.Ldarg_0); // the JobDriver_RelaxAlone
                    c.labels=code.labels; // someone may want to jump there, so take any labels
                    yield return c;
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(JobDriver_RelaxAlone).GetField("pawn"));
                    yield return new CodeInstruction(OpCodes.Call,
                          HarmonyLib.AccessTools.Method("LWM.PrayerSpot.Patch_JobDriver_RelaxAlone_Toil_Delegate:faceDir"));
                } else
                    yield return code;
            }
            yield break;
        }

        public static Verse.Rot4 faceDir(Pawn pawn) {
            // pawn is spawned: it just started a job:p
            // pawn has a map: it just started a job:p
            Thing spot=pawn.Map.thingGrid.ThingAt(pawn.Position, PrayerSpotDirectionalDef);
            if (spot==null) return Rot4.Random;
            return spot.Rotation;
        }
        static ThingDef PrayerSpotDirectionalDef=DefDatabase<ThingDef>.GetNamed("LWM_PrayerSpot_Dir");

    }
    /* Patch Rimworld/JobDriver_RelaxAlone's final Toil:
     *   toil.AddFinishAction(Utilities.GiveRelaxAloneThought((JobDriver)this));
     * #DeepMagic
     */
    /* Technical notes:
     * The toils are returned inside a hidden "inner" iterator class,
     * and are returned inside the MoveNext() method of that class.
     * So to patch the method, we first have to find it, then we
     * use Transpiler to add the AddFinishAction call right after
     * the (last) AddPreTickAction
     */
    [HarmonyPatch]
    public static class Patch_JobDriver_RelaxAlone_RelaxToil {
        public static MethodBase TargetMethod() {
            // Decompiler showed the hidden inner class is "<MakeNewToils>d__6"... okay, sure.
            Type hiddenClass = HarmonyLib.AccessTools.Inner(typeof(RimWorld.JobDriver_RelaxAlone), "<MakeNewToils>d__6");
            if (hiddenClass==null) {
                Log.Error("Couldn't find d__6 - check decompiler to find proper inner class");
                return null;
            }
            MethodBase iteratorMethod=HarmonyLib.AccessTools.Method(hiddenClass, "MoveNext");
            if (iteratorMethod==null) Log.Error("Couldn't find MoveNext");
            return iteratorMethod;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var code = new List<CodeInstruction>(instructions);
            var Toil_AddPreTickAction = HarmonyLib.AccessTools.Method(typeof(Verse.AI.Toil), "AddPreTickAction");
            /*if (Toil_AddPreTickAction == null) {
                Log.Error("Could not find AddPreTickAction");
                foreach (var c in code) {
                    yield return c;
                }
                yield break;
            }*/
            int locationOfLastPreTickAction;
            for (locationOfLastPreTickAction=code.Count-1; locationOfLastPreTickAction>=0;
                 locationOfLastPreTickAction--) {
                if (code[locationOfLastPreTickAction].opcode==OpCodes.Callvirt &&
                    (MethodInfo)code[locationOfLastPreTickAction].operand == Toil_AddPreTickAction) {
                    break;
                }
            }
            if (locationOfLastPreTickAction == 0) {
                // couldn't find it???
                Log.Warning("Could not find AddPreTickAction; prayer spots broken");
                foreach (var c in code) { yield return c; }
                yield break;
            }
            // Splice in toil.AddFinishAction(delegate () {Utilities.GiveRelaxAloneThought((JobDriver)this)));
            code.InsertRange(locationOfLastPreTickAction+1, //insert after it
                             new CodeInstruction[] {
                                 new CodeInstruction(OpCodes.Ldloc_2), // this toil
                                 new CodeInstruction(OpCodes.Ldloc_1), // this JobDriver_RelaxAlone
                                 // load the function we want to call as an Action:
                                 //   NOTE: GiveRelaxAloneThought is
                                 //   void GiveRelaxAloneThought(JobDriver driver)...
                                 new CodeInstruction(OpCodes.Ldftn,
                                        HarmonyLib.AccessTools.Method(typeof(Utilities), "GiveRelaxAloneThought")),
                                 // Make it an Action:
                                 //   NOTE: I have no idea how this constructor works to turn the
                                 //   LdFtn above into an Action. The typeof(JobDriver) seems reasonable,
                                 //   but the typeof(IntPtr)?  Who knows, but it's needed!
                                 new CodeInstruction(OpCodes.Newobj, HarmonyLib.AccessTools.
                                     Constructor(typeof(System.Action), new Type[] {typeof(JobDriver), typeof(IntPtr)})),
                                 // put that Action into the toil as a FinishAction:
                                 new CodeInstruction(OpCodes.Callvirt, HarmonyLib.AccessTools.
                                                     Method(typeof(Verse.AI.Toil), "AddFinishAction"))
                             });
            // finally, return the new method IL to Harmony:
            foreach (var c in code) {
                yield return c;
            }

/*                if (c.opcode==OpCodes.Callvirt &&
                    (MethodInfo)c.operand == Toil_AddPreTickAction) {
                    yield return new CodeInstruction(OpCodes.Ldloc_2); // this toil
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // this JobDriver_RelaxAlone
                    // load the function we want to call as an Action:
                    yield return new CodeInstruction(OpCodes.Ldftn, HarmonyLib.AccessTools.Method(typeof(Utilities), "GivePrayerT"));
                    // Make it an Action:
                    yield return new CodeInstruction(OpCodes.Newobj,
                                                     HarmonyLib.AccessTools.
                                                     Constructor(typeof(System.Action), new Type[] {typeof(JobDriver), typeof(IntPtr)}));
                    //typeof(System.Action).GetConstructor(new Type[] {}));
                    // put that Action into the toil as a FinishAction:
                    yield return new CodeInstruction(OpCodes.Callvirt, HarmonyLib.AccessTools.Method(typeof(Verse.AI.Toil), "AddFinishAction"));
                }
            }*/
        }

    }

    /* Original approach:
        patch xml def of Pray to use:
        public class JobDriver_Spiritual_Simple : JobDriver_RelaxAlone {
          protected override IEnumerable<Toil> MakeNewToils() {
            // grab toils from parent, if it has a preinit action, it's the one that
            // makes facing, so modify it, etc.
          } // end toils
        }
     * But.  This will be more compatible, I think.
     */
}

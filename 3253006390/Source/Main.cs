using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Noise;
using Verse.Grammar;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using System.Reflection.Emit;
using GestaltEngine;

namespace MechCaravans
{
    // Patch Gestalt Engine to not forbid Gestalt-controlled mechanoids on caravans
    [HarmonyPatchCategory("GestaltEngine")]
    [HarmonyPatch]
    public static class Skip_Gestalt_CaravanUI_Patch {
        public static bool Prefix(ref bool __result, Rect rect, Transferable trad, int index, int min, int max, bool flash, bool readOnly) {
            __result = true;
            return false;
        }
    }

    // Patch Dialog_FormCaravan CheckErrors so we can start a caravan with only mechanoids in the UI
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch]
    public static class Dialog_FormCaravan_CheckErrors_NoColonist_Patch {

        public static MethodBase TargetMethod() {
            
            return AccessTools.Method(AccessTools.Inner(typeof(Dialog_FormCaravan), "<>c"), "<CheckForErrors>b__95_0");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            codes[0].labels.Add(jumpLabel);

            var newCodes = new List<CodeInstruction>();

            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps")));
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_IsMechanoid")));
            newCodes.Add(new CodeInstruction(OpCodes.Brfalse_S, jumpLabel));
            newCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            newCodes.Add(new CodeInstruction(OpCodes.Ret));

            codes.InsertRange(0, newCodes);
            return codes.AsEnumerable();
        }
    }

    // Patch Dialog_FormCaravan CheckErrors so that mechanoids are included when the "can caravan pawns reach this item" check happens
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch]
    public static class Dialog_FormCaravan_CheckErrors_Unreachable_Patch {

        public static MethodBase TargetMethod() {
            
            return AccessTools.Method(AccessTools.Inner(typeof(Dialog_FormCaravan), "<>c__DisplayClass95_0"), "<CheckForErrors>b__1");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            codes[3].labels.Add(jumpLabel);

            var newCodes = new List<CodeInstruction>();
            
            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps")));
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_IsMechanoid")));
            newCodes.Add(new CodeInstruction(OpCodes.Brtrue_S, jumpLabel));

            codes.InsertRange(0, newCodes);
            return codes.AsEnumerable();
        }
    }

    // Patch SplitCaravan CheckErrors so we can start a caravan with only mechanoids in the UI
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch]
    public static class Dialog_SplitCaravan_CheckErrors_Patch {

        public static MethodBase TargetMethod() {
            
            return AccessTools.Method(AccessTools.Inner(typeof(Dialog_SplitCaravan), "<>c"), "<CheckForErrors>b__85_0");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            codes[0].labels.Add(jumpLabel);

            var newCodes = new List<CodeInstruction>();

            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1)); //Load Pawn variable onto stack
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps"))); // Load Pawn.RaceProps onto stack
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_IsMechanoid"))); // Load RaceProps.IsMechanoid onto stack
            newCodes.Add(new CodeInstruction(OpCodes.Brfalse_S, jumpLabel)); // Jump if IsMechanoid was false
            newCodes.Add(new CodeInstruction(OpCodes.Ldc_I4_1)); // Otherwise, load 1 (true) onto stack...
            newCodes.Add(new CodeInstruction(OpCodes.Ret)); //... and then return true

            codes.InsertRange(0, newCodes);
            return codes.AsEnumerable();
        }
    }

    // Patch AllOwnersDowned to not count if there are Mechanoids in the caravan (otherwise Mech-only caravans can't move; 'all pawns downed in caravan')
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(Caravan), "get_AllOwnersDowned")]
    public static class Caravan_AllOwnersDowned_Patch {
        public static void Postfix(ref bool __result, ref Caravan __instance) {
            foreach(Pawn p in __instance.pawns) {
                if(p.RaceProps.IsMechanoid && !p.Downed) {
                    __result = false;
                    break;
                }
            }
        }
    }

    // Patch AllOwnersHaveMentalBreak to not count if there are Mechanoids in the caravan (otherwise Mech-only caravans come up as 'all pawns have mental break')
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(Caravan), "get_AllOwnersHaveMentalBreak")]
    public static class Caravan_AllOwnersHaveMentalBreak_Patch {
        public static void Postfix(ref bool __result, ref Caravan __instance) {
            foreach(Pawn p in __instance.pawns) {
                if(p.RaceProps.IsMechanoid) {
                    __result = false;
                    break;
                }
            }
        }
    }

    // Patch CanFormOrReformCaravanNow method. Normally reforming is possible if there are no colonists; we allow reforming if there are mechanoids
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(FormCaravanComp), "get_CanFormOrReformCaravanNow")]
    public static class FormCaravanComp_CanFormOrReformCaravanNow_Patch {
        public static void Postfix(ref bool __result, ref FormCaravanComp __instance) {
            MapParent mapParent = (MapParent) __instance.parent;
            if(mapParent.HasMap && mapParent.Map.mapPawns.SpawnedColonyMechs.Count > 0) {
                __result = true;
            }
        }
    }

    // Patch CanReformNow method. Normally reforming is possible if there are no colonists; we allow reforming if there are mechanoids
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(FormCaravanComp), "CanReformNow")]
    public static class FormCaravanComp_CanReformNow_Patch {
        public static void Postfix(ref bool __result, ref FormCaravanComp __instance) {
            MapParent mapParent = (MapParent) __instance.parent;
            if(mapParent.HasMap && mapParent.Map.mapPawns.SpawnedColonyMechs.Count > 0) {
                __result = true;
            }
        }
    }

    // Patch FormCaravanComp GetGizmos method to allow the 'Reform Caravan' button to pop up if our caravan has no colonists but does have mechanoids
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch]
    public static class FormCaravanComp_GetGizmos_Patch {

        public static MethodBase TargetMethod() {
            
            return AccessTools.Method(AccessTools.Inner(typeof(FormCaravanComp), "<GetGizmos>d__18"), "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();

            var insertionPoint = -1;

            for(int i=0; i<codes.Count; i++) {
                if(codes[i].opcode == OpCodes.Castclass) {
                    insertionPoint = i + 13;
                    codes[i+20].labels.Add(jumpLabel);
                    break;
                }
            }

            if(insertionPoint == -1) {
                Log.Error("MechsCaravan could not find FormCaravanComp_GetGizmos_Patch insertion point!");
            } else {
                var newCodes = new List<CodeInstruction>();

                newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Load arg 0 onto stack. I'm not sure what this is but I know this is what you have to do
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(FormCaravanComp), "<GetGizmos>d__18"), "<>8__1"))); // get <>8__1 field from arg 0. Again not sure what this is but I know you have to do it, has something to do with how an IEnumerable method is compiled
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(FormCaravanComp), "<>c__DisplayClass18_0"), "mapParent"))); // get mapParent field. This appears as just the 'mapParent' variable in the method, if you look at the C# code
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapParent), "get_Map"))); // put mapParent.Map onto stack
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Map), "mapPawns"))); // put mapParent.Map.mapPawns onto stack
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapPawns), "get_SpawnedColonyMechs"))); // put mapParent.Map.mapPawns.SpawnedColonyMechs onto stack
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Pawn>), "get_Count"))); // put mapParent.Map.mapPawns.SpawnedColonyMechs.Count onto stack
                newCodes.Add(new CodeInstruction(OpCodes.Brtrue, jumpLabel)); // Jump over the next if statement in C# code, if there's at least 1 mech (Brtrue will jump for any non-zero value)

                codes.InsertRange(insertionPoint, newCodes);
            }
            return codes.AsEnumerable();
        }
    }

    // Patch ShouldShowWarningForMechWithoutMechanitor method, so that if we make a caravan with mechs and no colonists (the whole point of the mod), we won't get a warning message for not having a mechanitor.
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(Dialog_FormCaravan), "ShouldShowWarningForMechWithoutMechanitor")]
    public static class Dialog_FormCaravan_ShouldShowWarningForMechWithoutMechanitor_Patch {
        public static void Postfix(ref bool __result, ref Dialog_FormCaravan __instance) {
            foreach (TransferableOneWay transferable in __instance.transferables) {
                if (transferable.HasAnyThing && transferable.AnyThing is Pawn) {
                    for (int i = 0; i < transferable.CountToTransfer; i++) {
                        Pawn p = (Pawn) transferable.things[i];

                        if(p.IsColonist) {
                            return;
                        }
                    }
                }
            }

            __result = false;
        }
    }

    // Patch Dialog_FormCaravan TrySend method to prevent 'Caravan has no one with social skills' warning when Caravan has no colonists anyways
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(Dialog_FormCaravan), "TrySend")]
    public static class Dialog_FormCaravan_TrySend_Patch {
    
        public static bool PawnListHasNoColonists(List<Pawn> pawns) {
            foreach(Pawn p in pawns) {
                if(p.IsColonist) {
                    return false;
                }
            }
            return true;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();

            var insertionPoint = -1;

            for(int i=0; i<codes.Count; i++) {
                if(codes[i].opcode == OpCodes.Ldstr && codes[i].operand.ToString() == "CaravanFoodWillRotSoonWarningDialog") {
                    insertionPoint = i + 5;
                    codes[i+22].labels.Add(jumpLabel);
                    break;
                }
            }

            if(insertionPoint == -1) {
                Log.Error("MechsCaravan could not find Dialog_FormCaravan_TrySend_Patch insertion point!");
            } else {
                var newCodes = new List<CodeInstruction>();

                // newCodes.Add(new CodeInstruction(OpCodes.Ldloc_0)); Normally we would do this here, but there happens to already be a Ldloc_0 at the right place for us. Furthermore, there are two Labels on it, so the preceding if statements could skip our code. Instead, I just insert after the existing Ldloc_0, and insert a new one at the end so that the next bit of source code works properly
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(Dialog_FormCaravan), "<>c__DisplayClass89_0"), "pawns"))); // Add List<Pawn>, I think, to the stack. Honestly not sure why this is necessary. I thought Ldloc_0 added the pawns variable to the stack, but I guess you need this too? It won't work without this in any case
                newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_FormCaravan_TrySend_Patch), "PawnListHasNoColonists", new Type[] {typeof(List<Pawn>)}))); // Call our own method to check if the pawns list has no colonists
                newCodes.Add(new CodeInstruction(OpCodes.Brtrue, jumpLabel)); // Jump, if there are no colonists in the list, skipping the CaravanIncapableOfSocial code
                newCodes.Add(new CodeInstruction(OpCodes.Ldloc_0)); // Add back the Ldloc_0 that we originally stole from the CaravanIncapableOfSocial code section

                codes.InsertRange(insertionPoint, newCodes);
            }
            return codes.AsEnumerable();
        }
    }

    // Patch CaravanUtility RandomOwner method; normally a Warning is logged if this method is called on a Caravan with no colonists, since it tries to pick randomly from 0 colonists. If the Caravan has no colonists, pick a random Mechanoid in the caravan instead.
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(CaravanUtility), "RandomOwner")]
    public static class CaravanUtility_RandomOwner_Patch {
        public static bool Prefix(ref Pawn __result, Caravan caravan) {            
            foreach(Pawn p in caravan.PawnsListForReading) {
                if(CaravanUtility.IsOwner(p, caravan.Faction)) {
                    return true;
                }
            }
            
            __result = caravan.PawnsListForReading.Where((Pawn p) => p.RaceProps.IsMechanoid).RandomElement();
            return false;
        }
    }

    // Patch CaravansBattlefield CheckWonBattle method to select a Mechanoid for RecordTale if there are no colonists on the battlefield
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(CaravansBattlefield), "CheckWonBattle")]
    public static class CaravansBattlefield_CheckWonBattle_Patch {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            var finishJumpLabel = il.DefineLabel();
            var jumpCode = new CodeInstruction(OpCodes.Ldarg_0);
            jumpCode.labels.Add(jumpLabel);

            var insertionPoint = -1;

            for(int i=0; i<codes.Count; i++) {
                if(codes[i].opcode == OpCodes.Stelem_Ref) {
                    insertionPoint = i - 1;
                    codes[i+1].labels.Add(finishJumpLabel);
                    break;
                }
            }

            if(insertionPoint == -1) {
                Log.Error("MechsCaravan could not find CaravansBattlefield_CheckWonBattle_Patch insertion point!");
            } else {
                var newCodes = new List<CodeInstruction>();
                
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Pawn>), "get_Count"))); // call getFreeColonists.Count
                newCodes.Add(new CodeInstruction(OpCodes.Brtrue, jumpLabel)); // If getFreeColonists.Count is greater than 0, jump to jumpCode

                newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapParent), "get_Map")));
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Map), "mapPawns")));
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapPawns), "get_SpawnedColonyMechs")));
                newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenCollection), "RandomElement", generics: new Type[] {typeof(Pawn)})));
                newCodes.Add(new CodeInstruction(OpCodes.Stelem_Ref)); // All these lines collectively do map.Map.mapPawns.SpawnedColonyMechs.RandomElement()
                newCodes.Add(new CodeInstruction(OpCodes.Br, finishJumpLabel)); // Now, jump to skip original code

                newCodes.Add(jumpCode);
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapParent), "get_Map")));
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Map), "mapPawns")));
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapPawns), "get_FreeColonists"))); // These lines do map.Map.mapPawns.get_FreeColonists, which was originally there but we 'stole' it so now we're putting it back
                codes.InsertRange(insertionPoint, newCodes);
            }
            
            return codes.AsEnumerable();
        }
    }

    // Patch SettlementDefeatUtility CheckDefeated method to select a Mechanoid for RecordTale if there are no colonists on the battlefield
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class SettlementDefeatUtility_CheckDefeated_Patch {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            var finishJumpLabel = il.DefineLabel();
            var jumpCode = new CodeInstruction(OpCodes.Ldloc_0);
            jumpCode.labels.Add(jumpLabel);

            var insertionPoint = -1;

            for(int i=0; i<codes.Count; i++) {
                if(codes[i].opcode == OpCodes.Stelem_Ref) {
                    insertionPoint = i - 1;
                    codes[i+1].labels.Add(finishJumpLabel);
                    break;
                }
            }

            if(insertionPoint == -1) {
                Log.Error("MechsCaravan could not find SettlementDefeatUtility_CheckDefeated_Patch insertion point!");
            } else {
                var newCodes = new List<CodeInstruction>();
                
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Pawn>), "get_Count"))); // call getFreeColonists.Count
                newCodes.Add(new CodeInstruction(OpCodes.Brtrue, jumpLabel)); // If getFreeColonists.Count is greater than 0, jump to jumpCode

                newCodes.Add(new CodeInstruction(OpCodes.Ldloc_0));
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Map), "mapPawns")));
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapPawns), "get_SpawnedColonyMechs")));
                newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenCollection), "RandomElement", generics: new Type[] {typeof(Pawn)})));
                newCodes.Add(new CodeInstruction(OpCodes.Stelem_Ref)); // All these lines collectively do map.Map.mapPawns.SpawnedColonyMechs.RandomElement()
                newCodes.Add(new CodeInstruction(OpCodes.Br, finishJumpLabel)); // Now, jump to skip original code

                newCodes.Add(jumpCode);
                newCodes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Map), "mapPawns")));
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(MapPawns), "get_FreeColonists"))); // These lines do map.Map.mapPawns.get_FreeColonists, which was originally there but we 'stole' it so now we're putting it back
                codes.InsertRange(insertionPoint, newCodes);
            }
            
            return codes.AsEnumerable();
        }
    }

    // Patch JobDriver_PrepareCaravan_GatherItems IsUsableCarrier method to allow Mechanoids to carry items when forming a caravan in the colony. This probably isn't necessary but I'm doing it anyways. I think it should allow mechs that can't Haul to be loaded like pack animals
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(JobDriver_PrepareCaravan_GatherItems), "IsUsableCarrier")]
    public static class JobDriver_PrepareCaravan_GatherItems_IsUsableCarrier_Patch {
        public static void Postfix(ref bool __result, Pawn p, Pawn forPawn, bool allowColonists) {            
            if ((p.RaceProps.IsMechanoid || p.HostFaction == Faction.OfPlayer) && !p.IsBurning() && !p.Downed) {
                __result = !MassUtility.IsOverEncumbered(p);
            }
        }
    }

    // Patch LordToil_PrepareCaravan_GatherItems UpdateAllDuties so Mechanoids can gather items when forming a caravan
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(LordToil_PrepareCaravan_GatherItems), "UpdateAllDuties")]
    public static class LordToil_PrepareCaravan_GatherItems_UpdateAllDuties_Patch {


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            var insertionPoint = -1;

            for(int i=0; i<codes.Count; i++) {
                if(codes[i].opcode == OpCodes.Callvirt) {
                    insertionPoint = i + 2;
                    codes[i+5].labels.Add(jumpLabel);
                    break;
                }
            }

            if(insertionPoint == -1) {
                Log.Error("MechsCaravan could not find LordToil_PrepareCaravan_GatherItems_UpdateAllDuties_Patch insertion point!");
            } else {
                var newCodes = new List<CodeInstruction>();

                newCodes.Add(new CodeInstruction(OpCodes.Ldloc_1)); // Load local Pawn variable
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps"))); // Pawn.RaceProps
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_IsMechanoid"))); // Pawn.RaceProps.Is_Mechanoid
                newCodes.Add(new CodeInstruction(OpCodes.Brtrue_S, jumpLabel)); // Jump if it is a mechanoid (effectively equivalent to changing original line of code to: (Pawn.RaceProps.Is_Mechanoid || Pawn.IsColonist))

            codes.InsertRange(insertionPoint, newCodes);
            }
            return codes.AsEnumerable();
        }
    }

    // Patch LordToil_PrepareCaravan_GatherItems LordToilTick so Mechanoids can gather items when forming a caravan
    [HarmonyPatchCategory("BaseGame")]
    [HarmonyPatch(typeof(LordToil_PrepareCaravan_GatherItems), "LordToilTick")]
    public static class LordToil_PrepareCaravan_GatherItems_LordToilTick_Patch {


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = new List<CodeInstruction>(instructions);
            
            var jumpLabel = il.DefineLabel();
            var insertionPoint = -1;

            for(int i=0; i<codes.Count; i++) {
                if(codes[i].opcode == OpCodes.Stloc_2) {
                    insertionPoint = i + 1;
                    codes[i+4].labels.Add(jumpLabel);
                    break;
                }
            }

            if(insertionPoint == -1) {
                Log.Error("MechsCaravan could not find LordToil_PrepareCaravan_GatherItems_LordToilTick_Patch insertion point!");
            } else {
                var newCodes = new List<CodeInstruction>();

                newCodes.Add(new CodeInstruction(OpCodes.Ldloc_2)); // Load local Pawn variable
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps"))); // Pawn.RaceProps
                newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_IsMechanoid"))); // Pawn.RaceProps.Is_Mechanoid
                newCodes.Add(new CodeInstruction(OpCodes.Brtrue_S, jumpLabel)); // Jump if it is a mechanoid (effectively equivalent to changing original line of code to: (Pawn.RaceProps.Is_Mechanoid || Pawn.IsColonist))

            codes.InsertRange(insertionPoint, newCodes);
            }
            return codes.AsEnumerable();
        }
    }

    [StaticConstructorOnStartup]
    public static class Start {
        static Start() {

            Harmony harmony = new Harmony("MechCaravans");
            harmony.PatchCategory("BaseGame");
            
            var gestaltMethodType = Type.GetType("GestaltEngine.TransferableUIUtility_DoCountAdjustInterfaceInternal_Patch, GestaltEngine");
            if(gestaltMethodType != null) {
                // harmony.PatchCategory("GestaltEngine");
                var originalGestaltMethod = gestaltMethodType.GetMethod("Prefix");
                
                var gestaltPrefixMethod = typeof(Skip_Gestalt_CaravanUI_Patch).GetMethod("Prefix");
                harmony.Patch(originalGestaltMethod, prefix: new HarmonyMethod(gestaltPrefixMethod));
            }

            
        }

    }
}

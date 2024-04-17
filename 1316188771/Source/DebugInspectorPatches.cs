﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DoorsExpanded
{
    public static class DebugViewSettingsMoreDraw
    {
        public static bool drawDoorTempEqualization;
    }

    public static class DebugViewSettingsMoreWrite
    {
        public static bool writeTemperature;
        public static bool writeEdificeGrid;
        public static bool writeDoors;
        public static bool writePatchCallRegistry;
    }

    public static class DebugInspectorPatches
    {
        public static void PatchDebugInspector()
        {
            //var harmony = HarmonyPatches.harmony;
            //var type = typeof(DebugInspectorPatches);
            //harmony.Patch(original: AccessTools.Constructor(typeof(Dialog_DebugSettingsMenu)),
            //    transpiler: new HarmonyMethod(type, nameof(AddMoreDebugViewSettingsTranspiler)));
            //harmony.Patch(original: AccessTools.Method(typeof(Dialog_DebugSettingsMenu), "DoListingItems"),
            //    transpiler: new HarmonyMethod(type, nameof(AddMoreDebugViewSettingsTranspiler)),
            //    postfix: new HarmonyMethod(type, nameof(AddMoreDebugViewSettingsPostfix)));
            //harmony.Patch(original: AccessTools.Method(typeof(EditWindow_DebugInspector), "CurrentDebugString"),
            //    transpiler: new HarmonyMethod(type, nameof(EditWindowDebugInspectorTranspiler)));
            //harmony.Patch(original: AccessTools.Method(typeof(District), "DebugString"),
            //    postfix: new HarmonyMethod(type, nameof(DistrictMoreDebugString)));
            //harmony.Patch(original: AccessTools.Method(typeof(Room), nameof(Room.DebugString)),
            //    postfix: new HarmonyMethod(type, nameof(RoomMoreDebugString)));
            //harmony.Patch(original: AccessTools.Method(typeof(DoorsDebugDrawer), nameof(DoorsDebugDrawer.DrawDebug)),
            //    postfix: new HarmonyMethod(type, nameof(AddMoreDoorsDebugDrawer)));
        }

        private static Dictionary<string, bool> patchCallRegistry;

        // Note: Callers of RegisterPatch and RegisterPatchCalled need to have #define PATCH_CALL_REGISTRY in their file,
        // since its callers of Conditional-attributed methods that are ellided rather than the method itself.
        [Conditional("PATCH_CALL_REGISTRY")]
        public static void RegisterPatch(string name)
        {
            if (name is not null)
            {
                patchCallRegistry ??= new Dictionary<string, bool>();
                patchCallRegistry[name] = false;
            }
        }

        [Conditional("PATCH_CALL_REGISTRY")]
        public static void RegisterPatchCalled(string name) => patchCallRegistry[name] = true;

        // Dialog_DebugSettingsMenu constructor (for RW 1.2)
        // Dialog_DebugSettingsMenu.DoListingItems
        public static IEnumerable<CodeInstruction> AddMoreDebugViewSettingsTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            // This transforms the following code:
            //  typeof(DebugViewSettings).GetFields()
            // into:
            //  AddMoreDebugViewSettings(typeof(DebugViewSettings).GetFields())

            var instructionList = instructions.AsList();
            var debugViewSettingsIndex = instructionList.FindIndex(instr => instr.OperandIs(typeof(DebugViewSettings)));
            if (debugViewSettingsIndex >= 0)
            {
                var getFieldsIndex = instructionList.FindIndex(debugViewSettingsIndex + 1, instr => instr.Calls(methodof_Type_GetFields));
                instructionList.SafeInsertRange(getFieldsIndex + 1, new[]
                {
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DebugInspectorPatches), nameof(AddMoreDebugViewSettings))),
                });
            }
            return instructionList;
        }

        private static FieldInfo[] AddMoreDebugViewSettings(FieldInfo[] fields)
        {
            var fieldList = fields.ToList();
            var fieldsToInsert = typeof(DebugViewSettingsMoreWrite).GetFields().AsEnumerable();
            if (patchCallRegistry is null)
                fieldsToInsert = fieldsToInsert.Where(field => field != fieldof_MoreDebugViewSettings_writePatchCallRegistry);
            fieldList.InsertRange(fieldList.IndexOf(fieldof_DebugViewSettings_writePathCosts) + 1, fieldsToInsert);
            fieldList.InsertRange(fieldList.IndexOf(fieldof_DebugViewSettings_drawDoorsDebug) + 1,
                typeof(DebugViewSettingsMoreDraw).GetFields());
            return fieldList.ToArray();
        }

        private static readonly MethodInfo methodof_Type_GetFields = AccessTools.Method(typeof(Type), nameof(Type.GetFields));
        private static readonly FieldInfo fieldof_DebugViewSettings_drawDoorsDebug =
            AccessTools.Field(typeof(DebugViewSettings), nameof(DebugViewSettings.drawDoorsDebug));
        private static readonly FieldInfo fieldof_DebugViewSettings_writePathCosts =
            AccessTools.Field(typeof(DebugViewSettings), nameof(DebugViewSettings.writePathCosts));
        private static readonly FieldInfo fieldof_MoreDebugViewSettings_writePatchCallRegistry =
            AccessTools.Field(typeof(DebugViewSettingsMoreWrite), nameof(DebugViewSettingsMoreWrite.writePatchCallRegistry));

        public static void AddMoreDebugViewSettingsPostfix()
        {
            // We do dynamic patching here to try to avoid the performance overhead in the patched methods when toggled off.
            if (temperatureEqualizationMethodsPatched != DebugViewSettingsMoreDraw.drawDoorTempEqualization)
            {
                foreach (var pair in temperatureEqualizationMethods)
                {
                    var origMethod = pair.Key;
                    var patchMethod = pair.Value;
                    if (temperatureEqualizationMethodsPatched)
                        HarmonyPatches.harmony.Unpatch(origMethod, patchMethod);
                    else
                        HarmonyPatches.harmony.Patch(origMethod, transpiler: new HarmonyMethod(patchMethod));
                }
                temperatureEqualizationMethodsPatched = DebugViewSettingsMoreDraw.drawDoorTempEqualization;
            }
        }

        private static bool temperatureEqualizationMethodsPatched = false;
        private static readonly Dictionary<MethodInfo, MethodInfo> temperatureEqualizationMethods = new()
        {
            [AccessTools.Method(typeof(RoomTempTracker), nameof(RoomTempTracker.EqualizeTemperature))] =
                AccessTools.Method(typeof(DebugInspectorPatches), nameof(RoomTempTracker_EqualizeTemperature_Transpiler)),
            [AccessTools.Method(typeof(GenTemperature), nameof(GenTemperature.EqualizeTemperaturesThroughBuilding))] =
                AccessTools.Method(typeof(DebugInspectorPatches), nameof(GenTemperature_EqualizeTemperaturesThroughBuilding_Transpiler)),
        };

        public static IEnumerable<CodeInstruction> RoomTempTracker_EqualizeTemperature_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // This transforms the following code:
            //  this.room.Temperature += WallEqualizationTempChangePerInterval()
            // into:
            //  RoomTempTracker_EqualizeTemperature_SetRoomTemperature(this.room,
            //      this.room.Temperature + WallEqualizationTempChangePerInterval())

            return Transpilers.MethodReplacer(instructions,
                AccessTools.PropertySetter(typeof(Room), nameof(Room.Temperature)),
                AccessTools.Method(typeof(DebugInspectorPatches),
                    nameof(RoomTempTracker_EqualizeTemperature_SetRoomTemperature)));
        }

        private static void RoomTempTracker_EqualizeTemperature_SetRoomTemperature(Room room, float temperature)
        {
            SetRoomTemperatureAndDrawDelta(room, temperature, Color.red);
        }

        public static IEnumerable<CodeInstruction> GenTemperature_EqualizeTemperaturesThroughBuilding_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            // This transforms the following code:
            //  room.Temperature += tempDelta
            // into:
            //  SetRoomTemperatureAndDrawDelta(room, room.Temperature + tempDelta)

            return Transpilers.MethodReplacer(instructions,
                AccessTools.PropertySetter(typeof(Room), nameof(Room.Temperature)),
                AccessTools.Method(typeof(DebugInspectorPatches),
                    nameof(GenTemperature_EqualizeTemperaturesThroughBuilding_SetRoomTemperature)));
        }

        private static void GenTemperature_EqualizeTemperaturesThroughBuilding_SetRoomTemperature(Room room, float temperature)
        {
            SetRoomTemperatureAndDrawDelta(room, temperature, Color.white);
        }

        private static void SetRoomTemperatureAndDrawDelta(Room room, float temperature, Color color)
        {
            if (room.IsDoorway)
            {
                var tempDelta = temperature - room.Temperature;
                var door = room.FirstRegion.door;
                if (door is Building_DoorRegionHandler)
                    Log.Message($"[{color}] temperature of {door}: {room.Temperature} => {temperature}");
                if (tempDelta != 0f)
                    MoteMaker.ThrowText(room.ExtentsClose.CenterVector3, room.Map, $"{tempDelta:+0.##;-0.##}", color);
            }
            room.Temperature = temperature;
        }

        // EditWindow_DebugInspector.CurrentDebugString
        public static IEnumerable<CodeInstruction> EditWindowDebugInspectorTranspiler(IEnumerable<CodeInstruction> instructions,
            MethodBase method, ILGenerator ilGen)
        {
            var methodof_UI_MouseCell = AccessTools.Method(typeof(UI), nameof(UI.MouseCell));
            var methodof_object_ToString = AccessTools.Method(typeof(object), nameof(ToString));
            var instructionList = instructions.AsList();
            var locals = new Locals(method, ilGen);

            // Assume first found StringBuilder var (currently the only StringBuilder var) is the one we want.
            var stringBuilderVar = locals.FromStloc(instructionList.Find(instr =>
                locals.IsStloc(instr, out var local) && local.LocalType == typeof(StringBuilder)));

            var mouseCellIndex = instructionList.FindIndex(instr => instr.Calls(methodof_UI_MouseCell));
            var mouseCellVar = locals.FromStloc(instructionList[mouseCellIndex + 1]);

            var mouseCellToStringIndex = instructionList.FindSequenceIndex(
                instr => locals.IsLdloca(instr, mouseCellVar),
                instr => instr.Is(OpCodes.Constrained, typeof(IntVec3)),
                instr => instr.Calls(methodof_object_ToString));
            instructionList.SafeReplaceRange(mouseCellToStringIndex, 3, new[]
            {
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DebugInspectorPatches), nameof(MousePositionToString))),
            });

            var writePathCostsFlagIndex = instructionList.FindIndex(
                instr => instr.LoadsField(fieldof_DebugViewSettings_writePathCosts));
            var nextFlagIndex = instructionList.FindIndex(writePathCostsFlagIndex + 1, IsDebugViewSettingFlagAccess);
            // Note: Not using SafeInsertRange, since labels need to be transferred to a new instruction in the middle.
            instructionList.InsertRange(nextFlagIndex - 1, new[]
            {
                stringBuilderVar.ToLdloc(),
                mouseCellVar.ToLdloc(),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DebugInspectorPatches), nameof(WriteMorePathCostsDebugOutput))),
                stringBuilderVar.ToLdloc().TransferLabelsAndBlocksFrom(instructionList[nextFlagIndex]),
                mouseCellVar.ToLdloc(),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DebugInspectorPatches), nameof(WriteMoreDebugOutput))),
            });

            return instructionList;
        }

        private static string MousePositionToString()
        {
            var mousePos = UI.MouseMapPosition();
            return $"({mousePos.x:000.00}, {mousePos.z:000.00})";
        }

        private static bool IsDebugViewSettingFlagAccess(CodeInstruction instruction) =>
            instruction.opcode == OpCodes.Ldsfld && ((FieldInfo)instruction.operand).DeclaringType == typeof(DebugViewSettings);

        private static void WriteMorePathCostsDebugOutput(StringBuilder debugString, IntVec3 mouseCell)
        {
            if (Find.Selector.SingleSelectedObject is Pawn pawn)
            {
                debugString.AppendLine($"CalculatedCostAt({mouseCell}, false, {pawn.Position}): " +
                    pawn.Map.pathing.For(pawn).pathGrid.CalculatedCostAt(mouseCell, perceivedStatic: false, pawn.Position));
                debugString.AppendLine($"CostToMoveIntoCell({pawn}, {mouseCell}): " + CostToMoveIntoCell(pawn, mouseCell));
                using var pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, mouseCell, pawn);
                debugString.AppendLine($"FindPath({pawn.Position}, {mouseCell}, {pawn}).TotalCost: " + pawnPath.TotalCost);
            }
        }

        private static readonly Func<Pawn, IntVec3, int> CostToMoveIntoCell =
            (Func<Pawn, IntVec3, int>)Delegate.CreateDelegate(typeof(Func<Pawn, IntVec3, int>),
                AccessTools.Method(typeof(Pawn_PathFollower), "CostToMoveIntoCell", new[] { typeof(Pawn), typeof(IntVec3) }));

        private static void WriteMoreDebugOutput(StringBuilder debugString, IntVec3 mouseCell)
        {
            var map = Find.CurrentMap;

            if (DebugViewSettingsMoreWrite.writeTemperature)
            {
                debugString.AppendLine("---");
                debugString.AppendLine("Temperature: " +
                    (GenTemperature.TryGetTemperatureForCell(mouseCell, map, out var temperature) ? $"{temperature:f1}" : ""));
            }

            if (DebugViewSettingsMoreWrite.writeEdificeGrid)
            {
                debugString.AppendLine("---");
                debugString.AppendLine("Building at edifice grid: " + map.edificeGrid[mouseCell]);
            }

            if (DebugViewSettingsMoreWrite.writeDoors)
            {
                debugString.AppendLine("---");
                var pawn = Find.Selector.SingleSelectedObject as Pawn;
                if (pawn is not null)
                {
                    debugString.AppendLine("From selected pawn " + pawn + " to door");
                    debugString.AppendLine("- CaresAboutForbidden(p, false): " + ForbidUtility.CaresAboutForbidden(pawn, false));
                    //debugString.AppendLine("- mindState.maxDistToSquadFlag: " + pawn.mindState.maxDistToSquadFlag);
                }

                foreach (var thing in map.thingGrid.ThingsAt(mouseCell))
                {
                    if (thing is Building_Door door)
                    {
                        debugString.AppendLine("Door: " + door);
                        debugString.AppendLine("- Open: " + door.Open);
                        debugString.AppendLine("- HoldOpen: " + door.HoldOpen);
                        debugString.AppendLine("- FreePassage: " + door.FreePassage);
                        debugString.AppendLine("- WillCloseSoon: " + door.WillCloseSoon);
                        debugString.AppendLine("- BlockedOpenMomentary: " + door.BlockedOpenMomentary);
                        debugString.AppendLine("- SlowsPawns: " + door.SlowsPawns);
                        debugString.AppendLine("- TicksToOpenNow: " + door.TicksToOpenNow);
                        debugString.AppendLine("- FriendlyTouchedRecently: " +
                            methodof_Building_Door_FriendlyTouchedRecently.Invoke(door, Array.Empty<object>()));
                        debugString.AppendLine("- lastFriendlyTouchTick: " + Building_Door_lastFriendlyTouchTick(door));
                        debugString.AppendLine("- ticksUntilClose: " + Building_Door_ticksUntilClose(door));
                        debugString.AppendLine("- ticksSinceOpen: " + Building_Door_ticksSinceOpen(door));
                        debugString.AppendLine("- IsForbidden(player): " + door.IsForbidden(Faction.OfPlayer));
                        debugString.AppendLine("- def.Fillage: " + door.def.Fillage);
                        debugString.AppendLine("- CanBeSeenOver: " + door.CanBeSeenOver());
                        debugString.AppendLine("- BaseBlockChance: " + door.BaseBlockChance());

                        if (pawn is not null)
                        {
                            debugString.AppendLine("- For selected pawn: " + pawn);
                            debugString.AppendLine("  - CanPhysicallyPass(p): " + door.CanPhysicallyPass(pawn));
                            debugString.AppendLine("  - PawnCanOpen(p): " + door.PawnCanOpen(pawn));
                            debugString.AppendLine("  - BlocksPawn(p): " + door.BlocksPawn(pawn));
                            debugString.AppendLine("  - p.HostileTo(this): " + pawn.HostileTo(door));
                            debugString.AppendLine("  - PathWalkCostFor(p): " + door.PathWalkCostFor(pawn));
                            debugString.AppendLine("  - IsDangerousFor(p): " + door.IsDangerousFor(pawn));
                            debugString.AppendLine("  - IsForbidden(p): " + door.IsForbidden(pawn));
                            debugString.AppendLine("  - Position.IsForbidden(p): " + door.Position.IsForbidden(pawn));
                            debugString.AppendLine("  - Position.InAllowedArea(p): " + door.Position.InAllowedArea(pawn));
                            //debugString.AppendLine("  - Position.InHorDistOf(p.DutyLocation,p.mindState.maxDistToSquadFlag): " +
                            //    (pawn.mindState.maxDistToSquadFlag > 0f ?
                            //        door.Position.InHorDistOf(pawn.DutyLocation(), pawn.mindState.maxDistToSquadFlag).ToString() :
                            //        "N/A"));
                            //debugString.AppendLine("  - IsForbidden(p.Faction): " + door.IsForbidden(pawn.Faction));
                            //debugString.AppendLine("  - IsForbidden(p.HostFaction): " + door.IsForbidden(pawn.HostFaction));
                            //debugString.AppendLine("  - pawn.Lord.extraForbiddenThings.Contains(this): " +
                            //    (pawn.GetLord()?.extraForbiddenThings?.Contains(door) ?? false));
                            debugString.AppendLine("  - IsForbiddenToPass(p): " + door.IsForbiddenToPass(pawn));
                        }

                        if (door is Building_DoorRegionHandler invisDoor)
                        {
                            var parentDoor = invisDoor.ParentDoor;
                            debugString.AppendLine("- ParentDoor: " + parentDoor);
                            debugString.AppendLine("  - DrawPos: " + parentDoor.DrawPos);
                            debugString.AppendLine("  - debugDrawVectors.openPct: " + parentDoor.debugDrawVectors?.openPct);
                            debugString.AppendLine("  - debugDrawVectors.offsetVector: " + parentDoor.debugDrawVectors?.offsetVector);
                            debugString.AppendLine("  - debugDrawVectors.scaleVector: " + parentDoor.debugDrawVectors?.scaleVector);
                            debugString.AppendLine("  - debugDrawVectors.graphicVector: " + parentDoor.debugDrawVectors?.graphicVector);
                            debugString.AppendLine("  - Open: " + parentDoor.Open);
                            debugString.AppendLine("  - HoldOpen: " + parentDoor.HoldOpen);
                            debugString.AppendLine("  - FreePassage: " + parentDoor.FreePassage);
                            debugString.AppendLine("  - WillCloseSoon: " + parentDoor.WillCloseSoon);
                            debugString.AppendLine("  - BlockedOpenMomentary: " + parentDoor.BlockedOpenMomentary);
                            debugString.AppendLine("  - SlowsPawns: " + parentDoor.SlowsPawns);
                            debugString.AppendLine("  - TicksToOpenNow: " + parentDoor.TicksToOpenNow);
                            debugString.AppendLine("  - FriendlyTouchedRecently: " + parentDoor.FriendlyTouchedRecently);
                            debugString.AppendLine("  - lastFriendlyTouchTick: " + Building_DoorExpanded_lastFriendlyTouchTick(parentDoor));
                            debugString.AppendLine("  - ticksUntilClose: " + parentDoor.TicksUntilClose);
                            debugString.AppendLine("  - ticksSinceOpen: " + parentDoor.TicksSinceOpen);
                            debugString.AppendLine("  - Forbidden: " + parentDoor.Forbidden);
                            debugString.AppendLine("  - def.Fillage: " + parentDoor.def.Fillage);
                            debugString.AppendLine("  - CanBeSeenOver: " + parentDoor.CanBeSeenOver());
                            debugString.AppendLine("  - BaseBlockChance: " + parentDoor.BaseBlockChance());

                            if (parentDoor is Building_DoorRemote parentDoorRemote)
                            {
                                debugString.AppendLine("  - Button: " + parentDoorRemote.Button);
                                debugString.AppendLine("  - SecuredRemotely: " + parentDoorRemote.SecuredRemotely);
                                debugString.AppendLine("  - HoldOpenRemotely: " + parentDoorRemote.HoldOpenRemotely);
                                debugString.AppendLine("  - ForcedClosed: " + parentDoorRemote.ForcedClosed);
                            }

                            if (pawn is not null)
                            {
                                debugString.AppendLine("  - For selected pawn: " + pawn);
                                //debugString.AppendLine("    - CanPhysicallyPass(p): " + parentDoor.CanPhysicallyPass(pawn));
                                debugString.AppendLine("    - PawnCanOpen(p): " + parentDoor.PawnCanOpen(pawn));
                                debugString.AppendLine("    - BlocksPawn(p): " + parentDoor.BlocksPawn(pawn));
                                debugString.AppendLine("    - p.HostileTo(this): " + pawn.HostileTo(parentDoor));
                                debugString.AppendLine("    - PathWalkCostFor(p): " + parentDoor.PathWalkCostFor(pawn));
                                debugString.AppendLine("    - IsDangerousFor(p): " + parentDoor.IsDangerousFor(pawn));
                                debugString.AppendLine("    - IsForbidden(p): " + parentDoor.IsForbidden(pawn));
                                debugString.AppendLine("    - Position.IsForbidden(p): " + parentDoor.Position.IsForbidden(pawn));
                                debugString.AppendLine("    - Position.InAllowedArea(p): " + parentDoor.Position.InAllowedArea(pawn));
                                //debugString.AppendLine("    - Position.InHorDistOf(p.DutyLocation,p.mindState.maxDistToSquadFlag): " +
                                //    (pawn.mindState.maxDistToSquadFlag > 0f ?
                                //        parentDoor.Position.InHorDistOf(pawn.DutyLocation(), pawn.mindState.maxDistToSquadFlag).ToString() :
                                //        "N/A"));
                                //debugString.AppendLine("    - IsForbidden(p.Faction): " + parentDoor.IsForbidden(pawn.Faction));
                                //debugString.AppendLine("    - IsForbidden(p.HostFaction): " + parentDoor.IsForbidden(pawn.HostFaction));
                                //debugString.AppendLine("    - pawn.Lord.extraForbiddenThings.Contains(this): " +
                                //    (pawn.GetLord()?.extraForbiddenThings?.Contains(parentDoor) ?? false));
                            }
                        }
                        var room = door.GetRoom();
                        if (room is not null)
                        {
                            debugString.AppendLine("- " + room.DebugString().Replace("\n  ", "\n    - ").Replace("\n\n", "\n"));
                        }
                    }
                    else if (thing is Building_DoorRemoteButton remoteButton)
                    {
                        debugString.AppendLine("RemoteButton: " + remoteButton);
                        debugString.AppendLine("- LinkedDoors: " + remoteButton.LinkedDoors.ToStringSafeEnumerable());
                        debugString.AppendLine("- ButtonOn: " + remoteButton.ButtonOn);
                        debugString.AppendLine("- NeedsToBeSwitched: " + remoteButton.NeedsToBeSwitched);
                    }
                }
            }

            if (DebugViewSettingsMoreWrite.writePatchCallRegistry)
            {
                debugString.AppendLine("---");
                debugString.AppendLine("Harmony Patch Call Registry:");
                foreach (var pair in patchCallRegistry)
                    debugString.AppendLine($"- {pair.Key}: {pair.Value}");
            }
        }

        private static readonly MethodInfo methodof_Building_Door_FriendlyTouchedRecently =
            AccessTools.PropertyGetter(typeof(Building_Door), "FriendlyTouchedRecently");
        private static readonly AccessTools.FieldRef<Building_Door, int> Building_Door_lastFriendlyTouchTick =
            AccessTools.FieldRefAccess<Building_Door, int>("lastFriendlyTouchTick");
        private static readonly AccessTools.FieldRef<Building_DoorExpanded, int> Building_DoorExpanded_lastFriendlyTouchTick =
            AccessTools.FieldRefAccess<Building_DoorExpanded, int>("lastFriendlyTouchTick");
        private static readonly AccessTools.FieldRef<Building_Door, int> Building_Door_ticksUntilClose =
            AccessTools.FieldRefAccess<Building_Door, int>("ticksUntilClose");
        private static readonly AccessTools.FieldRef<Building_Door, int> Building_Door_ticksSinceOpen =
            AccessTools.FieldRefAccess<Building_Door, int>("ticksSinceOpen");

        // District.DebugString
        public static string DistrictMoreDebugString(string result, District __instance)
        {
            return $"{result}\n  Neighbors=\n  - {__instance.Neighbors.Join(delimiter: "\n  - ")}";
        }

        // Room.DebugString
        public static string RoomMoreDebugString(string result, Room __instance)
        {
            return $"{result}\n  IsDoorway={__instance.IsDoorway}\n  TouchesMapEdge={__instance.TouchesMapEdge}" +
                $"\n  UsesOutdoorTemperature={__instance.UsesOutdoorTemperature}";
        }

        // DoorsDebugDrawer.DrawDebug
        public static void AddMoreDoorsDebugDrawer()
        {
            if (DebugViewSettingsMoreDraw.drawDoorTempEqualization)
            {
                var mouseCell = UI.MouseCell();
                var things = mouseCell.GetThingList(Find.CurrentMap);
                foreach (var thing in things)
                {
                    if (thing is Building_Door and not Building_DoorRegionHandler || thing is Building_DoorExpanded)
                    {
                        var adjCells = GenAdj.CellsAdjacentCardinal(thing);
                        foreach (var cell in adjCells)
                            CellRenderer.RenderCell(cell, 0.3f);
                    }
                }
            }
        }
    }
}

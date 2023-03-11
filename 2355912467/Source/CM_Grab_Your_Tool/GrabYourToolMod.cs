using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CM_Grab_Your_Tool
{
    public class GrabYourToolMod : Mod
    {
        private static GrabYourToolMod _instance;
        public static GrabYourToolMod Instance => _instance;

        public static bool UsingCombatExtended => ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == "Combat Extended");

        public ToolMemoryTracker ToolMemories => Current.Game.World.GetComponent<ToolMemoryTracker>();

        public GrabYourToolMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("CM_Grab_Your_Tool");
            harmony.PatchAll();

            _instance = this;
        }

        public bool IsPawnUsingTool(Pawn pawn)
        {
            return ToolMemories?.GetMemory(pawn)?.IsUsingTool ?? false;
        }

        public ToolMemory GetMemory(Pawn pawn)
        {
            return ToolMemories?.GetMemory(pawn);
        }

        public void ClearMemory(Pawn pawn)
        {
            ToolMemories?.ClearMemory(pawn);
        }
    }

    [StaticConstructorOnStartup]
    public static class ToilPatches
    {
        [HarmonyPatch(typeof(Toil))]
        [HarmonyPatch(MethodType.Constructor)]
        public static class Toil_Constructor
        {
            [HarmonyPostfix]
            public static void Postfix(Toil __instance)
            {
                if (__instance == null)
                    return;

                __instance.AddPreInitAction(delegate
                {
                    Pawn pawn = __instance.GetActor();

                    if (pawn == null || pawn.Dead || pawn.equipment == null || pawn.inventory == null || !pawn.RaceProps.Humanlike)
                        return;

                    if (pawn.Drafted)
                    {
                        GrabYourToolMod.Instance.ClearMemory(pawn);
                        return;
                    }

                    SkillDef activeSkill = pawn.CurJob?.RecipeDef?.workSkill;
                    if (__instance.activeSkill != null && __instance.activeSkill() != null)
                        activeSkill = __instance.activeSkill();

                    if (activeSkill != null)
                    {
                        ToolMemory memory = GrabYourToolMod.Instance.GetMemory(pawn);

                        if (!memory.UpdateSkill(activeSkill))
                            return;

                        // Don't do it if this job uses weapons (i.e. hunting)
                        if (activeSkill == SkillDefOf.Shooting || activeSkill == SkillDefOf.Melee)
                        {
                            memory.UpdateUsingTool(null, false);
                        }
                        // Check currently equipped item
                        else if (pawn.equipment.Primary != null && ToolMemoryTracker.HasReleventStatModifiers(pawn.equipment.Primary, activeSkill))
                        {
                            memory.UpdateUsingTool(null, true);
                        }
                        // Try and find something else in inventory
                        else
                        {
                            memory.UpdateUsingTool(pawn.equipment.Primary, ToolMemoryTracker.EquipAppropriateWeapon(pawn, activeSkill));
                        }
                    }
                    else
                    {
                        GrabYourToolMod.Instance.ClearMemory(pawn);
                    }
                });
            }
        }
    }

    //PawnRenderer
    [StaticConstructorOnStartup]
    public static class PawnRendererPatches
    {
        [HarmonyPatch(typeof(PawnRenderer))]
        [HarmonyPatch("CarryWeaponOpenly", MethodType.Normal)]
        public static class PawnRenderer_CarryWeaponOpenly
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn ___pawn)
            {
                if (!__result && GrabYourToolMod.Instance.IsPawnUsingTool(___pawn))
                    __result = true;
            }
        }
    }

    //FloatMenuMakerMap
    [StaticConstructorOnStartup]
    public static class FloatMenuMakerMapPatches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("AddHumanlikeOrders", MethodType.Normal)]
        public static class FloatMenuMakerMap_AddHumanlikeOrders
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo playerHome = AccessTools.Property(typeof(Map), "IsPlayerHome").GetGetMethod();
                bool replaceCall = !GrabYourToolMod.UsingCombatExtended;
                foreach (CodeInstruction instruction in instructions)
                {
                    if (replaceCall && instruction.Calls(playerHome))
                    {
                        instruction.opcode = OpCodes.Ldc_I4_0;
                        instruction.operand = null;
                        replaceCall = false;
                    }

                    yield return instruction;
                }
            }
        }
    }
}

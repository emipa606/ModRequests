

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_GlobalTextureAtlasManager
    {

        internal static void FreeAllRuntimeAtlases_Postfix()
        {
            StylishAtlasManager.AtlasMap.Clear();
            StylishAtlasManager.PreCreateCacheLeaveRaces();
        }
        /*
        internal static bool TryGetPawnFrameSet_Prefix(bool __result, Pawn pawn, ref PawnTextureAtlasFrameSet frameSet, bool allowCreatingNew)
        {
            if (!allowCreatingNew) return true;
            bool createdNew = default;
            if (StylishAtlasManager.AtlasMap.ContainsKey(pawn.def.defName))
            {
                if(StylishAtlasManager.AtlasMap[pawn.def.defName].TryGetFrameSet(pawn, out frameSet, out createdNew))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
        */
        [HarmonyPriority(999)]
        internal static bool TryGetPawnFrameSet_Prefix(bool __result, Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike) return !StylishAtlasManager.disableAnimalCache;
            if (StylishAtlasManager.forCorpseCache && pawn.Dead) return true;
            if (StylishAtlasManager.IsDisableCache(pawn))
            {
                return false;
            }
            else
            {
                PawnStyleSet pss = PawnStyleSet.Get(pawn);
                if (pss?.disableCache ?? false)
                {
                    StylishAtlasManager.disableCacheIDList.Add(pss.key);
                    return false;
                }
            }
            return true;
        }
        internal static IEnumerable<CodeInstruction> TryGetPawnFrameSet_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            int step = 0;
            Label jmp = iL.DefineLabel();
            foreach (CodeInstruction code in instructions)
            {
                if (step == 0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(StylishAtlasManager), nameof(StylishAtlasManager.OptimizeList));
                    step++;
                    continue;
                }
                else if (step == 1 && code.opcode == OpCodes.Stloc_2)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(StylishAtlasManager), nameof(StylishAtlasManager.AddAtlasMap));
                    step++;
                    continue;
                }
                else if (code.opcode == OpCodes.Newobj)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(StylishAtlasManager), nameof(StylishAtlasManager.CreateAtlasByPawn));
                    continue;
                }
                yield return code;
            }
        }

        internal static IEnumerable<CodeInstruction> GlobalTextureAtlasManagerUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            Label jmp = iL.DefineLabel();
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Stloc_0)
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishAtlasManager), nameof(StylishAtlasManager.GCCountCheck));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, jmp);
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>() { jmp } };
                    continue;
                }
                yield return code;
            }
        }
    }
    internal class HarmonyPatch_PawnTextureAtlas
    {
        public static IEnumerable<CodeInstruction> Constructor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            Label jmp = iL.DefineLabel();
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(StylishAtlasManager), nameof(StylishAtlasManager.defaultAtlasCancel)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, jmp);
            yield return new CodeInstruction(OpCodes.Ret);
            yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>() { jmp } };
            foreach (CodeInstruction code in instructions)
            {
                yield return code;
            }
        }
    }
}

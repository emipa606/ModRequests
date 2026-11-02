using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Draws the animated tool over a working colonist (Postfix only). The pawn body is
    /// never moved, offset, or re-rendered — this only draws an extra sprite on top — so it
    /// stays compatible with pawn-animation mods that patch the same render path.
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
    public static class Patch_PawnRenderer_RenderPawnAt
    {
        public static void Postfix(Vector3 drawLoc, Pawn ___pawn)
        {
            // Fast path when the dev overlay is off: no Stopwatch, single bool read.
            if (!Diag.Enabled)
            {
                try { ToolAnimator.OnPawnRendered(___pawn, drawLoc); }
                catch { }
                return;
            }
            long ts = System.Diagnostics.Stopwatch.GetTimestamp();
            try { ToolAnimator.OnPawnRendered(___pawn, drawLoc); }
            catch { }
            Diag.AddRenderTicks(System.Diagnostics.Stopwatch.GetTimestamp() - ts);
        }
    }

    /// <summary>Drop the pawn's cached animation state when it leaves the map (keeps the
    /// per-pawn state dictionary bounded and clears any holstered tool).</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
    public static class Patch_Pawn_DeSpawn
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance != null) ToolAnimator.ClearState(__instance.thingIDNumber);
        }
    }

    /// <summary>Effort puff when a colonist hoists a heavy haul stack.</summary>
    [HarmonyPatch(typeof(Pawn_CarryTracker), nameof(Pawn_CarryTracker.TryStartCarry),
        new[] { typeof(Thing), typeof(int), typeof(bool) })]
    public static class Patch_CarryTracker_TryStartCarry
    {
        public static void Postfix(int __result, Thing item, Pawn ___pawn)
        {
            if (!JobEffectsSettings.CompletionEffects) return;
            if (__result <= 0 || item == null || ___pawn == null || !___pawn.Spawned || ___pawn.Map == null) return;
            if (!___pawn.RaceProps.Humanlike) return;
            float mass = item.GetStatValue(StatDefOf.Mass) * __result;
            if (mass < 18f) return;
            try { EffectsUtil.EffortPuff(___pawn); } catch { }
        }
    }

    /// <summary>Quality-colored sparkle pop when a quality item is crafted (bench or build).</summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.SendCraftNotification))]
    public static class Patch_QualityUtility_SendCraftNotification
    {
        public static void Postfix(Thing thing, Pawn worker)
        {
            if (!JobEffectsSettings.CompletionEffects) return;
            if (thing == null || worker == null || !worker.Spawned || worker.Map == null) return;
            try { EffectsUtil.QualityBurst(thing, worker); } catch { }
        }
    }

    /// <summary>Timber/leaf burst when a tree is felled or a crop fully harvested.</summary>
    [HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
    public static class Patch_Plant_PlantCollected
    {
        public struct PlantState
        {
            public bool valid;
            public Map map;
            public IntVec3 pos;
            public bool isTree;
            public bool isStump;
            public bool hasCanopy;
            public Vector3 canopyCenter;
            public float canopyW;
            public float canopyH;
            public bool hasFoliage;
            public Color foliageCol;
        }

        public static void Prefix(Plant __instance, out PlantState __state)
        {
            __state = default;
            if (__instance.Spawned && __instance.Map != null
                && __instance.def?.plant != null && __instance.def.plant.HarvestDestroys)
            {
                __state.valid = true;
                __state.map = __instance.Map;
                __state.pos = __instance.Position;
                __state.isTree = __instance.def.plant.IsTree;
                __state.isStump = __instance.def.plant.isStump;
                // Capture the canopy geometry + the tree's actual sampled leaf colour NOW, while
                // the plant is still spawned — PlantCollected destroys it before our Postfix runs.
                // Stumps shed no leaves, so we don't bother for them.
                if (__state.isTree && !__state.isStump)
                {
                    if (ToolAnimator.TryGetTreeCanopy(__instance, out Vector3 cc, out float cw, out float ch))
                    {
                        __state.hasCanopy = true;
                        __state.canopyCenter = cc;
                        __state.canopyW = cw;
                        __state.canopyH = ch;
                    }
                    if (MaterialColor.TryResolveFoliage(__instance, out Color fc))
                    {
                        __state.hasFoliage = true;
                        __state.foliageCol = fc;
                    }
                }
            }
        }

        public static void Postfix(PlantState __state)
        {
            if (!JobEffectsSettings.CompletionEffects || !__state.valid) return;
            // Wood chips + settling dust (trees & stumps), plus the crop-leaf pop for non-trees.
            try { EffectsUtil.PlantBurst(__state.map, __state.pos, __state.isTree); } catch { }
            // Felled tree (never a stump): shower leaves down from the captured canopy, drifting
            // and colour-matched to the tree's foliage — same look as the per-chop leaves.
            if (__state.isTree && !__state.isStump && __state.hasCanopy)
            {
                try
                {
                    ToolAnimator.SpawnTreeFallLeaves(__state.map, __state.canopyCenter,
                        __state.canopyW, __state.canopyH, 14,
                        __state.hasFoliage ? __state.foliageCol : (Color?)null);
                }
                catch { }
            }
        }
    }

    /// <summary>Lightbulb breakthrough over the researcher when a project completes.</summary>
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResearchPerformed))]
    public static class Patch_ResearchManager_ResearchPerformed
    {
        public struct ResState
        {
            public ResearchProjectDef proj;
            public bool wasFinished;
        }

        public static void Prefix(ResearchProjectDef ___currentProj, out ResState __state)
        {
            __state.proj = ___currentProj;
            __state.wasFinished = ___currentProj != null && ___currentProj.IsFinished;
        }

        public static void Postfix(Pawn researcher, ResState __state)
        {
            if (!JobEffectsSettings.CompletionEffects) return;
            if (__state.proj == null || __state.wasFinished || !__state.proj.IsFinished) return;
            if (researcher == null || !researcher.Spawned || researcher.Map == null) return;
            try { EffectsUtil.ResearchBurst(researcher); } catch { }
        }
    }
}

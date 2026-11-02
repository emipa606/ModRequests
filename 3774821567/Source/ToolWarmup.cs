using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Amortized material warm-up — the fix for the render path's PEAK frame cost.
    ///
    /// The profiler told two very different stories about the render postfix: an average of
    /// 0.044 ms/frame, and a <b>max-for-frame of 17.5 ms</b>. A 400x ratio like that is never pose
    /// math; it is first-touch lazy initialization happening inside the hot path. Every one of
    /// JobToolDef's material properties is a <c>cachedX ?? (cachedX = MaterialPool.MatFrom(...))</c>,
    /// so the very first frame a given tool is drawn pays for creating its Material(s) — plus the
    /// first-ever draw of a shader variant, the split grip meshes, the hand material, and the arm
    /// sleeve material. That cost is paid exactly once per tool, but it is paid AT the worst possible
    /// moment: mid-gameplay, on the frame a colonist picks up a tool nobody has used yet.
    ///
    /// So we pay it up front instead, a couple of defs per frame from GameComponentUpdate. Spreading
    /// it matters: touching all ~N tools in one go would just relocate the same hitch to load. At
    /// <see cref="DefsPerFrame"/> defs/frame the whole set is warm within a second or two of the map
    /// appearing, long before anyone starts a job.
    ///
    /// Every tool is warmed, not just enabled ones — a player can toggle a tool back on from the
    /// settings window at any time, and we don't want that to re-arm the hitch.
    ///
    /// Idempotent and cheap once finished: a single bool read per frame.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ToolWarmup
    {
        // Two defs per frame. Each def touches a handful of material getters; at 60 fps a 100-tool
        // set is fully warm in under a second, with no measurable per-frame cost while it runs.
        private const int DefsPerFrame = 2;

        private static List<JobToolDef> queue;
        private static int cursor;
        private static bool done;
        private static bool sharedDone;

        /// <summary>Dev-overlay status line: "12/68 tools" while running, "done (68 tools)" after.</summary>
        public static string StatusLine =>
            done ? ("done (" + (queue != null ? queue.Count : 0) + " tools)")
                 : (cursor + "/" + (queue != null ? queue.Count : 0) + " tools");

        /// <summary>Re-arm the warm-up (new game / save load). Materials themselves survive — this
        /// only replays the walk, which is a no-op for anything already cached.</summary>
        public static void Reset()
        {
            queue = null;
            cursor = 0;
            done = false;
        }

        /// <summary>Called once per frame from JobEffectsGameComponent.GameComponentUpdate.</summary>
        public static void Tick()
        {
            if (done) return;

            // One-time shared assets: the split grip meshes and the fleck defs. Cheap, but they
            // carry the same "first draw of the session pays for it" problem.
            if (!sharedDone)
            {
                sharedDone = true;
                try { ToolAnimator.WarmShared(); } catch { }
            }

            if (queue == null)
            {
                queue = new List<JobToolDef>(DefDatabase<JobToolDef>.AllDefsListForReading);
                cursor = 0;
                if (queue.Count == 0) { done = true; return; }
            }

            int end = Mathf.Min(cursor + DefsPerFrame, queue.Count);
            for (; cursor < end; cursor++)
            {
                try { queue[cursor].WarmCache(); }
                catch { /* a bad texPath must not stall the walk or spam the log every frame */ }
            }
            if (cursor >= queue.Count) done = true;
        }
    }
}

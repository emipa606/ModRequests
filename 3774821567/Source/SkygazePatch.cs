using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace JobEffects
{
    /// <summary>
    /// Dynamic apply/unapply manager for <see cref="Patch_SkygazeStandUp"/>.
    ///
    /// WHY THIS EXISTS. The spyglass needs a Skygaze pawn to stand instead of lie down, and the only
    /// place to intercept that is <c>PawnUtility.GetPosture</c> — which is a universal chokepoint
    /// called ~1,400 times per frame (renderer, pather, every posture query, every pawn). Profiling
    /// showed the postfix as the single most-called thing this mod owns: 1386 avg calls/frame at
    /// 0.03us each, ~0.035ms/frame, more calls than every other JobEffects patch combined.
    ///
    /// The critical insight: **that cost is dispatch, not body.** The postfix already returns on its
    /// first line for essentially every call. No guard clause can make it cheaper — a Harmony patch
    /// that early-returns is still invoked. Guarding a patch body makes a disabled feature do
    /// nothing; it does not make it stop being called. The only lever is to not have the patch
    /// applied, so this class owns its lifetime instead of letting PatchAll install it forever.
    /// (<see cref="Patch_SkygazeStandUp"/> deliberately carries NO [HarmonyPatch] attribute.)
    ///
    /// Two gates decide whether the patch is live:
    ///  1. SETTINGS — <c>AnimatedTools</c> AND the per-tool toggle for JE_Tool_Spyglass. The old code
    ///     only ever checked the global switch, so turning off just the spyglass left the patch
    ///     running 1386x/frame to serve a tool that could never draw.
    ///  2. DEMAND — whether anyone on any map is actually stargazing right now. Skygaze is rare, so
    ///     for the overwhelming majority of a session the patch is simply absent and GetPosture runs
    ///     completely unpatched.
    ///
    /// All patching happens from <see cref="Tick"/> (i.e. from GameComponentTick), never from inside
    /// another Harmony patch — re-entrant patching of a method while it may be on the stack is the
    /// one genuinely dangerous way to use the runtime API. The StartJob hook only raises a flag.
    /// </summary>
    public static class SkygazePatch
    {
        private static Harmony harmony;
        private static MethodInfo target;
        private static MethodInfo postfixMethod;
        private static bool ready;      // reflection resolved and no unrecoverable failure yet
        private static bool applied;    // the postfix is currently installed on GetPosture

        // Ticks between "is anyone still stargazing?" sweeps. 4s: Skygaze jobs last thousands of
        // ticks, so this is far tighter than it needs to be and still costs nothing.
        private const int SweepInterval = 240;
        // Once applied, stay applied at least this long. Pure hysteresis — stops any pathological
        // start/stop pattern from flapping the patch (each apply/unapply is a re-JIT).
        private const int MinHoldTicks = 300;

        private static int nextSweepTick;
        private static int holdUntilTick;
        private static bool pendingApply;

        internal static readonly LazyDef<JobDef> SkygazeJob = new LazyDef<JobDef>("Skygaze");

        /// <summary>Dev/diagnostic: is the GetPosture postfix currently installed?</summary>
        public static bool Applied => applied;

        private static int CurTick => Find.TickManager != null ? Find.TickManager.TicksGame : 0;

        private static bool Allowed =>
            JobEffectsSettings.AnimatedTools
            && JobEffectsSettings.IsToolEnabled("JE_Tool_Spyglass");

        public static void Init(Harmony h)
        {
            harmony = h;
            try
            {
                target = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPosture));
                postfixMethod = AccessTools.Method(typeof(Patch_SkygazeStandUp),
                                                   nameof(Patch_SkygazeStandUp.Postfix));
                ready = harmony != null && target != null && postfixMethod != null;
                if (!ready)
                    Log.Warning("[Show Me Your Tools] Skygaze posture patch could not resolve its target; "
                              + "stargazing colonists will lie down.");
            }
            catch (Exception e)
            {
                ready = false;
                Log.Warning("[Show Me Your Tools] Skygaze posture patch init failed: " + e.Message);
            }
        }

        /// <summary>
        /// Settings were written. Drop the patch immediately when it's no longer permitted, and force
        /// the next sweep to re-evaluate so re-enabling takes effect on the next tick rather than
        /// after the sweep interval.
        /// </summary>
        public static void Sync()
        {
            if (!ready) return;
            if (!Allowed) { Unapply(); return; }
            nextSweepTick = 0;
        }

        /// <summary>New game / save load: tick counters restart, so drop the stale schedule.</summary>
        public static void ResetTransientState()
        {
            nextSweepTick = 0;
            holdUntilTick = 0;
            pendingApply = false;
        }

        /// <summary>
        /// Called from the Pawn_JobTracker.StartJob postfix. Only raises a flag — the actual patching
        /// is deferred to the next Tick so we never call Harmony from inside a Harmony patch.
        /// </summary>
        public static void NotifyJobStarted(JobDef def)
        {
            if (!ready || applied || def == null) return;
            if (def != SkygazeJob.Value) return;
            pendingApply = true;
        }

        /// <summary>Driven from JobEffectsGameComponent.GameComponentTick.</summary>
        public static void Tick()
        {
            if (!ready) return;
            int now = CurTick;

            // Instant response to a pawn starting Skygaze (one tick after StartJob). The pawn still
            // has to walk to the cell before the driver sets its posture, so this is never late.
            if (pendingApply)
            {
                pendingApply = false;
                if (Allowed)
                {
                    holdUntilTick = now + MinHoldTicks;
                    Apply();
                }
            }

            if (now < nextSweepTick) return;
            nextSweepTick = now + SweepInterval;

            if (!Allowed) { Unapply(); return; }

            // Also catches the cases StartJob can't: a save loaded mid-stargaze, or the player
            // re-enabling the spyglass while someone is already at it.
            if (AnySkygazer())
            {
                holdUntilTick = now + MinHoldTicks;
                Apply();
            }
            else if (applied && now >= holdUntilTick)
            {
                Unapply();
            }
        }

        // Runs at most once every SweepInterval ticks, and only reads CurJobDef — negligible even on
        // a large multi-map colony.
        private static bool AnySkygazer()
        {
            JobDef sg = SkygazeJob.Value;
            if (sg == null) return false;
            List<Map> maps = Find.Maps;
            if (maps == null) return false;
            for (int m = 0; m < maps.Count; m++)
            {
                List<Pawn> pawns = maps[m]?.mapPawns?.AllHumanlikeSpawned;
                if (pawns == null) continue;
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn p = pawns[i];
                    if (p != null && p.CurJobDef == sg) return true;
                }
            }
            return false;
        }

        private static void Apply()
        {
            if (applied || !ready) return;
            try
            {
                harmony.Patch(target, postfix: new HarmonyMethod(postfixMethod));
                applied = true;
            }
            catch (Exception e)
            {
                ready = false;   // don't retry every tick
                Log.Warning("[Show Me Your Tools] Failed to apply Skygaze posture patch: " + e.Message);
            }
        }

        private static void Unapply()
        {
            if (!applied || !ready) return;
            try
            {
                harmony.Unpatch(target, postfixMethod);
                applied = false;
            }
            catch (Exception e)
            {
                ready = false;
                Log.Warning("[Show Me Your Tools] Failed to remove Skygaze posture patch: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Cheap demand signal for <see cref="SkygazePatch"/>. Pawn_JobTracker.StartJob fires on every
    /// job change — a few hundred times a second colony-wide at worst, versus GetPosture's ~83,000
    /// calls/second — so watching job starts to decide when to install the posture patch is ~400x
    /// cheaper than leaving the posture patch permanently installed.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class Patch_JobTracker_StartJob_Skygaze
    {
        public static void Postfix(Job newJob)
        {
            if (newJob == null) return;
            try { SkygazePatch.NotifyJobStarted(newJob.def); } catch { }
        }
    }
}

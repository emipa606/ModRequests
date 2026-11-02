using System.Collections.Generic;
using UnityEngine;

namespace JobEffects
{
    /// <summary>
    /// ONE object per pawn holding EVERY per-pawn store the tool animator owns.
    ///
    /// Why this exists (perf, Stage 7). This state used to live in a dozen separate int-keyed
    /// dictionaries — <c>states</c>, <c>resolvedCache</c>, <c>activeToolMemo</c>, <c>drawBuffers</c>,
    /// <c>lastFoleyTime</c>, <c>beatFireCounts</c>, <c>workHitTime</c>, <c>workInterval</c>,
    /// <c>suppressWorkMoteUntil</c> — so a single animating pawn paid 10-15 separate hash+probe
    /// round trips per frame, and the seven callers of <c>HasActiveTool</c> each re-entered that
    /// chain. Now it's ONE probe (usually zero — see <see cref="PawnStates"/>'s last-pawn memo).
    ///
    /// The animation block was additionally a ~130-byte STRUCT stored by value in
    /// <c>Dictionary&lt;int, AnimState&gt;</c>: every read copied all 130 bytes out, and
    /// <c>states[id] = st</c> copied them back — up to THREE times per pawn per frame (the active
    /// branch, the holster "dirty" branch, and the foley edge). As a class it is mutated in place
    /// and never written back at all.
    ///
    /// Lifetime: created on demand, dropped by <c>ToolAnimator.ClearState</c> on despawn and wiped
    /// wholesale on new-game / save-load (<c>ResetTransientState</c>), so it can never leak across
    /// games where thingIDNumbers get reused.
    /// </summary>
    internal sealed class PawnToolState
    {
        internal readonly int id;

        // ---- swing / pose animation (was ToolAnimator.AnimState) ----
        public float phase;       // 0..1 within the current swing
        public float prevPhase;
        public float glowAccum;   // toward next tip glow
        public float headAccum;   // toward next head emote
        public float pourAccum;   // toward next acid-pour drip/vapor pulse (holdPour flask)
        public JobToolDef lastTool; // most recent active tool, for the idle holster
        public Vector3 lastDir;     // facing of that tool when work stopped
        public float idleTimer;     // real seconds since work stopped
        public float sprayKick;     // 0..1 recoil from the last beat-fire lunge, decays each frame
        public int beatConsumed;    // last beat-fire count we already emitted a puff for
        public float workLeftAtLastStrike; // DoBill workLeft at the previous strike; gates impacts
        public int lastBillStartTick;      // detects bill changes so a new bill resets the gate
        public float jobStartRealTime;     // real time when this tool became active (for frame animations)
        public int maxToilTicks;           // largest ticksLeftThisToil seen since the tool became active -> denominator for job-progress depletion frames
        public float maxWorkLeft;          // largest DoBill workLeft seen since activation -> denominator for holdPour progress
        public float stabJitterAng;        // stabVaried: this cycle's stab-direction offset (deg)
        public float stabJitterLat;        // stabVaried: this cycle's lateral shift (cells)
        public int cycleCount;             // completed swing cycles (drives every-Nth-cycle behaviors)
        public float wipeT;                // browWipe: progress of the active wipe (0 = idle)
        public float wipeAccum;            // browWipe: seconds worked since the last wipe
        public float wipeNext;             // browWipe: randomized seconds until the next wipe
        public float stuckT;               // canStick: progress of the active jam (0 = idle)
        public float stuckAccum;           // canStick: seconds worked since the last jam
        public float stuckNext;            // canStick: randomized seconds until the next jam
        public bool pinPulled;             // Spray: the one-time pin-pull fleck already fired
        public float strikeSoundAccum;     // worked seconds toward the next interval sound pulse
        public bool shownLastFrame;        // a tool (active OR holster) was drawn last frame — drives the foley edges
        public bool carriedOnly;           // the held tool is out ONLY because the pawn is walking TO a job (never worked it yet) — dropped instantly when they stop heading there, vs the after-work hip linger
        public float lastComputeTime;      // animTime at this pawn's last throttled pose recompute (0 = never); drives the accumulated swing delta on compute frames

        // ---- unified tool-resolution cache (was ToolAnimator.resolvedCache) ----
        // resValid is explicit rather than leaning on "epoch 0 can't match": resolveEpoch AND
        // Job.loadID both legitimately start at 0, so a freshly-allocated state would otherwise
        // report a bogus cache hit for tool=null on the pawn's very first resolve.
        public bool resValid;
        public int resEpoch;
        public int resJobLoadID;
        public int resTargetAHash;
        public int resTargetBHash;
        public int resRefreshTick;
        public JobToolDef resTool;   // effective post-tech-gate tool; null = nothing to draw for this job

        // ---- active-tool ANSWER memo (was ToolAnimator.activeToolMemo) ----
        // Stamped with BOTH Time.frameCount and TicksGame — see ActiveDrawnTool for why either
        // alone would be wrong. -1 sentinels so frame/tick 0 can never produce a false hit.
        public int memoFrame = -1;
        public int memoTick = -1;
        public JobToolDef memoTool;

        // ---- retained draw list for the pose throttle (was ToolAnimator.drawBuffers) ----
        // Allocated lazily: only pawns that actually reach a throttled compute frame ever get one.
        public List<ToolAnimator.DrawEntry> drawBuf;

        // ---- tool foley throttle (was ToolAnimator.lastFoleyTime) ----
        // Far-past sentinel so the first pickup always plays (dictionary-absent used to mean this).
        public float lastFoleyTime = -999f;

        // ---- beat-fire lunge counter (was ToolAnimator.beatFireCounts) ----
        public int beatFireCount;

        // ---- work-sound hit sync (was ToolAnimator.workHitTime / workInterval) ----
        public bool hasWorkHit;
        public float workHitTime;
        public bool hasWorkInterval;
        public float workInterval;

        // ---- vanilla work-effecter mote suppression (was ToolAnimator.suppressWorkMoteUntil) ----
        // Game tick the flag expires. -1 = never armed (TicksGame is never negative).
        public int suppressWorkMoteUntil = -1;

        public PawnToolState(int id)
        {
            this.id = id;
            ResetAnim();
        }

        /// <summary>
        /// Restart the swing cleanly (dev gallery assignment). Resets the animation block only —
        /// the resolution cache, memo and foley throttle are all self-invalidating.
        /// </summary>
        public void ResetAnim()
        {
            // Seed the starting phase off the pawn id so a work crew on the same vein/wall doesn't
            // swing in robotic lockstep.
            phase = Mathf.Repeat(id * 0.61803399f, 1f);
            prevPhase = 0f;
            glowAccum = 0f; headAccum = 0f; pourAccum = 0f;
            lastTool = null; lastDir = default; idleTimer = 0f;
            sprayKick = 0f; beatConsumed = 0;
            workLeftAtLastStrike = 0f; lastBillStartTick = 0;
            jobStartRealTime = 0f; maxToilTicks = 0; maxWorkLeft = 0f;
            stabJitterAng = 0f; stabJitterLat = 0f; cycleCount = 0;
            wipeT = 0f; wipeAccum = 0f; wipeNext = 0f;
            stuckT = 0f; stuckAccum = 0f; stuckNext = 0f;
            pinPulled = false; strikeSoundAccum = 0f;
            shownLastFrame = false; carriedOnly = false; lastComputeTime = 0f;
            drawBuf?.Clear();
        }

        /// <summary>Retained draw list for the ~30 Hz pose throttle, allocated on first use.</summary>
        public List<ToolAnimator.DrawEntry> DrawBuffer =>
            drawBuf ?? (drawBuf = new List<ToolAnimator.DrawEntry>(16));
    }

    /// <summary>
    /// The per-pawn state store. One dictionary, fronted by a single-entry "last pawn" memo.
    ///
    /// The memo is what makes the seven <c>HasActiveTool</c> callers cheap: within one frame they
    /// all ask about the SAME pawn back to back (render postfix → DrawEquipment prefix → SMYH hands
    /// → Yayo body patches → …), so after the first probe every follow-up is a single int compare.
    /// A null <c>last</c> is a VALID memo result (it means "no entry for that id"), which is why
    /// <see cref="Forget"/> and <see cref="Clear"/> must reset <c>lastId</c> rather than just the
    /// reference. Main-thread only.
    /// </summary>
    internal static class PawnStates
    {
        private static readonly Dictionary<int, PawnToolState> map = new Dictionary<int, PawnToolState>();
        private static int lastId = -1;
        private static PawnToolState last;

        /// <summary>Existing state, or null. Never allocates — use this on any path that must stay
        /// free for the ~90% of pawns that have no tool (the render gate, mote suppression).</summary>
        public static PawnToolState Peek(int id)
        {
            if (id == lastId) return last;
            map.TryGetValue(id, out PawnToolState s);
            lastId = id;
            last = s;
            return s;
        }

        /// <summary>Existing state, allocating one if this pawn has never had any.</summary>
        public static PawnToolState GetOrCreate(int id)
        {
            if (id == lastId && last != null) return last;
            if (!map.TryGetValue(id, out PawnToolState s))
            {
                s = new PawnToolState(id);
                map[id] = s;
            }
            lastId = id;
            last = s;
            return s;
        }

        public static void Forget(int id)
        {
            map.Remove(id);
            lastId = -1;
            last = null;
        }

        public static void Clear()
        {
            map.Clear();
            lastId = -1;
            last = null;
        }
    }
}

using System.Diagnostics;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Dev-only, per-frame diagnostics for the tool render path (Stage 4 of the perf pass).
    ///
    /// Design contract: <b>zero cost when the overlay is off</b>. Every collector method early-outs
    /// on <see cref="Enabled"/> before touching a field, and <see cref="Enabled"/> is only true when
    /// the player turns on the dev overlay setting AND RimWorld is in dev mode. The hot render/patch
    /// code guards its own timing calls with <c>if (Diag.Enabled)</c> too, so when the toggle is off
    /// the instrumentation compiles down to a single static bool read per pawn.
    ///
    /// Frame model: work accumulates into "current frame" buckets and is PUBLISHED into the
    /// <c>Last*</c> fields on the first collector call of a new <see cref="Time.frameCount"/>. The
    /// overlay only ever reads the published snapshot, so it always shows the last fully-completed
    /// frame — never a mid-frame partial — regardless of whether RenderPawnAt runs before or after
    /// OnGUI within a Unity frame.
    /// </summary>
    public static class Diag
    {
        public static bool Enabled =>
            JobEffectsSettings.DiagnosticsOverlay && Prefs.DevMode;

        // Stopwatch ticks -> milliseconds. Cached once (Stopwatch.Frequency is constant per process).
        private static readonly double TicksToMs = 1000.0 / Stopwatch.Frequency;

        // ---- current-frame accumulators ----
        private static FrameGate gate;           // Stage 2 primitive: "first call this frame?"
        private static long renderTicksAccum;    // summed OnPawnRendered wall time across all pawns this frame
        private static int consideredAccum;      // humanlike pawns the render postfix was invoked for
        private static int passedGateAccum;      // ...of those, how many survived the cheap JobCouldHaveTool reject
        private static int activeDrawnAccum;     // pawns that drew an active (working) tool
        private static int holsterDrawnAccum;    // pawns that drew a holstered/belt tool
        private static int workerComputedAccum;  // (Stage 3) poses computed off the main thread this frame

        // ---- published snapshot of the last completed frame ----
        public static double LastRenderMs { get; private set; }
        public static int LastConsidered { get; private set; }
        public static int LastPassedGate { get; private set; }
        public static int LastActiveDrawn { get; private set; }
        public static int LastHolsterDrawn { get; private set; }
        public static int LastWorkerComputed { get; private set; }

        // Worst single frame seen since the last reset. This is the number that matters for FELT
        // smoothness: the average can sit at 0.04 ms while one frame spikes to 17 ms as a tool's
        // materials get created inside the render path the first time it's ever drawn. Reset when the
        // overlay checkbox is toggled and on every new game / save load.
        public static double PeakRenderMs { get; private set; }
        public static int PeakFrameIndex { get; private set; }

        // ---- single-value-per-frame frame state (latest wins; may be one frame stale, fine for a HUD) ----
        public static bool Capped;
        public static bool Throttle;
        public static int AnimatorCap;
        public static string Lod = "Full";
        public static bool WorkerActive;   // (Stage 3) worker pre-pass engaged this session

        /// <summary>
        /// Publish the just-ended frame's accumulators into the <c>Last*</c> snapshot and reset the
        /// buckets. Idempotent within a frame (guarded on <see cref="Time.frameCount"/>), so any
        /// number of collector calls — plus the overlay's own call — roll the frame exactly once.
        /// </summary>
        public static void MarkFrame()
        {
            if (!gate.Advance()) return;
            LastRenderMs = renderTicksAccum * TicksToMs;
            LastConsidered = consideredAccum;
            LastPassedGate = passedGateAccum;
            LastActiveDrawn = activeDrawnAccum;
            LastHolsterDrawn = holsterDrawnAccum;
            LastWorkerComputed = workerComputedAccum;
            if (LastRenderMs > PeakRenderMs)
            {
                PeakRenderMs = LastRenderMs;
                PeakFrameIndex = Time.frameCount;
            }
            renderTicksAccum = 0;
            consideredAccum = 0;
            passedGateAccum = 0;
            activeDrawnAccum = 0;
            holsterDrawnAccum = 0;
            workerComputedAccum = 0;
        }

        public static void ResetPeak() { PeakRenderMs = 0.0; PeakFrameIndex = 0; }

        public static void AddRenderTicks(long ticks) { if (!Enabled) return; MarkFrame(); renderTicksAccum += ticks; }
        public static void NoteConsidered() { if (!Enabled) return; MarkFrame(); consideredAccum++; }
        public static void NotePassedGate() { if (!Enabled) return; MarkFrame(); passedGateAccum++; }
        public static void NoteActiveDrawn() { if (!Enabled) return; MarkFrame(); activeDrawnAccum++; }
        public static void NoteHolsterDrawn() { if (!Enabled) return; MarkFrame(); holsterDrawnAccum++; }
        public static void NoteWorkerComputed(int n) { if (!Enabled) return; MarkFrame(); workerComputedAccum += n; }

        public static void NoteFrameStats(bool capped, bool throttle, int cap)
        {
            if (!Enabled) return;
            Capped = capped;
            Throttle = throttle;
            AnimatorCap = cap;
        }

        public static void NoteLod(string lod) { if (Enabled) Lod = lod; }
    }

    /// <summary>Draws the perf HUD each OnGUI pass while the dev overlay is enabled.</summary>
    public static class DiagOverlay
    {
        private static readonly StringBuilder sb = new StringBuilder(256);

        public static void Draw()
        {
            Diag.MarkFrame();   // roll the frame even when no pawn rendered (paused, off-map, etc.)

            Map map = Find.CurrentMap;
            int pop = map != null ? map.mapPawns.AllHumanlikeSpawned.Count : 0;

            sb.Length = 0;
            sb.Append("Show Me Your Tools — perf");
            sb.Append("\nrender: ").Append(Diag.LastRenderMs.ToString("0.000")).Append(" ms/frame")
              .Append("   peak ").Append(Diag.PeakRenderMs.ToString("0.00")).Append(" ms");
            sb.Append("\npop ").Append(pop)
              .Append("  considered ").Append(Diag.LastConsidered)
              .Append("  passed ").Append(Diag.LastPassedGate)
              .Append("  animating ").Append(Diag.LastActiveDrawn)
              .Append("  holster ").Append(Diag.LastHolsterDrawn);
            sb.Append("\nwarm: ").Append(ToolWarmup.StatusLine);
            sb.Append("\ncap ").Append(Diag.AnimatorCap)
              .Append("  capped: ").Append(Diag.Capped ? "yes" : "no")
              .Append("  throttle: ").Append(Diag.Throttle ? "on" : "off");
            sb.Append("\nLOD: ").Append(Diag.Lod);
            if (Diag.WorkerActive)
                sb.Append("\nworker pose: ").Append(Diag.LastWorkerComputed).Append(" computed/frame");
            string text = sb.ToString();

            GameFont prevFont = Text.Font;
            Color prevColor = GUI.color;
            Text.Font = GameFont.Tiny;

            const float w = 300f;
            float h = Text.CalcHeight(text, w);
            Rect box = new Rect(8f, 92f, w + 16f, h + 12f);
            Widgets.DrawBoxSolid(box, new Color(0f, 0f, 0f, 0.68f));
            GUI.color = new Color(0.6f, 0.95f, 0.6f);
            Widgets.Label(new Rect(box.x + 8f, box.y + 6f, w, h), text);

            GUI.color = prevColor;
            Text.Font = prevFont;
        }
    }

    [HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
    public static class Patch_DiagOverlay_OnGUI
    {
        public static void Postfix()
        {
            if (!Diag.Enabled) return;
            try { DiagOverlay.Draw(); } catch { }
        }
    }
}

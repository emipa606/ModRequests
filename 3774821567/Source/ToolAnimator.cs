using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace JobEffects
{
    /// <summary>
    /// Per-pawn, per-frame animation: swinging/stirring tools and head emotes (sweat/steam
    /// over the worker). Driven from a Harmony Postfix on PawnRenderer.RenderPawnAt that only
    /// draws extra sprites — the pawn body itself is never moved or re-rendered.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ToolAnimator
    {
        private static Dictionary<int, List<JobToolDef>> jobLookup;   // key = JobDef.index — int, so no string hashing on the hot path
        private static bool[] jobCovered;                             // indexed by JobDef.index; true = at least one JobToolDef maps to it

        // Def-reference compares instead of defName string compares on the per-frame Yayo / fishing paths.
        private static readonly LazyDef<JobToolDef> welderTool = new LazyDef<JobToolDef>("JE_Tool_Welder");
        private static readonly LazyDef<JobToolDef> benchWelderTool = new LazyDef<JobToolDef>("JE_Tool_BenchWelder");
        private static readonly LazyDef<JobToolDef> fishingRodTool = new LazyDef<JobToolDef>("JE_Tool_FishingRod");

        // ---- Unified resolution cache (perf) ----
        // The render postfix, HasActiveTool/HasActiveWelder, WantsBenchSoundSync and the holster path
        // all funnel through ResolveEffectiveTool, which memoizes the expensive candidate match + tech
        // gate per pawn. Previously each flow re-ran the full match every frame, so one working pawn
        // paid for it 3-5x per frame (render + DrawEquipment prefix + both Yayo body patches). The memo
        // is invalidated when the pawn's job instance (loadID) or work target changes, when the global
        // epoch bumps (settings written / a tool toggled), or after a short staggered safety window
        // (catches Electricity completing mid-job). The cheap gates (MovingNow / stun) are NOT baked in
        // — each caller applies them fresh after the cache so a walking pawn still suppresses correctly.
        // Storage lives on PawnToolState (res* fields), not in its own dictionary — see that class.
        private static int resolveEpoch;
        private const int ResolveRefreshInterval = 30;   // ticks; staggered per pawn so re-resolves spread across frames

        // ---- Active-tool ANSWER memo (perf) ----
        // HasActiveTool / HasActiveWelder / WantsBenchSoundSync run the identical gate chain and end at
        // the same effective tool, and SEVEN callers ask it per pawn: Melee Animation's ShouldBeActive
        // hook (per TICK, for every humanlike on every map), the DrawEquipment prefix, SMYH hands, the
        // beat-fire lunge prefix, carried-medicine, and both Yayo body patches. The chain was therefore
        // re-running many times for one pawn inside a single frame.
        //
        // The memo is stamped with BOTH Time.frameCount and TicksGame: a new tick invalidates it (job /
        // stance / pather changed), and a new frame invalidates it too (the camera may have panned,
        // which moves the animator-cap set). So it is never staler than the cheapest correct recompute,
        // while collapsing every same-frame, same-tick caller down to one evaluation. Only pawns that
        // pass the job-coverage gate ever get an entry, so the store stays tiny.
        // Storage lives on PawnToolState (memo* fields) — see that class.

        // Force every pawn to re-resolve on next access (settings written, a tool enabled/disabled,
        // the tech-gate toggled). Cheap — just invalidates the memo keys.
        public static void BumpResolveEpoch() { resolveEpoch++; }

        // Called by JobEffectsGameComponent on every new-game / save-load init. Static fields live for
        // the whole RimWorld process, so without this a previous loaded map can leave per-pawn state,
        // retained draw buffers, suppression flags, or SMYH/arm bindings keyed by thingIDNumber. Those
        // IDs can be reused by the next loaded game and make tools appear to stop drawing after the
        // first session. Def/material caches are intentionally kept; only live per-game/per-pawn state
        // is cleared.
        public static void ResetTransientState()
        {
            unchecked { resolveEpoch++; }
            PawnStates.Clear();   // one store now: anim, resolve cache, memo, draw buffer, foley, beat-fire, work-hit, mote suppression
            galleryTools.Clear();
            GalleryEffects = true;
            GalleryFacing = Rot4.South;
            GalleryHolster = false;
            animatorSet.Clear();
            animatorScratch.Clear();
            animatorSetFrame = -1;
            cappedActive = false;
            throttleEnabled = false;
            captureSink = null;
            lodFrame = -1;
            lodLevel = LodLevel.Full;
            lodSimplified = false;
            currentRenderQueue = 0;
            SmyhHands.ResetTransientState();
            ArmRenderer.ResetTransientState();
        }

        // The expensive resolution, memoized. Returns the effective (post-tech-gate) tool for the
        // pawn's CURRENT job + work target, or null. IGNORES the MovingNow/stun gate — callers apply it.
        // The job-coverage lookup happens BEFORE any state is allocated, so a pawn doing an uncovered
        // job never gets a PawnToolState from this path.
        private static JobToolDef ResolveEffectiveTool(Pawn pawn) => ResolveEffectiveTool(pawn, null);

        // st may be null (fetched on demand) or pre-fetched by a caller that already has it — the
        // render path does, which saves the probe entirely.
        private static JobToolDef ResolveEffectiveTool(Pawn pawn, PawnToolState st)
        {
            Job job = pawn.CurJob;
            if (job == null || job.def == null) return null;
            if (!Lookup.TryGetValue(job.def.index, out List<JobToolDef> candidates)) return null;
            LocalTargetInfo target = job.targetA;
            if (!target.IsValid) return null;

            int id = pawn.thingIDNumber;
            int taHash = target.Thing != null ? target.Thing.thingIDNumber : target.Cell.GetHashCode();
            LocalTargetInfo tb = job.targetB;
            int tbHash = tb.Thing != null ? tb.Thing.thingIDNumber : (tb.IsValid ? tb.Cell.GetHashCode() : 0);
            int nowTick = Find.TickManager != null ? Find.TickManager.TicksGame : 0;

            if (st == null) st = PawnStates.GetOrCreate(id);
            if (st.resValid
                && st.resEpoch == resolveEpoch
                && st.resJobLoadID == job.loadID
                && st.resTargetAHash == taHash
                && st.resTargetBHash == tbHash
                && nowTick < st.resRefreshTick)
            {
                return st.resTool;
            }

            JobToolDef tool = ApplyTechGate(ResolveCandidate(pawn, job, candidates, target));
            st.resValid = true;
            st.resEpoch = resolveEpoch;
            st.resJobLoadID = job.loadID;
            st.resTargetAHash = taHash;
            st.resTargetBHash = tbHash;
            st.resRefreshTick = nowTick + ResolveRefreshInterval + (id % 13);   // stagger so all pawns don't refresh on the same tick
            st.resTool = tool;
            return tool;
        }

        // ---- Zoom level-of-detail (perf) ----
        // At Far/Furthest zoom the tool/hand/arm sprites are only a few pixels — skip the whole
        // resolve + draw. At Middle zoom keep the tool + hands but drop the translucent forearms
        // (sub-pixel, pure draw-call cost there). Camera zoom is global, so this is computed once per
        // frame and frame-guarded like the anim clock. Gated by the player setting (default on).
        private enum LodLevel { Full, Simplified, Culled }
        private static LodLevel lodLevel = LodLevel.Full;
        private static int lodFrame = -1;
        private static bool lodSimplified;   // set per-pawn from the frame LOD; read by the forearm draws
        private static LodLevel CurrentLod()
        {
            int f = Time.frameCount;
            if (f != lodFrame)
            {
                lodFrame = f;
                lodLevel = LodLevel.Full;
                if (JobEffectsSettings.ZoomLod)
                {
                    CameraDriver cd = Find.CameraDriver;
                    if (cd != null)
                    {
                        CameraZoomRange z = cd.CurrentZoom;
                        if (z >= CameraZoomRange.Far) lodLevel = LodLevel.Culled;
                        else if (z == CameraZoomRange.Middle) lodLevel = LodLevel.Simplified;
                    }
                }
                if (Diag.Enabled) Diag.NoteLod(lodLevel.ToString());
            }
            return lodLevel;
        }

        // Beat-fire sync: vanilla Verb_BeatFire fires one cast (one forward lunge) per beat. A
        // Harmony postfix bumps this per-pawn counter on each successful beat; the Spray tool emits
        // exactly one foam puff per increment, so the discharge lands on the pawn's real lunge
        // instead of our free-running clock. Keyed by pawn.thingIDNumber.
        public static void NotifyBeatFire(Pawn p)
        {
            if (p == null) return;
            PawnStates.GetOrCreate(p.thingIDNumber).beatFireCount++;
        }
        private static int BeatFireBeats(PawnToolState st) => st.beatFireCount;

        // Work-sound hit sync: some continuous work jobs play their work sound as a sustainer that
        // re-fires its PRIMARY grain back-to-back (one grain == one audible "hit"/scrape) — the meat
        // chop while butchering, the chisel "chink" while stonecutting at a bench, etc. A Harmony
        // hook on the grain start calls NotifyWorkHit per pawn; we record the wall-clock time of
        // each hit and an EMA of the interval between them, so the tool can predict the next hit and
        // land its strike on it. Keyed by pawn.thingIDNumber. Wall-clock based => correct at any
        // game speed. Shared store: a pawn is only ever doing one of these jobs at a time.
        private const float WorkHitDefaultInterval = 0.55f;
        public static void NotifyWorkHit(Pawn p)
        {
            if (p == null) return;
            PawnToolState st = PawnStates.GetOrCreate(p.thingIDNumber);
            float now = Time.realtimeSinceStartup;
            if (st.hasWorkHit)
            {
                float dt = now - st.workHitTime;
                if (dt > 0.12f && dt < 2.5f)   // ignore double-triggers / long gaps (bench loops can run ~2s)
                {
                    st.workInterval = st.hasWorkInterval
                        ? Mathf.Lerp(st.workInterval, dt, 0.5f)   // smooth toward the latest interval
                        : dt;
                    st.hasWorkInterval = true;
                }
            }
            st.workHitTime = now;
            st.hasWorkHit = true;
        }

        // Phase whose STRIKE CROSSING coincides with the next predicted work-sound hit. Returns
        // false (caller free-runs) until a hit has been seen, and once the hits go stale (work
        // stopped) so the tool relaxes back to its free-run clock. Uses the tool's OWN strike phase
        // (StrikePoint for a Chop cleaver, stabStrikePhase for the chisel's planted-stab mallet rap)
        // so it's the visible impact — not an arbitrary phase point — that lands on the sound.
        private static bool TryGetWorkHitPhase(PawnToolState st, JobToolDef tool, out float phase)
        {
            phase = 0f;
            if (!st.hasWorkHit) return false;
            float t = st.workHitTime;
            float now = Time.realtimeSinceStartup;
            if (now - t > 2.8f) return false;   // no recent hit -> work stopped
            float d = st.hasWorkInterval ? st.workInterval : WorkHitDefaultInterval;
            if (d < 0.05f) return false;
            float sp = StrikePhaseFor(tool);
            // Strike lands on the predicted next hit (t + d); between hits the tool winds back up.
            phase = Mathf.Repeat(sp + (now - (t + d)) / d, 1f);
            return true;
        }

        // Real seconds the spray recoil-kick takes to decay back to the upright rest pose.
        private const float SprayKickTime = 0.45f;

        // Private countdown on JobDriver_Mine: ticks until the next pick-hit (sound + damage).
        // Read via reflection so tools flagged syncToMineHit can land their strike on the hit.
        private static readonly System.Reflection.FieldInfo MineTicksField =
            typeof(JobDriver_Mine).GetField("ticksToPickHit",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Maps the pawn's current mining progress to a swing phase whose StrikePoint coincides with
        // the next vanilla pick-hit. Returns false (caller free-runs) until the countdown is primed.
        private static bool TryGetMineHitPhase(Pawn pawn, out float phase)
        {
            phase = 0f;
            if (MineTicksField == null) return false;
            if (!(pawn.jobs?.curDriver is JobDriver_Mine drv)) return false;
            int ticks;
            try { ticks = (int)MineTicksField.GetValue(drv); }
            catch { return false; }
            if (ticks < -100) return false;   // -1000 sentinel before the first ResetTicksToPickHit
            float spd = pawn.GetStatValue(StatDefOf.MiningSpeed);
            if (spd < 0.6f && pawn.Faction != Faction.OfPlayer) spd = 0.6f;
            if (spd < 0.0001f) return false;
            int total = (int)System.Math.Round(100f / spd);
            if (total <= 0) return false;
            float f = Mathf.Clamp01((float)ticks / total);   // 1 right after a hit -> 0 at the next hit
            phase = Mathf.Repeat(StrikePoint + (1f - f), 1f);
            return true;
        }

        // ---- Dev tool gallery ----
        // When set, the model pawn (galleryPawnId) renders galleryTool every frame, bypassing the
        // job system so each swing style can be inspected/captured in isolation. galleryEffects is
        // a non-persistent live A/B of the debris layer (does NOT touch saved settings).
        // Gallery assignments: each gallery/lineup pawn (thingIDNumber) -> the tool it is forced to
        // render every frame, bypassing the job system. The single-pawn dialog keeps exactly ONE
        // entry; the "tool lineup" debug fills it with one entry per spawned pawn so an entire row
        // of tools renders at once.
        private static readonly Dictionary<int, JobToolDef> galleryTools = new Dictionary<int, JobToolDef>();
        public static bool GalleryEffects = true;
        // When true, gallery/lineup pawns render their forced tool in the IDLE HOLSTER pose (tucked
        // on the belt) instead of the active work swing — so the body-type-aware belt fit can be
        // inspected directly. Live A/B; touches no saved state.
        public static bool GalleryHolster = false;
        // Dev gallery: which way the model faces. Drives BOTH the model's body Rotation (set by the
        // gallery dialog each frame) and the tool's "work" direction below, so a tester can inspect
        // every swing at N/E/S/W. Defaults to South (front-facing, toward the camera).
        public static Rot4 GalleryFacing = Rot4.South;

        // Both are called for EVERY rendered humanlike, every frame, and galleryTools is empty in
        // 100% of real play (it's a dev-only debug action) — so the Count check short-circuits the
        // hash+probe entirely rather than paying it to be told "no" tens of times a frame.
        public static bool IsGalleryPawn(int id) => galleryTools.Count != 0 && galleryTools.ContainsKey(id);
        private static JobToolDef GalleryToolFor(int id) =>
            galleryTools.Count != 0 && galleryTools.TryGetValue(id, out var t) ? t : null;

        // Single-pawn dialog: force just this one pawn to the chosen tool, dropping any previous
        // gallery/lineup assignment first so the two never overlap.
        public static void SetGallery(Pawn pawn, JobToolDef tool)
        {
            int newId = pawn?.thingIDNumber ?? -1;
            foreach (int oldId in galleryTools.Keys) PawnStates.Peek(oldId)?.ResetAnim();
            galleryTools.Clear();
            if (newId != -1) { galleryTools[newId] = tool; PawnStates.Peek(newId)?.ResetAnim(); }   // restart the swing phase clean
        }

        // Tool-lineup debug: add one forced-tool pawn WITHOUT clearing the others.
        public static void AddGalleryPawn(Pawn pawn, JobToolDef tool)
        {
            if (pawn == null) return;
            galleryTools[pawn.thingIDNumber] = tool;
            PawnStates.Peek(pawn.thingIDNumber)?.ResetAnim();
        }

        public static void ClearGallery()
        {
            foreach (int oldId in galleryTools.Keys) PawnStates.Peek(oldId)?.ResetAnim();
            galleryTools.Clear();
            GalleryEffects = true;
            GalleryFacing = Rot4.South;
            GalleryHolster = false;
        }

        private const float StrikePoint = 0.66f;
        private const float HolsterHold = 2f;    // real seconds the idle tool sits at full opacity
        private const float HolsterFade = 0.5f;  // then fades out over this many real seconds
        private const float HolsterLife = HolsterHold + HolsterFade;   // total before it's gone

        // Near-white motes used by seasonal / material tinting so the multiply reads true.
        private static FleckDef chipPaleFleck, leafPaleFleck, woodChipPaleFleck;
        // Canopy-fall variants: long-lived, gently-damped leaves that drift down from a tree's crown.
        private static FleckDef leafFallFleck, leafFallPaleFleck;
        // Glowing ember flung outward when the fire-extinguisher foam clump lands on the tile.
        private static FleckDef extinguisherEmberFleck;
        // Sweat / salt / pin bits + steam for the "human moment" animations.
        private static FleckDef sweatFleck, saltFleck, pinFleck, steamFleck;
        private static bool paleResolved;
        private static void ResolvePale()
        {
            if (paleResolved) return;
            paleResolved = true;
            chipPaleFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_ChipPale");
            woodChipPaleFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_WoodChipPale");
            leafPaleFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_LeafPale");
            leafFallFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_LeafFall");
            leafFallPaleFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_LeafFallPale");
            extinguisherEmberFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_ExtinguisherEmber");
            sweatFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_SweatDrop");
            saltFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_SaltGrain");
            pinFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_PinBit");
            steamFleck = DefDatabase<FleckDef>.GetNamedSilentFail("JE_Steam");
        }

        /// <summary>
        /// One-time warm of the assets shared by every tool, off the render path. See
        /// <see cref="ToolWarmup"/> for why first-touch lazy init is a peak-frame problem.
        /// </summary>
        public static void WarmShared()
        {
            ResolvePale();          // the shared pale/leaf/ember/sweat FleckDef lookups
            EnsureGripMeshes();     // the split-grip hand half meshes (two Mesh allocations + uploads)
            if (jobLookup == null) BuildLookup();   // jobDef -> tool table + coverage array, before the first gate read
            ArmRenderer.WarmCache();
        }

        private static Dictionary<int, List<JobToolDef>> Lookup
        {
            get
            {
                if (jobLookup == null) BuildLookup();
                return jobLookup;
            }
        }

        // Built once, on first use (all defs are loaded and indexed by then). The authored XML still
        // names job defs as strings; they are resolved to JobDef ONCE here so the per-call lookup is an
        // int hash instead of a string hash. A jobDef naming a job from a mod that isn't loaded simply
        // resolves to null and is skipped — exactly what the old string keying did implicitly.
        private static void BuildLookup()
        {
            jobLookup = new Dictionary<int, List<JobToolDef>>();
            // Sized to the whole JobDef database so the coverage test is a plain array index.
            jobCovered = new bool[DefDatabase<JobDef>.DefCount];
            foreach (JobToolDef def in DefDatabase<JobToolDef>.AllDefs)
            {
                if (def.jobDefs == null) continue;
                for (int i = 0; i < def.jobDefs.Count; i++)
                {
                    JobDef jd = DefDatabase<JobDef>.GetNamedSilentFail(def.jobDefs[i]);
                    if (jd == null) continue;
                    int key = jd.index;
                    if (!jobLookup.TryGetValue(key, out List<JobToolDef> list))
                    {
                        list = new List<JobToolDef>();
                        jobLookup[key] = list;
                    }
                    list.Add(def);
                    if ((uint)key < (uint)jobCovered.Length) jobCovered[key] = true;
                }
            }
        }

        // ---- Cheapest possible discriminator (perf) ----
        // "Could this pawn's CURRENT job ever produce a drawn tool?" — two null checks and one array
        // read, no hashing, no allocation, no engine calls. The overwhelming majority of pawns at any
        // instant are doing an uncovered job (Wait / Goto / LayDown / Ingest / SocialRelax / …), so this
        // is what keeps the per-TICK Melee Animation hook and the per-frame render/equipment patches off
        // the expensive machinery entirely. It MUST stay ahead of ActivePawnPassesCap, because that call
        // triggers EnsureAnimatorSet's whole-map scan on the first caller of each frame.
        internal static bool JobCouldHaveTool(Pawn pawn)
        {
            Job job = pawn.CurJob;
            if (job == null) return false;
            JobDef d = job.def;
            if (d == null) return false;
            bool[] cov = jobCovered;
            if (cov == null) { BuildLookup(); cov = jobCovered; }
            // Out of range means a JobDef was registered after we built the table (some mods add
            // generated defs at runtime). Rebuild once against the current DefCount, then answer.
            if (d.index >= cov.Length)
            {
                BuildLookup();
                cov = jobCovered;
                if (d.index >= cov.Length) return false;
            }
            return cov[d.index];
        }

        // ---- Pause-aware, game-speed-independent animation clock ----
        // Every tool's motion — swing phase, idle Hold wobbles, holster linger/fade, frame
        // animations — is driven off this clock so it (a) always advances at a constant wall-clock
        // 1x no matter the game speed (RealTime.deltaTime is never scaled by TickRateMultiplier),
        // and (b) FREEZES the instant the game is paused (time controls OR a force-pausing dialog),
        // holding every tool mid-pose instead of drifting. animDelta is this frame's advance (0 when
        // paused); animTime is the accumulated clock. Advanced once per frame, frameCount-guarded.
        private static float animTime;
        private static float animDelta;
        private static int animClockFrame = -1;
        private static void TickAnimClock()
        {
            int f = Time.frameCount;
            if (f == animClockFrame) return;
            animClockFrame = f;
            bool paused = Find.TickManager != null && Find.TickManager.Paused;
            animDelta = paused ? 0f : RealTime.deltaTime;
            animTime += animDelta;
        }

        // ---- Max-animators cap (perf slider) ----
        // Per frame, only the N work-animating pawns NEAREST the camera get the full animated tool
        // (sprite + hands + forearms + props + effects); everyone past the cap falls back to vanilla.
        // N = JobEffectsSettings.MaxAnimatedPawns. The candidate set (active-tool, non-moving, on-screen
        // humanlikes) is gathered once per frame, distance-sorted only when it actually exceeds the cap,
        // and short-circuited entirely when the whole map has <= N humanlikes (the common case) so no
        // per-frame cost is added unless a colony is genuinely large. Walking/holster pawns draw a
        // single cheap belt sprite and are NOT subject to the cap.
        private static readonly HashSet<int> animatorSet = new HashSet<int>();
        private static readonly List<Pawn> animatorScratch = new List<Pawn>();
        private static int animatorSetFrame = -1;
        private static bool cappedActive;          // true this frame when active animators in view exceed the cap
        private static IntVec3 animSortCenter;
        private static readonly System.Comparison<Pawn> animByDist = (a, b) =>
            (a.Position - animSortCenter).LengthHorizontalSquared.CompareTo(
                (b.Position - animSortCenter).LengthHorizontalSquared);

        // ---- Retained draw-list + ~30 Hz pose-recompute throttle (perf) ----
        // The full per-pawn pose compute (swing math, matrix build, forearm mesh upload, fleck
        // spawns) ran every render frame; on busy colonies that's the dominant main-thread cost.
        // Instead, on "compute" frames the whole tool+hands+forearms+props assembly is computed and
        // CAPTURED into a per-pawn list of draw entries; on the in-between "skip" frames we just
        // REPLAY that list. The whole assembly freezes in lockstep for one frame, so there is no
        // hand/forearm disconnect — only a 30 Hz pose update, imperceptible on a ~0.46 s swing. This
        // also halves draw-call submission and mote spawns. It is the exact compute/submit split a
        // future worker-thread pre-pass would build on. Engaged only when throttleEnabled (busy map),
        // never for dev-gallery pawns. Side effects (flecks/sound) fire only on compute frames, with
        // the swing advanced by the ACCUMULATED real-time delta since the pawn's last compute so the
        // motion speed is unchanged.
        internal readonly struct DrawEntry
        {
            public readonly Mesh mesh; public readonly Matrix4x4 matrix; public readonly Material mat; public readonly int layer;
            public DrawEntry(Mesh mesh, Matrix4x4 matrix, Material mat, int layer)
            { this.mesh = mesh; this.matrix = matrix; this.mat = mat; this.layer = layer; }
        }
        // Non-null while a pawn's pose is being captured (inside a throttled DrawActive). Every tool/
        // hand/prop/forearm draw funnels through Emit, so while capturing they accumulate instead of
        // drawing; otherwise (holster, gallery, unthrottled) Emit draws immediately as before. Touched
        // only on the main render thread.
        private static List<DrawEntry> captureSink;
        private static bool throttleEnabled;
        private const int ThrottlePopThreshold = 12;   // map humanlikes at/above which the 30 Hz throttle engages

        internal static void Emit(Mesh mesh, Matrix4x4 matrix, Material mat, int layer)
        {
            List<DrawEntry> sink = captureSink;
            if (sink != null) sink.Add(new DrawEntry(mesh, matrix, mat, layer));
            else Graphics.DrawMesh(mesh, matrix, mat, layer);
        }

        private static void ReplayBuffer(List<DrawEntry> buf)
        {
            if (buf == null) return;
            for (int i = 0; i < buf.Count; i++)
            {
                DrawEntry e = buf[i];
                if (e.mesh != null && e.mat != null) Graphics.DrawMesh(e.mesh, e.matrix, e.mat, e.layer);
            }
        }

        private static void EnsureAnimatorSet()
        {
            int f = Time.frameCount;
            if (f == animatorSetFrame) return;
            animatorSetFrame = f;
            animatorSet.Clear();
            cappedActive = false;
            throttleEnabled = false;

            Map map = Find.CurrentMap;
            if (map == null) return;
            int pop = map.mapPawns.AllHumanlikeSpawned.Count;
            // Pose-recompute throttle engages only on busy colonies (small colonies stay 60 Hz for
            // crispness, where perf isn't a concern). The 30 Hz cap halves per-pawn pose math + mesh
            // uploads + fleck spawns + draw-call submission for the active workers — where the cost is.
            throttleEnabled = pop >= ThrottlePopThreshold;
            int cap = JobEffectsSettings.MaxAnimatedPawns;
            // Cheap upper bound: if the whole map has <= cap humanlikes, the in-view active subset can't
            // exceed the cap — skip the gather entirely (nothing is ever capped, no list/sort cost).
            if (pop <= cap)
            {
                if (Diag.Enabled) Diag.NoteFrameStats(false, throttleEnabled, cap);
                return;
            }

            CameraDriver cd = Find.CameraDriver;
            CellRect view = cd != null ? cd.CurrentViewRect.ExpandedBy(2) : CellRect.WholeMap(map);
            animSortCenter = view.CenterCell;

            animatorScratch.Clear();
            List<Pawn> pawns = map.mapPawns.AllHumanlikeSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p == null || !p.Spawned) continue;
                if (!view.Contains(p.Position)) continue;
                if (!JobCouldHaveTool(p)) continue;                                                // array read — rejects most pawns before MovingNow
                if (p.pather != null && p.pather.MovingNow) continue;                              // walking = cheap holster only
                if (p.stances != null && p.stances.stunner != null && p.stances.stunner.Stunned) continue;
                if (ResolveEffectiveTool(p) == null) continue;                                      // only active-tool pawns count
                animatorScratch.Add(p);
            }

            if (animatorScratch.Count <= cap)
            {
                for (int i = 0; i < animatorScratch.Count; i++)
                    animatorSet.Add(animatorScratch[i].thingIDNumber);
                animatorScratch.Clear();
                if (Diag.Enabled) Diag.NoteFrameStats(false, throttleEnabled, cap);
                return;
            }
            cappedActive = true;
            animatorScratch.Sort(animByDist);
            for (int i = 0; i < cap; i++)
                animatorSet.Add(animatorScratch[i].thingIDNumber);
            animatorScratch.Clear();   // don't hold pawn refs between frames
            if (Diag.Enabled) Diag.NoteFrameStats(true, throttleEnabled, cap);
        }

        // True when an ACTIVE-tool pawn is allowed to render this frame (within the nearest-to-camera
        // cap). Cheap: returns true immediately whenever the colony is under the cap.
        private static bool ActivePawnPassesCap(Pawn pawn)
        {
            EnsureAnimatorSet();
            return !cappedActive || animatorSet.Contains(pawn.thingIDNumber);
        }

        // ---- Entry point (called from the render postfix) ----

        // A pawn whose CURRENT job can't produce a tool may still have unfinished business with us,
        // and must not be rejected by the cheap gate until it's settled. Exactly two cases:
        //
        //  1. shownLastFrame — we drew something last frame, so we still owe the edge-triggered STOW
        //     foley. Missing this would desync shownLastFrame permanently and kill the pickup clatter
        //     on the pawn's next job.
        //  2. a lingering holstered tool — only when holstering is ENABLED, because that's the only
        //     thing that draws (and eventually clears) lastTool.
        //
        // Note case 2 is deliberately gated on the setting rather than just `lastTool != null`. With
        // holstering off nothing ever clears lastTool, so testing it alone would mean every colonist
        // who has ever worked stays permanently past the gate. But we must not CLEAR lastTool either:
        // it's what makes `toolChanged` false when a pawn takes a step mid-job, and resetting it would
        // restart frame-animated tools every time the worker repositions.
        private static bool NeedsIdleWork(PawnToolState st)
        {
            if (st == null) return false;
            if (st.shownLastFrame) return true;
            return JobEffectsSettings.HolsterTools && st.lastTool != null;
        }

        public static void OnPawnRendered(Pawn pawn, Vector3 drawLoc)
        {
            if (!JobEffectsSettings.AnimatedTools || pawn == null) return;
            // Tools are a humanlike-only conceit. Anything that isn't humanlike — animals, mechs,
            // entities, etc. — bails before the job lookup so big herds/swarms cost nothing on the
            // render path.
            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return;
            if (Diag.Enabled) Diag.NoteConsidered();

            // Advance the pause-aware, speed-independent animation clock. Frame-guarded (one int
            // compare after the first call), and it MUST run before the rejection gate below —
            // otherwise a frame in which every pawn bails would leave animTime frozen.
            TickAnimClock();

            int id = pawn.thingIDNumber;

            // ---- Cheapest-first rejection gate (perf) ----
            // The house rule is "order gates cheapest-and-most-rejecting first", and JobCouldHaveTool
            // (one bool[] read indexed by JobDef.index) rejects ~90% of pawns — but until now the
            // RENDER path never used it. It went straight to a gallery probe, the LOD calc, then
            // TryResolve, which reaches the job table via a DICTIONARY hash rather than the array.
            // So every idle / hauling / eating / socializing colonist on screen paid the full chain to
            // be told "no".
            //
            // Now: one array read plus (at most) one state probe. A pawn is only interesting if its
            // current job could produce a tool, OR it still has a holstered tool lingering from the
            // last one. PeekState never allocates, so the ~90% leave no state object behind.
            PawnToolState st = PawnStates.Peek(id);
            bool gallery = IsGalleryPawn(id);
            if (!gallery && !JobCouldHaveTool(pawn) && !NeedsIdleWork(st)) return;
            if (Diag.Enabled) Diag.NotePassedGate();

            // Zoom level-of-detail (perf). Gallery/dev pawns render in a fixed-scale dialog, not the
            // map camera, so they always render at full detail.
            LodLevel lod = gallery ? LodLevel.Full : CurrentLod();
            if (lod == LodLevel.Culled) return;          // zoomed too far out to see tools — skip resolve + draw
            lodSimplified = lod == LodLevel.Simplified;  // drop forearms (sub-pixel extras) at medium zoom

            // Reset per-pawn so a frame-welder's queue bump never leaks into the next pawn's draw
            // (or this pawn's holstered-tool fade). DrawActive re-arms it after resolving the target.
            currentRenderQueue = 0;

            // Past the gate this pawn is genuinely interesting, so materialize its state now and
            // hand it to TryResolve — that skips the store probe inside ResolveEffectiveTool too.
            if (st == null) st = PawnStates.GetOrCreate(id);

            bool active = TryResolve(pawn, st, drawLoc, out JobToolDef tool, out Vector3 dir, out Map map);

            // Per-frame animator cap: on a large colony only the nearest N active workers draw the
            // full tool. A capped-out worker bails here → fully vanilla (HasActiveTool agrees, so the
            // real equipment shows). Walking/holster pawns aren't "active", so they're never capped.
            if (active && !gallery && !ActivePawnPassesCap(pawn)) return;

            bool shown = false;
            if (active)
            {
                if (Diag.Enabled) Diag.NoteActiveDrawn();
                bool toolChanged = st.lastTool != tool;
                if (toolChanged)
                {
                    st.jobStartRealTime = animTime;
                    st.pinPulled = false;
                    st.maxToilTicks = 0;   // fresh activation -> re-capture the toil duration for depletion progress
                    st.maxWorkLeft = 0f;   // fresh activation -> re-capture the bill's work amount for the pour progress
                }
                st.lastTool = tool;
                st.lastDir = dir;
                st.idleTimer = 0f;
                st.carriedOnly = false;   // they're actually working it now → earns the after-work hip linger
                if (GalleryHolster && IsGalleryPawn(id))
                {
                    // Dev gallery/lineup: render the idle HOLSTER pose for this tool instead of the
                    // work swing, so the body-type-aware belt fit can be inspected per build.
                    SetDrawAltitudes(pawn, dir, tool);
                    shown = DrawHolster(drawLoc, tool, dir, 0f, pawn);
                }
                else
                {
                    // ~30 Hz pose-recompute throttle on busy colonies: compute + CAPTURE the full
                    // assembly on compute frames, REPLAY it on skip frames (see the DrawEntry region).
                    // Gallery pawns and small colonies always compute (throttle off).
                    bool throttle = throttleEnabled && !gallery;
                    List<DrawEntry> buf = throttle ? st.DrawBuffer : null;
                    bool compute = !throttle || toolChanged || buf.Count == 0
                        || (((Time.frameCount + id) & 1) == 0);
                    if (compute)
                    {
                        float savedDelta = animDelta;
                        try
                        {
                            if (throttle)
                            {
                                // Advance the swing by the real time since THIS pawn last computed (it
                                // skipped a frame) so motion speed is unchanged; clamp so a hitch or a
                                // pause-resume can't teleport the pose.
                                float d = st.lastComputeTime > 0f ? animTime - st.lastComputeTime : animDelta;
                                animDelta = Mathf.Clamp(d, 0f, 0.2f);
                                st.lastComputeTime = animTime;
                                buf.Clear();
                                captureSink = buf;
                            }
                            DrawActive(pawn, drawLoc, tool, dir, map, st);
                        }
                        finally
                        {
                            if (throttle) { captureSink = null; animDelta = savedDelta; }
                        }
                    }
                    if (throttle) ReplayBuffer(buf);
                    shown = tool.swingStyle != SwingStyle.Scratch;   // Scratch poses (research / book-scribe) draw no tool sprite — no foley
                }
                // (no write-back: st is a reference now, mutated in place)
            }
            else
            {
                // Holster ON THE WAY to a job: while the pawn is still pathing to a job our tool
                // covers, the tool rides on the belt at full opacity (steady, no fade) until they
                // arrive and start working. Resolved WITHOUT the MovingNow gate TryResolve uses.
                JobToolDef carried = null;
                bool walking = pawn.pather != null && pawn.pather.MovingNow;
                if (JobEffectsSettings.HolsterTools && walking
                    && TryResolveCarried(pawn, st, out carried)
                    && carried.swingStyle != SwingStyle.Scratch
                    && !carried.noHolster)   // noHolster tools (medicine kits) never ride the belt en route — vanilla draws the carried item
                {
                    if (st.lastTool != carried)
                    {
                        st.jobStartRealTime = animTime;
                        st.pinPulled = false;
                    }
                    st.lastTool = carried;
                    Vector3 fd = pawn.Rotation.AsVector2.ToVector3();
                    st.lastDir = fd.sqrMagnitude > 0.0001f ? fd.normalized : new Vector3(0f, 0f, -1f);
                    st.idleTimer = 0f;
                    st.carriedOnly = true;   // out only for the approach — drop it the moment they're no longer headed there
                    SetDrawAltitudes(pawn, st.lastDir, carried);   // tuck the belted tool under the body when it sits behind the pawn
                    shown = DrawHolster(drawLoc, carried, st.lastDir, 0f, pawn);   // idleTimer 0 => full alpha; shown only if a sprite actually drew
                }
                else if (JobEffectsSettings.HolsterTools && st.lastTool != null)
                {
                    if (st.carriedOnly || st.lastTool.noHolster)
                    {
                        // carriedOnly: the tool was out ONLY for the walk to a job it covers, and the
                        // pawn is no longer on their way (job changed / interrupted / never started
                        // working). noHolster: a kit (e.g. medicine) that must vanish the instant work
                        // ends because vanilla draws the real carried item. Either way, put it straight
                        // back — the lingering hip fade is reserved for a tool they actually used.
                        st.lastTool = null;
                    }
                    else
                    {
                        st.idleTimer += animDelta;   // hold, then fade over real seconds; frozen while paused
                        if (st.idleTimer >= HolsterLife)
                            st.lastTool = null;
                        else
                        {
                            SetDrawAltitudes(pawn, st.lastDir, st.lastTool);   // hide the belted tool under the body when it sits behind the pawn
                            if (DrawHolster(drawLoc, st.lastTool, st.lastDir, st.idleTimer, pawn)) shown = true;
                        }
                    }
                }
                if (Diag.Enabled && shown) Diag.NoteHolsterDrawn();
            }

            // Tool foley: a short handle clatter when a tool first appears (drawn from the belt)
            // and when it finally disappears (stowed). Edge-triggered + throttled (see PlayFoley)
            // so the rapid work / reposition / work cadence within a job doesn't chatter.
            // Suppress the clatter while the game is PAUSED: the holster linger/fade advances on
            // wall-clock (RealTime.deltaTime keeps ticking when paused), so a tool finishing its
            // fade after the user hits pause would otherwise pop a random stow sound (read as a
            // stray "deconstruct" clatter). We still sync shownLastFrame so it never double-fires
            // on unpause.
            if (shown != st.shownLastFrame)
            {
                if (JobEffectsSettings.ToolFoley && pawn.Map != null && !Find.TickManager.Paused)
                    PlayFoley(pawn, st, pawn.Map);
                st.shownLastFrame = shown;
            }

            // Disarm the render-queue override so a north pawn's under-body bump (or a frame
            // welder's over-bump) can't leak onto the SMYH forearm hook, which renders in a separate
            // pass between pawns and must keep its own (hand-relative) depth. Already-issued draws
            // hold cloned materials with the queue baked in, so this never affects this frame's tool.
            currentRenderQueue = 0;
        }

        // ---- Tool resolution shared by draw + lean ----

        // Among the JobToolDefs registered for a job, choose the one to draw. Normally the FIRST
        // enabled candidate that Matches (XML order = priority, which the surgery / grave / build
        // material splits rely on). But if that winner declares a variantGroup, every enabled +
        // matching candidate in the same group is pooled and ONE is picked at random — stable per
        // job instance (seeded off the pawn id + Job.loadID), so it holds for the whole job and only
        // varies between activations (e.g. the baby grabbing a different toy each play session).
        private static readonly List<JobToolDef> tmpVariants = new List<JobToolDef>();
        private static JobToolDef ResolveCandidate(Pawn pawn, Job job, List<JobToolDef> candidates, LocalTargetInfo target)
        {
            JobToolDef first = null;
            for (int i = 0; i < candidates.Count; i++)
            {
                JobToolDef c = candidates[i];
                if (JobEffectsSettings.IsToolEnabled(c.defName) && Matches(c, target, job)) { first = c; break; }
            }
            if (first == null || first.variantGroup.NullOrEmpty()) return first;
            tmpVariants.Clear();
            for (int i = 0; i < candidates.Count; i++)
            {
                JobToolDef c = candidates[i];
                if (c.variantGroup == first.variantGroup
                    && JobEffectsSettings.IsToolEnabled(c.defName) && Matches(c, target, job))
                    tmpVariants.Add(c);
            }
            if (tmpVariants.Count <= 1) return first;
            uint h = (uint)pawn.thingIDNumber * 2654435761u + (uint)job.loadID * 2246822519u;
            h ^= h >> 13;
            return tmpVariants[(int)(h % (uint)tmpVariants.Count)];
        }

        // Pre-Electricity tech-gate. Applied to the RESOLVED tool so every consumer (draw, holster,
        // SMYH/Yayo compat) sees the same effective tool. When the gate is on and the Electricity
        // research isn't finished, a tool flagged requiresElectricity is swapped to its period-correct
        // equivalent (preElectricityTool) or suppressed (null). Resolved swap defs are cached. A swap
        // target that's missing OR disabled by the player suppresses the tool instead.
        private static readonly Dictionary<string, JobToolDef> techSwapCache = new Dictionary<string, JobToolDef>();
        private static JobToolDef ApplyTechGate(JobToolDef tool)
        {
            if (tool == null || !tool.requiresElectricity) return tool;
            if (!JobEffectsSettings.GateModernTools || JobEffectsSettings.ElectricityResearched) return tool;
            if (tool.preElectricityTool.NullOrEmpty()) return null;   // gated, no swap -> suppress
            if (!techSwapCache.TryGetValue(tool.preElectricityTool, out JobToolDef swap))
            {
                swap = DefDatabase<JobToolDef>.GetNamedSilentFail(tool.preElectricityTool);
                techSwapCache[tool.preElectricityTool] = swap;
            }
            if (swap == null || !JobEffectsSettings.IsToolEnabled(swap.defName)) return null;
            return swap;
        }

        private static bool TryResolve(Pawn pawn, PawnToolState st, Vector3 drawLoc, out JobToolDef tool, out Vector3 dir, out Map map)
        {
            tool = null; dir = default; map = null;
            if (pawn == null || !pawn.Spawned || pawn.Dead) return false;

            // Dev gallery: force the chosen tool onto the model pawn, skipping the job/path/stun
            // checks below so the pure swing renders even though the pawn is just standing.
            JobToolDef gTool = GalleryToolFor(pawn.thingIDNumber);
            if (gTool != null)
            {
                map = pawn.Map;
                if (map == null) return false;
                tool = gTool;
                // Work in whatever direction the model is facing (settable N/E/S/W from the dialog).
                dir = GalleryFacing.AsVector2.ToVector3();
                if (dir.sqrMagnitude < 0.0001f) dir = new Vector3(0f, 0f, -1f);
                dir = dir.normalized;
                return true;
            }

            // Active-work gate — cheap, applied fresh every frame (NOT baked into the resolution
            // cache, which is keyed on job/target identity only).
            if (pawn.pather != null && pawn.pather.MovingNow) return false;
            if (pawn.stances != null && pawn.stances.stunner != null && pawn.stances.stunner.Stunned) return false;

            tool = ResolveEffectiveTool(pawn, st);   // memoized candidate match + tech-gate, shared by every flow
            if (tool == null) return false;

            Job job = pawn.CurJob;
            if (job == null) return false;       // resolved non-null already implies a job; guard anyway
            LocalTargetInfo target = job.targetA;

            map = pawn.Map;
            if (map == null) return false;

            Vector3 pawnFlat = new Vector3(drawLoc.x, 0f, drawLoc.z);
            Vector3 targetFlat = target.CenterVector3; targetFlat.y = 0f;
            dir = targetFlat - pawnFlat; dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                dir = pawn.Rotation.AsVector2.ToVector3();
            dir = dir.normalized;
            return true;
        }

        // Resolve the animated tool for a pawn's CURRENT job WITHOUT the MovingNow / stun gate
        // that TryResolve applies — used to keep the tool holstered on the belt while the pawn is
        // still pathing toward the job it covers. Needs no draw position or work direction.
        private static bool TryResolveCarried(Pawn pawn, PawnToolState st, out JobToolDef tool)
        {
            tool = null;
            if (pawn == null || !pawn.Spawned || pawn.Dead) return false;
            tool = ResolveEffectiveTool(pawn, st);   // no MovingNow gate: the carried/holster path runs while walking
            return tool != null;
        }

        // ---- Tool foley (handle clatter on pickup / stow) ----
        // Throttled per pawn so the work/reposition cadence can't chatter; resolved once.
        private static readonly LazyDef<SoundDef> foleySound = new LazyDef<SoundDef>("JE_ToolFoley");
        private static void PlayFoley(Pawn pawn, PawnToolState st, Map map)
        {
            SoundDef snd = foleySound.Value;
            if (snd == null || pawn == null || map == null) return;
            float now = animTime;
            if (now - st.lastFoleyTime < 0.8f) return;
            st.lastFoleyTime = now;
            snd.PlayOneShot(SoundInfo.InMap(new TargetInfo(pawn.Position, map)));
        }

        // The phase at which each swing style lands its (primary) strike — must match the per-style
        // Crossed(...) trigger points in DrawActive. Used to phase-lock a tool to an external clock
        // (Yayo's body lunge) so its strike coincides with that clock's hit frame.
        private static float StrikePhaseFor(JobToolDef tool)
        {
            switch (tool.swingStyle)
            {
                case SwingStyle.Dig:   return DigStrike;
                case SwingStyle.Stab:  return tool.stabStrikePhase;
                case SwingStyle.Sweep: return SweepStrike;
                case SwingStyle.Pry:   return PryStrike;
                case SwingStyle.Crank: return CrankStrike;
                case SwingStyle.Load:  return LoadStrike;
                case SwingStyle.Beat:  return BeatStrike1;
                case SwingStyle.Spray: return SprayStrike;
                case SwingStyle.Saw:   return 0.2f;
                case SwingStyle.Hold:  return 0f;
                default:               return StrikePoint;   // Chop (and continuous Stir, harmless)
            }
        }

        // ---- Drawing + per-frame emissions ----

        // Draw altitude for the active tool + its hands, recomputed per pawn each frame. Tools and
        // forearms normally sit ABOVE the body (MoteOverhead); when the pawn faces NORTH (back to the
        // camera) they tuck UNDER the body sprite so the torso occludes them, mirroring how vanilla
        // draws a held weapon behind a north-facing pawn.
        private static float currentToolY = AltitudeLayer.MoteOverhead.AltitudeFor();
        private static float currentHandY = AltitudeLayer.MoteOverhead.AltitudeFor() + 0.03f;

        // When a pawn works a half-built structure, the construction Frame draws its fill tiles +
        // corner brackets with the Map/MetaOverlay shader (render queue ~3600). A higher render
        // queue paints OVER everything in a lower one regardless of geometric altitude, so our
        // tool/hand/arm sprites (Cutout ~2450, Transparent ~3000) get covered by the frame fill no
        // matter how high we lift them in world Y. While working a Frame/Blueprint we re-issue those
        // materials on a render queue ABOVE the overlay band so the working hands stay on top of the
        // structure. Scoped to construction only: a global bump would also shove the tool over its
        // own spark/chip motes (~3000) everywhere else. 0 = no override active this draw.
        private const int OverlayWorkQueue = 3700;
        // Render queue dropped just BELOW the pawn body's Cutout band (~2450) so a north-facing
        // pawn's ENTIRE tool/hand/arm/mask/prop stack is painted over by skin + clothes + head +
        // hair. Queue dominance (a lower queue always draws under a higher one regardless of world
        // Y) is the only thing that can pull a Transparent-shader part — the arm sleeves and the
        // welder/holster/shell resting sprites (~3000) — under the body; altitude alone cannot. The
        // value still sits above terrain/floors (Geometry ~2000), so the tucked stack stays on top
        // of the ground rather than vanishing beneath it.
        private const int UnderBodyQueue = 2440;
        private static int currentRenderQueue;

        // The pawn body's ACTUAL shader render queue (identical for all humanlikes — one body shader),
        // read live from a body material and cached. "Under the body" then means body-queue − 1, which
        // is correct no matter what queue the shader actually uses. The old hardcoded UnderBodyQueue
        // (2440) assumed the body sat at ~2450; in practice it does NOT, so cloning our stack to 2440
        // left it ABOVE the body — the tool/hands/forearm drew OVER a north-facing colonist. Reading the
        // real value fixes that and also buries the Transparent forearm (which can't depth-occlude).
        private static int cachedBodyQueue = -1;
        private static int BodyRenderQueue(Pawn pawn)
        {
            if (cachedBodyQueue > 0) return cachedBodyQueue;
            try
            {
                Graphic bg = pawn?.Drawer?.renderer?.BodyGraphic;
                Material m = bg?.MatSouth;
                if (m != null && m.renderQueue > 0) { cachedBodyQueue = m.renderQueue; return cachedBodyQueue; }
            }
            catch { }
            return UnderBodyQueue + 10;   // not resolved yet -> sane default this frame, retry next
        }
        // Render queue one step below the body, so the whole north stack (Cutout tool/hands AND the
        // Transparent forearm) is painted under the colonist by queue dominance.
        private static int UnderBodyQueueFor(Pawn pawn) => BodyRenderQueue(pawn) - 1;

        // Clone cache keyed by (baseMat, queue): the same source material can need both the under-
        // body (2440) and over-overlay (3700) variant across different pawns/frames, so the queue
        // MUST be part of the key — otherwise the first variant cached for a material would stick.
        private readonly struct MatQKey : System.IEquatable<MatQKey>
        {
            public readonly Material mat; public readonly int q;
            public MatQKey(Material m, int queue) { mat = m; q = queue; }
            public bool Equals(MatQKey o) => ReferenceEquals(mat, o.mat) && q == o.q;
            public override bool Equals(object o) => o is MatQKey x && Equals(x);
            public override int GetHashCode() => (mat == null ? 0 : mat.GetHashCode()) * 31 + q;
        }
        private static readonly Dictionary<MatQKey, Material> queuedMatCache = new Dictionary<MatQKey, Material>();

        // Returns baseMat unchanged unless a render-queue override is armed for this draw (north
        // under-body, or a non-north pawn working over a construction overlay), in which case a
        // cached clone on the armed queue is returned. Only the render queue changes (shader/texture/
        // colour/depth-write preserved), so the hand>tool>arm relative ordering — decided by altitude
        // WITHIN the queue — is identical; the trio just collectively sits under the body (north) or
        // above the frame fill (non-north construction).
        public static Material QueueAdjust(Material baseMat) => QueueAdjustTo(baseMat, currentRenderQueue);

        // Clone-to-queue cache shared by QueueAdjust (ambient currentRenderQueue) and the holster
        // (which passes an explicit queue). queue <= 0 means "leave the material on its own queue".
        public static Material QueueAdjustTo(Material baseMat, int queue)
        {
            if (baseMat == null || queue <= 0) return baseMat;
            var key = new MatQKey(baseMat, queue);
            if (!queuedMatCache.TryGetValue(key, out Material q) || q == null)
            {
                q = new Material(baseMat) { renderQueue = queue };
                queuedMatCache[key] = q;
            }
            return q;
        }

        // Returns true when the stack was tucked under the body. "North" for occlusion means EITHER
        // the pawn's body faces north (back to camera) OR the tool is being DRAWN pointing away from
        // the camera (dir.z > 0.45 -- the SAME draw-space "north" test the per-tool positioning uses).
        // Held / idle-pose jobs (taming, surgery, telescope, lessons, mech repair) don't always rotate
        // the body toward a northern target, yet the tool is still drawn up-and-away over the pawn --
        // so keying occlusion off the draw direction (not just body rotation) makes EVERY tool tuck
        // under the body whenever it's drawn behind the pawn, not just when the body sprite faces north.
        private static bool SetDrawAltitudes(Pawn pawn, Vector3 dir, JobToolDef tool)
        {
            // BODY-FACING decides the WHOLE stack's depth as one unit: a north-facing body (back to
            // camera) buries tool + hands + forearms + mask + props UNDER the body; every other facing
            // draws the ENTIRE stack OVER it. No per-part or per-draw-direction split. (Was keyed partly
            // off the draw direction dir.z, which made held-pose tools tuck under inconsistently.)
            bool north = pawn != null && pawn.Rotation == Rot4.North;
            if (north)
            {
                // NORTH: bury the whole stack BELOW the bare body sprite. The body skin draws at
                // pawn-layer 0, body apparel ~20, shell apparel 88, head 50, hair 62 — so the old
                // positive layers (11/13) sat OVER any bare skin that showed (neck, shoulders, an
                // unclothed back) and the tool/hand poked through. Negative layers are still inside
                // the Pawn altitude band (well above ground items), so going under 0 guarantees the
                // ENTIRE pawn — skin, clothes, head, hair — occludes the tool, hands, mask and arms.
                // Relative order kept: tool below hand so the fist still reads on top of the haft.
                // Match VANILLA's own north rule: PawnRenderer draws equipment/apparel-extras at
                // AltitudeForLayer(-10) when facing north (the clamp floor) so a held weapon sits
                // BEHIND the back. We park the whole tool stack at that same -10 depth (hands one
                // notch up at -8 so the fist still reads over the haft) — decisively under every
                // body sub-layer, not just barely below it.
                float baseY = AltitudeLayer.Pawn.AltitudeFor();
                currentToolY = baseY + PawnRenderUtility.AltitudeForLayer(-10f);  // under the bare body sprite (vanilla north-equip depth)
                currentHandY = baseY + PawnRenderUtility.AltitudeForLayer(-8f);   // fists just over the tool, still well under body
                // Altitude alone can't pull a Transparent-shader part (arm sleeves, the welder/
                // holster/shell resting sprites) under the body — a higher render QUEUE paints over
                // a lower one regardless of world Y. So additionally drop the whole stack onto a
                // queue just below the body band; every tool/hand/arm/mask/prop draw routes through
                // QueueAdjust, which honours this. Negative altitudes above still order our own
                // parts (hand over tool over arm) within that single queue. The queue is read LIVE
                // from the body material (body-queue − 1) — a hardcoded guess was above the real body
                // queue, so the stack drew over north pawns.
                currentRenderQueue = UnderBodyQueueFor(pawn);
            }
            else if (tool != null && tool.alwaysInFront)
            {
                // Per-tool override: this tool must ALWAYS read in front of the pawn (and the world),
                // never world-occluded, for every facing except north (handled above). Float it at
                // MoteOverhead like the legacy always-on-top path. Used by the welder (face mask), the
                // medicine kits, cleaver, crowbar, smelt rod and hacking device.
                currentToolY = AltitudeLayer.MoteOverhead.AltitudeFor();
                currentHandY = currentToolY + 0.03f;
                currentRenderQueue = 0;
            }
            else if (JobEffectsSettings.ToolsMatchPawnDepth)
            {
                // HYBRID DEPTH (#2): the SOLID parts (tool / hands / props / mask — all Cutout) sit in
                // the Pawn band at the vanilla held-weapon depth (sublayer ~90) and depth-write, so they
                // sort by world Y exactly like the colonist's OWN equipped weapon — a tree canopy, wall
                // edge or foreground object that occludes the colonist occludes the tool too. NO render-
                // queue override (currentRenderQueue 0 = native Cutout, world-sorted). The translucent
                // forearm can't depth-write, so it rides its own Transparent queue (over the body) — a
                // faint wrist wash over the haft is the only residual of that shader split.
                float baseY = AltitudeLayer.Pawn.AltitudeFor();
                currentToolY = baseY + PawnRenderUtility.AltitudeForLayer(90f);  // vanilla weapon depth, over the body
                currentHandY = baseY + PawnRenderUtility.AltitudeForLayer(93f);  // fists a hair over the haft
                currentRenderQueue = 0;
            }
            else
            {
                // Setting off: legacy "always on top" — float the stack at MoteOverhead, over the world.
                currentToolY = AltitudeLayer.MoteOverhead.AltitudeFor();
                currentHandY = currentToolY + 0.03f;
                currentRenderQueue = 0;
            }
            return north;
        }

        // Rendered body SIZE of the pawn being drawn this frame (1.0 for a stock adult, ~0.75 for a
        // child, gene/HAR-scaled otherwise). Set per-pawn in DrawActive; consumed by DrawTool's toy
        // pose so the held toy shrinks onto a child to match the body-scaled hands + forearms.
        private static float currentBodyScale = 1f;

        private static void DrawActive(Pawn pawn, Vector3 drawLoc, JobToolDef tool, Vector3 dir, Map map, PawnToolState st)
        {
            currentBodyScale = ArmRenderer.BodyScaleFor(pawn);
            Vector3 pawnFlat = new Vector3(drawLoc.x, 0f, drawLoc.z);
            Thing worked = pawn.CurJob?.targetA.Thing;
            bool north = SetDrawAltitudes(pawn, dir, tool);

            // SetDrawAltitudes (just above) already armed the under-body render queue when the tool is
            // drawn behind a north-facing pawn — queue dominance keeps the WHOLE stack (including the
            // Transparent arm sleeves) beneath the body. Only a NON-north draw working a construction
            // Frame/Blueprint needs the opposite bump: its MetaOverlay fill (~3600) would paint over
            // the tool/hands/arms, so we lift them above it. A north draw never takes this branch (it
            // stays tucked under the body; the frame fill reclaiming the far tip is the correct trade).
            // Construction over-bump: a Frame/Blueprint draws its fill + brackets on the MetaOverlay
            // queue (~3600), which would cover the working stack (the Cutout tool depth-writes, but the
            // frame fill is a higher queue and wins regardless). While building a frame, lift the WHOLE
            // stack onto a queue above that band so the hands/tool stay visible. North stays tucked
            // under the body; other non-frame facings keep the native Cutout world-sort.
            if (!north && worked != null && worked.def != null
                && (worked.def.IsFrame || worked.def.IsBlueprint))
            {
                currentRenderQueue = OverlayWorkQueue;
            }

            // Bead-on-target (welder): anchor the work point ON the worked structure. Re-aim at
            // the occupied cell nearest the pawn and extend the grip so the painted tip — plus
            // the arc flash / spark shower that ride it — sits on top of the building / frame
            // sprite instead of waving at fixed reach beside the pawn's face. Most visible on
            // diagonal work, where the fixed reach left the torch welding thin air.
            beadReachOverride = -1f;
            if (tool.beadOnTarget && worked != null && worked.Spawned
                && !IsGalleryPawn(pawn.thingIDNumber))
            {
                Vector3 bead = worked.OccupiedRect().ClosestCellTo(pawn.Position).ToVector3Shifted();
                Vector3 toBead = bead - pawnFlat; toBead.y = 0f;
                float beadDist = toBead.magnitude;
                if (beadDist > 0.35f)   // standing on/inside the target: keep the def reach
                {
                    dir = toBead / beadDist;
                    // The geometric tip sits one `scale` past the grip; the painted hot end is a
                    // touch inside the canvas, so aim that at the bead point.
                    float tipLen = tool.scale * 0.80f;
                    // Aim the DIRECTION at the worked cell, but cap how far the arm actually
                    // reaches: a diagonal neighbour's cell CENTRE is ~1.41 cells away vs ~1.0
                    // orthogonally, which over-extended the arm on angled work (the tip waved a
                    // half-cell past the pawn's grip). Clamp the reach distance to the orthogonal
                    // baseline so the tip plants on the NEAR corner of the sprite (still on the
                    // building/frame) and the arm reads identically at every facing.
                    float reachDist = Mathf.Min(beadDist, 1.05f);
                    beadReachOverride = Mathf.Clamp(reachDist - tipLen, tool.reach, 0.95f);
                }
            }

            // Particle effects (debris, dust, glints, tip glow, emotes, litter) can be muted
            // per-tool while the tool keeps swinging — DrawTool/DrawHands below are unaffected.
            bool fx = JobEffectsSettings.AreToolEffectsEnabled(tool.defName);
            if (IsGalleryPawn(pawn.thingIDNumber)) fx = fx && GalleryEffects;
            // Simplified LOD (medium zoom): shed ALL particle effects/motes. They're sub-pixel there,
            // and unlike a one-shot sprite each spawned fleck carries ongoing tick + render cost for
            // its whole lifetime (this is also what trims the per-frame spawn spikes). Vanilla work
            // motes reappear since suppressWorkMotes won't arm below — a fine mid-zoom fallback.
            if (lodSimplified) fx = false;

            // Flag this pawn so the vanilla work-effecter sprayer (e.g. the smithy's feet sparks)
            // is suppressed this tick: our welder throws its own sparks at the electrode tip, and
            // the vanilla BetweenPositions motes erupting from the pawn's feet just look wrong.
            // Re-armed every render frame; the effecter patch reads a few-tick freshness window.
            if (tool.suppressWorkMotes && fx)
                st.suppressWorkMoteUntil = Find.TickManager.TicksGame + 4;

            // 0 when paused via time controls OR force-paused by a window (Esc menu, dialogs,
            // long-event loads); otherwise 1 — animations play at constant real-time 1x speed
            // regardless of the game's tick rate (2x/3x/4x), so tools never look frantic.
            float speed = Find.TickManager.Paused ? 0f : 1f;

            // ---- Brow wipe (axe/pickaxe): periodically shoulder the tool and wipe the forehead ----
            if (tool.browWipe)   // not speed-gated: when paused, animDelta is 0 so the wipe holds its pose
            {
                if (st.wipeNext <= 0f) st.wipeNext = Rand.Range(16f, 26f);
                if (st.wipeT > 0f)
                {
                    float prevW = st.wipeT;
                    st.wipeT += animDelta / 1.6f;
                    if (st.wipeT >= 1f)
                    {
                        st.wipeT = 0f; st.wipeAccum = 0f; st.wipeNext = Rand.Range(16f, 26f);
                    }
                    else
                    {
                        // Sweat drops flicked off mid-wipe.
                        if (fx && (Crossed(prevW, st.wipeT, 0.30f) || Crossed(prevW, st.wipeT, 0.65f)))
                            SpawnSweat(map, drawLoc, dir.x < 0f ? -1f : 1f);
                        DrawBrowWipe(pawn, drawLoc, tool, dir, st.wipeT);
                        return;
                    }
                }
                else
                {
                    st.wipeAccum += animDelta;
                    if (st.wipeAccum >= st.wipeNext) st.wipeT = 0.0001f;
                }
            }
            // Phase sources, in priority order:
            //  1. Yayo's Animation (when loaded): lock one swing per Yayo BODY cycle, with this tool's
            //     own strike landing on Yayo's lunge frame — so body + tool read as one motion.
            //  2. syncToMineHit (the pickaxe, no Yayo): drive phase off the vanilla mining countdown
            //     so the strike lands on the real pick-hit.
            //  3. Otherwise free-run at constant real time.
            // BOTH sync sources are tick-driven (TicksGame / the mining countdown), so they would
            // speed the swing up 3x/6x/15x with the game's time controls. To keep every tool at a
            // calm real-time 1x at 2x/3x/4x, sync is only engaged at Normal speed; once the game is
            // sped up (TickRateMultiplier > 1) the tool free-runs at real time like everything else.
            float mhPhase = 0f;
            bool synced = false;
            bool allowSync = speed > 0f && Find.TickManager.TickRateMultiplier <= 1.0001f;
            if (allowSync)
            {
                if (YayoCompat.Active && YayoCompat.TryGetCyclePos(pawn, out float yayoPos))
                {
                    st.prevPhase = st.phase;
                    // Align the tool's own strike phase to Yayo's lunge (cycle pos 0).
                    st.phase = Mathf.Repeat(StrikePhaseFor(tool) + yayoPos, 1f);
                    synced = true;
                }
                else if (tool.syncToMineHit && TryGetMineHitPhase(pawn, out mhPhase))
                {
                    st.prevPhase = st.phase;
                    st.phase = mhPhase;
                    synced = true;
                }
            }
            // Work-sound hit sync (butcher cleaver, bench chisels) is wall-clock based (the work
            // sound plays in real time at any game speed), so it engages even when sped up — just
            // not while paused.
            if (!synced && speed > 0f && (tool.syncToButcherHit || tool.syncToBenchSound)
                && TryGetWorkHitPhase(st, tool, out float bhPhase))
            {
                st.prevPhase = st.phase;
                st.phase = bhPhase;
                synced = true;
            }
            if (!synced && speed > 0f && tool.period > 0.0001f)
            {
                st.prevPhase = st.phase;
                st.phase += animDelta / tool.period;
                while (st.phase >= 1f) st.phase -= 1f;
            }

            // Cycle bookkeeping on phase wrap: bump the cycle counter (drives every-Nth-cycle
            // behaviors like paint dips, dustpan passes, quenches, instrument swaps) and reroll
            // the sewing jitter so each stitch comes in from a slightly different angle and spot.
            if (st.phase < st.prevPhase)
            {
                st.cycleCount++;
                if (tool.stabVaried)
                {
                    st.stabJitterAng = Rand.Range(-16f, 16f);
                    st.stabJitterLat = Rand.Range(-0.09f, 0.09f);
                }
            }

            // Impact burst on the strike crossing(s). Beat slams twice per cycle, Saw scrapes on
            // every pass; the rest strike once. Stir tools emit continuously and never "strike".
            bool struck = false;
            if (speed > 0f)
            {
                float p0 = st.prevPhase, p1 = st.phase;
                switch (tool.swingStyle)
                {
                    case SwingStyle.Beat:
                        struck = Crossed(p0, p1, BeatStrike1) || Crossed(p0, p1, BeatStrike2);
                        break;
                    case SwingStyle.Saw:
                        struck = Crossed(p0, p1, 0.2f) || Crossed(p0, p1, 0.45f)
                              || Crossed(p0, p1, 0.7f) || Crossed(p0, p1, 0.95f);
                        break;
                    case SwingStyle.Dig:   struck = Crossed(p0, p1, DigStrike); break;
                    case SwingStyle.Crank: struck = Crossed(p0, p1, CrankStrike); break;
                    case SwingStyle.Load:  struck = Crossed(p0, p1, LoadStrike); break;
                    case SwingStyle.Pry:   struck = Crossed(p0, p1, PryStrike); break;
                    case SwingStyle.Stab:  struck = Crossed(p0, p1, tool.stabStrikePhase); break;
                    case SwingStyle.Sweep: struck = Crossed(p0, p1, SweepStrike); break;
                    case SwingStyle.Chop:  struck = Crossed(p0, p1, StrikePoint); break;
                    // Spray is NOT driven by the phase clock — it pulses on the real beat-fire lunge (below).
                }
            }

            // ---- Stuck pickaxe: occasionally the pick jams in the rock on a strike ----
            float extraSwing = 0f;
            if (tool.canStick)   // not speed-gated: when paused, animDelta is 0 so a stuck pick holds its pose
            {
                if (st.stuckNext <= 0f) st.stuckNext = Rand.Range(22f, 40f);
                if (st.stuckT > 0f)
                {
                    st.stuckT += animDelta / 0.9f;
                    if (st.stuckT >= 1f) { st.stuckT = 0f; st.stuckAccum = 0f; st.stuckNext = Rand.Range(22f, 40f); }
                    else
                    {
                        // Frozen at the impact pose, wiggling it loose; the resume windup reads
                        // as the pull-free.
                        st.phase = StrikePoint; st.prevPhase = StrikePoint;
                        struck = false;
                        extraSwing = Mathf.Sin(animTime * 28f) * 3.5f * (1f - st.stuckT * 0.4f);
                    }
                }
                else
                {
                    st.stuckAccum += animDelta;
                    if (struck && st.stuckAccum >= st.stuckNext)
                    {
                        st.stuckT = 0.0001f;
                        struck = false;   // this hit BIT and stuck — no debris burst
                    }
                }
            }

            // Paintbrush dip cycles: the brush is at the pot, not the wall — no paint specks there.
            bool sideSpecial = tool.SidePropMaterial != null && tool.sidePropEvery > 0
                && (st.cycleCount % tool.sidePropEvery) == tool.sidePropEvery - 1;
            if (sideSpecial && tool.sidePropMode == "dip") struck = false;

            // ---- Spray (fire extinguisher): one foam puff per vanilla beat-fire lunge ----
            // Event-driven off Verb_BeatFire casts (see NotifyBeatFire), so the discharge lands
            // exactly when the pawn lunges to beat the fire, not on our free-running clock. In the
            // dev gallery (no real fire/beats) fall back to a phase pulse so the style is visible.
            float sprayKick = 0f;
            bool sprayPuff = false;
            if (tool.swingStyle == SwingStyle.Spray)
            {
                bool galleryMode = IsGalleryPawn(pawn.thingIDNumber);
                if (galleryMode)
                {
                    sprayPuff = speed > 0f && Crossed(st.prevPhase, st.phase, SprayStrike);
                    sprayKick = SprayRecoil(st.phase);
                }
                else
                {
                    st.sprayKick = Mathf.Max(0f, st.sprayKick - animDelta / SprayKickTime);
                    int beats = BeatFireBeats(st);
                    if (beats != st.beatConsumed)
                    {
                        st.beatConsumed = beats;   // one or more new beats -> one puff + fresh kick
                        st.sprayKick = 1f;
                        sprayPuff = true;
                    }
                    sprayKick = st.sprayKick;
                }
            }

            // ---- Interval / stir sound pulses ----
            // Stir-style tools never cross a strike point, so their strikeSound would stay
            // silent (the welder-crackle bug). Fire it on a timer instead: strikeSoundInterval
            // when set, else once per completed stir cycle (phase wrap).
            bool soundPulse = false;
            if (tool.StrikeSound != null && speed > 0f)
            {
                if (tool.strikeSoundInterval > 0f)
                {
                    st.strikeSoundAccum += animDelta;
                    if (st.strikeSoundAccum >= tool.strikeSoundInterval)
                    {
                        st.strikeSoundAccum -= tool.strikeSoundInterval;
                        if (st.strikeSoundAccum > tool.strikeSoundInterval)
                            st.strikeSoundAccum = 0f;   // don't burst-catch-up after a hitch
                        soundPulse = true;
                    }
                }
                else if (tool.swingStyle == SwingStyle.Stir)
                    soundPulse = st.phase < st.prevPhase;
            }

            // For DoBill workbench jobs: only apply impacts/sounds when work is actually
            // progressing. workLeftAtLastStrike is updated on each strike so we compare the
            // current workLeft against the value at the previous strike; if it hasn't moved
            // (paused, finished, or in a transition toil) the hit is visual-only.
            bool workProgressed = true;
            pourProgress = -1f;
            if (pawn.jobs?.curDriver is JobDriver_DoBill billDriver)
            {
                float wl = billDriver.workLeft;
                bool billChanged = billDriver.billStartTick != st.lastBillStartTick;
                if (billChanged) { st.workLeftAtLastStrike = float.MaxValue; st.maxWorkLeft = 0f; }
                workProgressed = wl > 0f && (st.workLeftAtLastStrike <= 0f || wl < st.workLeftAtLastStrike);
                if (struck || soundPulse) st.workLeftAtLastStrike = wl;
                st.lastBillStartTick = billDriver.billStartTick;
                // holdPour pacing: capture the bill's total work (first frame ~= full) and derive
                // 0..1 progress so the flask inverts at the halfway mark and empties at the end.
                if (tool.holdPour)
                {
                    if (wl > st.maxWorkLeft) st.maxWorkLeft = wl;
                    pourProgress = st.maxWorkLeft > 0f ? Mathf.Clamp01(1f - wl / st.maxWorkLeft) : 0f;
                }
            }

            // Per-swing strike sound, synced to the visual hit. The vanilla effecter/sustainer
            // sound keeps playing underneath as a tools-off fallback; percussive SoundDefs cap
            // their own concurrency (maxSimultaneous), so the two never audibly stack.
            if ((((struck || soundPulse) && workProgressed) || sprayPuff) && fx && tool.StrikeSound != null)
                tool.StrikeSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(pawn.Position, map)));

            // Job-progress pacing for a tool that empties out over the job (herbal bundle): only
            // computed when the tool actually has depletion frames, otherwise stays -1.
            depletionProgress = (tool.depletionTexPaths != null && tool.depletionTexPaths.Count > 0)
                ? ComputeToilProgress(pawn, st)
                : -1f;

            DrawTool(tool, drawLoc, dir, pawn.Rotation, st.phase, sprayKick, st.jobStartRealTime,
                st.stabJitterAng, st.stabJitterLat, st.cycleCount, extraSwing,
                out Vector3 tip, out Vector3 grip);

            // Impact burst AFTER the tool draw so impactAtTip tools can anchor the effect on the
            // actual tool tip (computed from tip/grip) rather than a generic point in front.
            // soundPulse: interval-pulsed tools (welder bead) never strike, so their burst rides
            // the same pulse as the strike sound — the spark shower erupts from the electrode
            // tip at the exact moment the sizzle/snap plays.
            if ((struck || soundPulse) && workProgressed && fx)
            {
                // Subtract the draw-only strikeLift so the burst anchors to the un-shifted strike
                // pose (the sprite was nudged onto the spark, not the other way round). Zero for
                // tools without strikeLift; overwritten entirely on the Stab path below.
                Vector3 impactTip = tip - strikeDrawShift, impactGrip = grip - strikeDrawShift;
                if (tool.swingStyle == SwingStyle.Stab && tool.impactAtTip && tool.MalletMaterial == null)
                {
                    // (Mallet tools skip this: the chisel is PLANTED, never extended, so the
                    // drawn tip is already the true impact point.)
                    // Stab holds at full extension from stabStrikePhase to 0.5. Anchor the burst
                    // at the exact full-extension pose so it lands at the true tip even if the
                    // frame phase jumped past the hold.
                    Vector3 strikeHand = pawnFlat + dir * (tool.reach + tool.stabDistance);
                    impactTip = strikeHand + dir * tool.scale;
                    impactTip.y = 0f;
                    impactGrip = new Vector3(strikeHand.x, 0f, strikeHand.z);
                }
                SpawnImpact(tool, map, pawnFlat, dir, worked, impactTip, impactGrip,
                    NSOffset(tool, pawn.Rotation, tool.impactTipOffset, tool.impactTipOffsetNorthSouth));

                // Chisel-and-mallet: a small spark "ding" right at the chisel BUTT where the
                // mallet lands, so the blow itself visibly connects (the chips at the tip show
                // the result; this shows the hit).
                if (tool.MalletMaterial != null && tool.FlashFleck != null)
                {
                    // Nudge the spark down onto the chisel top / strike face (matches the lowered
                    // mallet contact) instead of floating at the bare grip point.
                    Vector3 ding = impactGrip + dir * (tool.scale * 0.22f); ding.y = 0f;
                    if (ding.ToIntVec3().ShouldSpawnMotesAt(map))
                        FleckMaker.Static(ding, map, tool.FlashFleck, tool.impactFlashScale * 0.7f);
                }
            }

            // Foam dump: each beat-fire lunge throws a white foam clump straight onto the fire's
            // tile (worked == the Fire being beaten), smothering it and kicking a small ember gust.
            if (sprayPuff && fx)
            {
                Vector3 fireCenter = (worked != null && worked.Spawned) ? worked.DrawPos : (pawnFlat + dir);
                fireCenter.y = 0f;
                Vector3 nozzle = tip; nozzle.y = 0f;
                SpawnSpray(tool, map, nozzle, fireCenter);
            }

            // Tiny thread wisps rising from the tool tip on each strike (sewing needle stitching).
            if (struck && workProgressed && fx && tool.ThreadFleck != null)
                SpawnThreads(tool, map, tip);

            // Cook's salt pinch: grains sprinkled from the raised off hand during the final laps.
            if (tool.stirVaried && fx && speed > 0f && offHandWristValid
                && (Crossed(st.prevPhase, st.phase, 0.78f) || Crossed(st.prevPhase, st.phase, 0.81f)
                 || Crossed(st.prevPhase, st.phase, 0.84f) || Crossed(st.prevPhase, st.phase, 0.87f)))
                SpawnSalt(map, offHandWrist);

            // Smelt-rod quench: steam erupts off the hot end as the rod dips into the bath.
            if (tool.quenchEvery > 0 && fx && speed > 0f
                && (st.cycleCount % tool.quenchEvery) == tool.quenchEvery - 1
                && Crossed(st.prevPhase, st.phase, 0.34f))
                SpawnQuench(tool, map, tip, grip, dir);

            // Paintbrush dip: a drip or two on the way back from the pot.
            if (sideSpecial && tool.sidePropMode == "dip" && fx && tool.BitFleck != null
                && (Crossed(st.prevPhase, st.phase, 0.62f) || Crossed(st.prevPhase, st.phase, 0.74f)))
            {
                Vector3 dp = tip; dp.y = 0f;
                if (dp.ToIntVec3().ShouldSpawnMotesAt(map))
                {
                    FleckCreationData d = FleckMaker.GetDataStatic(dp, map, tool.BitFleck, Rand.Range(0.25f, 0.4f));
                    d.velocityAngle = 180f;
                    d.velocitySpeed = 0.25f;
                    map.flecks.CreateFleck(d);
                }
            }

            // Extinguisher pin-pull: one tiny pin flicked away as the job starts.
            if (tool.swingStyle == SwingStyle.Spray && !st.pinPulled
                && animTime - st.jobStartRealTime >= 0.45f)
            {
                st.pinPulled = true;
                if (fx) SpawnPin(map, tip, dir);
            }

            // Show Me Your Hands integration: draw the pawn's hands gripping the haft. For the
            // Load style, hide them during the seat-and-grab gap so they don't hang in mid-air.
            bool loadGap = tool.swingStyle == SwingStyle.Load && LoadAlpha(st.phase) < 0.35f;
            // Only humanlikes get hands on the tool. Mechs/robots (e.g. a cleaning bot sweeping)
            // have no hands, so we draw just the tool with nothing gripping it.
            bool humanlike = pawn.RaceProps != null && pawn.RaceProps.Humanlike;
            // noHands suppresses grip hands outright; noHandsBabyOnly only suppresses them on the
            // Baby body type (tiny), so a CHILD holding the same toy still gets hands + forearms.
            bool suppressHands = tool.noHands
                || (tool.noHandsBabyOnly && pawn.story?.bodyType?.defName == "Baby");
            if (JobEffectsSettings.HandsOnTools && !loadGap && humanlike && !suppressHands)
                DrawHands(pawn, tool, grip, tip, drawLoc, dir, st.phase, dir.x < 0f);

            // Hot tip glow / tool glint. tipGlowOffset maps the spawn onto the authored hot end
            // (rod glow centre / torch flame heart) instead of the geometric sprite tip.
            if (fx && tool.TipGlow != null && speed > 0f && tool.tipGlowInterval > 0.0001f)
            {
                st.glowAccum += animDelta;
                if (st.glowAccum >= tool.tipGlowInterval)
                {
                    st.glowAccum = 0f;
                    Vector3 glowPos = TipSpacePos(tool, tip, grip, dir,
                        NSOffset(tool, pawn.Rotation, tool.tipGlowOffset, tool.tipGlowOffsetNorthSouth));
                    if (glowPos.ToIntVec3().ShouldSpawnMotesAt(map))
                    {
                        FleckMaker.Static(glowPos, map, tool.TipGlow, tool.tipGlowScale);
                        // Rising embers off the hot tip. tipEmberChance/tipEmberCount tune the
                        // cadence: smelt rod keeps the old occasional single ember (0.4 / 1), the
                        // welder runs a constant shower (1.0 / several) straight off the electrode.
                        if (tool.TipEmber != null && Rand.Chance(tool.tipEmberChance))
                        {
                            int n = tool.tipEmberCount;
                            for (int ei = 0; ei < n; ei++)
                            {
                                FleckCreationData e = FleckMaker.GetDataStatic(glowPos, map, tool.TipEmber, Rand.Range(0.5f, 0.9f));
                                e.velocityAngle = Rand.Range(-45f, 55f);   // near-vertical, rising fan
                                e.velocitySpeed = Rand.Range(0.6f, 1.6f);
                                map.flecks.CreateFleck(e);
                            }
                        }
                    }
                }
            }

            // Head emote (steam / fume).
            if (fx && tool.HeadFleck != null && speed > 0f && tool.headInterval > 0.0001f)
            {
                st.headAccum += animDelta;
                if (st.headAccum >= tool.headInterval)
                {
                    st.headAccum = 0f;
                    EmitHead(tool, map, drawLoc);
                }
            }

            // Acid pour: while the drug-lab flask is tipped over to empty, dribble acid droplets
            // out of the (now-low) mouth and curl a faint blue acid vapor up off the pour. Gated
            // on the flask being meaningfully tilted (tiltFrac), so nothing leaks while it's held
            // upright at the start or after it has righted itself. tiltFrac is recomputed here from
            // the same live pourProgress + pourPeak the pose uses, so the stream tracks the visual
            // tip exactly. While untilted the accumulator is primed so the first drip falls the
            // instant the mouth points down.
            if (fx && tool.holdPour && speed > 0f && (tool.PourDripFleck != null || tool.PourSmokeFleck != null))
            {
                float peakP = Mathf.Clamp(tool.pourPeak, 0.05f, 0.95f);
                float progP = pourProgress >= 0f ? pourProgress : 0f;
                float halfP = progP < peakP ? progP / peakP : (1f - progP) / (1f - peakP);
                float tiltFrac = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(halfP));
                if (tiltFrac > 0.4f)
                {
                    st.pourAccum += animDelta;
                    if (st.pourAccum >= tool.pourEmitInterval)
                    {
                        st.pourAccum = 0f;
                        EmitPour(tool, map, tip, tiltFrac);
                    }
                }
                else st.pourAccum = tool.pourEmitInterval;   // primed: fire as soon as it tips over
            }

        }

        // ---- Idle holster: the last tool rests at the hip, fading out over HolsterFade seconds ----
        // Returns true when a holster sprite was actually drawn (false for Scratch poses / kits with
        // no loadable material) so the caller can gate the foley on a tool the player can SEE.
        // Per-body-type hip-WIDTH factor for the holster: how far out the belt line sits versus a
        // Male — the build every per-tool idleHipOffset value was tuned on. This is pure SHAPE; the
        // overall body SIZE (children, Biotech body-size genes, HAR, texture-resize mods) is folded
        // in separately by ArmRenderer.BodyScaleFor. So a stock adult Male is exactly {1.0, 1.0} and
        // NONE of the existing per-tool calibration moves — only off-Male builds get corrected.
        private static float HipFactor(Pawn pawn)
        {
            switch (pawn?.story?.bodyType?.defName)
            {
                case "Hulk":   return 1.55f;   // broad torso, belt line pushed well out
                case "Fat":    return 1.70f;   // widest silhouette at the belly / hips
                case "Female": return 1.05f;   // narrow shoulders but flared hips ≈ male belt, a hair wider
                case "Thin":   return 0.80f;   // narrowest adult build, belt pulled in
                case "Child":  return 0.95f;   // proportionally small (overall size handled by BodyScaleFor)
                case "Baby":   return 0.85f;
                default:       return 1.0f;    // Male / default — the calibration baseline
            }
        }

        private static bool DrawHolster(Vector3 drawLoc, JobToolDef tool, Vector3 dir, float idleTimer, Pawn pawn)
        {
            // Scratch "tools" have no visible item — never draw their sprite at the hip.
            if (tool.swingStyle == SwingStyle.Scratch) return false;
            float alpha = Mathf.Clamp01(1f - (idleTimer - HolsterHold) / HolsterFade);

            // Depth: north (back to camera) tucks the belted tool UNDER the body (vanilla north-equip
            // depth, layer -10, + the under-body queue below) so it sits BEHIND the back. For every
            // other facing, when "tools share the colonist's depth" is on we park it in the Pawn band
            // at the vanilla held-weapon depth (layer 90) so the belted tool occludes — and is occluded
            // — exactly like the colonist (e.g. it hides behind a tree canopy with the body). The
            // full-alpha rest uses the crisp depth-writing Cutout (Y-sorted); the brief fade-out swaps
            // to the Transparent material, which ZTests against that same Y so it occludes too. With
            // the setting off, it floats at MoteOverhead like before (always on top).
            // Body-north tucks the belted tool UNDER the body (vanilla north-equip depth + under-body
            // queue, so the Transparent fade is buried too); every other facing sorts it in the Pawn
            // band at the vanilla weapon depth (Cutout, world-sorted like the active tool) — or floats
            // at MoteOverhead when "share depth" is off. Body-only (no draw-direction split).
            bool north = pawn != null && pawn.Rotation == Rot4.North;
            float holsterY;
            if (north)
                holsterY = AltitudeLayer.Pawn.AltitudeFor() + PawnRenderUtility.AltitudeForLayer(-10f);
            else if (tool.alwaysInFront)
                holsterY = AltitudeLayer.MoteOverhead.AltitudeFor();   // always-in-front tools float over everything off-north too
            else if (JobEffectsSettings.ToolsMatchPawnDepth)
                holsterY = AltitudeLayer.Pawn.AltitudeFor() + PawnRenderUtility.AltitudeForLayer(90f);
            else
                holsterY = AltitudeLayer.MoteOverhead.AltitudeFor();
            Material mat;
            if (alpha >= 0.999f)
            {
                mat = tool.Material;   // Cutout
                if (mat == null) return false;
            }
            else
            {
                Material baseMat = tool.MaterialTransparent;
                if (baseMat == null) return false;
                mat = FadedMaterialPool.FadedVersionOf(baseMat, alpha);
                if (mat == null) return false;
            }

            // Pick the hip from the last work facing, but everything below is SCREEN-relative
            // (world X = screen right, world Z = screen up) so the tool lands on the belt no
            // matter which way the finished job was oriented.
            bool faceLeft = dir.x < 0f;
            float side = faceLeft ? -1f : 1f;
            bool useFlip = faceLeft ^ tool.flipHead;
            Mesh mesh = useFlip ? MeshPool.plane10Flip : MeshPool.plane10;

            // Body-type fit: scale the hip reach by this build's belt WIDTH (HipFactor) and the
            // whole pose — hip reach, belt drop, tool size — by the pawn's rendered body SIZE
            // (BodyScaleFor). A stock Male is {1.0, 1.0}, so this is a no-op on the calibration
            // build and a correction on every other body: the belt sits on the actual hip of a
            // Hulk/Fat instead of over the gut, hugs the narrow waist of a Thin/Female, and shrinks
            // onto a child rather than dwarfing them.
            float bodyScale = ArmRenderer.BodyScaleFor(pawn);
            float hipFac = HipFactor(pawn);

            // Tuck the tool through the belt: out to one hip (screen X) and down to the waist
            // line (screen -Z). The grip is pinned near the belt and the head hangs down the leg.
            Vector3 pos = drawLoc;
            pos.x += tool.idleHipOffset * side * hipFac * bodyScale;
            pos.z += tool.idleBeltDrop * bodyScale;
            // Self-contained depth (computed above): the colonist's own band when matching depth,
            // MoteOverhead when not. No longer reads the ambient currentToolY, so calling this from
            // the active brow-wipe pose doesn't disturb that animation's own altitude.
            pos.y = holsterY;

            // Screen-relative tilt: head points down-and-outward like a sheathed blade. Mirror
            // the lean across the chosen hip so a left-hip tool hangs down-left, right hip down-right.
            float angle = tool.idleAngle * side;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            float s = tool.scale * tool.idleScale * bodyScale;
            Matrix4x4 m = Matrix4x4.Translate(pos)
                * Matrix4x4.Rotate(rot)
                * Matrix4x4.Scale(new Vector3(s, 1f, s))
                * Matrix4x4.Translate(new Vector3(0f, 0f, tool.idleGripAnchor));
            // North: also drop the material onto the under-body render queue. Queue dominance is the
            // only thing that can pull a sprite beneath the body sprite — altitude alone can't,
            // especially for the Transparent faded variant (queue 3000) used during the fade-out.
            Emit(mesh, m, north ? QueueAdjustTo(mat, UnderBodyQueueFor(pawn)) : mat, 0);
            return true;
        }

        // Generic off-hand override for this draw (mallet handle, nail pinch, tongs, brace hand,
        // paint pot, dustpan, pin pull, salt pinch). Consumed by DrawHands: when set, the off
        // hand leaves the haft and grips/presses here instead.
        private static bool offHandWristValid;
        private static Vector3 offHandWrist;

        // DRAW-ONLY tool translation applied this frame (JobToolDef.strikeLift, resolved to world).
        // The drawn sprite + hands include it; the caller subtracts it before SpawnImpact so the
        // burst stays anchored to the un-shifted strike pose. Reset at the top of every DrawTool.
        private static Vector3 strikeDrawShift;

        // beadOnTarget reach for this draw, set per-pawn in DrawActive (-1 = use the def's reach).
        private static float beadReachOverride = -1f;

        // Live 0..1 progress of the active job toil for this draw, set per-pawn in DrawActive
        // (-1 = not applicable / no depletion frames). Consumed by the Hold-frame block to pace a
        // tool's depletionTexPaths so the last frame lands as the job completes.
        private static float depletionProgress = -1f;

        // Live 0..1 progress of the active DoBill, set per-pawn in DrawActive (-1 = not a holdPour
        // tool / no bill). Consumed by the Hold holdPour block to time the flask's pour-tilt and its
        // full->empty sprite swap so it inverts at the halfway mark and rights itself empty at the end.
        private static float pourProgress = -1f;

        // Progress (0..1) of the active job's current toil, from ticksLeftThisToil relative to the
        // largest value seen since the tool activated (captured in st.maxToilTicks). Generic: works
        // for any fixed-duration wait toil (e.g. TendPatient) without hard-coding the duration
        // formula. Returns 0 while the toil's countdown hasn't been established (sentinel 99999) so
        // the bundle reads full during the opening; returns 1 once the toil has run out.
        private static float ComputeToilProgress(Pawn pawn, PawnToolState st)
        {
            Verse.AI.JobDriver driver = pawn.jobs?.curDriver;
            if (driver == null) return -1f;
            int tl = driver.ticksLeftThisToil;
            // 99999 is the JobDriver sentinel for "no countdown set"; <=0 means the toil finished.
            if (tl >= 99999 || tl < 0) return st.maxToilTicks > 0 ? 1f : 0f;
            if (tl == 0) return st.maxToilTicks > 0 ? 1f : 0f;
            if (tl > st.maxToilTicks) st.maxToilTicks = tl;
            if (st.maxToilTicks <= 0) return 0f;
            return Mathf.Clamp01(1f - (float)tl / st.maxToilTicks);
        }

        private static void DrawTool(JobToolDef tool, Vector3 drawLoc, Vector3 dir, Rot4 bodyFacing, float phase, float sprayKick, float jobStartRealTime, float jitterAng, float jitterLat, int cycle, float extraSwing, out Vector3 tip, out Vector3 grip)
        {
            tip = drawLoc;
            grip = drawLoc;
            offHandWristValid = false;
            strikeDrawShift = Vector3.zero;
            Material mat = QueueAdjust(tool.Material);
            // Periodic alternate sprite (scalpel <-> forceps): the last altFor of every altEvery
            // cycles draw the alternate texture — reads as the surgeon switching instruments.
            if (tool.altEvery > 0 && tool.AltMaterial != null
                && (cycle % tool.altEvery) >= tool.altEvery - tool.altFor)
                mat = QueueAdjust(tool.AltMaterial);
            // Strike sprite (shears snip): swap to the CLOSED frame for a short window around the
            // strike so the blades read as open (texPath) on the windup and snapping shut on the
            // cut. Phase-synced WITHIN one swing (vs the cycle-counted alt swap above) and wrap-safe
            // around whatever the swing's strike phase is. Closed a touch BEFORE through a bit AFTER
            // the strike so the snip lands together with the JE_ShearSnip sound / wool-tuft flecks.
            if (tool.StrikeMaterial != null)
            {
                const float snipLead = 0.12f;   // close this far (phase) before the strike
                const float snipHold = 0.18f;   // stay shut this far after it
                float fromStrike = Mathf.Repeat(phase - StrikePhaseFor(tool), 1f);
                if (fromStrike <= snipHold || fromStrike >= 1f - snipLead)
                    mat = QueueAdjust(tool.StrikeMaterial);
            }
            // North/South override (welder straight view): front-on / back-on facings swap to the
            // dedicated art; east/west profile keeps texPath. Keyed off the BODY facing (clean
            // cardinal), so diagonal bead re-aim never flips it. More specific than alt, so it wins.
            if (!tool.texPathNorthSouth.NullOrEmpty()
                && (bodyFacing == Rot4.North || bodyFacing == Rot4.South))
                mat = QueueAdjust(tool.NorthSouthMaterial);
            // Scratch draws no tool sprite (research / book-writing "thinking" pose) — it only
            // positions the hand against the head — so it must run even with no material.
            if (mat == null && tool.swingStyle != SwingStyle.Scratch) return;

            float baseAngle = dir.AngleFlat();
            bool faceLeft = dir.x < 0f;
            float side = faceLeft ? -1f : 1f;
            bool useFlip = faceLeft ^ tool.flipHead;
            // Saw blades have their teeth on one edge; when working east/west the default mirror
            // leaves the teeth pointing UP. Flip across the haft axis so the cutting edge faces
            // down whenever the saw is horizontal on screen.
            if ((tool.swingStyle == SwingStyle.Saw || tool.flipEdgeProfile) && Mathf.Abs(dir.x) >= Mathf.Abs(dir.z))
                useFlip = !useFlip;
            Mesh mesh = useFlip ? MeshPool.plane10Flip : MeshPool.plane10;

            // Hold: tool is held stationary in front of the pawn; optional frame-based animation.
            if (tool.swingStyle == SwingStyle.Hold)
            {
                // Offer pose (feeding bowl, taming): both hands hold a wide, low bowl OUT in front
                // of the pawn, extended toward the animal (the work target). A slow coaxing cycle
                // eases the bowl a little further out toward the animal and draws it back, dwelling
                // a beat at full presentation, with a gentle bob — as if enticing the animal to
                // approach and eat. Held SCREEN-UPRIGHT (the bowl never spins to point at the
                // target); a faint sway adds life. Scoped to the bowl; everything else falls
                // through to the generic centred Hold draw below.
                if (tool.holdOffer)
                {
                    bool fnorthO = dir.z > 0.45f;
                    float bsO = tool.scale;

                    // Coaxing cycle: ease the bowl out toward the animal and back, dwelling a beat
                    // at full extension ("here, come eat") and a beat drawn in. Runs off animTime
                    // so it is constant-1x and freezes on pause. offer in [0,1].
                    float oCycle = Mathf.Max(0.5f, tool.period);
                    float oPh = (animTime / oCycle) % 1f;
                    const float oOut = 0.30f, oOutHold = 0.22f, oIn = 0.30f;   // remaining 0.18 = drawn-in hold
                    float offer;
                    if (oPh < oOut) offer = oPh / oOut;
                    else if (oPh < oOut + oOutHold) offer = 1f;
                    else if (oPh < oOut + oOutHold + oIn) offer = 1f - (oPh - oOut - oOutHold) / oIn;
                    else offer = 0f;
                    offer = Mathf.SmoothStep(0f, 1f, offer);   // ease in/out so the present isn't robotic

                    float bobO = Mathf.Sin(animTime * 1.7f) * 0.012f * bsO;

                    // Held out in front at waist/chest height, extended toward the animal; the
                    // coaxing cycle adds extra reach toward the target.
                    float fwdO = tool.reach * (fnorthO ? 0.45f : 0.95f) + offer * tool.offerReach;
                    Vector3 holdO = drawLoc + dir * fwdO;
                    holdO.z += bsO * (fnorthO ? -0.05f : 0.04f) + tool.holdRaise + bobO;
                    holdO.y = currentToolY;

                    float wobO = Mathf.Sin(animTime * 1.4f) * 1.6f;   // gentle screen-upright sway only
                    Quaternion hrotO = Quaternion.AngleAxis(wobO + tool.holdAngleOffset * side, Vector3.up);

                    Matrix4x4 hmO = Matrix4x4.Translate(holdO)
                        * Matrix4x4.Rotate(hrotO)
                        * Matrix4x4.Scale(new Vector3(bsO, 1f, bsO));
                    Emit(mesh, hmO, mat, 0);

                    // Both hands cup the bowl's lower side edges (wide grip corners).
                    grip = holdO + hrotO * new Vector3(-bsO * tool.bookGripHalfWidth, 0f, -bsO * tool.bookGripDrop); grip.y = 0f;
                    tip  = holdO + hrotO * new Vector3( bsO * tool.bookGripHalfWidth, 0f, -bsO * tool.bookGripDrop); tip.y  = 0f;
                    return;
                }

                // Bottle pose (baby bottle, BottleFeedBaby): the carer cradles the baby (CARRIED, so
                // the work target rides on the carrier and the work dir is unreliable — anchor to the
                // clean BODY facing instead) and tips the bottle nipple-down toward the cradle with a
                // gentle feeding rock. One hand holds the bottle body; the quad pivots about
                // pourGripFrac up the sprite so the tilt swings the nipple down, not the hand.
                if (tool.holdBottle)
                {
                    Vector3 fvecB = bodyFacing.FacingCell.ToVector3(); fvecB.y = 0f;
                    if (fvecB.sqrMagnitude < 0.01f) fvecB = new Vector3(0f, 0f, -1f);
                    fvecB = fvecB.normalized;
                    bool bnorthB = bodyFacing == Rot4.North;
                    float sideB = bodyFacing == Rot4.West ? -1f : 1f;   // tilt toward the cradle; W mirrors
                    Mesh bmesh = bodyFacing == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10;
                    float bsB = tool.scale;

                    float bobB = Mathf.Sin(animTime * 1.5f) * 0.012f * bsB;
                    float wobB = Mathf.Sin(animTime * 1.1f + 0.4f) * 1.2f;

                    // Held forward (toward the cradled baby in front of the body) and low.
                    float fwdB = tool.reach * (bnorthB ? 0.45f : 0.95f);
                    Vector3 holdB = new Vector3(drawLoc.x, currentToolY, drawLoc.z) + fvecB * fwdB;
                    holdB.z += bsB * (bnorthB ? -0.10f : -0.04f) + tool.holdRaise + bobB;

                    Quaternion hrotB = Quaternion.AngleAxis(wobB + tool.holdAngleOffset * sideB, Vector3.up);
                    Matrix4x4 hmB = Matrix4x4.Translate(holdB)
                        * Matrix4x4.Rotate(hrotB)
                        * Matrix4x4.Translate(new Vector3(0f, 0f, (0.5f - tool.pourGripFrac) * bsB))
                        * Matrix4x4.Scale(new Vector3(bsB, 1f, bsB));
                    Emit(bmesh, hmB, mat, 0);

                    Vector3 axisUpB = hrotB * new Vector3(0f, 0f, 1f);
                    grip = holdB - axisUpB * (bsB * 0.06f); grip.y = 0f;
                    tip  = holdB + axisUpB * (bsB * 0.22f); tip.y  = 0f;
                    return;
                }

                // Toy pose (baby holding a toy, BabyPlay). Two cute motions chosen by toyMotion:
                //   "shake"  -> a rattle waved in quick swelling bursts, pivoting about its handle
                //               (sprite bottom) so the star-ball head whips side to side, then a rest.
                //   "cuddle" -> a plush hugged to the chest with a soft bob + slow rock + squash.
                // Eased toward the playmate (work dir, valid — the baby is spawned during play, not
                // carried). noHands keeps it a clean animated item on a tiny baby.
                if (tool.holdToy)
                {
                    bool fnorthT = dir.z > 0.45f;
                    // Body-scale the whole toy pose (size + hold offsets) so a child holds a child-sized
                    // toy that matches the body-scaled hands + forearms; no-op (1.0) on a stock adult.
                    float bscT = currentBodyScale;
                    float bsT = tool.scale * bscT;
                    float fwdT = tool.reach * (fnorthT ? 0.45f : 0.80f) * bscT;
                    Vector3 holdT = drawLoc + dir * fwdT;
                    holdT.x += side * tool.holdLateral * bscT;
                    holdT.y = currentToolY;

                    if (tool.toyMotion == "cuddle")
                    {
                        float bob = Mathf.Sin(animTime * 2.4f) * 0.03f * bsT;
                        float rock = Mathf.Sin(animTime * 1.7f) * 8f;
                        float squash = 1f + Mathf.Sin(animTime * 2.4f) * 0.03f;
                        holdT.z += bsT * 0.04f + tool.holdRaise * bscT + bob;
                        Quaternion hrotT = Quaternion.AngleAxis(rock + tool.holdAngleOffset * side, Vector3.up);
                        Matrix4x4 hmT = Matrix4x4.Translate(holdT)
                            * Matrix4x4.Rotate(hrotT)
                            * Matrix4x4.Scale(new Vector3(bsT, 1f, bsT * squash));
                        Emit(mesh, hmT, mat, 0);
                        grip = holdT + hrotT * new Vector3(-bsT * tool.bookGripHalfWidth, 0f, -bsT * tool.bookGripDrop); grip.y = 0f;
                        tip  = holdT + hrotT * new Vector3( bsT * tool.bookGripHalfWidth, 0f, -bsT * tool.bookGripDrop); tip.y  = 0f;
                    }
                    else
                    {
                        // Rattle: burst-shake then pause, swelling in/out so it isn't a constant buzz.
                        float cyc = Mathf.Max(0.5f, tool.period);
                        float ph = (animTime / cyc) % 1f;
                        float env = ph < 0.5f ? Mathf.Sin(ph / 0.5f * Mathf.PI) : 0f;
                        float shake = Mathf.Sin(animTime * 34f) * 14f * env;
                        float jit = Mathf.Cos(animTime * 34f) * 0.02f * bsT * env;
                        holdT.x += jit;
                        holdT.z += bsT * 0.06f + tool.holdRaise * bscT + env * 0.02f * bsT;
                        Quaternion hrotT = Quaternion.AngleAxis(shake + tool.holdAngleOffset * side, Vector3.up);
                        Matrix4x4 hmT = Matrix4x4.Translate(holdT)
                            * Matrix4x4.Rotate(hrotT)
                            * Matrix4x4.Translate(new Vector3(0f, 0f, 0.40f * bsT))   // pivot about the handle (sprite bottom)
                            * Matrix4x4.Scale(new Vector3(bsT, 1f, bsT));
                        Emit(mesh, hmT, mat, 0);
                        Vector3 axisUpT = hrotT * new Vector3(0f, 0f, 1f);
                        grip = holdT + axisUpT * (bsT * 0.02f); grip.y = 0f;
                        tip  = holdT + axisUpT * (bsT * 0.20f); tip.y  = 0f;
                    }
                    return;
                }

                // Pour pose (test-tube / flask, drug lab): held upright, then tilted over to empty
                // like a jug as the job runs — fully inverted (mouth down) at the pourPeak fraction
                // of the bill (default 0.72), then righted back to vertical over the shorter
                // remainder (so the right-up reads quicker than the pour). At the inverted peak the FULL
                // sprite (mat) swaps to the EMPTY one (PourEmptyMaterial), so the flask finishes
                // upright and empty. Paced by pourProgress (live DoBill work); upright/full when
                // no progress is available. Scoped to the flask; everything else falls through.
                if (tool.holdPour)
                {
                    float prog = pourProgress >= 0f ? pourProgress : 0f;
                    bool fnorthP = dir.z > 0.45f;
                    float bsP = tool.scale;

                    // Tilt fraction: ease IN to the inverted peak, then ease OUT back to upright.
                    // The peak sits at prog == pourPeak (not a fixed 0.5), so the down-tilt occupies
                    // prog [0, pourPeak] and the right-up occupies prog [pourPeak, 1]. With
                    // pourPeak > 0.5 the pour is long and deliberate while the recovery to upright
                    // is brisk — "pour slow, snap back up." SmoothStep keeps both ends from snapping.
                    float peakP = Mathf.Clamp(tool.pourPeak, 0.05f, 0.95f);
                    float halfP = prog < peakP ? prog / peakP : (1f - prog) / (1f - peakP);
                    float tiltFrac = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(halfP));
                    // Pour toward the work side (east/west work); defaults to screen-right front-on.
                    float pourSide = Mathf.Abs(dir.x) > 0.05f ? Mathf.Sign(dir.x) : 1f;

                    // Subtle life so the pour never reads as a frozen prop: a gentle vertical bob
                    // and lateral sway on the hand, plus a tilt wobble that breathes a little
                    // harder while the flask is actively tipped over (sells the "shaking the last
                    // drops out" feel near the inverted peak).
                    float pourT   = animTime;
                    float pourBob = Mathf.Sin(pourT * 1.8f)        * 0.010f * bsP;
                    float pourSwy = Mathf.Sin(pourT * 1.3f + 0.7f) * 0.008f * bsP;
                    float pourWob = Mathf.Sin(pourT * 3.2f) * (1.0f + 2.2f * tiltFrac); // deg
                    // Lower the whole flask as it tips over to pour, raising it back up once it's
                    // righted — tracks tiltFrac, so it bottoms out at the inverted peak (prog 0.5).
                    float pourDip = tiltFrac * 0.13f * bsP;

                    float pourAng = pourSide * tool.pourMaxTilt * tiltFrac + pourWob;

                    // Empty the flask at the inverted peak: full up to pourPeak, empty after.
                    Material pourMat = (prog >= peakP && tool.PourEmptyMaterial != null)
                        ? QueueAdjust(tool.PourEmptyMaterial) : mat;

                    // Held in front at chest height; tucked close when back-to-camera.
                    float fwdP = tool.reach * (fnorthP ? 0.30f : 0.70f);
                    Vector3 holdP = drawLoc + dir * fwdP;
                    holdP.x += pourSide * tool.holdLateral + pourSwy;
                    holdP.z += bsP * (fnorthP ? -0.05f : 0.08f) + tool.holdRaise + pourBob - pourDip;
                    holdP.y = currentToolY;

                    Quaternion protP = Quaternion.AngleAxis(pourAng, Vector3.up);
                    // Pivot about the gripping hand (pourGripFrac up the sprite) so the tube
                    // rotates about the wrist and the mouth swings down to pour.
                    Matrix4x4 hmP = Matrix4x4.Translate(holdP)
                        * Matrix4x4.Rotate(protP)
                        * Matrix4x4.Translate(new Vector3(0f, 0f, (0.5f - tool.pourGripFrac) * bsP))
                        * Matrix4x4.Scale(new Vector3(bsP, 1f, bsP));
                    Emit(mesh, hmP, pourMat, 0);

                    // One hand on the flask at the pivot; the axis runs up toward the mouth.
                    Vector3 axisUpP = protP * new Vector3(0f, 0f, 1f);
                    grip = holdP - axisUpP * (bsP * 0.05f); grip.y = 0f;
                    tip  = holdP + axisUpP * (bsP * 0.30f); tip.y  = 0f;
                    return;
                }

                // Mouthpiece-anchored vertical instrument (recorder/flute): pin the painted
                // MOUTHPIECE to the pawn's lips and hang the barrel DOWN and OUT, so the colonist
                // reads as blowing into it instead of holding a pole over the face. The tilt
                // pivots about the mouthpiece, so the bell swings off the body while the mouthpiece
                // stays on the lips. Scoped to the recorder; everything else falls through to the
                // generic centred Hold draw below.
                if (tool.holdMouthAnchor)
                {
                    // Pin to the rendered FACE via the clean cardinal BODY facing -- NOT the work
                    // dir (which points pawn->instrument and can be diagonal when the pawn stands
                    // off-axis from the piano/harp; that was dragging the recorder sideways+down
                    // off the mouth). Mirrors the welding-mask face anchor.
                    Vector3 fvec = bodyFacing.FacingCell.ToVector3(); fvec.y = 0f;
                    if (fvec.sqrMagnitude < 0.01f) fvec = new Vector3(0f, 0f, -1f);
                    fvec = fvec.normalized;
                    Vector3 fright = Vector3.Cross(Vector3.up, fvec).normalized;   // pawn's right (screen)
                    bool fnorth = bodyFacing == Rot4.North;
                    bool fprofile = bodyFacing.IsHorizontal;
                    float bsM = tool.scale;
                    // Mouth = lower-centre of the face. Net height ~0.27 above body centre (the
                    // welding-mask face calibration) so the mouthpiece lands on the lips; a small
                    // forward nudge toward the facing dir (bigger in profile, where the lips sit at
                    // the front edge of the head). Tucked low+close when the back is to camera.
                    float fwdPush = fprofile ? 0.13f : 0.03f;
                    Vector3 mouth = new Vector3(drawLoc.x, currentToolY, drawLoc.z);
                    mouth += fvec * fwdPush;
                    mouth += fright * tool.holdLateral;
                    mouth.z += (fnorth ? 0.20f : fprofile ? 0.28f : 0.30f) + tool.holdRaise;

                    float wobM = Mathf.Sin(animTime * 1.5f) * 1.5f;
                    // Lean the bell toward the playing side: screen-right for front/back, forward
                    // (down-and-into the facing) in profile. West mirrors so it hangs down-forward.
                    float leanSign = bodyFacing == Rot4.West ? -1f : 1f;
                    Quaternion hrotM = Quaternion.AngleAxis(wobM + tool.holdAngleOffset * leanSign, Vector3.up);

                    // Pivot the quad about the PAINTED MOUTHPIECE (~14% down the 256-px canvas, NOT
                    // the canvas top edge -- the old -0.5 left the mouthpiece floating ~0.085 below
                    // the lips) so the tilt swings the bell, not the lips: scale -> drop the
                    // mouthpiece onto the origin -> rotate -> place at the mouth.
                    Matrix4x4 hmM = Matrix4x4.Translate(mouth)
                        * Matrix4x4.Rotate(hrotM)
                        * Matrix4x4.Translate(new Vector3(0f, 0f, -0.359f * bsM))
                        * Matrix4x4.Scale(new Vector3(bsM, 1f, bsM));
                    Emit(mesh, hmM, mat, 0);

                    // Both fists stack DOWN the barrel from just below the mouthpiece toward the
                    // bell (grip = low, tip = high; handPosA/B interpolate up it).
                    Vector3 axisDown = hrotM * new Vector3(0f, 0f, -1f);
                    grip = mouth + axisDown * (bsM * 0.62f); grip.y = 0f;   // low (toward the bell)
                    tip  = mouth + axisDown * (bsM * 0.16f); tip.y  = 0f;   // high (just below the mouthpiece)
                    return;
                }

                // Eye-anchored vertical instrument (spyglass): pin the BOTTOM of the sprite (the
                // small eyepiece end) to the pawn's eye and raise the barrel UP toward the sky, so
                // the colonist reads as squinting through it at the stars rather than holding a
                // pole over the face. The tilt pivots about the eyepiece, so the big objective end
                // swings up/out while the eyepiece stays on the eye. One hand grips the eyepiece.
                if (tool.holdEyeAnchor)
                {
                    bool fnorth = dir.z > 0.45f;
                    float bsE = tool.scale;

                    // Stargazing rhythm: look through the glass for a good while, briefly lower it
                    // to rest the arm, then raise it back up to look again -- and loop. lower in
                    // [0,1]: 0 = pinned to the eye looking up, 1 = dropped to the side.
                    float cyclePeriod = 9f;
                    float tcyc = animTime % cyclePeriod;
                    const float upHold   = 6f;    // seconds spent looking through it
                    const float trans    = 0.6f;  // raise/lower transition time
                    const float downHold = 1f;    // seconds held lowered
                    float lower;
                    if (tcyc < upHold)
                        lower = 0f;
                    else if (tcyc < upHold + trans)
                        lower = Mathf.SmoothStep(0f, 1f, (tcyc - upHold) / trans);
                    else if (tcyc < upHold + trans + downHold)
                        lower = 1f;
                    else
                        lower = Mathf.SmoothStep(1f, 0f, (tcyc - upHold - trans - downHold) / trans);

                    // Eye anchor: a little forward of the face toward the look direction, raised to
                    // eye height, nudged out to the looking side. Tucked low + close when the
                    // pawn's back is to camera. As it lowers, the eyepiece drops to chest level
                    // and eases forward/away from the face.
                    Vector3 eye = drawLoc + dir * (tool.reach * (fnorth ? 0.20f : 0.45f));
                    eye.x += side * tool.holdLateral;
                    // Eyepiece sits ON the eye, not the chin: ~0.20 above body centre for S/E/W
                    // lands on the eye band of the head sprite; north tucks lower so the barrel
                    // rises off the back of the head. Drops ~0.42 when lowered.
                    eye.z += (fnorth ? 0.13f : 0.20f) + tool.holdRaise - lower * 0.42f;
                    eye += dir * (lower * tool.reach * 0.6f);
                    eye.y = currentToolY;

                    float wobE = Mathf.Sin(animTime * 1.5f) * 1.2f;
                    // Up-looking angle = holdAngleOffset; lowered the barrel cants down toward
                    // horizontal/in front of the body.
                    float angE = Mathf.Lerp(tool.holdAngleOffset, 110f, lower);
                    Quaternion hrotE = Quaternion.AngleAxis(wobE + angE * side, Vector3.up);

                    // Pivot the quad about its BOTTOM edge (eyepiece) so the tilt swings the
                    // objective end up/out, not the eyepiece: scale -> lift the bottom edge onto
                    // the origin -> rotate -> place at the eye.
                    Matrix4x4 hmE = Matrix4x4.Translate(eye)
                        * Matrix4x4.Rotate(hrotE)
                        * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f * bsE))
                        * Matrix4x4.Scale(new Vector3(bsE, 1f, bsE));
                    Emit(mesh, hmE, mat, 0);

                    // One hand grips the eyepiece (low) end; the axis runs UP the barrel toward the
                    // objective (high). grip = at the eyepiece, tip = near the objective; handPosA
                    // sits the single fist right down on the eyepiece end.
                    Vector3 axisUp = hrotE * new Vector3(0f, 0f, 1f);
                    grip = eye + axisUp * (bsE * 0.06f); grip.y = 0f;   // low (eyepiece, at the eye)
                    tip  = eye + axisUp * (bsE * 0.92f); tip.y  = 0f;   // high (objective, up to sky)
                    return;
                }

                // Fishing rod (Odyssey Fish job): held in BOTH hands out over the water, the long
                // blank rising up-and-out toward the fishing spot (the work target, which the pawn
                // faces). The angler patiently works the lure — a slow continuous sweep of the rod
                // tip up and down — with a quick "set the hook" twitch once per cycle, plus a gentle
                // bob + sway. The whole rod pivots about the REAR (lower) hand at the butt, so the
                // tip is what sweeps. Leans toward the water side (like the spyglass); animTime-
                // driven so it is constant-1x and freezes on pause. Scoped to the rod; everything
                // else falls through to the generic centred Hold draw below.
                if (tool.holdFishing)
                {
                    bool fnorthFi = dir.z > 0.45f;
                    float bsFi = tool.scale;

                    float jCycle = Mathf.Max(0.5f, tool.period);
                    float jPh = (animTime / jCycle) % 1f;
                    // Slow continuous sweep of the rod tip about the base lean: the tip dips toward
                    // the water (angle up) and lifts back (angle down), as if working the lure.
                    float sweep = Mathf.Sin(animTime * (Mathf.PI * 2f / jCycle)) * tool.jigSweep;
                    // Quick "set the hook" twitch near the start of each cycle: a fast lift of the
                    // tip (toward vertical), then a settle back to the working sweep.
                    float twitch;
                    const float tUp = 0.06f, tDown = 0.16f;
                    if (jPh < tUp) twitch = jPh / tUp;
                    else if (jPh < tUp + tDown) twitch = 1f - (jPh - tUp) / tDown;
                    else twitch = 0f;
                    twitch = Mathf.SmoothStep(0f, 1f, twitch);

                    float bobFi = Mathf.Sin(animTime * 1.6f) * 0.010f * bsFi + twitch * 0.03f * bsFi;
                    float swyFi = Mathf.Sin(animTime * 1.2f + 0.5f) * 0.008f * bsFi;

                    // Rear hand (butt) held out toward the water at waist/hip height.
                    float fwdFi = tool.reach * (fnorthFi ? 0.45f : 0.95f);
                    Vector3 holdFi = drawLoc + dir * fwdFi;
                    holdFi.x += side * tool.holdLateral + swyFi;
                    holdFi.z += bsFi * (fnorthFi ? -0.05f : 0.02f) + tool.holdRaise + bobFi;
                    holdFi.y = currentToolY;

                    float wobFi = Mathf.Sin(animTime * 1.4f) * 1.2f;
                    // Tip jerks UP (toward vertical, smaller lean) on the twitch; the sweep slowly
                    // raises/lowers it about the base lean toward the water.
                    float angFi = tool.holdAngleOffset + sweep - twitch * tool.jigTwitch;
                    Quaternion hrotFi = Quaternion.AngleAxis(wobFi + angFi * side, Vector3.up);

                    // Pivot about the rear hand at the butt (~0.40 below the sprite centre) so the
                    // long blank above it sweeps while the grip stays planted at the hand.
                    Matrix4x4 hmFi = Matrix4x4.Translate(holdFi)
                        * Matrix4x4.Rotate(hrotFi)
                        * Matrix4x4.Translate(new Vector3(0f, 0f, 0.40f * bsFi))
                        * Matrix4x4.Scale(new Vector3(bsFi, 1f, bsFi));
                    Emit(mesh, hmFi, mat, 0);

                    // Two hands stacked up the grip end: rear hand at the butt, front hand at the
                    // reel/foregrip just above it. handPosA/handPosB slide them along this span.
                    Vector3 axisUpFi = hrotFi * new Vector3(0f, 0f, 1f);
                    grip = holdFi + axisUpFi * (bsFi * 0.0f);  grip.y = 0f;   // rear hand (butt / pivot)
                    tip  = holdFi + axisUpFi * (bsFi * 0.26f); tip.y  = 0f;   // front hand (reel / foregrip)
                    return;
                }

                // Prayer-beads pose with per-facing art + per-facing hand placement.
                //   SOUTH/NORTH -> symmetric necklace (texPath / mat), a hand on EACH side of the
                //                  strands (grip = upper-left, tip = upper-right, wide span).
                //   EAST/WEST   -> coiled loop (texPathProfile) cupped low in both hands clasped
                //                  near centre (grip/tip pulled in close, narrow span).
                if (tool.holdRosary)
                {
                    bool fnorthR = dir.z > 0.45f;
                    bool rprofile = Mathf.Abs(dir.x) >= Mathf.Abs(dir.z);   // facing east/west
                    Material rmat = rprofile ? QueueAdjust(tool.ProfileMaterial) : mat;
                    if (rmat == null) rmat = mat;
                    float rfwd = tool.reach * (fnorthR ? 0.30f : 0.70f);
                    Vector3 rhold = drawLoc + dir * rfwd;
                    rhold.z += tool.scale * (fnorthR ? -0.05f : 0.08f) + tool.holdRaise;

                    // Raise / lower the beads with a contemplative dwell at the top and bottom:
                    // lift from the clasped-belly rest height up toward the chest, hold in prayer,
                    // lower back down, hold, repeat. Runs off animTime so it is constant-1x and
                    // freezes on pause. lift in [0,1]: 0 = belly rest, 1 = raised.
                    float rCycle = Mathf.Max(0.5f, tool.period * 2f);   // full up+hold+down+hold loop (wall-clock seconds)
                    float rPh = (animTime / rCycle) % 1f;
                    const float rRise = 0.28f, rTopHold = 0.22f, rFall = 0.28f;   // remaining 0.22 = bottom hold
                    float lift;
                    if (rPh < rRise) lift = rPh / rRise;
                    else if (rPh < rRise + rTopHold) lift = 1f;
                    else if (rPh < rRise + rTopHold + rFall) lift = 1f - (rPh - rRise - rTopHold) / rFall;
                    else lift = 0f;
                    lift = Mathf.SmoothStep(0f, 1f, lift);   // ease in/out so the raise/lower isn't robotic
                    rhold.z += lift * (tool.scale * 0.9f);   // travel ~0.9 scale-units belly -> chest

                    rhold.y = currentToolY;

                    float wobR = Mathf.Sin(animTime * 1.5f) * 1.5f;
                    Quaternion hrotR = Quaternion.AngleAxis(wobR + tool.holdAngleOffset * side, Vector3.up);
                    float bsR = tool.scale;
                    Matrix4x4 hmR = Matrix4x4.Translate(rhold)
                        * Matrix4x4.Rotate(hrotR)
                        * Matrix4x4.Scale(new Vector3(bsR, 1f, bsR));
                    Emit(mesh, hmR, rmat, 0);

                    if (rprofile)
                    {
                        // Coil: both fists cupped close together, just below the loop's centre.
                        grip = rhold + hrotR * new Vector3(-bsR * 0.14f, 0f, -bsR * 0.06f); grip.y = 0f;
                        tip  = rhold + hrotR * new Vector3( bsR * 0.14f, 0f, -bsR * 0.06f); tip.y  = 0f;
                    }
                    else
                    {
                        // Necklace: a hand on each upper side, holding the two strand ends, with
                        // the beads draping down between them.
                        grip = rhold + hrotR * new Vector3(-bsR * 0.40f, 0f, bsR * 0.20f); grip.y = 0f;
                        tip  = rhold + hrotR * new Vector3( bsR * 0.40f, 0f, bsR * 0.20f); tip.y  = 0f;
                    }
                    return;
                }

                Material frameMat = mat;
                if (tool.frameTexPaths != null && tool.frameTexPaths.Count > 0)
                {
                    float elapsed = animTime - jobStartRealTime;
                    float openDuration = tool.frameTexPaths.Count * tool.frameDuration;
                    bool depleting = tool.depletionTexPaths != null && tool.depletionTexPaths.Count > 0
                        && elapsed >= openDuration && depletionProgress >= 0f;
                    if (depleting)
                    {
                        // Opening bloom is done: hand off to the emptying set, paced by the tend
                        // job's live progress so the final (empty) frame lands as the job finishes.
                        int d = Mathf.FloorToInt(depletionProgress * tool.depletionTexPaths.Count);
                        d = Mathf.Clamp(d, 0, tool.depletionTexPaths.Count - 1);
                        frameMat = tool.DepletionMaterial(d);
                    }
                    else
                    {
                        int frame = Mathf.FloorToInt(elapsed / tool.frameDuration);
                        int count = tool.frameTexPaths.Count;
                        // loopFrames: wrap continuously (a cyclic animation that repeats over and
                        // over) instead of clamping/freezing on the final frame.
                        if (tool.loopFrames)
                            frame = ((frame % count) + count) % count;
                        else
                            frame = Mathf.Clamp(frame, 0, count - 1);
                        frameMat = tool.FrameMaterial(frame);
                    }
                }

                // Frame-animated items (medicine kits, herbal bundle) are authored UPRIGHT — the box
                // top / bouquet tip points up in the texture. They read as the colonist holding an
                // item out in front, so the sprite must stay SCREEN-UPRIGHT at every facing and must
                // NOT rotate to "point" at the patient. Only the work-direction *position* changes
                // between N/E/S/W; orientation is fixed (plus a tiny breathing sway).
                bool facingNorth = dir.z > 0.45f;
                float fwd = tool.reach * (facingNorth ? 0.30f : 0.70f);   // tuck close when back-to-camera
                Vector3 hold = drawLoc + dir * fwd;
                hold.z += tool.scale * (facingNorth ? -0.05f : 0.08f) + tool.holdRaise;    // chest height (low + occluded when north); holdRaise lifts e.g. a spyglass to eye level
                hold.y = currentToolY;

                float wob = Mathf.Sin(animTime * 1.5f) * 1.5f;
                // No baseAngle term -> the item never spins with the work direction; holdAngleOffset
                // (0 for the upright kits) trims any authored lean.
                Quaternion hrot = Quaternion.AngleAxis(wob + tool.holdAngleOffset * side, Vector3.up);   // tilt mirrors with facing (spyglass leans up-and-outward both ways)

                float bs = tool.scale;
                // Default: stretch the frame to fill the square quad. When aspectCorrectFrames is
                // set, keep each frame's native aspect ratio and scale it against the set's largest
                // dimension so bounding-box-cropped frames (varying w/h) don't distort or jitter,
                // and BOTTOM-anchor them (not centre-anchor): pin every frame's content bottom to a
                // common line so the held base stays fixed and the bloom grows upward. Centre-
                // anchoring breaks when later frames carry extra content up top (the kit lid/pouch
                // floating above the leaf fan) — that inflates the bbox upward and shoves the
                // persistent fan down-screen, so it visibly jumps between frames.
                Vector3 quadScale = new Vector3(bs, 1f, bs);         // center-anchored upright quad
                Vector3 frameHold = hold;
                if (tool.aspectCorrectFrames && frameMat != null && frameMat.mainTexture != null)
                {
                    float refS = tool.FrameRefSize();
                    if (refS > 1f)
                    {
                        float hFrac = frameMat.mainTexture.height / refS;
                        quadScale = new Vector3(bs * (frameMat.mainTexture.width / refS), 1f, bs * hFrac);
                        // Shift the (centre-anchored) quad up so its BOTTOM edge lands on the bottom
                        // of a full-size quad (hold.z - 0.5*bs), shared by every frame.
                        frameHold.z -= 0.5f * bs * (1f - hFrac);
                    }
                }
                Matrix4x4 hm = Matrix4x4.Translate(frameHold)
                    * Matrix4x4.Rotate(hrot)
                    * Matrix4x4.Scale(quadScale);
                Emit(mesh, hm, QueueAdjust(frameMat), 0);

                if (tool.holdVertical)
                {
                    // Tall item (recorder, spyglass): the two fists STACK up the item's central
                    // axis — grip = low on the barrel, tip = high — so handPosA/handPosB place the
                    // hands along it. Rotates with hrot so a tilted spyglass keeps the grip on it.
                    grip = hold + hrot * new Vector3(0f, 0f, -bs * 0.38f); grip.y = 0f;  // low (bell / eyepiece end)
                    tip  = hold + hrot * new Vector3(0f, 0f,  bs * 0.34f); tip.y  = 0f;  // high (mouthpiece / objective end)
                }
                else
                {
                    // Wide item (book, medicine kit): hands grip the LOWER corners; handPosA -> the
                    // screen-left corner, handPosB -> the screen-right corner along the bottom edge.
                    grip = hold + hrot * new Vector3(-bs * tool.bookGripHalfWidth, 0f, -bs * tool.bookGripDrop); grip.y = 0f;  // lower-left
                    tip  = hold + hrot * new Vector3( bs * tool.bookGripHalfWidth, 0f, -bs * tool.bookGripDrop); tip.y  = 0f;  // lower-right
                }
                return;
            }

            // Scratch: no tool at all. Most of each cycle the pawn simply works; near the end the
            // hand comes up and RESTS against the side of the head — a thinking pose, not a brow
            // scrub — with only a gentle slow bob, then drops. Nothing is drawn here; we only
            // place the grip→tip axis so DrawHands puts the SMYH fist (and its forearm) at the
            // spot; when the hand is down the axis is degenerate and DrawHands draws nothing.
            if (tool.swingStyle == SwingStyle.Scratch)
            {
                float sc = ScratchAmount(phase);
                if (sc <= 0.001f) { tip = drawLoc; grip = drawLoc; return; }
                float bob = Mathf.Sin(animTime * 3.2f) * 0.012f * sc;      // gentle pondering bob
                Vector3 headPos = drawLoc + new Vector3(side * 0.27f, 0f, 0.18f + 0.12f * sc + bob);
                grip = new Vector3(headPos.x, 0f, headPos.z - 0.02f);
                tip  = new Vector3(headPos.x, 0f, headPos.z + 0.02f);
                return;
            }

            // Read: the notepad is held low (jotting), lifted up toward the face and held there
            // a while (reading), then lowered again — a clean lift / read / put-down loop rather
            // than any circular motion. Screen-upright at every facing, like Hold.
            if (tool.swingStyle == SwingStyle.Read)
            {
                bool facingNorth = dir.z > 0.45f;
                float lift = ReadLift(phase);
                float fwd = tool.reach * (facingNorth ? 0.30f : 0.70f);
                Vector3 hold = drawLoc + dir * fwd;
                hold.z += tool.scale * ((facingNorth ? -0.05f : 0.06f) + lift * 0.30f) + tool.holdRaise;
                hold.y = currentToolY;
                // Raised: a slow scanning sway (eyes running over the page). Lowered: a faster,
                // tighter wobble that reads as the pen scratching out a note.
                float ang = lift > 0.5f ? Mathf.Sin(animTime * 1.8f) * 2.2f
                                        : Mathf.Sin(animTime * 7f) * 1.2f;
                Quaternion hrot = Quaternion.AngleAxis(ang + tool.holdAngleOffset, Vector3.up);
                float bs = tool.scale * (1f + lift * 0.06f);
                Matrix4x4 hm = Matrix4x4.Translate(hold)
                    * Matrix4x4.Rotate(hrot)
                    * Matrix4x4.Scale(new Vector3(bs, 1f, bs));
                Emit(mesh, hm, mat, 0);
                // Hands grip the lower corners, like Hold (corner span configurable per-tool).
                grip = hold + hrot * new Vector3(-bs * tool.bookGripHalfWidth, 0f, -bs * tool.bookGripDrop); grip.y = 0f;
                tip  = hold + hrot * new Vector3( bs * tool.bookGripHalfWidth, 0f, -bs * tool.bookGripDrop); tip.y  = 0f;
                return;
            }

            // Pry: plant the tip in the block and lever the handle around it (pivot at the tip).
            if (tool.swingStyle == SwingStyle.Pry)
            {
                Vector3 plant = drawLoc + dir * (tool.reach + tool.scale * 0.92f + PryInsert(phase) * 0.06f);
                plant.y = currentToolY;
                float pry = PryAngle(tool, phase) * side;
                Quaternion prot = Quaternion.AngleAxis(baseAngle + pry, Vector3.up);
                Matrix4x4 pm = Matrix4x4.Translate(plant)
                    * Matrix4x4.Rotate(prot)
                    * Matrix4x4.Scale(new Vector3(tool.scale, 1f, tool.scale))
                    * Matrix4x4.Translate(new Vector3(0f, 0f, -0.5f));   // head (tip) sits on the pivot
                Emit(mesh, pm, mat, 0);
                tip = plant; tip.y = 0f;
                grip = plant - (prot * Vector3.forward) * tool.scale; grip.y = 0f;
                return;
            }

            // Beat: a flat pad raised high overhead, then smacked straight down flat onto the
            // fire — twice per cycle. The lift reads as an up-the-screen offset + scale "pop"
            // (raised = high/big, slammed = flat on the fire); it does NOT lunge forward onto the
            // flames. A faint rock adds life.
            if (tool.swingStyle == SwingStyle.Beat)
            {
                float ext = BeatExtend(phase);                       // 0 = raised overhead, 1 = slammed flat
                float lift = Mathf.Lerp(1.32f, 0.80f, ext);          // raised = big/near, slammed = flat
                float raise = tool.beatLunge * (1f - ext);           // straight up the screen while overhead
                float rock = tool.beatRock * (1f - ext) * Mathf.Sin(phase * Mathf.PI * 4f) * side;
                Vector3 bhand = drawLoc + dir * (tool.reach + tool.scale * 0.25f);
                bhand.z += raise;                                    // lift up the screen, not forward onto the fire
                bhand.y = currentToolY;
                Quaternion brot = Quaternion.AngleAxis(baseAngle + rock, Vector3.up);
                float bs = tool.scale * lift;
                Matrix4x4 bm = Matrix4x4.Translate(bhand)
                    * Matrix4x4.Rotate(brot)
                    * Matrix4x4.Scale(new Vector3(bs, 1f, bs))
                    * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f));
                Emit(mesh, bm, mat, 0);
                tip = bhand + (brot * Vector3.forward) * bs; tip.y = 0f;
                grip = new Vector3(bhand.x, 0f, bhand.z);
                return;
            }

            // Spray: the extinguisher is held UPRIGHT in front of the pawn — tank hanging down, the
            // funnel/horn up and canted toward the fire. The texture is drawn screen-vertical and
            // only mirrored so the horn faces whichever way the fire is; a gentle lean cants it
            // toward the flames. Both hands grip the tank body stacked along its axis (DrawHands).
            // Each discharge pulse kicks the tank straight back (recoil along -dir) with a brief
            // shudder. White discharge jets from the funnel toward the flames (SpawnSpray).
            if (tool.swingStyle == SwingStyle.Spray)
            {
                float kick = sprayKick;                              // 0 = settled, 1 = full recoil at the beat lunge
                Vector3 dirFlat = dir; dirFlat.y = 0f; dirFlat = dirFlat.normalized;
                Vector3 hold = drawLoc + dirFlat * (tool.reach + tool.scale * 0.10f);  // held out front
                hold.z += tool.scale * 0.16f;                        // raised to chest height
                hold -= dirFlat * kick * 0.10f;                      // recoils straight back off the discharge
                hold.y = currentToolY;
                // Screen-vertical hold: a gentle lean cants the funnel toward the fire; faint sway +
                // a quick recoil shudder on discharge.
                float tilt = side * 8f;
                float wob = Mathf.Sin(phase * Mathf.PI * 2f) * 1.5f + Mathf.Sin(phase * Mathf.PI * 9f) * kick * 4f * side;
                Quaternion srot = Quaternion.AngleAxis(tilt + wob, Vector3.up);
                float bs = tool.scale * (1f + kick * 0.03f);
                Mesh smesh = faceLeft ? MeshPool.plane10 : MeshPool.plane10Flip;  // horn faces the fire
                Matrix4x4 sm = Matrix4x4.Translate(hold)
                    * Matrix4x4.Rotate(srot)
                    * Matrix4x4.Scale(new Vector3(bs, 1f, bs));      // tank centre at sprite centre
                Emit(smesh, sm, mat, 0);
                Vector3 upLocal = srot * Vector3.forward;            // sprite's local up (screen-up) in world
                tip = hold + upLocal * (bs * 0.33f) + dirFlat * (bs * 0.10f); tip.y = 0f;  // funnel mouth (upper, horn side)
                grip = hold - upLocal * (bs * 0.40f); grip.y = 0f;   // tank base (bottom)
                // Pin pull: for the first beat of the job the off hand is up at the valve,
                // yanking the safety pin out and away before the first discharge.
                float jobAge = animTime - jobStartRealTime;
                if (jobAge < 0.70f)
                {
                    float pp = jobAge / 0.70f;
                    float yank = pp < 0.55f ? 0f : EaseOut((pp - 0.55f) / 0.45f);
                    Vector3 pinHand = hold + upLocal * (bs * 0.20f) - dirFlat * (0.05f + yank * 0.24f);
                    pinHand.z += yank * 0.10f;
                    offHandWrist = new Vector3(pinHand.x, 0f, pinHand.z);
                    offHandWristValid = true;
                }
                return;
            }

            // Crank: plant the head on the bolt and rotate the handle around it (wrench/ratchet).
            // The head/tip stays put on the pivot; the handle sweeps a turn, then snaps back to re-grip.
            if (tool.swingStyle == SwingStyle.Crank)
            {
                Vector3 bolt = drawLoc + dir * (tool.reach + tool.scale * 0.5f);
                bolt.y = currentToolY;
                float turn = CrankAngle(phase, tool.crankSweepAngle) * side;
                Quaternion crot = Quaternion.AngleAxis(baseAngle + turn, Vector3.up);
                // Pin the actual jaw THROAT (crankHeadDepth/Lateral) on the pivot — not the sprite's
                // leading edge — so the head stays planted on the bolt instead of orbiting it (which
                // swung the jaw out to the side at the strike pose). Lateral mirrors with facing.
                float headLat = useFlip ? tool.crankHeadLateral : -tool.crankHeadLateral;
                Matrix4x4 cm = Matrix4x4.Translate(bolt)
                    * Matrix4x4.Rotate(crot)
                    * Matrix4x4.Scale(new Vector3(tool.scale, 1f, tool.scale))
                    * Matrix4x4.Translate(new Vector3(headLat, 0f, -tool.crankHeadDepth));   // jaw throat sits on the pivot
                Emit(mesh, cm, mat, 0);
                tip = bolt; tip.y = 0f;
                // Hand grips mid-shaft, between the pinned head and the far jaw.
                float gripDist = (tool.crankHeadDepth + 0.5f) * 0.5f;
                grip = bolt - (crot * Vector3.forward) * (tool.scale * gripDist); grip.y = 0f;
                return;
            }

            // Load: feed shells into the turret one after another. Each cycle a shell appears at
            // the pawn, slides forward and seats into the turret, then fades out (loaded) before
            // the next is grabbed — reads as reloading many rounds rather than a single stab.
            if (tool.swingStyle == SwingStyle.Load)
            {
                Material lbase = tool.MaterialTransparent;
                if (lbase == null) return;
                float adv = LoadAdvance(phase);
                Vector3 shellPos = drawLoc + dir * (tool.reach + tool.stabDistance * adv);
                shellPos.y = currentToolY;
                Quaternion lrot = Quaternion.AngleAxis(baseAngle, Vector3.up);
                Material lmat = FadedMaterialPool.FadedVersionOf(lbase, LoadAlpha(phase));
                if (lmat != null)
                {
                    Matrix4x4 lm = Matrix4x4.Translate(shellPos)
                        * Matrix4x4.Rotate(lrot)
                        * Matrix4x4.Scale(new Vector3(tool.scale, 1f, tool.scale))
                        * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f));
                    Emit(mesh, lm, QueueAdjust(lmat), 0);
                }
                tip = shellPos + (lrot * Vector3.forward) * tool.scale; tip.y = 0f;
                grip = new Vector3(shellPos.x, 0f, shellPos.z);
                return;
            }

            float swing = SwingOffset(tool, phase) * side + extraSwing;

            Vector3 hand = drawLoc + dir * (beadReachOverride > 0f ? beadReachOverride : tool.reach);
            float stirLean = 0f;   // stirVaried: wrist-pivot lean (deg), replaces the generic sway
            bool tipPlantedActive = false;   // tipPlanted: hand recomputed from the planted tip below
            float tipPlantedWob = 0f;        // tipPlanted: this frame's correction angle (deg)
            if (tool.swingStyle == SwingStyle.Stir)
            {
                Vector3 rightPerp = Vector3.Cross(Vector3.up, dir).normalized;
                bool quenchCycle = tool.quenchEvery > 0 && (cycle % tool.quenchEvery) == tool.quenchEvery - 1;
                if (quenchCycle || tool.stirPlunge)
                {
                    // Plunge: push the rod forward and down into the work, HOLD it there for a
                    // beat, then withdraw — a slow, deliberate dip repeated every cycle. On quench
                    // cycles DrawActive also erupts steam off the hot end at full depth.
                    float q = QuenchCurve(phase);
                    hand += dir * (tool.plungeForward * q);
                    hand.z -= tool.plungeDown * q;
                }
                else if (tool.stirFigure8)
                {
                    // Torch wave: a sideways figure-8 (1:2 Lissajous), embers trailing the tip.
                    float th8 = phase * Mathf.PI * 2f;
                    hand += rightPerp * (Mathf.Sin(th8) * tool.stirRadius * 1.4f)
                          + dir * (Mathf.Sin(th8 * 2f) * tool.stirRadius * 0.6f);
                }
                else if (tool.stirVaried)
                {
                    // ---- A convincing cook's stir ----
                    // The spoon pivots about the GRIP like a wrist-stir: the head traces a
                    // foreshortened ellipse in the pot (the lean supplies most of the side-to-
                    // side sweep, a small push/pull supplies the depth), each lap advances
                    // unevenly (scraping the far rim slower, pulling through faster), and the
                    // loop mixes in a rest at the rim, a direction REVERSAL, and a quick
                    // double tap on the pot rim to knock the spoon clean.
                    float laps; float leanMul = 1f; float tapDip = 0f;
                    if (phase < 0.30f)
                        laps = (phase / 0.30f) * 2.5f;                    // 2.5 brisk laps
                    else if (phase < 0.36f)
                    { laps = 2.5f; leanMul = 0.35f; }                     // rest at the rim
                    else if (phase < 0.62f)
                        laps = 2.5f - ((phase - 0.36f) / 0.26f) * 2.0f;   // 2 laps the OTHER way
                    else if (phase < 0.70f)
                    {                                                     // tap-tap on the rim
                        laps = 0.5f; leanMul = 0.2f;
                        tapDip = Mathf.Abs(Mathf.Sin((phase - 0.62f) / 0.08f * Mathf.PI * 2f)) * 0.055f;
                    }
                    else
                        laps = 0.5f + ((phase - 0.70f) / 0.30f) * 2.5f;   // 2.5 laps to finish
                    // Uneven lap speed: slower against the far rim, faster pulling through.
                    float lapAngle = laps * Mathf.PI * 2f + Mathf.Sin(laps * Mathf.PI * 2f) * 0.45f;
                    float rx = tool.stirRadius;            // lateral half-width of the pot orbit
                    float rz = tool.stirRadius * 0.65f;    // foreshortened depth
                    hand += dir * (Mathf.Sin(lapAngle) * rz + tapDip);                 // push/pull + taps
                    hand += rightPerp * (Mathf.Cos(lapAngle) * rx * 0.35f);            // a little hand drift
                    stirLean = Mathf.Cos(lapAngle) * 11f * leanMul;                    // wrist lean does the rest
                }
                else if (tool.tipPlanted)
                {
                    // Arc welder: the electrode tip stays planted ON the weld point; the only
                    // visible motion is the pawn nudging the HANDLE end — slight, slow angle
                    // corrections like keeping a bead steady. Three incommensurate sines on a
                    // worked-time clock (cycle + phase, period-scaled — freezes on pause, never
                    // visibly loops), seeded per job so two welders don't wobble in unison.
                    // The hand is recomputed from the planted tip AFTER the final rotation is
                    // known (below); here we only contribute the correction angle.
                    float wt = (cycle + phase) * tool.period;
                    float ws = Mathf.Repeat(jobStartRealTime * 0.731f, 19f);
                    tipPlantedWob = Mathf.Sin(wt * 1.9f + ws) * 3.2f
                                  + Mathf.Sin(wt * 3.7f + ws * 1.7f) * 1.9f
                                  + Mathf.Sin(wt * 6.3f + ws * 2.9f) * 1.1f;
                    tipPlantedActive = true;
                }
                else
                {
                    float th = phase * Mathf.PI * 2f;
                    hand += (rightPerp * Mathf.Cos(th) + dir * Mathf.Sin(th)) * tool.stirRadius;
                }
            }
            else if (tool.swingStyle == SwingStyle.Stab)
            {
                Vector3 sdir = dir;
                if (tool.stabVaried)
                {
                    // Sewing: this cycle's stitch comes in from a slightly different angle and a
                    // small lateral shift along the seam (rerolled per cycle in DrawActive).
                    sdir = Quaternion.AngleAxis(jitterAng, Vector3.up) * dir;
                    hand += Vector3.Cross(Vector3.up, dir).normalized * jitterLat;
                    baseAngle = sdir.AngleFlat();
                }
                if (tool.MalletMaterial != null)
                    // Chisel-and-mallet: the chisel stays PLANTED against the target (no stab
                    // thrust); it only bites in a touch on each mallet blow, easing back out.
                    hand += dir * (tool.stabDistance * 0.30f * ChiselJab(phase, tool.stabStrikePhase));
                else
                    hand += sdir * tool.stabDistance * StabCurve(phase);
            }
            else if (tool.swingStyle == SwingStyle.Saw)
            {
                hand += dir * Mathf.Sin(phase * Mathf.PI * 2f * tool.sawStrokes) * tool.sawAmplitude;
            }
            else if (tool.swingStyle == SwingStyle.Dig)
            {
                hand += dir * tool.stabDistance * DigThrust(phase);
            }

            // Sweep side props: a paint pot held at the off side (with periodic brush dips) or a
            // dustpan set down ahead on its special cycle.
            float dipAmt = 0f;
            if (tool.swingStyle == SwingStyle.Sweep && tool.SidePropMaterial != null)
            {
                Vector3 sFlat = new Vector3(drawLoc.x, 0f, drawLoc.z);
                Vector3 sperp = Vector3.Cross(Vector3.up, dir).normalized;
                bool special = tool.sidePropEvery > 0 && (cycle % tool.sidePropEvery) == tool.sidePropEvery - 1;
                float sps = tool.sidePropScale;
                if (tool.sidePropMode == "dip")
                {
                    // Pot held low at the off side every frame; on the special cycle the brush
                    // arcs over to it, dips in, and returns (DrawActive adds the return drips).
                    Vector3 potPos = sFlat - sperp * (side * 0.30f) - dir * 0.04f;
                    potPos.z -= 0.05f;
                    Vector3 potDraw = potPos; potDraw.y = currentToolY - 0.008f;
                    Matrix4x4 potM = Matrix4x4.Translate(potDraw)
                        * Matrix4x4.Scale(new Vector3(sps, 1f, sps));
                    Emit(MeshPool.plane10, potM, QueueAdjust(tool.SidePropMaterial), 0);
                    offHandWrist = potPos + new Vector3(0f, 0f, 0.10f);
                    offHandWristValid = true;
                    if (special) dipAmt = DipCurve(phase);
                    if (dipAmt > 0f)
                        hand = Vector3.Lerp(hand, new Vector3(potPos.x, hand.y, potPos.z + 0.16f), dipAmt);
                }
                else if (special)   // "pan": dustpan set down ahead, sweep biased into it
                {
                    Vector3 panPos = sFlat + dir * (tool.reach + 0.52f);
                    Vector3 panDraw = panPos; panDraw.y = currentToolY - 0.012f;   // under the broom head
                    Quaternion panRot = Quaternion.AngleAxis(dir.AngleFlat() + 180f, Vector3.up); // mouth toward the pawn
                    Matrix4x4 panM = Matrix4x4.Translate(panDraw)
                        * Matrix4x4.Rotate(panRot)
                        * Matrix4x4.Scale(new Vector3(sps, 1f, sps));
                    Emit(MeshPool.plane10, panM, QueueAdjust(tool.SidePropMaterial), 0);
                    offHandWrist = panPos - dir * 0.14f;
                    offHandWristValid = true;
                    hand += dir * 0.10f;   // sweep toward the pan
                }
            }
            hand.y = currentToolY;

            // stirVaried replaces the generic sway with its own wrist-pivot lean so the two
            // don't fight each other.
            float swingAng = (tool.swingStyle == SwingStyle.Stir && tool.stirVaried) ? stirLean
                : tipPlantedActive ? tipPlantedWob : swing;
            float finalAng = baseAngle + swingAng;
            // Dipping into the paint pot: the brush tips over to point down into it.
            if (dipAmt > 0f) finalAng = Mathf.LerpAngle(finalAng, 180f, dipAmt * 0.85f);
            Quaternion rot = Quaternion.AngleAxis(finalAng, Vector3.up);
            if (tipPlantedActive)
            {
                // Keep the tip EXACTLY on the weld point: back the hand out from the planted
                // tip along the corrected angle, so the whole wobble shows up at the handle
                // end (and the tip output / arc flecks stay glued to the bead).
                Vector3 tipAnchor = new Vector3(drawLoc.x, 0f, drawLoc.z)
                    + dir * ((beadReachOverride > 0f ? beadReachOverride : tool.reach) + tool.scale);
                Vector3 backedOut = tipAnchor - (rot * Vector3.forward) * tool.scale;
                hand = new Vector3(backedOut.x, hand.y, backedOut.z);
            }
            // Draw-only haft-space lift: shift the whole sprite + hands so an authored feature
            // (e.g. the pickaxe's lower point) lands on the impact spark, while the burst itself
            // stays put (the caller subtracts strikeDrawShift before SpawnImpact). Same x/y
            // convention as impactTipOffset, so the lateral term flips with facing identically.
            // Applied AFTER the tipPlanted recompute so it isn't wiped for that path.
            if (tool.strikeLift != Vector2.zero)
            {
                Vector3 axisL = (rot * Vector3.forward) * tool.scale;          // grip -> tip
                Vector3 latL = Vector3.Cross(Vector3.up, axisL.normalized) * axisL.magnitude;
                bool useFlipL = (dir.x < 0f) ^ tool.flipHead;
                float gxL = useFlipL ? -tool.strikeLift.x : tool.strikeLift.x;
                strikeDrawShift = axisL * tool.strikeLift.y + latL * gxL;
                strikeDrawShift.y = 0f;
                hand += strikeDrawShift;
            }
            Matrix4x4 m = Matrix4x4.Translate(hand)
                * Matrix4x4.Rotate(rot)
                * Matrix4x4.Scale(new Vector3(tool.scale, 1f, tool.scale))
                * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f));

            // Subtle motion blur: while a rotary swing (axe/pickaxe Chop) whips down, draw a few
            // faded trailing ghost copies at the recent swing angles to read as fast motion. The
            // ghosts share the hand pivot and only the rotation differs, so they streak along the
            // arc behind the head. Only fires when the head is moving fast (steep downstroke).
            if (tool.swingStyle == SwingStyle.Chop && tool.motionBlur)
            {
                float aNow  = swing;
                float aPrev = SwingOffset(tool, Mathf.Repeat(phase - 0.02f, 1f)) * side;
                float angSpeed = Mathf.Abs(aNow - aPrev);   // degrees travelled over a 0.02 phase step
                if (angSpeed > 6f)
                {
                    const int ghosts = 3;
                    for (int g = 1; g <= ghosts; g++)
                    {
                        float gswing = SwingOffset(tool, Mathf.Repeat(phase - 0.012f * g, 1f)) * side;
                        Quaternion grot = Quaternion.AngleAxis(baseAngle + gswing, Vector3.up);
                        Matrix4x4 gm = Matrix4x4.Translate(hand)
                            * Matrix4x4.Rotate(grot)
                            * Matrix4x4.Scale(new Vector3(tool.scale, 1f, tool.scale))
                            * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f));
                        float alpha = 0.26f * (1f - (g - 1) / (float)ghosts);
                        // Re-pin the faded ghost onto the ambient render queue (matching the
                        // reload-shell faded draw): FadedVersionOf derives a NEW material whose
                        // queue can reset to the shader default, which would paint the trails OVER
                        // the body when facing north. QueueAdjust keeps them under the body.
                        Material gmat = QueueAdjust(FadedMaterialPool.FadedVersionOf(mat, alpha));
                        if (gmat != null) Emit(mesh, gm, gmat, 0);
                    }
                }
            }

            // Art is authored head-up (head at image top -> +Z -> toward target), so no vertical
            // flip; head points at the work, grip sits in hand. Left-facing uses the mirrored quad;
            // flipHead inverts that for asymmetric heads (axe/pickaxe).
            Emit(mesh, m, mat, 0);

            // Chisel-and-mallet: a small hand hammer in the OFF hand winds back behind the chisel
            // butt, arcs in and raps it right on stabStrikePhase (the existing strike crossing —
            // chips + tick sound land on the blow), recoils a touch, dwells, repeats.
            if (tool.swingStyle == SwingStyle.Stab && tool.MalletMaterial != null)
            {
                float gap = MalletGap(phase, tool.stabStrikePhase);   // 0 = head on the butt, 1 = wound back
                float ms = tool.scale * tool.malletScale;
                Vector3 mperp = Vector3.Cross(Vector3.up, dir).normalized;
                Vector3 butt = new Vector3(hand.x, 0f, hand.z);       // chisel grip end
                // Anchor on the pawn's anatomical LEFT (off-hand shoulder side); for east-side
                // work that points up over the face, so mirror it below the chisel instead.
                Vector3 mlat = dir.x > 0.001f ? mperp : -mperp;
                float ax = Mathf.Abs(dir.x), az = Mathf.Abs(dir.z);

                // ---- Three placement models, blended across the 45-degree seams so diagonal jobs
                //      cross between schemes smoothly instead of snapping. ----
                // SOUTH (facing camera): the off hand taps a small hammer onto the chisel TOP from
                //   the OFF side. The hammer holds a head-down-toward-centre / handle-up-to-the-off-
                //   side tilt so it never lies collinear with the vertical chisel (a straight-down
                //   contact made the head vanish into the chisel as a stack of balls), and the whole
                //   hammer LIFTS up-and-out on the windback then drops onto the butt — a low, lateral
                //   arc that never crosses the pawn's face. The grip is derived FROM the head so the
                //   head lands exactly on the butt regardless of tilt.
                Vector3 sUp = -dir;                                          // screen-up for a south chisel
                Vector3 sHead = Vector3.Lerp(
                    butt - sUp * (ms * 0.36f),                               // contact: head drops ONTO the chisel top so it actually touches
                    butt + sUp * (ms * 0.30f) + mlat * (ms * 0.34f),         // windback: lifted up & out to the off side (trimmed so the dwell pose hugs the chisel)
                    gap);
                sHead.y = currentToolY + 0.015f;
                Vector3 sHdir = (dir * 0.80f - mlat * 0.62f).normalized;     // head points down toward the butt
                Quaternion sRot = Quaternion.AngleAxis(sHdir.AngleFlat() - side * (18f * gap), Vector3.up);
                Vector3 sHand = sHead - (sRot * Vector3.forward) * ms;       // grip = handle end, up & to the off side
                sHand.y = currentToolY + 0.015f;
                // DRIVE: wind back BEHIND the butt and drive forward onto it. Vertical (north) comes
                //   from below with extra lateral (0.40) + a small lift so the cock peeks past the
                //   shoulder instead of hiding dead behind the torso; horizontal (E/W) drives in-line
                //   along the haft with LESS overlap (0.80 base vs 0.72) so the head taps the butt
                //   rather than sinking through it.
                Vector3 offV = (mlat * 0.40f - dir * 1.05f).normalized;
                Vector3 offH = (mlat * 0.50f - dir * 0.90f).normalized;
                Vector3 dHandV = butt + offV * (ms * (0.72f + 0.46f * gap));
                dHandV.z += ms * 0.10f; dHandV.y = currentToolY + 0.015f;
                Vector3 dHandH = butt + offH * (ms * (0.80f + 0.46f * gap));
                dHandH.y = currentToolY + 0.015f;
                Quaternion dRotV = Quaternion.AngleAxis((-offV).AngleFlat() - side * (34f * gap), Vector3.up);
                Quaternion dRotH = Quaternion.AngleAxis((-offH).AngleFlat() - side * (34f * gap), Vector3.up);

                // Blend weights. driveH: 0 = vertical (N/S), 1 = horizontal (E/W), smoothed across the
                // |x|=|z| seam so NE/NW jobs morph between the two drives. southW: how much the SOUTH
                // tap applies — south side only, fading to the horizontal drive as the work turns
                // sideways (so SE/SW jobs crossfade instead of snapping).
                bool south = dir.z < -0.001f;
                float driveH = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.37f, 0.63f, ax / Mathf.Max(0.0001f, ax + az)));
                float southW = south ? (1f - driveH) : 0f;
                Vector3 driveHand = south ? dHandH : Vector3.Lerp(dHandV, dHandH, driveH);
                Quaternion driveRot = south ? dRotH : Quaternion.Slerp(dRotV, dRotH, driveH);
                Vector3 mhand = Vector3.Lerp(driveHand, sHand, southW);
                Quaternion mrot = Quaternion.Slerp(driveRot, sRot, southW);
                // E/W contact pop: head swells a hair AT the blow (gap->0), only while in the
                // horizontal-drive regime, so the tap reads as impact energy not interpenetration.
                float msPop = ms * (1f + 0.06f * (1f - gap) * driveH * (1f - southW));

                Matrix4x4 mm = Matrix4x4.Translate(mhand)
                    * Matrix4x4.Rotate(mrot)
                    * Matrix4x4.Scale(new Vector3(msPop, 1f, msPop))
                    * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f));
                Emit(mesh, mm, QueueAdjust(tool.MalletMaterial), 0);
                // Off hand grips the mallet handle just above the wrist anchor, close to the chisel.
                offHandWrist = new Vector3(mhand.x, 0f, mhand.z) + (mrot * Vector3.forward) * (ms * 0.20f);
                offHandWristValid = true;
            }

            // Chop work props: a nail standing at the strike point (off hand pinching beside it,
            // retreating just before each blow) or tongs pinning a glowing billet on it.
            if (tool.swingStyle == SwingStyle.Chop && tool.PropMaterial != null)
            {
                Vector3 pFlat = new Vector3(drawLoc.x, 0f, drawLoc.z);
                Vector3 pperp = Vector3.Cross(Vector3.up, dir).normalized;
                Vector3 anchor = pFlat + dir * (tool.reach + tool.scale * 0.92f);
                float ps = tool.propScale;
                if (tool.propMode == "hold")
                {
                    // Tongs: held from the off side, jaws pinning the workpiece ON the strike point.
                    Vector3 tOff = (pperp * (-side) * 0.80f - dir * 0.40f).normalized;
                    Vector3 thand = anchor + tOff * (ps * 0.80f);
                    Vector3 thandDraw = thand; thandDraw.y = currentToolY - 0.012f;   // under the hammer
                    Quaternion trot = Quaternion.AngleAxis((-tOff).AngleFlat(), Vector3.up);
                    Matrix4x4 tm2 = Matrix4x4.Translate(thandDraw)
                        * Matrix4x4.Rotate(trot)
                        * Matrix4x4.Scale(new Vector3(ps, 1f, ps))
                        * Matrix4x4.Translate(new Vector3(0f, 0f, 0.5f));
                    Emit(MeshPool.plane10, tm2, QueueAdjust(tool.PropMaterial), 0);
                    offHandWrist = thand + (trot * Vector3.forward) * (ps * 0.12f);
                    offHandWristValid = true;
                }
                else
                {
                    // Nail: upright at the strike point; nobody holds a nail through the hit, so
                    // the pinching hand pulls back just before the head lands and returns after.
                    Vector3 nailDraw = anchor; nailDraw.z += ps * 0.22f; nailDraw.y = currentToolY - 0.012f;
                    Matrix4x4 nm = Matrix4x4.Translate(nailDraw)
                        * Matrix4x4.Scale(new Vector3(ps, 1f, ps));
                    Emit(MeshPool.plane10, nm, QueueAdjust(tool.PropMaterial), 0);
                    float pre = StrikePoint - 0.14f;
                    float retreat = phase < pre ? 0f
                        : phase < StrikePoint ? EaseIn((phase - pre) / 0.14f)
                        : Mathf.Max(0f, 1f - (phase - StrikePoint) / 0.20f);
                    offHandWrist = anchor - pperp * (side * 0.11f) - dir * (0.05f + retreat * 0.20f);
                    offHandWristValid = true;
                }
            }

            // Off hand braced flat on the worked target (bonesaw pressing the patient steady,
            // sickle hand grabbing the stalks), with a small bob synced to the work.
            if (tool.braceOffHand)
            {
                Vector3 bFlat = new Vector3(drawLoc.x, 0f, drawLoc.z);
                Vector3 bperp = Vector3.Cross(Vector3.up, dir).normalized;
                Vector3 brace = bFlat + dir * (tool.reach + tool.scale * 0.50f) + bperp * (side * 0.12f);
                brace.z -= Mathf.Abs(Mathf.Sin(phase * Mathf.PI * 2f * Mathf.Max(1, tool.sawStrokes))) * 0.015f;
                offHandWrist = brace;
                offHandWristValid = true;
            }

            // Face shield (welding mask): the off hand holds the mask raised over the pawn's
            // FACE for the whole job. Anchored to the head (drawLoc), not the work point, so the
            // weld bead's wander doesn't drag the shield around. The sprite follows the BODY
            // facing — dedicated East/West profile and North back-of-shell art when authored,
            // mirrored/canted front view otherwise. Facing north, the mask draws UNDER the body
            // sprite along with the tool, hands and arms. Listed LAST so it wins the off-hand
            // override for tools that combine features.
            // Welding mask: shed at Simplified LOD (medium zoom). Skipping the whole block also drops
            // the off-hand grip override it sets, so the off hand cleanly falls back to the haft (welder
            // held two-handed, no mask) rather than gripping where the mask would have been.
            if (tool.MaskMaterial != null && !lodSimplified)
            {
                Material kmat = QueueAdjust(tool.MaskMaterialFor(bodyFacing, out bool kdedicated));
                Vector3 facingVec = bodyFacing.FacingCell.ToVector3(); facingVec.y = 0f;
                if (facingVec.sqrMagnitude < 0.01f) facingVec = new Vector3(0f, 0f, -1f);
                facingVec = facingVec.normalized;
                Vector3 klat = Vector3.Cross(Vector3.up, facingVec).normalized;   // pawn's right
                bool maskNorth = bodyFacing == Rot4.North;
                bool maskProfile = bodyFacing.IsHorizontal;
                // Profile (E/W) facings can carry their own scale — the side shell reads
                // slimmer than the front view at equal scale.
                float ksc = tool.scale * ((maskProfile && tool.maskScaleProfile > 0f)
                    ? tool.maskScaleProfile : tool.maskScale);
                // A touch of held-up bob so it reads as gripped, not bolted to the head.
                float kbob = Mathf.Sin(animTime * 2.3f) * 0.008f;
                Vector3 maskPos = new Vector3(drawLoc.x, 0f, drawLoc.z);
                maskPos.z += 0.32f + kbob;                      // body centre -> face height
                // Out toward the face the body sprite is showing.
                maskPos += facingVec * (maskProfile ? 0.16f : maskNorth ? 0.08f : 0.05f);
                Vector3 maskDraw = maskPos;
                // Above the tool but below the hands. CRUCIAL: a fraction of the tool->hand gap,
                // NOT a fat constant — facing north the whole under-body band is only ~0.003
                // units deep (PawnRenderUtility layer spacing), so any constant tuned for the
                // south case (e.g. +0.018) punches the mask through the body sprite.
                maskDraw.y = currentToolY + (currentHandY - currentToolY) * 0.6f;
                Mesh kmesh = MeshPool.plane10;
                Quaternion krot = Quaternion.identity;
                if (!kdedicated)
                {
                    // Front-view fallback: mirror with the work side and cant toward the arc.
                    kmesh = faceLeft ? MeshPool.plane10Flip : MeshPool.plane10;
                    krot = Quaternion.AngleAxis(side * (maskProfile ? 14f : 6f), Vector3.up);
                }
                Matrix4x4 km = Matrix4x4.Translate(maskDraw)
                    * Matrix4x4.Rotate(krot)
                    * Matrix4x4.Scale(new Vector3(ksc, 1f, ksc));
                Emit(kmesh, km, kmat, 0);
                // The off hand grips the mask's BOTTOM rim (handheld shields are held by a
                // bottom handle), nudged toward the anatomical-LEFT side (-klat — the same
                // side as the off-hand shoulder the forearm binds to, matching the mallet
                // convention) so the arm never crosses the body to reach the shield. klat
                // derives from the BODY facing, so facing north it mirrors automatically.
                // OFFSETS ARE PAINTED-ART AWARE: the mask art fills only ~52% of its canvas
                // width and is dome-shaped, so the old "lower-outer corner" in canvas units
                // (0.30 lat / 0.34 drop) landed in transparent padding past the rounded rim
                // and the hand floated free of the mask. 0.12 lat / 0.36 drop puts the hand
                // centre ON the painted bottom rim — fingers visibly wrapping the edge.
                // E/W profile facings: klat lies along the VERTICAL screen axis, so the −klat
                // "anatomical-left" nudge lifts the hand for east but DROPS it for west — breaking
                // symmetry (west's hand floated a whole mask below the shield). Both profiles get
                // the symmetric drop that already reads right facing east, centred under the rim.
                if (maskProfile)
                {
                    offHandWrist = maskPos;
                    offHandWrist.z -= ksc * 0.24f;   // (bob already inherited via maskPos)
                }
                else
                {
                    offHandWrist = maskPos - klat * (ksc * 0.12f);
                    offHandWrist.z -= ksc * 0.36f;   // (bob already inherited via maskPos)
                }
                offHandWristValid = true;
            }

            // Off-hand CARRIED basket (forage / seed basket): a static container held low in the
            // OFF hand for the whole job while the main hand works the tool. Anchored to the BODY
            // (drawLoc) and BODY facing -- not the wandering work point -- so it stays put at the
            // hip. Per-facing placement: out to the off-hand (anatomical-left) side front-on/
            // back-on, forward-and-low in profile (always near side, no E/W asymmetry). Facing
            // north it draws UNDER the body (currentToolY + QueueAdjust route to the under-body
            // queue). The off hand grips the handle apex (offHandWrist), so the arm binds to it.
            // Listed after the mask block; no tool carries both, so ordering is moot.
            if (tool.OffhandMaterial != null)
            {
                Vector3 fv = bodyFacing.FacingCell.ToVector3(); fv.y = 0f;
                if (fv.sqrMagnitude < 0.01f) fv = new Vector3(0f, 0f, -1f);
                fv = fv.normalized;
                Vector3 bright = Vector3.Cross(Vector3.up, fv).normalized;   // pawn's right
                bool bprofile = bodyFacing.IsHorizontal;
                float bsc = tool.offhandScale;
                float bbob = Mathf.Sin(animTime * 1.7f) * 0.012f * bsc;       // gentle carry bob

                Vector3 bpos = new Vector3(drawLoc.x, 0f, drawLoc.z);
                if (bprofile)
                {
                    // Forward (toward the work) and low; the drop also pulls it to the near side.
                    bpos += fv * tool.offhandForwardProfile;
                    bpos.z -= tool.offhandDropProfile;
                }
                else
                {
                    // Out to the off-hand side, small front/back nudge, dropped to the hip.
                    bpos += bright * (-1f * tool.offhandLateral);
                    bpos += fv * tool.offhandForward;
                    bpos.z -= tool.offhandDrop;
                }
                bpos.z += bbob;

                Vector3 bdraw = bpos; bdraw.y = currentToolY;
                Matrix4x4 bm = Matrix4x4.Translate(bdraw)
                    * Matrix4x4.Scale(new Vector3(bsc, 1f, bsc));
                Emit(MeshPool.plane10, bm, QueueAdjust(tool.OffhandMaterial), 0);

                // Off hand on the handle apex (high on the sprite) so the fist reads gripping it.
                offHandWrist = bpos;
                offHandWrist.z += bsc * tool.offhandGripRaise;
                offHandWrist.y = 0f;
                offHandWristValid = true;
            }

            tip = hand + (rot * Vector3.forward) * tool.scale;
            tip.y = 0f;
            grip = new Vector3(hand.x, 0f, hand.z);
        }

        // Map a sprite-space offset (delta from the geometric tip) onto the drawn tool, in world
        // space. Shared by the strike glint and the impactAtTip burst so both land on the same
        // authored point (cutting edge / pick point) for any swing style or facing.
        // Pick the facing-correct tip offset: when the straight front/back art (texPathNorthSouth)
        // is drawn, the electrode is centred, so the profile torch's lateral offset would shove the
        // glow/sparks sideways off the tip. Use the N/S override when set and the straight art is up.
        private static Vector2 NSOffset(JobToolDef tool, Rot4 facing, Vector2 baseOff, Vector2 nsOff)
        {
            if (nsOff.x > -900f && !tool.texPathNorthSouth.NullOrEmpty()
                && (facing == Rot4.North || facing == Rot4.South))
                return nsOff;
            return baseOff;
        }

        private static Vector3 TipSpacePos(JobToolDef tool, Vector3 tip, Vector3 grip, Vector3 dir, Vector2 off)
        {
            if (off == Vector2.zero) return tip;
            Vector3 axis = tip - grip;                       // grip -> tip, |axis| = effective scale
            if (axis.sqrMagnitude < 1e-6f) return tip;
            Vector3 lat = Vector3.Cross(Vector3.up, axis.normalized) * axis.magnitude;
            bool useFlip = (dir.x < 0f) ^ tool.flipHead;
            float gx = useFlip ? -off.x : off.x;
            Vector3 pos = grip + axis * (1f + off.y) + lat * gx;
            pos.y = tip.y;
            return pos;
        }

        // ---- Show Me Your Hands: draw the pawn's hands gripping the tool ----
        // Reuses the "Hand" texture tinted to skin colour, gated on SMYH being loaded
        // (JobEffectsSettings.HandsOnTools). We draw the hands ourselves at the haft so they
        // sit on the animated tool correctly; SMYH draws no weapon-hands during undrafted work.
        // Texture set: Show Me Your Hands' own hand art.
        private static readonly Dictionary<Color, Material> handMats = new Dictionary<Color, Material>();
        private static Texture2D cachedHandTex;
        private static bool handTexResolved;

        // Hand textures are owned by SMYH — NEVER use ContentFinder here. A global lookup would
        // pick up our own mod's folder if modmixer (or any future file) ever places a stub
        // Hand.png there, causing the magenta checkerboard bug. We always pull directly from
        // SMYH's content pack.
        private static Texture2D HandTexture()
        {
            if (handTexResolved) return cachedHandTex;
            handTexResolved = true;
            ModContentPack src = ModByPackageId("mlie.showmeyourhands");
            if (src != null)
                cachedHandTex = src.GetContentHolder<Texture2D>().Get("Hand");
            return cachedHandTex;
        }

        private static ModContentPack ModByPackageId(string id)
        {
            // Case-insensitive ORDINAL compare — no per-mod string allocation (the old
            // pid.ToLower() == id allocated a lowercased copy of every running mod's packageId).
            List<ModContentPack> mods = LoadedModManager.RunningModsListForReading;
            for (int i = 0; i < mods.Count; i++)
            {
                string pid = mods[i].PackageIdPlayerFacing;
                if (pid != null && string.Equals(pid, id, System.StringComparison.OrdinalIgnoreCase))
                    return mods[i];
            }
            return null;
        }

        // The grip→tip vector is the haft axis (grip = sprite butt/near end, tip = head). Each
        // tool authors handPosA / handPosB as fractions along that axis, so the fists land on the
        // actual handle in its texture. We mirror the hand mesh when facing left.
        private static void DrawHands(Pawn pawn, JobToolDef tool, Vector3 grip, Vector3 tip,
            Vector3 drawLoc, Vector3 dir, float phase, bool faceLeft, bool oneHandOnly = false)
        {
            Vector3 along = tip - grip; along.y = 0f;
            if (along.sqrMagnitude < 0.0000001f) return;
            // Match Show Me Your Hands' own hand size: it draws hands at 0.8 * pawn body size
            // (MeshMakerPlanes.NewPlaneMesh(0.8 * baseBodySize * BaseHandSize)). handScale is a
            // per-tool multiplier on top of that, so handScale = 1 reads the same as SMYH's hands.
            float body = pawn.RaceProps != null ? Mathf.Clamp(pawn.RaceProps.baseBodySize, 0.5f, 2.5f) : 1f;
            // baseBodySize is RACE-level (1.0 for a human child as well as an adult), so also fold in the
            // pawn's rendered body SIZE (BodyScaleFor: ~0.75 for a child, gene/HAR-scaled otherwise) so
            // the hands match the body-scaled forearms instead of reading adult-sized on a small pawn.
            float s = tool.handScale * 0.8f * body * ArmRenderer.BodyScaleFor(pawn);

            // Prefer SMYH's own computed hand materials so we inherit its exact colour
            // (gloves -> glove colour + HandClean texture, artificial limbs, etc.) and its
            // missing-hand handling. Fall back to our own skin-tinted hand if that fails.
            Vector3 wristMain, wristOff;
            if (tool.swingStyle == SwingStyle.Spray)
            {
                // Fire extinguisher: both hands grip the tank body, stacked ALONG its (near-vertical)
                // axis — handPosB = upper hand at the neck, handPosA = lower hand at the silver band —
                // nudged onto the near (pawn-facing) edge of the cylinder.
                Vector3 perp = new Vector3(-along.z, 0f, along.x).normalized;
                float nearSide = Vector3.Dot(perp, dir) > 0f ? -1f : 1f;   // edge toward the pawn, away from the fire
                float edge = along.magnitude * 0.12f;
                wristOff  = grip + along * tool.handPosA + perp * (edge * nearSide);  // lower hand (silver band)
                wristMain = grip + along * tool.handPosB + perp * (edge * nearSide);  // upper hand (neck)
            }
            else
            {
                wristMain = grip + along * tool.handPosA;
                wristOff = grip + along * tool.handPosB;
            }
            bool twoHands = tool.handCount >= 2;
            // Off-hand override: the off hand leaves the haft for whatever this draw placed it
            // on — mallet handle, nail pinch, tongs, brace on the patient, paint pot, dustpan,
            // extinguisher pin, salt pinch.
            if (offHandWristValid)
            {
                wristOff = offHandWrist;
                twoHands = true;
            }
            if (oneHandOnly) twoHands = false;

            // Nice Hands ships a DIRECTIONAL fist (vs vanilla SMYH's rotationally-symmetric disk), so
            // when it's the loaded hand art we rotate each fist onto the haft (like SMYH rotates hands
            // to the weapon angle) and depth-split it: the half on the far side of the haft tucks UNDER
            // the tool, the camera-near half rides OVER it. The disk has no orientation, so it (and our
            // fallback hand) keep drawing as one flat unrotated quad (split=false).
            bool niceHands = JobEffectsSettings.NiceHandsActive;
            float haftAngle = along.AngleFlat();   // world heading of the grip->tip axis, drives the fist rotation
            if (SmyhHands.TryGet(pawn, out Material mainMat, out Material offMat, out bool missingHand))
            {
                // Arms first so the fists draw on top of the wrist ends. Skipped at simplified LOD
                // (medium zoom) — the translucent forearms are sub-pixel there and just add draw calls.
                if (!lodSimplified)
                    ArmRenderer.DrawArms(pawn, drawLoc, dir, wristMain, twoHands, wristOff, missingHand, phase,
                        tool.swingStyle == SwingStyle.Stir);
                DrawHandBillboard(wristMain, s, mainMat, faceLeft, niceHands, haftAngle);
                if (twoHands && !missingHand)
                    // An overridden off-hand grips something OTHER than the haft (mallet handle, tongs,
                    // paint pot…), so it isn't rotated/split onto the haft — it draws as one quad over the prop.
                    DrawHandBillboard(wristOff, s, offMat, faceLeft, niceHands && !offHandWristValid, haftAngle);
            }
            else
            {
                Material mat = HandMat(pawn);
                if (mat == null) return;
                if (!lodSimplified)
                    ArmRenderer.DrawArms(pawn, drawLoc, dir, wristMain, twoHands, wristOff, false, phase,
                        tool.swingStyle == SwingStyle.Stir);
                DrawHandBillboard(wristMain, s, mat, faceLeft);
                if (twoHands)
                    DrawHandBillboard(wristOff, s, mat, faceLeft);
            }
        }

        // Nice Hands grip tuning (top-of-region constants for fast in-game iteration):
        //   NiceHandSplitFrac  — texture V (0=bottom/fingers, 1=top/back-of-hand) of the split line. The
        //                        haft passes through the fist CENTRE (the wrist anchor sits on it), so
        //                        0.5 puts the divide on the haft; bias it to show more/less fist in front.
        //   NiceHandRotOffset  — extra degrees layered on the haft rotation to fine-tune how Hand.png
        //                        (a generic, non-directional fist) sits on the handle. 0 leaves a
        //                        horizontal (east) haft with the fist in its natural upright orientation.
        //   NiceHandGripDead   — |sin(haftAngle)| below which the haft is too near-vertical on screen to
        //                        fake a front/back grip; we just draw the whole fist upright over the tool.
        private const float NiceHandSplitFrac = 0.5f;
        private const float NiceHandRotOffset = 0f;
        private const float NiceHandGripDead = 0.25f;

        // Lazily-built V-split half meshes: handPalm = top of the texture (back of hand), handFingers =
        // bottom (fingers/knuckles). Built once on the render thread.
        private static Mesh handPalm, handFingers;
        private static void EnsureGripMeshes()
        {
            if (handPalm != null) return;
            handPalm    = MakeBandMesh(NiceHandSplitFrac, 1f);   // back of hand / palm
            handFingers = MakeBandMesh(0f, NiceHandSplitFrac);   // fingers / knuckles
        }

        // Unit X-Z quad band: full width, spanning V (and local Z) from v0..v1 where 0 = bottom of the
        // texture, 1 = top. Matches MeshPool.plane10's footprint/winding so it drops straight into the
        // same translate/rotate/scale matrix — the two halves together tile the full quad with no gap.
        private static Mesh MakeBandMesh(float v0, float v1)
        {
            float z0 = v0 - 0.5f, z1 = v1 - 0.5f;
            Mesh m = new Mesh();
            m.vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0f, z0),
                new Vector3(-0.5f, 0f, z1),
                new Vector3( 0.5f, 0f, z1),
                new Vector3( 0.5f, 0f, z0),
            };
            m.uv = new Vector2[]
            {
                new Vector2(0f, v0),
                new Vector2(0f, v1),
                new Vector2(1f, v1),
                new Vector2(1f, v0),
            };
            m.SetTriangles(new int[] { 0, 1, 2, 0, 2, 3 }, 0);
            return m;
        }

        // Billboard hand. SMYH's symmetric disk (and our fallback / overridden off-hands) draws as one
        // unrotated quad with a left/right mesh flip. The Nice Hands directional fist (split=true) is
        // instead ROTATED onto the haft and depth-split: the half on the far side of the handle tucks
        // UNDER the tool, the camera-near half rides OVER it, so the haft reads as threaded through the
        // grip at every tool facing. haftAngle is the world heading of the grip->tip axis.
        private static void DrawHandBillboard(Vector3 pos, float scale, Material mat, bool faceLeft,
            bool split = false, float haftAngle = 0f)
        {
            Material qmat = QueueAdjust(mat);
            Vector3 sc = new Vector3(scale, 1f, scale);
            if (split)
            {
                EnsureGripMeshes();
                // Rotate the fist onto the haft. The -90 makes a horizontal (east) haft leave the fist
                // upright/natural; the rotation also aligns the V-split line with the handle so the two
                // halves land on the two sides of it. Rotation is about +Y (in the ground plane), so it
                // never changes altitude — the front/back depth is purely the per-half Y below.
                Quaternion rot = Quaternion.AngleAxis(haftAngle - 90f + NiceHandRotOffset, Vector3.up);
                // southness = (rot * +Z).z = sin(haftAngle): +1 east, -1 west, ~0 when the haft is
                // near-vertical on screen. Its SIGN says which way the texture-top (palm) half points:
                // >0 => palm points away from the camera (north) => palm goes BEHIND, fingers in front.
                float southness = (rot * Vector3.forward).z;
                if (Mathf.Abs(southness) >= NiceHandGripDead)
                {
                    float behindY = 2f * currentToolY - currentHandY;  // symmetric gap on the far side of the tool
                    bool palmBehind = southness > 0f;
                    Emit(palmBehind ? handPalm : handFingers,
                        Matrix4x4.TRS(new Vector3(pos.x, behindY, pos.z), rot, sc), qmat, 0);
                    Emit(palmBehind ? handFingers : handPalm,
                        Matrix4x4.TRS(new Vector3(pos.x, currentHandY, pos.z), rot, sc), qmat, 0);
                    return;
                }
                // Near-vertical haft: too edge-on to fake a grip — whole fist upright over the tool.
                Mesh dm = faceLeft ? MeshPool.plane10Flip : MeshPool.plane10;
                Emit(dm, Matrix4x4.TRS(new Vector3(pos.x, currentHandY, pos.z), Quaternion.identity, sc), qmat, 0);
                return;
            }
            pos.y = currentHandY; // just above the tool sprite (under the body when facing north)
            Mesh mesh = faceLeft ? MeshPool.plane10Flip : MeshPool.plane10;
            Emit(mesh, Matrix4x4.TRS(pos, Quaternion.identity, sc), qmat, 0);
        }

        private static Material HandMat(Pawn pawn)
        {
            Texture2D tex = HandTexture();
            if (tex == null) return null;
            Color c = pawn.story != null ? pawn.story.SkinColor : Color.white;
            if (!handMats.TryGetValue(c, out Material m))
            {
                m = MaterialPool.MatFrom(tex, ShaderDatabase.Cutout, c);
                handMats[c] = m;
            }
            return m;
        }

        // ---- Helpers ----

        private static bool Matches(JobToolDef tool, LocalTargetInfo target, Job job)
        {
            // Surgery: a DoBill whose target is a patient pawn under a medical operation. The
            // optional recipe-keyword filter lets a bonesaw claim amputations while a keyword-less
            // scalpel catches every other operation (installs, transplants, excisions, etc.).
            if (tool.surgeryOnly)
            {
                if (!(target.Thing is Pawn)) return false;
                if (!(job?.bill is Bill_Medical)) return false;
                var kw = tool.recipeNameKeywords;
                if (kw != null && kw.Count > 0)
                {
                    string rd = job.bill.recipe?.defName;
                    if (rd.NullOrEmpty()) return false;
                    bool any = false;
                    for (int i = 0; i < kw.Count; i++)
                        if (rd.IndexOf(kw[i], System.StringComparison.OrdinalIgnoreCase) >= 0) { any = true; break; }
                    if (!any) return false;
                }
                return true;
            }
            if (tool.treeOnly)
            {
                if (!(target.Thing is Plant tp) || tp.def?.plant == null || !tp.def.plant.IsTree)
                    return false;
            }
            if (tool.nonTreeOnly)
            {
                if (!(target.Thing is Plant ntp) || ntp.def?.plant == null || ntp.def.plant.IsTree)
                    return false;
            }
            if (tool.workbenchDefs != null && tool.workbenchDefs.Count > 0)
            {
                Thing t = target.Thing;
                if (t == null || !tool.workbenchDefs.Contains(t.def.defName))
                    return false;
            }
            // Terrain-build gating: a tool that lists buildTerrainDefs claims ONLY FinishFrame jobs
            // whose frame builds one of those TerrainDefs (e.g. a hoe tilling Medieval Overhaul's
            // plowed soil). Normal build tools EXCLUDE every TerrainDef frame (the frameStuffCategories
            // block below), so a terrain build otherwise matches no tool — this fills that gap without
            // colliding with the hammer/chisel/welder. A tool with buildTerrainDefs set matches nothing
            // else (a normal building's FinishFrame fails the TerrainDef test and returns false).
            if (tool.buildTerrainDefs != null && tool.buildTerrainDefs.Count > 0)
            {
                if (job?.def?.defName != "FinishFrame") return false;
                BuildableDef built = target.Thing?.def?.entityDefToBuild;
                if (!(built is TerrainDef) || !tool.buildTerrainDefs.Contains(built.defName))
                    return false;
            }
            // Construction tool selection is purely material-based below: hammer = wood / fabric /
            // leather, chisel = stone, welder = metal / unstuffed. The welder is then swapped to the
            // hammer pre-Electricity by the tech-gate (ApplyTechGate, applied after resolution).
            // Frame-material gating (FinishFrame only): the frame's stuff category picks the
            // build tool — hammer for wood, chisel + mallet for stone, welder for metal. The
            // special list entry "None" matches unstuffed frames (machines / fixed-cost
            // buildings — steel-and-component work, so the welder claims them). Every other
            // job ignores this filter (a Repair matches whatever the building is made of).
            if (tool.frameStuffCategories != null && tool.frameStuffCategories.Count > 0
                && job?.def?.defName == "FinishFrame")
            {
                // Floors are FinishFrame jobs too, but their frames are unstuffed (costList),
                // so they used to fall into the welder's "None" bucket and a colonist would
                // "weld" a floor down (e.g. VFE-Tribals painted floors). A frame that builds a
                // TerrainDef is a floor/terrain — none of the material construction tools
                // (hammer = wood, chisel = stone, welder = metal) apply to it.
                if (target.Thing?.def?.entityDefToBuild is TerrainDef)
                    return false;
                ThingDef stuff = target.Thing?.Stuff;
                bool stuffOk = false;
                if (stuff?.stuffProps?.categories != null && stuff.stuffProps.categories.Count > 0)
                {
                    var cats = stuff.stuffProps.categories;
                    for (int i = 0; i < cats.Count && !stuffOk; i++)
                        if (cats[i] != null && tool.frameStuffCategories.Contains(cats[i].defName))
                            stuffOk = true;
                }
                else
                {
                    stuffOk = tool.frameStuffCategories.Contains("None");
                }
                if (!stuffOk) return false;
            }
            // Tend medicine-type gating: the medicine being applied lives in job.targetB (null when
            // tending with no medicine). Lets the herbal bundle claim herbal tends while the default
            // kit covers everything else. A medicine filter implies "medicine required", so a
            // bare-handed (no-medicine) tend matches none of the medicine tools.
            bool hasForbid = tool.forbidMedicineDefs != null && tool.forbidMedicineDefs.Count > 0;
            if (!tool.requireMedicineDef.NullOrEmpty() || hasForbid)
            {
                string medDef = job?.targetB.Thing?.def?.defName;
                if (medDef == null)
                    return false;
                if (!tool.requireMedicineDef.NullOrEmpty() && medDef != tool.requireMedicineDef)
                    return false;
                if (hasForbid && tool.forbidMedicineDefs.Contains(medDef))
                    return false;
            }
            return true;
        }

        private static float SwingOffset(JobToolDef tool, float t)
        {
            if (tool.swingStyle == SwingStyle.Stab || tool.swingStyle == SwingStyle.Pry
                || tool.swingStyle == SwingStyle.Saw)
                return 0f;
            if (tool.swingStyle == SwingStyle.Dig)
                return -tool.windupAngle * DigLift(t);   // head scoops up as it lifts out of the soil
            if (tool.swingStyle == SwingStyle.Sweep || tool.swingStyle == SwingStyle.Stir)
                return tool.windupAngle * Mathf.Sin(t * Mathf.PI * 2f);

            float up = -tool.windupAngle;
            // strikeAngle (when set) is the impact pose: the head settles here with its working tip
            // on the forward line and never rotates past it. Otherwise fall back to the old
            // follow-through overshoot.
            float through = tool.HasStrikeAngle ? tool.strikeAngle : tool.followThroughAngle;
            // Hit-synced tools (pickaxe) span one slow vanilla hit-interval per swing: raise the head
            // steadily across most of the interval, then snap it down fast onto the strike. e is the
            // elapsed fraction since the last strike (0 right after a hit -> 1 at the next hit).
            if (tool.syncToMineHit)
            {
                // e = elapsed fraction since the last strike (0 right after a hit -> 1 at the next hit).
                // Hold the pick still at a low ready pose for most of the slow interval, then snap it
                // up (quick cock) and drive it down fast onto the strike. No lazy drawn-out raise.
                float e = Mathf.Repeat(t - StrikePoint, 1f);
                const float settle = 0.1f;     // ease out of the previous strike into the rest hold
                const float holdEnd = 0.6f;    // hold dead still until here
                const float raiseEnd = 0.78f;  // quick cock up to the apex
                const float apexHold = 0.982f; // poised high at the apex (anticipation); tight strike window
                                               // so the pick snaps down just before the hit, instead of dropping
                                               // lazily across the long mining interval. Widened ~20% from 0.985
                                               // so the swing-down whip reads ~20% slower.
                float rest = through * 0.4f;   // low ready pose, head near the rock
                if (e < settle) return Mathf.Lerp(through, rest, EaseOut(e / settle));
                if (e < holdEnd) return rest;
                if (e < raiseEnd) return Mathf.Lerp(rest, up, EaseOut((e - holdEnd) / (raiseEnd - holdEnd)));
                if (e < apexHold) return up;   // hold at the top before the strike
                // Whip down hard into the strike in a tight window for an impactful hit.
                return Mathf.Lerp(up, through, EaseInStrong((e - apexHold) / (1f - apexHold)));
            }
            const float chopTop = 0.632f;  // raised and poised here; hold for anticipation (~30% shorter strike window)
            if (t < 0.46f) return Mathf.Lerp(0f, up, EaseOut(t / 0.46f));   // raise to full windup
            if (t < chopTop) return up;                                     // hold high (anticipation)
            // Whip down hard into the strike in a tight window for an impactful hit.
            if (t < StrikePoint) return Mathf.Lerp(up, through, EaseInStrong((t - chopTop) / (StrikePoint - chopTop)));
            float r = (t - StrikePoint) / (1f - StrikePoint);
            return Mathf.Lerp(through, 0f, EaseInOut(r));
        }

        // Stab reaches full extension at ~0.3 (see StabCurve) — fire debris there.
        private const float StabStrike = 0.3f;

        // Dig flings soil as the head starts to lift; Crank emits as the turn completes;
        // Load pops a brass glint as the shell seats.
        private const float DigStrike = 0.2f;
        private const float CrankStrike = 0.7f;
        private const float LoadStrike = 0.6f;

        // ---- Beat (fire blanket): two downbeats per cycle, then a brief raised rest ----
        private const float BeatStrike1 = 0.18f;   // first slam lands
        private const float BeatStrike2 = 0.45f;   // second slam lands
        private static bool Crossed(float prev, float cur, float p) => prev <= p && cur > p;
        // 0 = pad raised back/up, 1 = pad slammed flat onto the fire.
        private static float BeatExtend(float t)
        {
            if (t < BeatStrike1) return EaseIn(t / BeatStrike1);                                          // raise -> slam 1
            if (t < 0.30f) return Mathf.Lerp(1f, 0.45f, EaseOut((t - BeatStrike1) / (0.30f - BeatStrike1))); // lift between beats
            if (t < BeatStrike2) return Mathf.Lerp(0.45f, 1f, EaseIn((t - 0.30f) / (BeatStrike2 - 0.30f)));  // drop -> slam 2
            if (t < 0.66f) return Mathf.Lerp(1f, 0f, EaseOut((t - BeatStrike2) / (0.66f - BeatStrike2)));    // lift fully off
            return 0f;                                                                                       // raised rest
        }

        // Sweep peaks (blade at the far side of the stroke) at ~0.25 — scatter cuttings there.
        private const float SweepStrike = 0.25f;

        // ---- Spray (fire extinguisher): one discharge pulse per cycle ----
        // The pulse (forward mist burst + recoil peak) lands early, then the tool settles and
        // holds aimed at the fire until the next cycle. One burst per cycle == "one puff per tick".
        private const float SprayStrike = 0.12f;
        // 0 = settled/aimed, 1 = full recoil right at the discharge.
        private static float SprayRecoil(float t)
        {
            if (t < SprayStrike) return EaseIn(t / SprayStrike);                              // snap back as the trigger pulls
            if (t < 0.50f) return Mathf.Lerp(1f, 0f, EaseOut((t - SprayStrike) / (0.50f - SprayStrike))); // recover
            return 0f;                                                                          // hold aimed, steady
        }

        // ---- Pry (crowbar): wedge in, YANK hard right onto the strike, slow smooth reset ----
        private const float PryStrike = 0.58f;
        private static float PryInsert(float t) => t < PryStrike ? EaseOut(t / PryStrike) : 1f;
        private static float PryAngle(JobToolDef tool, float t)
        {
            float amp = tool.windupAngle;
            if (t < 0.30f) return Mathf.Lerp(0f, amp * 0.15f, t / 0.30f);                                    // wedge in
            // Hard-accelerating heave that lands its peak exactly on PryStrike — the yank
            // coincides with the impact burst.
            if (t < PryStrike) return Mathf.Lerp(amp * 0.15f, amp, EaseInStrong((t - 0.30f) / (PryStrike - 0.30f)));
            if (t < 0.70f) return amp;                                                                          // strain at the peak
            // Slow, smooth reset — no end-of-cycle snap-back jolt.
            return Mathf.Lerp(amp, 0f, EaseInOut((t - 0.70f) / 0.30f));
        }
        // ---- Brow wipe: tool rests at the hip while the main hand wipes across the forehead ----
        private static void DrawBrowWipe(Pawn pawn, Vector3 drawLoc, JobToolDef tool, Vector3 dir, float t)
        {
            DrawHolster(drawLoc, tool, dir, 0f, pawn);     // tool at the hip, full alpha
            bool faceLeft = dir.x < 0f;
            float side = faceLeft ? -1f : 1f;
            float raise = t < 0.15f ? EaseInOut(t / 0.15f)
                : (t > 0.85f ? 1f - EaseInOut((t - 0.85f) / 0.15f) : 1f);
            float wig = Mathf.Sin(t * 19f) * 0.07f * raise;      // ~3 passes across the brow
            Vector3 brow = drawLoc + new Vector3(side * 0.08f + wig, 0f, 0.16f + 0.16f * raise);
            Vector3 g = new Vector3(brow.x, 0f, brow.z - 0.02f);
            Vector3 tp = new Vector3(brow.x, 0f, brow.z + 0.02f);
            offHandWristValid = false;
            bool humanlike = pawn.RaceProps != null && pawn.RaceProps.Humanlike;
            if (JobEffectsSettings.HandsOnTools && humanlike && raise > 0.05f)
                DrawHands(pawn, tool, g, tp, drawLoc, dir, t, faceLeft, true);
        }

        private static void SpawnSweat(Map map, Vector3 drawLoc, float side)
        {
            ResolvePale();
            if (sweatFleck == null) return;
            Vector3 o = drawLoc + new Vector3(side * 0.16f, 0f, 0.30f);
            if (!o.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            for (int i = 0; i < 2; i++)
            {
                FleckCreationData d = FleckMaker.GetDataStatic(
                    o + new Vector3(Rand.Range(-0.04f, 0.04f), 0f, Rand.Range(-0.02f, 0.04f)),
                    map, sweatFleck, Rand.Range(0.25f, 0.4f));
                d.velocityAngle = side > 0f ? Rand.Range(55f, 115f) : Rand.Range(245f, 305f);
                d.velocitySpeed = Rand.Range(0.9f, 1.8f);
                map.flecks.CreateFleck(d);
            }
        }

        private static void SpawnSalt(Map map, Vector3 handPos)
        {
            ResolvePale();
            if (saltFleck == null) return;
            Vector3 o = handPos; o.y = 0f;
            if (!o.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            int n = Rand.RangeInclusive(2, 3);
            for (int i = 0; i < n; i++)
            {
                FleckCreationData d = FleckMaker.GetDataStatic(o + FlatScatter(0.04f), map, saltFleck,
                    Rand.Range(0.16f, 0.28f));
                d.velocityAngle = 180f + Rand.Range(-25f, 25f);   // falls down-screen into the pot
                d.velocitySpeed = Rand.Range(0.4f, 0.8f);
                map.flecks.CreateFleck(d);
            }
        }

        private static void SpawnQuench(JobToolDef tool, Map map, Vector3 tip, Vector3 grip, Vector3 dir)
        {
            ResolvePale();
            if (steamFleck == null) return;
            Vector3 o = TipSpacePos(tool, tip, grip, dir, tool.tipGlowOffset); o.y = 0f;
            if (!o.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            for (int i = 0; i < 4; i++)
            {
                FleckCreationData d = FleckMaker.GetDataStatic(o + FlatScatter(0.10f), map, steamFleck,
                    Rand.Range(0.8f, 1.3f));
                d.velocityAngle = Rand.Range(-30f, 30f);   // rising
                d.velocitySpeed = Rand.Range(0.6f, 1.2f);
                map.flecks.CreateFleck(d);
            }
        }

        private static void SpawnPin(Map map, Vector3 tip, Vector3 dir)
        {
            ResolvePale();
            if (pinFleck == null) return;
            Vector3 o = tip; o.y = 0f;
            if (!o.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            FleckCreationData d = FleckMaker.GetDataStatic(o, map, pinFleck, Rand.Range(0.3f, 0.4f));
            d.velocityAngle = (-dir).AngleFlat() + Rand.Range(-35f, 35f);   // flicked back over the shoulder
            d.velocitySpeed = Rand.Range(1.5f, 2.5f);
            d.rotationRate = Rand.Range(-240f, 240f);
            map.flecks.CreateFleck(d);
        }

        // Smelt-rod quench: push forward/down into the bath, hold, withdraw.
        private static float QuenchCurve(float t)
        {
            if (t < 0.30f) return EaseInOut(t / 0.30f);
            if (t < 0.70f) return 1f;
            return 1f - EaseInOut((t - 0.70f) / 0.30f);
        }

        // Paintbrush dip: out to the pot, hold while it loads, back to the wall.
        private static float DipCurve(float t)
        {
            if (t < 0.30f) return EaseInOut(t / 0.30f);
            if (t < 0.55f) return 1f;
            if (t < 0.85f) return 1f - EaseInOut((t - 0.55f) / 0.30f);
            return 0f;
        }

        // Scratch (research): hand down for most of the cycle, then up beside the head for a
        // short scratching bout near the end.
        private static float ScratchAmount(float t)
        {
            if (t < 0.70f) return 0f;
            if (t < 0.76f) return EaseInOut((t - 0.70f) / 0.06f);    // hand comes up
            if (t < 0.94f) return 1f;                                // scratch scratch
            return 1f - EaseInOut((t - 0.94f) / 0.06f);              // hand drops
        }

        // Read (notepad): lift up to the face, hold while reading, lower, jot — repeat.
        private static float ReadLift(float t)
        {
            if (t < 0.14f) return EaseInOut(t / 0.14f);                  // lift up to read
            if (t < 0.58f) return 1f;                                    // reading
            if (t < 0.72f) return 1f - EaseInOut((t - 0.58f) / 0.14f);   // bring it back down
            return 0f;                                                   // held low — jotting notes
        }

        // Mallet swing: wind back from the ready dwell, drive in hard to land exactly on the
        // strike phase, HOLD the contact for a beat (so the eye catches the hit), then rebound
        // to the ready dwell. Starts at the dwell value (0.30) so the cycle wrap is continuous —
        // no pop from rebound back onto the butt.
        private static float MalletGap(float t, float strike)
        {
            float wind = strike * 0.64f;   // longer cock, shorter/faster drop so the slam lands hard
            if (t < wind) return Mathf.Lerp(0.30f, 1f, EaseInOut(t / wind));        // wind back
            if (t < strike) return 1f - EaseInStrong((t - wind) / (strike - wind)); // drive to contact
            float r = (t - strike) / Mathf.Max(0.0001f, 1f - strike);
            if (r < 0.22f) return 0f;                                               // hold the contact
            return Mathf.Min(0.30f, EaseOut((r - 0.22f) / 0.78f) * 0.40f);          // rebound to ready
        }

        // Chisel bite: driven a touch deeper the instant the mallet lands, easing back out.
        private static float ChiselJab(float t, float strike)
        {
            if (t < strike) return 0f;
            float r = (t - strike) / Mathf.Max(0.0001f, 1f - strike);
            if (r < 0.18f) return EaseOut(r / 0.18f);
            return 1f - EaseInOut((r - 0.18f) / 0.82f);
        }

        private static float StabCurve(float t)
        {
            if (t < 0.3f) return EaseOut(t / 0.3f);
            if (t < 0.5f) return 1f;
            return Mathf.Lerp(1f, 0f, EaseInOut((t - 0.5f) / 0.5f));
        }

        // Dig: a quick stab into the soil, then a SLOW lift up and out (raising soil), then a
        // brief reset to rest. The stab is fast; the raise occupies most of the cycle.
        private static float DigThrust(float t)
        {
            if (t < 0.18f) return EaseOut(t / 0.18f);                                    // quick stab in
            if (t < 0.85f) return Mathf.Lerp(1f, 0.12f, EaseInOut((t - 0.18f) / 0.67f)); // slow withdraw while lifting
            return Mathf.Lerp(0.12f, 0f, EaseInOut((t - 0.85f) / 0.15f));                // reset to rest
        }
        private static float DigLift(float t)
        {
            if (t < 0.18f) return 0f;                                                    // flat while stabbing in
            if (t < 0.85f) return EaseInOut((t - 0.18f) / 0.67f);                        // slow raise up and out
            return Mathf.Lerp(1f, 0f, EaseOut((t - 0.85f) / 0.15f));                     // settle back to rest
        }

        // Crank: sweep the handle `sweep` degrees to turn the bolt, then snap back to re-grip.
        private static float CrankAngle(float t, float sweep = 85f)
        {
            if (t < 0.7f) return Mathf.Lerp(0f, sweep, EaseInOut(t / 0.7f));           // turn
            return Mathf.Lerp(sweep, 0f, EaseIn((t - 0.7f) / 0.3f));                   // reposition grip
        }

        // Load: the shell advances 0->1 along the insert path, then a new one starts at 0 next cycle.
        private static float LoadAdvance(float t)
        {
            return t < 0.6f ? EaseInOut(t / 0.6f) : 1f;                  // bring forward and seat
        }
        // Alpha so each shell appears, rides in, then fades as it seats (loaded), then a brief gap.
        private static float LoadAlpha(float t)
        {
            if (t < 0.08f) return t / 0.08f;                            // a fresh shell appears
            if (t < 0.62f) return 1f;                                   // carried into the turret
            if (t < 0.78f) return 1f - (t - 0.62f) / 0.16f;             // seats and fades (loaded)
            return 0f;                                                  // empty hands, then repeat
        }

        private static float EaseOut(float x) => 1f - (1f - x) * (1f - x);
        private static float EaseIn(float x) => x * x;
        // Sharper than EaseIn — used for the axe/pickaxe downstroke so the head accelerates hard into the hit.
        private static float EaseInStrong(float x) => x * x * x;
        private static float EaseInOut(float x) => x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;

        private static void EmitHead(JobToolDef tool, Map map, Vector3 drawLoc)
        {
            Vector3 pos = drawLoc + new Vector3(Rand.Range(-0.28f, 0.28f), 0f, 0.78f);
            if (!pos.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            FleckCreationData d = FleckMaker.GetDataStatic(pos, map, tool.HeadFleck, tool.headScale * Rand.Range(0.85f, 1.15f));
            // Smoke/fume/steam always RISES NORTH (up the screen). velocityAngle is a world-space
            // flat angle (0 = north/screen-up, 90 = east), so a small symmetric spread around 0 keeps
            // the plume drifting straight up regardless of which way the bench or pawn faces (instead
            // of the old 10..80 arc that blew it up-and-to-the-east). Speed varies so it doesn't
            // fire like a metronome on long jobs.
            d.velocityAngle = Rand.Range(-16f, 16f);
            d.velocitySpeed = Rand.Range(0.3f, 0.85f);
            map.flecks.CreateFleck(d);
        }

        // Acid pour stream for the drug-lab flask: drips fall out of the inverted mouth (tip) and a
        // faint blue vapor curls up just below it. tiltFrac (0..1) scales the cadence + size so the
        // pour is heaviest at full inversion and lighter as it rights itself.
        private static void EmitPour(JobToolDef tool, Map map, Vector3 tip, float tiltFrac)
        {
            Vector3 mouth = tip; mouth.y = 0f;
            if (!mouth.ToIntVec3().ShouldSpawnMotesAt(map)) return;

            // Caustic droplet dribbling straight down out of the inverted mouth, arcing and falling.
            // Not every pulse drops one (more likely the more tilted the flask is) so the stream
            // reads as an irregular dribble rather than a steady spout.
            if (tool.PourDripFleck != null && Rand.Chance(0.5f + 0.4f * tiltFrac))
            {
                Vector3 dp = mouth + new Vector3(Rand.Range(-0.05f, 0.05f), 0f, 0.02f);
                dp.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                FleckCreationData d = FleckMaker.GetDataStatic(dp, map, tool.PourDripFleck, Rand.Range(0.28f, 0.45f));
                d.velocityAngle = 180f + Rand.Range(-12f, 12f);   // down the screen
                d.velocitySpeed = Rand.Range(0.35f, 0.7f);
                map.flecks.CreateFleck(d);
            }

            // Faint blue acid vapor curling up off the pour, just below the mouth, rising straight up
            // (velocityAngle is world-space, 0 = screen-up) like the chemical fume.
            if (tool.PourSmokeFleck != null)
            {
                Vector3 sp = mouth + new Vector3(Rand.Range(-0.1f, 0.1f), 0f, Rand.Range(-0.22f, -0.06f));
                sp.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                FleckCreationData s = FleckMaker.GetDataStatic(sp, map, tool.PourSmokeFleck,
                    Rand.Range(0.5f, 0.8f) * (0.7f + 0.5f * tiltFrac));
                s.velocityAngle = Rand.Range(-18f, 18f);
                s.velocitySpeed = Rand.Range(0.25f, 0.55f);
                map.flecks.CreateFleck(s);
            }
        }

        // Thread effects for the needle: snipped ends lie at the impact site, while little wisps
        // fly up into the air and float back down under gravity. Scaled by intensity + debris.
        private static void SpawnThreads(JobToolDef tool, Map map, Vector3 origin)
        {
            origin.y = 0f;
            if (!origin.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            float debrisScale = JobEffectsSettings.DebrisScale;
            float intensity = JobEffectsSettings.Intensity;

            // 1) Larger snipped thread ends that stay at the impact site and fade slowly.
            if (tool.ThreadSnipFleck != null)
            {
                int snipCount = Mathf.Max(1, Mathf.RoundToInt(tool.threadSnipCount * intensity));
                for (int i = 0; i < snipCount; i++)
                {
                    Vector3 p = origin + new Vector3(Rand.Range(-0.06f, 0.06f), 0f, Rand.Range(-0.05f, 0.05f));
                    p.y = 0f;
                    FleckCreationData d = FleckMaker.GetDataStatic(p, map, tool.ThreadSnipFleck,
                        tool.threadSnipScale.RandomInRange * debrisScale);
                    d.velocitySpeed = 0f;
                    d.velocityAngle = 0f;
                    d.rotationRate = Rand.Range(-25f, 25f);
                    map.flecks.CreateFleck(d);
                }
            }

            // 2) Little threads that launch upward and drift down with gravity.
            if (tool.ThreadFleck != null)
            {
                int count = Mathf.Max(1, Mathf.RoundToInt(tool.threadCount * intensity));
                for (int i = 0; i < count; i++)
                {
                    Vector3 p = origin + new Vector3(Rand.Range(-0.05f, 0.05f), 0f, Rand.Range(-0.04f, 0.04f));
                    p.y = 0f;
                    FleckCreationData d = FleckMaker.GetDataStatic(p, map, tool.ThreadFleck,
                        tool.threadScale.RandomInRange * debrisScale);
                    // Upward arc with slight horizontal drift; gravity (def acceleration) pulls them down.
                    d.velocity = new Vector3(
                        Rand.Range(-0.2f, 0.2f),
                        Rand.Range(0.9f, 1.6f),
                        Rand.Range(-0.2f, 0.2f));
                    d.rotationRate = Rand.Range(-40f, 40f);
                    map.flecks.CreateFleck(d);
                }
            }
        }

        private static void SpawnImpact(JobToolDef tool, Map map, Vector3 pawnFlat, Vector3 dir, Thing worked,
            Vector3 tip, Vector3 grip, Vector2 tipOffset)
        {
            Vector3 impact;
            if (tool.impactAtTip)
            {
                // Pin the burst to the tool's tip at the strike (chisel cutting edge, pickaxe point).
                impact = TipSpacePos(tool, tip, grip, dir, tipOffset);
            }
            else
            {
                // Beat slams land further out (the pad lunges onto the fire); fire the gust/embers there.
                float impactReach = tool.swingStyle == SwingStyle.Beat
                    ? tool.reach + tool.scale * 0.4f
                    : tool.reach + tool.scale * 0.8f;
                impact = pawnFlat + dir * impactReach;
            }
            // World screen-down drop: seats the burst on a crosswise head's lower striking face
            // (hammer) for the E/W profile. The head only sits crosswise when the tool is drawn in
            // profile (E/W); at N/S the head rotates to point ALONG the haft, so its contact point
            // is already at the tip and a full world-z drop just shoves the spark below the head.
            // Scale by |dir.x| (horizontal facing component): this is exactly the projection of the
            // head's perpendicular half-width onto world-z. E/W -> full drop, N/S -> none,
            // diagonal -> cosine-correct. Zero by default (impactDrop == 0).
            if (tool.impactDrop != 0f)
            {
                Vector3 dh = new Vector3(dir.x, 0f, dir.z);
                float horiz = dh.sqrMagnitude > 1e-6f ? Mathf.Abs(dh.normalized.x) : 0f;
                impact.z -= tool.impactDrop * horiz;
            }
            impact.y = 0f;
            if (!impact.ToIntVec3().ShouldSpawnMotesAt(map)) return;

            if (tool.ImpactDust != null)
            {
                FleckCreationData dd = FleckMaker.GetDataStatic(impact, map, tool.ImpactDust, Rand.Range(0.5f, 0.9f));
                dd.velocityAngle = Rand.Range(0f, 360f);
                dd.velocitySpeed = Rand.Range(0.2f, 0.6f);
                map.flecks.CreateFleck(dd);
            }

            if (tool.FlashFleck != null)
                FleckMaker.Static(impact, map, tool.FlashFleck, tool.impactFlashScale);

            // Thrown debris, hit-synced (this whole method only runs on the strike crossing).
            // Primary burst, then an optional secondary burst so one tool can fling two kinds of
            // debris per hit (e.g. an axe throwing wood chips AND leaves).
            SpawnBits(map, impact, dir, worked, tool.BitFleck, tool.impactBitCount,
                tool.impactBitSpeed, tool.impactBitScale, tool.impactSpread,
                tool.seasonTintBits, tool.materialTintBits);
            SpawnBits(map, impact, dir, worked, tool.BitFleck2, tool.impactBitCount2,
                tool.impactBitSpeed2, tool.impactBitScale2, tool.impactSpread2,
                tool.seasonTintBits2, tool.materialTintBits2);

            // Settling debris: a fraction of strikes drops litter that lands near the work
            // and fades out on its own (no Thing/filth, so zero tick/GC/save cost).
            if (JobEffectsSettings.GroundLitter && tool.RestFleck != null && Rand.Chance(tool.restChance))
            {
                int rc = Mathf.Max(1, Mathf.RoundToInt(Rand.RangeInclusive(1, Mathf.Max(1, tool.restCountMax)) * JobEffectsSettings.Intensity));
                for (int i = 0; i < rc; i++)
                {
                    Vector3 rp = impact + new Vector3(Rand.Range(-0.35f, 0.35f), 0f, Rand.Range(-0.35f, 0.35f));
                    rp.y = 0f;
                    if (!rp.ToIntVec3().ShouldSpawnMotesAt(map)) continue;
                    FleckMaker.Static(rp, map, tool.RestFleck, Rand.Range(0.5f, 0.9f));
                }
            }
        }

        // One discharge of fire-extinguisher foam, aimed AT the fire's tile (not a forward cone):
        //   1. a short white streak leaves the funnel and travels to the tile ("out of the horn"),
        //   2. a thick near-white foam clump dumps ONTO the tile — scattered blobs that grow, barely
        //      drift, and fade, smothering the flames for a beat,
        //   3. a small gust of glowing embers is knocked loose and flung radially outward across the
        //      tile, fading fast.
        // Scaled by the particle-intensity + debris-size sliders. One call == one beat-fire lunge.
        private static void SpawnSpray(JobToolDef tool, Map map, Vector3 nozzle, Vector3 fireCenter)
        {
            nozzle.y = 0f; fireCenter.y = 0f;
            if (!fireCenter.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            float debrisScale = JobEffectsSettings.DebrisScale;
            float intensity = JobEffectsSettings.Intensity;

            // 1) A tight foam JET from the funnel to the fire tile. Foam droplets are spawned evenly
            //    ALONG the whole nozzle→tile axis in one burst, fanning out in a narrow cone (tight
            //    at the horn, wider at the fire) and growing + slowing as they travel, so the spray
            //    reads as a continuous stream really shooting out of the nozzle and landing in the
            //    centre of the foam cloud — not a loose puff floating in the gap.
            if (tool.BitFleck != null)
            {
                Vector3 toFire = fireCenter - nozzle; toFire.y = 0f;
                float dist = toFire.magnitude;
                float toFireAng = toFire.AngleFlat();
                Vector3 dirN = dist > 0.001f ? toFire / dist : Vector3.forward;
                Vector3 perp = new Vector3(-dirN.z, 0f, dirN.x);   // screen-flat, perpendicular to the jet
                int jet = Mathf.Max(5, Mathf.RoundToInt(tool.impactBitCount * 0.75f * intensity));
                float denom = Mathf.Max(1, jet - 1);
                for (int i = 0; i < jet; i++)
                {
                    // Even spacing 0..1 down the jet with a little jitter -> a continuous line, not clumps.
                    float t = Mathf.Clamp01((i + Rand.Range(-0.4f, 0.4f)) / denom);
                    float spread = Mathf.Lerp(0.025f, 0.17f, t);          // cone: narrow at horn, fans toward fire
                    Vector3 o = nozzle + dirN * (t * dist) + perp * Rand.Range(-spread, spread);
                    o.y = 0f;
                    // Atomised mist at the horn -> fatter blobs as it nears the cloud.
                    float sc = Mathf.Lerp(0.5f, 1.15f, t) * tool.impactBitScale.RandomInRange * debrisScale;
                    FleckCreationData d = FleckMaker.GetDataStatic(o, map, tool.BitFleck, sc);
                    d.velocityAngle = toFireAng + Rand.Range(-9f, 9f);
                    // Fast leaving the horn, easing off as it lands in the cloud.
                    d.velocitySpeed = tool.impactBitSpeed.RandomInRange * Mathf.Lerp(1.25f, 0.45f, t);
                    d.rotationRate = Rand.Range(-90f, 90f);
                    map.flecks.CreateFleck(d);
                }
            }

            // 2) The foam clump dumped ON the tile — thick near-white blobs scattered across the cell
            //    that grow and settle (only a faint outward drift), then fade. Covers the flames.
            if (tool.BitFleck2 != null)
            {
                int blobs = Mathf.Max(2, Mathf.RoundToInt(tool.impactBitCount2 * 0.6f * intensity));
                for (int i = 0; i < blobs; i++)
                {
                    // Spread each blob across the tile so they don't pile up on one center point.
                    Vector3 o = fireCenter + FlatScatter(0.5f);
                    FleckCreationData d = FleckMaker.GetDataStatic(o, map, tool.BitFleck2,
                        tool.impactBitScale2.RandomInRange * debrisScale);
                    d.velocityAngle = Rand.Range(0f, 360f);
                    d.velocitySpeed = tool.impactBitSpeed2.RandomInRange * 0.25f;   // settles, not sprays
                    d.rotationRate = Rand.Range(-40f, 40f);
                    map.flecks.CreateFleck(d);
                }
            }

            // 3) Ember gust — a few glowing embers knocked loose by the foam and flung outward across
            //    the tile, decelerating quickly (speedPerTime) so they disperse around it then fade.
            ResolvePale();
            if (extinguisherEmberFleck != null)
            {
                int embers = Mathf.Max(2, Mathf.RoundToInt(6f * intensity));
                for (int i = 0; i < embers; i++)
                {
                    Vector3 o = fireCenter + FlatScatter(0.18f);
                    FleckCreationData d = FleckMaker.GetDataStatic(o, map, extinguisherEmberFleck,
                        Rand.Range(0.5f, 0.95f) * debrisScale);
                    d.velocityAngle = Rand.Range(0f, 360f);
                    d.velocitySpeed = Rand.Range(1.6f, 3.6f);   // burst radially outward
                    d.rotationRate = Rand.Range(-180f, 180f);
                    map.flecks.CreateFleck(d);
                }
            }
        }

        // Small flat (XZ) random offset for scattering motes across a tile.
        private static Vector3 FlatScatter(float r)
        {
            return new Vector3(Rand.Range(-r, r), 0f, Rand.Range(-r, r));
        }

        // One thrown-debris burst from the strike point. Picks an optional tint: material colour
        // (mining/masonry/build/butcher) or a seasonal hue (foliage tools); both swap to a near-
        // white mote so the per-fleck instanceColor multiply reads true. Scaled by the debris-size
        // slider and the particle-intensity slider.
        private static void SpawnBits(Map map, Vector3 impact, Vector3 dir, Thing worked,
            FleckDef baseFleck, int baseCount, FloatRange speed, FloatRange scale, float spread,
            bool seasonTint, bool materialTint)
        {
            if (baseFleck == null) return;
            // Stumps (chopped/smashed/burned tree remnants) report IsTree==true because their
            // harvestTag is "Wood", but they have no canopy: they shed wood when worked, never
            // leaves. Skip the seasonal/foliage leaf burst on a stump (the primary wood-chip
            // burst, which passes seasonTint==false, still fires).
            if (seasonTint && IsStump(worked)) return;
            FleckDef bit = baseFleck;
            Color? tint = null;
            bool seasonal = false;
            bool foliage = false;
            if (JobEffectsSettings.MaterialChips && materialTint
                && MaterialColor.TryResolveTrunk(worked, out Color tc))
            {
                // Felling a tree: tint the wood chips to the tree's actual trunk colour and swap to
                // the near-white pale wood-chip mote so the per-fleck multiply reads true.
                ResolvePale();
                if (woodChipPaleFleck != null) bit = woodChipPaleFleck;
                tint = tc;
            }
            else if (JobEffectsSettings.MaterialChips && materialTint
                && MaterialColor.TryResolve(worked, out Color mc))
            {
                ResolvePale();
                if (chipPaleFleck != null) bit = chipPaleFleck;
                tint = mc;
            }
            else if (JobEffectsSettings.SeasonalParticles && seasonTint)
            {
                ResolvePale();
                if (leafPaleFleck != null) bit = leafPaleFleck;
                // Prefer the ACTUAL foliage colour of the tree/crop being worked (oak green,
                // pine deep-green, rice green, ...). Fall back to the seasonal palette when the
                // worked thing isn't a plant or its texture can't be sampled.
                if (MaterialColor.TryResolveFoliage(worked, out Color fc))
                {
                    tint = fc;
                    foliage = true;
                }
                else
                {
                    seasonal = true;   // re-rolled per leaf below so autumn looks like a mix
                }
            }

            // Axe felling a TREE: instead of flinging the leaves out of the cut, shake them loose
            // from the tree's own canopy and let them flutter all the way down to the ground. Only
            // the seasonal/foliage (leaf) burst on an actual tree is redirected — wood chips, crop
            // harvests, and every material burst keep the original throw-from-impact behaviour.
            if (seasonTint && TryGetTreeCanopy(worked, out Vector3 canopyCenter, out float canopyW, out float canopyH))
            {
                SpawnLeafFall(map, canopyCenter, canopyW, canopyH, bit, baseCount, scale, seasonal, foliage, tint);
                return;
            }

            int count = Mathf.Max(1, Mathf.RoundToInt(baseCount * JobEffectsSettings.Intensity));
            float reboundBase = (-dir).AngleFlat();
            float debrisScale = JobEffectsSettings.DebrisScale;
            for (int i = 0; i < count; i++)
            {
                // randomGraphics flecks (wood chips, stone shards) pick their sprite from a hash of
                // the spawn position + tick. A whole burst spawned at one exact point would draw the
                // SAME variant; nudge each shard's origin a hair so one strike throws a real mix of
                // the 3 sprites. Single-texture debris (randomGraphics == null) is left untouched.
                Vector3 sp = impact;
                if (bit.randomGraphics != null) sp += FlatScatter(0.06f);
                FleckCreationData d = FleckMaker.GetDataStatic(sp, map, bit, scale.RandomInRange * debrisScale);
                d.velocityAngle = reboundBase + Rand.Range(-spread, spread);
                d.velocitySpeed = speed.RandomInRange;
                d.rotation = Rand.Range(0f, 360f);          // random orientation at spawn, not just spin
                d.rotationRate = Rand.Range(-320f, 320f);
                if (seasonal) d.instanceColor = SeasonalTint.Leaf(map);
                else if (foliage && tint.HasValue) d.instanceColor = JitterLeaf(tint.Value);
                else if (tint.HasValue) d.instanceColor = tint;
                map.flecks.CreateFleck(d);
            }
        }

        // Is the worked thing a tree, and where is its leafy crown? The canopy sits in the upper
        // part of the tree sprite, so we offset up from DrawPos by a fraction of the graphic height
        // and report the sprite footprint so leaves can be sprinkled across the whole crown.
        public static bool TryGetTreeCanopy(Thing worked, out Vector3 center, out float width, out float height)
        {
            center = default; width = 0f; height = 0f;
            if (!(worked is Plant plant) || plant.def?.plant == null || !plant.def.plant.IsTree) return false;
            Vector2 ds = Vector2.one;
            try { if (worked.Graphic != null) ds = worked.Graphic.drawSize; } catch { }
            width = Mathf.Max(1f, ds.x);
            height = Mathf.Max(1f, ds.y);
            Vector3 dp = worked.DrawPos;
            center = new Vector3(dp.x, 0f, dp.z + height * 0.30f);   // crown ~ upper third of the sprite
            return true;
        }

        // Leaves shaken from a tree's canopy: spawned across the crown and given a slow, mostly-
        // downward drift (south = toward the bottom of the screen) with a little sideways sway, so
        // they appear to peel off the foliage and settle on the ground. The dedicated JE_LeafFall*
        // flecks are long-lived and gently damped so they descend the full height of the tree.
        private static void SpawnLeafFall(Map map, Vector3 canopyCenter, float canopyW, float canopyH,
            FleckDef leafBit, int baseCount, FloatRange scale, bool seasonal, bool foliage, Color? tint)
        {
            FleckDef bit = leafBit;
            if (leafBit == leafPaleFleck) { if (leafFallPaleFleck != null) bit = leafFallPaleFleck; }
            else if (leafFallFleck != null) bit = leafFallFleck;

            int count = Mathf.Max(1, Mathf.RoundToInt(baseCount * JobEffectsSettings.Intensity));
            float debrisScale = JobEffectsSettings.DebrisScale;
            float halfW = canopyW * 0.42f;
            float halfH = canopyH * 0.18f;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = canopyCenter + new Vector3(Rand.Range(-halfW, halfW), 0f, Rand.Range(-halfH, halfH));
                p.y = 0f;
                if (!p.ToIntVec3().ShouldSpawnMotesAt(map)) continue;
                FleckCreationData d = FleckMaker.GetDataStatic(p, map, bit, scale.RandomInRange * debrisScale);
                d.velocityAngle = 180f + Rand.Range(-30f, 30f);                 // drift downward
                d.velocitySpeed = Rand.Range(0.45f, 0.8f) + canopyH * 0.12f;    // taller tree -> reach the ground
                d.rotationRate = Rand.Range(-150f, 150f);
                if (seasonal) d.instanceColor = SeasonalTint.Leaf(map);
                else if (foliage && tint.HasValue) d.instanceColor = JitterLeaf(tint.Value);
                else if (tint.HasValue) d.instanceColor = tint;
                map.flecks.CreateFleck(d);
            }
        }

        // True when the worked thing is a tree STUMP (DeadPlant remnant). Stumps are IsTree==true
        // but shed no leaves, so the leaf bursts are suppressed on them.
        private static bool IsStump(Thing t)
            => t is Plant p && p.def?.plant != null && p.def.plant.isStump;

        // Completion timber-fall: when a tree is fully felled, shower leaves down from its (now
        // captured) canopy with the same drift + foliage colour as the per-chop leaves. The Plant
        // is destroyed by the time the completion hook fires, so the canopy geometry and the
        // sampled foliage colour must be captured BEFORE destroy and passed in here.
        public static void SpawnTreeFallLeaves(Map map, Vector3 canopyCenter, float w, float h,
            int baseCount, Color? foliageTint)
        {
            if (map == null) return;
            ResolvePale();
            FleckDef leafBit;
            bool foliage = false, seasonal = false;
            Color? tint = null;
            if (foliageTint.HasValue)
            {
                // Same as the per-chop path: a near-white pale mote multiplied by the tree's
                // actual sampled leaf colour (oak green, pine deep-green, ...).
                leafBit = leafPaleFleck;
                foliage = true;
                tint = foliageTint;
            }
            else if (JobEffectsSettings.SeasonalParticles)
            {
                leafBit = leafPaleFleck;
                seasonal = true;
            }
            else
            {
                leafBit = leafFallFleck;   // colored fall variant, no tint
            }
            // Leaf size: per-chop leaves are 0.56~1.04 (20% off the authored 0.7~1.3); the
            // completion timber-fall shower is halved again (50%) to 0.28~0.52 so the felled-tree
            // leaves read smaller than the chopping ones.
            SpawnLeafFall(map, canopyCenter, w, h, leafBit, baseCount,
                new FloatRange(0.28f, 0.52f), seasonal, foliage, tint);
        }

        // Small per-leaf HSV jitter around the sampled foliage colour so a shower of leaves looks
        // like real foliage (a spread of nearby greens) instead of one flat fill.
        private static Color JitterLeaf(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            h = Mathf.Repeat(h + Rand.Range(-0.03f, 0.03f), 1f);
            s = Mathf.Clamp01(s + Rand.Range(-0.10f, 0.10f));
            v = Mathf.Clamp01(v + Rand.Range(-0.12f, 0.12f));
            Color outc = Color.HSVToRGB(h, s, v);
            outc.a = 1f;
            return outc;
        }

        // Deterministic "is this pawn currently showing an animated tool?" used by the SMYH
        // compat patch to suppress its duplicate resting hands. Mirrors TryResolve's gating but
        // needs no draw position, so render order doesn't matter.
        public static bool HasActiveTool(Pawn pawn) => ActiveDrawnTool(pawn) != null;

        // The single shared gate chain behind HasActiveTool / HasActiveWelder / WantsBenchSoundSync.
        // Returns the effective (post-tech-gate) tool this pawn is ACTUALLY showing right now, or null.
        //
        // Gate order is deliberate and is the whole point of this method — cheapest, most-rejecting
        // test first, so the expensive machinery is never reached by the ~90% of pawns that can't
        // possibly be showing a tool:
        //   1. static bool  2. null  3. job coverage (array read)  4. spawned/dead  5. humanlike
        //   6. the frame+tick memo  7. pather/stance  8. animator cap  9. full resolve
        private static JobToolDef ActiveDrawnTool(Pawn pawn)
        {
            if (!JobEffectsSettings.AnimatedTools) return null;
            if (pawn == null) return null;
            if (!JobCouldHaveTool(pawn)) return null;   // must precede ActivePawnPassesCap (whole-map scan)
            if (!pawn.Spawned || pawn.Dead) return null;
            // Humanlike-only, same as the render gate in OnPawnRendered. Non-humanlikes (animals,
            // Biotech work-mechs, entities) never get a drawn tool, so they must not trip the compat
            // suppression patches that key off this helper — otherwise a work-mech's real equipped
            // item would be hidden with nothing drawn in its place.
            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return null;

            int id = pawn.thingIDNumber;
            int frame = Time.frameCount;
            int tick = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            PawnToolState st = PawnStates.GetOrCreate(id);
            if (st.memoFrame == frame && st.memoTick == tick)
                return st.memoTool;

            JobToolDef tool = null;
            if ((pawn.pather == null || !pawn.pather.MovingNow)
                && (pawn.stances == null || pawn.stances.stunner == null || !pawn.stances.stunner.Stunned)
                && ActivePawnPassesCap(pawn))   // capped-out → report no tool so vanilla equipment draws
            {
                tool = ResolveEffectiveTool(pawn, st);   // shared memoized resolution
            }

            st.memoFrame = frame;
            st.memoTick = tick;
            st.memoTool = tool;
            return tool;
        }

        // True when the ACTIVE animated tool for this pawn is specifically the welder (hand or
        // bench). Used by the Yayo compat patches to strip Yayo's body offset/tilt for welding
        // jobs only, so the welder + hands (anchored to the un-offset body centre) stay glued.
        // Shares ActiveDrawnTool's gate chain + memo (humanlike-only, cap-aware — the Yayo body-strip
        // patches must not fire for a pawn we aren't drawing a welder for). The EFFECTIVE drawn tool
        // (post tech-gate) is what we report on: pre-Electricity the welder is swapped to a hammer, so
        // eff won't be a welder and the Yayo body-strip correctly won't fire.
        public static bool HasActiveWelder(Pawn pawn)
        {
            JobToolDef eff = ActiveDrawnTool(pawn);
            return eff != null && (eff == welderTool.Value || eff == benchWelderTool.Value);
        }

        // True when the pawn's ACTIVE animated tool requests work-sound sync at a workbench (the
        // bench chisels). The SubSustainer.StartSample Harmony hook calls this to decide whether to
        // measure a recipe sustainer's hit cadence for this pawn — so we only time intervals for a
        // pawn actually showing a sync-enabled chisel, not for every working colonist on the map.
        public static bool WantsBenchSoundSync(Pawn pawn)
        {
            JobToolDef eff = ActiveDrawnTool(pawn);   // the actual drawn tool decides whether we sync to the bench foley
            return eff != null && eff.syncToBenchSound;
        }

        // Pawns whose vanilla work-effecter sprayer motes should be hidden this moment (welder
        // feet-sparks). Value = game tick the flag expires; the Harmony patch on
        // SubEffecter_Sprayer.MakeMote checks freshness so a paused/stale render doesn't leak.
        public static bool IsSuppressingWorkMotes(Pawn pawn)
        {
            if (pawn == null) return false;
            // Peek, never create: this runs from the SubEffecter_Sprayer prefix for every sprayer
            // mote on the map, the overwhelming majority from pawns we've never touched.
            PawnToolState st = PawnStates.Peek(pawn.thingIDNumber);
            return st != null && Find.TickManager.TicksGame <= st.suppressWorkMoteUntil;
        }

        // ---- Competing fishing-rod mote suppression (Option A: our animated rod is the only rod) ----
        // The Odyssey vanilla "Fishing" effecter attaches Mote_FishingRod to the pawn, and Vanilla
        // Fishing Expanded's no-Odyssey path attaches its own VCEF_Mote_Fishing_* rod motes. Either
        // would double up with our drawn JE_Tool_FishingRod. We resolve the set of those rod motes
        // once (lazily, after defs load) and the SubEffecter_Sprayer.MakeMote prefix skips any of
        // them while OUR fishing rod is the pawn's active tool — so exactly one rod ever shows.
        // VFE's MECH rods (VCEF_Mote_FishingRod_Mech*) are only ever spawned for colony mechs, and
        // this only fires for humanlikes, so mech fishers keep VFE's rod untouched.
        private static HashSet<ThingDef> fishingRodMotes;
        private static HashSet<ThingDef> FishingRodMotes
        {
            get
            {
                if (fishingRodMotes == null)
                {
                    fishingRodMotes = new HashSet<ThingDef>();
                    ThingDef vanilla = DefDatabase<ThingDef>.GetNamedSilentFail("Mote_FishingRod");
                    if (vanilla != null) fishingRodMotes.Add(vanilla);
                    // VFE (VanillaExpanded.VCEF) no-Odyssey fishing motes: VCEF_Mote_Fishing_* and
                    // VCEF_Mote_FishingRod_Mech* (the latter never reaches the humanlike gate below).
                    List<ThingDef> all = DefDatabase<ThingDef>.AllDefsListForReading;
                    for (int i = 0; i < all.Count; i++)
                    {
                        ThingDef d = all[i];
                        if (d?.defName != null && d.defName.StartsWith("VCEF_Mote_Fishing"))
                            fishingRodMotes.Add(d);
                    }
                }
                return fishingRodMotes;
            }
        }

        // True when this sprayer mote is a vanilla/VFE fishing rod AND our animated fishing rod is
        // the pawn's active drawn tool — in which case the MakeMote prefix drops the competing rod.
        public static bool IsSuppressingFishingMote(Pawn pawn, ThingDef moteDef)
        {
            if (pawn == null || moteDef == null) return false;
            if (!FishingRodMotes.Contains(moteDef)) return false;            // cheap discriminator first
            if (!JobEffectsSettings.AnimatedTools) return false;             // tools off -> let theirs show
            if (!JobCouldHaveTool(pawn)) return false;                       // array read, before the cap's map scan
            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return false;  // mechs keep VFE's rod
            if (!ActivePawnPassesCap(pawn)) return false;                    // capped out -> we draw nothing, keep theirs
            JobToolDef eff = ResolveEffectiveTool(pawn);                     // null/another tool if disabled or tech-gated
            return eff != null && eff == fishingRodTool.Value;
        }

        public static void ClearState(int thingId)
        {
            PawnStates.Forget(thingId);   // anim, resolve cache, memo, draw buffer, foley, beat-fire, work-hit, mote suppression
            SmyhHands.Forget(thingId);
            ArmRenderer.Forget(thingId);
            galleryTools.Remove(thingId);
        }
    }
}

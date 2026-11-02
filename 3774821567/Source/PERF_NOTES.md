# Show Me Your Tools — performance house rules

This mod has **two** hot paths, and the second one is hotter than the first:

1. A **per-frame, per-pawn render Postfix** (`PawnRenderer.RenderPawnAt` → `ToolAnimator.OnPawnRendered`).
   On a busy colony that runs dozens-to-hundreds of times per frame.
2. A **per-TICK, per-pawn compat hook** — `MeleeAnimationCompat.ShouldDrawHook`, invoked from Melee
   Animation's `IdleControllerComp.CompTick()` → `ShouldBeActive()` → `AdditionalShouldBeActiveChecks()`
   for **every humanlike with the comp, on every map, regardless of the camera**. 120 pawns at 3x is
   ~21,600 calls/second, i.e. ~100x the traffic of the render postfix. AM also calls it *before* its
   own `GetMeleeWeapon()` check, so unarmed pawns hit it too.

The rules below are not optional style preferences — they are what keeps frame-time flat.

**Rule of thumb for any new compat hook: find out whether the host calls it from `CompTick` or from a
draw method before you decide what you can afford to do inside it.** Decompile the caller; don't
assume a hook named "ShouldDraw…" runs at draw time.

The runtime is **Unity Mono** (net472 API surface), **JIT**-compiled, and **single-threaded** for
all game logic (tick + render on one thread). That runtime decides which "generic .NET" advice
applies. When someone hands us a backend/ASP.NET optimization checklist, triage it against this,
don't apply it wholesale.

## Applies here (do these)

- **Cache / dictionaries / frame-gates.** Memoize anything constant-per-(def|pawn|frame). We already
  do: `PawnToolState` (all per-pawn state, see below), `Lookup` (jobDef→tools), `techSwapCache`,
  plus `Time.frameCount` gates on the anim clock, LOD, and the animator set.
  New rule of thumb: if a value is asked for more than once per frame and doesn't change within the
  frame, gate it behind `Time.frameCount`.
- **ONE per-pawn store, not twelve.** All per-pawn state lives on `PawnToolState`, fetched through
  `PawnStates.Peek` / `.GetOrCreate`, which front a single dictionary with a **one-entry last-pawn
  memo**. Do NOT add a new `Dictionary<int, …>` keyed by `thingIDNumber` — add a field to
  `PawnToolState` instead. The old layout had `states`, `resolvedCache`, `activeToolMemo`,
  `drawBuffers`, `lastFoleyTime`, `beatFireCounts`, `workHitTime`, `workInterval` and
  `suppressWorkMoteUntil` as separate dictionaries, so one animating pawn paid 10–15 hash+probe
  round trips per frame and the seven `HasActiveTool` callers each re-entered the whole chain.
  Two rules that come with it: **`Peek` on any path that runs for arbitrary pawns** (the render
  gate, `IsSuppressingWorkMotes`) so the ~90% never allocate a state object; and remember that a
  `null` from `Peek` is a *valid memo result*, which is why `Forget`/`Clear` reset `lastId`.
- **Per-pawn state is a CLASS, mutated in place — never write it back.** It used to be a ~130-byte
  `AnimState` **struct** stored by value: every read copied all 130 bytes out and `states[id] = st`
  copied them back, up to three times per pawn per frame (active branch, holster branch, foley edge).
  If you add a big per-pawn record, make it a reference type for exactly this reason.
- **Warm lazy assets off the render path.** Every `cachedX ?? (cachedX = MaterialPool.MatFrom(…))`
  getter is a hitch waiting to land on a gameplay frame — the profiler read 0.044 ms *average* against
  a **17.5 ms max-for-frame**, which was first-touch material creation for a tool nobody had used yet.
  `ToolWarmup` walks every `JobToolDef.WarmCache()` at 2 defs/frame from `GameComponentUpdate`
  (spread deliberately — doing them all at once just moves the hitch to load). Any new lazily-created
  Material/Mesh/Def handle should be touched from `WarmCache` / `ToolAnimator.WarmShared`.
  **Optimize the PEAK frame, not just the average** — the dev overlay now reports both.
- **Kill boxing.** Every hot dictionary is keyed by `int` (`thingIDNumber`), never an enum. Enum-keyed
  `Dictionary<TEnum,…>` boxes the key on *every* lookup under Mono unless you pass an
  `IEqualityComparer<TEnum>`. Key by `int`/`shortHash` instead. `SwingStyle` is only ever used in a
  `switch` (no boxing) — keep it that way.
- **Sentinels over `Nullable<T>` in hot paths.** Use `-1`, `float.NaN`, `> 0f`, or a paired
  `bool …Resolved` flag rather than `int?`/`float?` on anything touched per frame/tick.
- **Ordinal string compares.** Compare internal strings with `string.Equals(a, b, Ordinal[IgnoreCase])`
  or `IndexOf(…, Ordinal…)`, never `a.ToLower() == b.ToLower()` (allocates). Better yet, compare def
  *references* or `shortHash`, not `defName` strings.
- **`StringBuilder` for multi-line strings**, then cache the result. (Only the dev diag HUD builds
  strings today; keep it in `StringBuilder`.)
- **Keep exceptions exceptional.** The `try/catch` wrappers around render/patch bodies are *defensive*
  (one bad pawn must not blow up the whole frame) — that's fine: a `try` that never throws is ~free on
  Mono. Never use exceptions for control flow, and never let a per-frame/per-tick path throw routinely.
- **Allocation-free iteration in draw/tick paths.** Prefer `for` over LINQ / iterator-allocating
  `foreach`. Mono's GC is a non-generational Boehm collector, so per-frame allocations drive GC spikes
  that *grow over a play session* — the classic "it gets choppier the longer I play" symptom.
- **Resolve reflection handles once.** `MethodInfo`/`FieldInfo`/`AccessTools.*` are cached at first use
  (`MineTicksField`), never re-resolved per call.
- **Check whether you need reflection at all.** A cached `FieldInfo` is *not* cheap — `GetValue` is a
  Mono runtime-invoke with argument validation and no inlining, and boxes value types. Before caching a
  handle, confirm the member is actually non-public: `Verse.ThingComp.parent` is a **public** field, and
  the Melee Animation hook burned a `FieldInfo.GetValue` per pawn per tick for years on a wrong comment
  claiming it was protected. Decompile and check.
- **Order gates cheapest-and-most-rejecting first.** `ActiveDrawnTool` is the reference implementation:
  static bool → null → job-coverage array read → spawned/dead → humanlike → memo → pather/stance →
  animator cap → full resolve. The job-coverage test (`JobCouldHaveTool`, a `bool[]` indexed by
  `JobDef.index`) rejects ~90% of pawns in one array read, and it **must** stay ahead of
  `ActivePawnPassesCap`, because that call triggers `EnsureAnimatorSet`'s whole-map scan for the first
  caller of each frame.
  **Audit every entry point against this, not just the one you wrote the rule for.** `OnPawnRendered`
  went years without using `JobCouldHaveTool` at all: it opened with a gallery dictionary probe, the
  LOD calc, then `TryResolve`, which reaches the job table through a *dictionary hash* rather than the
  array. Every idle/hauling/eating colonist on screen paid the full chain to be told "no". The gate is
  now the first thing past the humanlike check, with `NeedsIdleWork` as the escape hatch for a pawn
  that still owes a stow-foley edge or a holster fade.
- **A cheap gate must not strand deferred work.** When you add an early return, ask what state the
  skipped code was responsible for *unwinding*. `shownLastFrame` drives an edge-triggered sound, so
  rejecting a pawn the frame after it stopped working would desync the edge permanently and kill the
  next job's pickup clatter. Note also that `NeedsIdleWork` tests `HolsterTools && lastTool != null`
  rather than `lastTool != null`: nothing clears `lastTool` when holstering is off, and *clearing* it
  isn't an option either because a `null` there makes `toolChanged` true every time a worker takes a
  step, restarting frame-animated tools.
- **Index by `Def.index`, not `defName`.** `Def.index` is a contiguous `ushort` assigned by
  `DefDatabase`, so it makes a perfect array index or int dictionary key. `jobLookup` is keyed by
  `JobDef.index`; the authored XML strings are resolved to defs exactly once in `BuildLookup`.
- **Memoize the ANSWER, not just the inputs, when many callers ask the same question.** `HasActiveTool`
  has seven callers (Melee Animation per tick, the DrawEquipment prefix, SMYH hands, beat-fire lunge,
  carried-medicine, both Yayo patches). `activeToolMemo` stamps the result with **both** `Time.frameCount`
  **and** `TicksGame`: the tick stamp catches job/stance changes, the frame stamp catches camera pans
  (which move the animator-cap set). Either alone would be wrong.
- **A guarded patch is still a called patch — measure CALLS/FRAME, not ms.** DPA's *Av Calls Per Frame*
  column is the one that matters. `Patch_SkygazeStandUp` profiled at 1386 calls/frame / 0.03us per
  call: that 0.03us is Harmony dispatch, not the body, which already returned on line 1. **No guard
  clause can reduce a patch below its dispatch cost.** If a patch sits on a universal chokepoint
  (`PawnUtility.GetPosture`, `Pawn.Tick`, `HediffSet.HasHead`…) the only lever is to not have it
  applied. Three escalating tools, in order of preference:
    1. `static bool Prepare()` — load-time decision, patch never applied (see the Yayo patches).
    2. `harmony.Patch` / `harmony.Unpatch` driven by settings (see `SkygazePatch.Sync`).
    3. `harmony.Patch` / `harmony.Unpatch` driven by *demand* — installed only while some pawn is
       actually doing the thing (see `SkygazePatch.Tick`). Needs a cheap demand signal on a colder
       method (`Pawn_JobTracker.StartJob` is ~400x colder than `GetPosture`) plus hysteresis.
  **Always call Harmony's runtime patch/unpatch from a tick/component context, never from inside
  another patch body** — patching a method that may be on the stack is the one genuinely unsafe use
  of the runtime API. `SkygazePatch.NotifyJobStarted` only raises a flag; `Tick()` does the work.
- **Per-tool settings must gate the code that serves that tool.** `Patch_SkygazeStandUp` checked only
  the global `AnimatedTools` for years, so disabling *just* the spyglass left it running 1386x/frame
  for a tool that could never draw. When adding any patch that exists for ONE tool, gate it on
  `JobEffectsSettings.IsToolEnabled("<its defName>")`, not only the master switch.
- **Deregister, don't early-return, when a feature is off.** If a host mod holds a public delegate list
  (`IdleControllerComp.ShouldDrawAdditional`), remove your entry when your setting is disabled
  (`MeleeAnimationCompat.SyncRegistration`, re-synced from `Mod.WriteSettings`) so the cost is zero
  rather than "one bool read, 20k times a second". Mutating the host's list is safe from `WriteSettings`
  because settings are written in `OnGUI` while the host enumerates in `Update` — they never interleave.

## Does NOT apply here (reject, with a reason)

- **`async` / `await` / `ValueTask` / `Task.Run` / `ThreadPool` for game logic — UNSAFE.**
  `Thing`/`Pawn`/`Map`/`Def` and all Verse/RimWorld API are main-thread-only and not thread-safe;
  touching them off-thread races the tick loop → state corruption or native Mono crash. The RimWorld
  substitute for "async" is **tick/frame spreading**: `IsHashIntervalTick`, `TickRare`/`TickLong`,
  staggered work across ticks (we stagger the resolve refresh by `id % 13`). The *only* safe
  off-thread work is pure CPU over an **immutable snapshot**, results published back via a `volatile`
  reference swap for the main thread to apply — the 1.6 `ParallelPreDraw` contract (that's the whole
  point of Stage 3's worker pre-pass).
- **AOT / IL2CPP — not a lever.** Desktop RimWorld is Mono+JIT; mods ship net472 IL that's JITed at
  load. There's no AOT step we control. The only transferable crumb is "don't do `Reflection.Emit` /
  runtime codegen on hot paths" — which we don't.
- **SqlClient / async-DB advice — irrelevant.** No database exists. Persistence is Scribe XML.

## Existing throttles (don't regress these)

- **Cheap-first render gate** (`JobCouldHaveTool` + `NeedsIdleWork` at the top of `OnPawnRendered`).
- **Gallery `Count != 0` short-circuits** (`IsGalleryPawn`/`GalleryToolFor`): dev-only dictionaries
  that are empty in all real play, probed for every rendered humanlike every frame.

- **Zoom LOD** (`CurrentLod`): cull tools at Far zoom, drop forearms at Middle.
- **Animator cap** (`EnsureAnimatorSet`): only the nearest N active workers draw the full tool on big
  colonies; everyone else falls back to vanilla.
- **30 Hz pose-recompute throttle + retained draw list** (`DrawEntry`/`captureSink`/`drawBuffers`):
  compute+capture on compute frames, replay on skip frames. This is the compute/submit split the
  worker pre-pass extends.

## Measuring (Stage 4)

Turn on **Settings → Developer → Perf diagnostics overlay** (dev mode only). It shows:

- `render … ms/frame` **and `peak … ms`** — the peak is the one that maps to felt stutter; the two can
  differ by 400x. Toggling the checkbox resets it.
- `considered` (humanlikes the postfix ran for) → `passed` (survived the cheap reject) → `animating` /
  `holster`. The considered→passed ratio is the health check on the gate ordering; if `passed` tracks
  `considered` on a busy map, the gate has stopped rejecting and something upstream regressed.
- `cap` / `capped` / `throttle` / `LOD` / `warm` (material warm-up progress).

All instrumentation is gated on `Diag.Enabled`, so it costs a single static-bool read per pawn when
off. Optimize against these numbers, not guesses.

using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JobEffects
{
    /// <summary>
    /// Dev-only "tool gallery": spawns one clean model colonist and cycles it through every
    /// JobToolDef on command (or auto-advances ~1/sec), bypassing the job system via
    /// ToolAnimator.SetGallery. Built for capturing per-tool footage and instantly spotting a
    /// broken swing style after a refactor. Open via the debug browser: bug icon → Actions tab →
    /// "Show Me Your Tools" → "Tool gallery (cycle every tool)".
    ///
    /// A companion "Tool lineup (all tools at once)" spawns one frozen pawn per tool in a grid so
    /// every tool can be eyeballed simultaneously. Both share a body-type selector and an idle-
    /// holster toggle so the body-type-aware swing AND belt fit can be inspected on every build.
    /// </summary>
    public static class ToolGallery
    {
        // Body types offered by both dialogs. Order = how the buttons read left→right.
        public static readonly string[] BodyTypeNames = { "Male", "Female", "Thin", "Fat", "Hulk", "Child" };

        public static void SetBodyType(Pawn p, string name)
        {
            if (p?.story == null) return;
            BodyTypeDef bt = DefDatabase<BodyTypeDef>.GetNamedSilentFail(name);
            if (bt == null) return;
            p.story.bodyType = bt;
            p.Drawer?.renderer?.SetAllGraphicsDirty();
        }

        // Keep a frozen model planted and facing the gallery direction (re-issued each frame so the
        // AI never wanders off to a real job or rotates the body away from the chosen facing).
        public static void HoldPosture(Pawn pawn)
        {
            if (pawn?.jobs == null) return;
            if (pawn.CurJobDef != JobDefOf.Wait_MaintainPosture)
            {
                Job job = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
                job.expiryInterval = -1;
                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            }
            pawn.Rotation = ToolAnimator.GalleryFacing;
        }

        // A clean pad: nuke plants / filth in a radius so the model(s) stand on bare ground.
        public static void ClearPad(Map map, IntVec3 center, float radius)
        {
            foreach (IntVec3 c in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!c.InBounds(map)) continue;
                foreach (Thing t in c.GetThingList(map).ToList())
                    if (t.def.category == ThingCategory.Plant || t is Filth) t.Destroy();
            }
        }

        [DebugAction("Show Me Your Tools", "Tool gallery (cycle every tool)",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void OpenGallery()
        {
            Map map = Find.CurrentMap;
            if (map == null) { Messages.Message("No current map.", MessageTypeDefOf.RejectInput, false); return; }

            var tools = DefDatabase<JobToolDef>.AllDefsListForReading
                .OrderBy(t => t.LabelNice).ToList();
            if (tools.Count == 0) { Messages.Message("No JobToolDefs loaded.", MessageTypeDefOf.RejectInput, false); return; }

            // A clean pad near map center so the model stands on bare ground with nothing behind it.
            // Robust: some generated maps have an impassable centre (water/rock). StandableCellNear
            // returns IntVec3.Invalid when nothing's in radius, and GenSpawn rejects that ("out of
            // bounds at (-1000,-1000,-1000)") so the model never spawns. Widen the search; bail if
            // even a whole-map scan finds nothing.
            if (!TryFindOpenCell(map, map.Center, out IntVec3 cell))
            {
                Messages.Message("Tool gallery: couldn't find an open cell to place the model.",
                    MessageTypeDefOf.RejectInput, false);
                return;
            }
            ClearPad(map, cell, 1.5f);

            Pawn model = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
            GenSpawn.Spawn(model, cell, map);
            ToolAnimator.GalleryFacing = Rot4.South;
            ToolAnimator.GalleryHolster = false;
            model.Rotation = Rot4.South;

            CameraJumper.TryJump(model);
            if (!JobEffectsSettings.AnimatedTools)
                Messages.Message("Tool gallery: 'Animated tools' is OFF in mod settings — turn it on to see the swing.",
                    MessageTypeDefOf.CautionInput, false);

            Find.WindowStack.Add(new Dialog_ToolGallery(model, tools));
        }

        // Find a standable, unfogged cell near `preferred`, falling back to a near-radius search
        // and then a whole-map scan, so the gallery/showcase never hands GenSpawn an Invalid cell
        // on a map whose centre is impassable (water/rock/fog). Returns false only if the entire
        // map has no open cell (effectively never on a playable map).
        public static bool TryFindOpenCell(Map map, IntVec3 preferred, out IntVec3 cell)
        {
            cell = preferred;
            if (preferred.InBounds(map) && preferred.Standable(map) && !preferred.Fogged(map)) return true;
            if (CellFinder.TryFindRandomCellNear(preferred, map, 30,
                    c => c.Standable(map) && !c.Fogged(map), out cell)) return true;
            return CellFinderLoose.TryGetRandomCellWith(
                c => c.Standable(map) && !c.Fogged(map), map, 1000, out cell);
        }

        // Generate a fitting model for a tool's lineup slot and spawn it at `cell` facing south.
        // Most tools get a stock adult colonist. The baby toys (holdToy) read as a child's item, so
        // they get a CHILD model (Biotech only) — a child renders standing, and with the calibrated
        // Child shoulders the kid grips the toy with body-scaled hands + forearms (noHandsBabyOnly
        // keeps real babies hand-less), the toy itself shrinking to child scale. Falls back to an
        // adult if child generation isn't available (no Biotech) or throws.
        public static Pawn MakeGalleryPawn(JobToolDef tool, Map map, IntVec3 cell)
        {
            Pawn p = null;
            if (tool != null && tool.holdToy && ModsConfig.BiotechActive)
            {
                try
                {
                    var req = new PawnGenerationRequest(
                        PawnKindDefOf.Colonist, Faction.OfPlayer,
                        forceGenerateNewPawn: true,
                        canGeneratePawnRelations: false,
                        developmentalStages: DevelopmentalStage.Child,
                        fixedBiologicalAge: 8f);
                    p = PawnGenerator.GeneratePawn(req);
                }
                catch { p = null; }
            }
            if (p == null)
                p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
            GenSpawn.Spawn(p, cell, map);
            p.Rotation = Rot4.South;
            return p;
        }

        [DebugAction("Show Me Your Tools", "Tool lineup (all tools at once)",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void OpenLineup()
        {
            Map map = Find.CurrentMap;
            if (map == null) { Messages.Message("No current map.", MessageTypeDefOf.RejectInput, false); return; }

            var allTools = DefDatabase<JobToolDef>.AllDefsListForReading
                .OrderBy(t => t.LabelNice).ToList();
            if (allTools.Count == 0) { Messages.Message("No JobToolDefs loaded.", MessageTypeDefOf.RejectInput, false); return; }

            ToolAnimator.ClearGallery();
            ToolAnimator.GalleryFacing = Rot4.South;
            ToolAnimator.GalleryHolster = false;

            IntVec3 origin = map.Center;

            // Workbench-specific tools (a DoBill tool bound to particular benches) get a REAL station
            // below the grid: a powered, forever-billed bench with a worker parked at it, so they
            // animate by doing the actual job. Everything else is shown as a frozen pose in the grid.
            var benchTools = allTools.Where(IsBenchTool).ToList();
            var galleryTools = allTools.Where(t => !IsBenchTool(t)).ToList();

            var workers = new List<Pawn>();
            var props = new List<Thing>();
            var failed = new List<JobToolDef>();
            if (benchTools.Count > 0)
                ShowcaseSetup.BuildLineupBenchStations(map, new IntVec3(origin.x - 34, 0, origin.z - 16),
                    benchTools, workers, props, failed);
            // Any bench tool that couldn't be stationed falls back to a frozen pose.
            galleryTools.AddRange(failed);

            int perRow = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(galleryTools.Count)));   // roughly square grid
            int rows = Mathf.CeilToInt(galleryTools.Count / (float)perRow);
            ClearPad(map, origin, perRow + 4f);

            var galleryPawns = new List<Pawn>();
            for (int i = 0; i < galleryTools.Count; i++)
            {
                int col = i % perRow;
                int row = i / perRow;
                // 3 cells apart laterally, 4 rows apart vertically so neither bodies nor the tools
                // that hang off their hips overlap.
                IntVec3 cell = new IntVec3(origin.x - perRow * 3 / 2 + col * 3, 0, origin.z + rows * 2 - row * 4);
                if ((!cell.InBounds(map) || !cell.Standable(map))
                    && !TryFindOpenCell(map, origin, out cell))
                    continue;   // no open cell anywhere -> skip rather than spawn at Invalid

                Pawn p = MakeGalleryPawn(galleryTools[i], map, cell);
                ToolAnimator.AddGalleryPawn(p, galleryTools[i]);
                galleryPawns.Add(p);
            }

            if (galleryPawns.Count > 0) CameraJumper.TryJump(galleryPawns[0]);
            else if (workers.Count > 0) CameraJumper.TryJump(workers[0]);

            if (!JobEffectsSettings.AnimatedTools)
                Messages.Message("Tool lineup: 'Animated tools' is OFF in mod settings — turn it on to see the swings.",
                    MessageTypeDefOf.CautionInput, false);
            if (workers.Count > 0)
                Messages.Message($"Tool lineup: {galleryPawns.Count} posed + {workers.Count} working a real forever-billed bench (below the grid). Give them a moment to walk over and start.",
                    MessageTypeDefOf.NeutralEvent, false);

            var extra = new List<Thing>();
            extra.AddRange(workers);
            extra.AddRange(props);
            Find.WindowStack.Add(new Dialog_ToolLineup(galleryPawns, allTools.Count, extra));
        }

        // A bench-specific tool: a DoBill tool bound to one or more workbenches (NOT a surgery tool,
        // and NOT a FinishFrame tool like the grave shovel that only uses workbenchDefs as a frame
        // filter). These get a real working bench in the lineup instead of a frozen pose.
        private static bool IsBenchTool(JobToolDef t)
        {
            return t != null
                && !t.surgeryOnly
                && t.jobDefs != null && t.jobDefs.Contains("DoBill")
                && t.workbenchDefs != null && t.workbenchDefs.Count > 0;
        }

        // ---- Shared dialog widgets ----

        // Facing N/E/S/W picker. Returns the height consumed.
        public static void FacingRow(Listing_Standard l)
        {
            l.Label("Facing  (↑/↓ to rotate)");
            Rect rotRow = l.GetRect(30f);
            Rot4[] dirs = { Rot4.North, Rot4.East, Rot4.South, Rot4.West };
            string[] names = { "N", "E", "S", "W" };
            float qw = (rotRow.width - 12f) / 4f;
            for (int i = 0; i < 4; i++)
            {
                Rect r = new Rect(rotRow.x + (qw + 4f) * i, rotRow.y, qw, rotRow.height);
                bool active = ToolAnimator.GalleryFacing.AsInt == dirs[i].AsInt;
                if (active) Widgets.DrawBoxSolid(r, new Color(0.25f, 0.5f, 0.3f, 0.65f));
                if (Widgets.ButtonText(r, names[i])) ToolAnimator.GalleryFacing = dirs[i];
            }
        }

        // Body-type picker. Highlights the currently-selected name; calls onPick when changed.
        public static void BodyTypeRow(Listing_Standard l, string current, System.Action<string> onPick)
        {
            l.Label("Body type");
            Rect row = l.GetRect(28f);
            float bw = (row.width - (BodyTypeNames.Length - 1) * 3f) / BodyTypeNames.Length;
            for (int i = 0; i < BodyTypeNames.Length; i++)
            {
                Rect r = new Rect(row.x + (bw + 3f) * i, row.y, bw, row.height);
                bool active = current == BodyTypeNames[i];
                if (active) Widgets.DrawBoxSolid(r, new Color(0.25f, 0.4f, 0.55f, 0.65f));
                if (Widgets.ButtonText(r, BodyTypeNames[i])) onPick(BodyTypeNames[i]);
            }
        }
    }

    public class Dialog_ToolGallery : Window
    {
        private readonly Pawn pawn;
        private readonly List<JobToolDef> tools;
        private int index;
        private string bodyType = "Male";

        private bool autoAdvance = true;
        private float interval = 1f;      // seconds between tools when auto-advancing
        private float sinceAdvance;

        public Dialog_ToolGallery(Pawn pawn, List<JobToolDef> tools)
        {
            this.pawn = pawn;
            this.tools = tools;
            this.bodyType = pawn?.story?.bodyType?.defName ?? "Male";
            draggable = true;
            doCloseX = true;
            preventCameraMotion = false;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = false;
            forcePause = false;
            SelectTool(0);
        }

        public override Vector2 InitialSize => new Vector2(360f, 430f);

        private JobToolDef Current => tools[index];

        private void SelectTool(int i)
        {
            index = ((i % tools.Count) + tools.Count) % tools.Count;   // wrap both directions
            sinceAdvance = 0f;
            ToolAnimator.SetGallery(pawn, Current);
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            // Model gone (despawned / killed) → bail.
            if (pawn == null || !pawn.Spawned || pawn.Dead) { Close(); return; }

            ToolGallery.HoldPosture(pawn);

            if (autoAdvance && !Find.TickManager.Paused)
            {
                sinceAdvance += Time.deltaTime;
                if (sinceAdvance >= interval) SelectTool(index + 1);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Arrow-key navigation: ←/→ step tools, ↑/↓ rotate the model.
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.RightArrow) { SelectTool(index + 1); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.LeftArrow) { SelectTool(index - 1); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.UpArrow) { ToolAnimator.GalleryFacing = ToolAnimator.GalleryFacing.Rotated(RotationDirection.Clockwise); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.DownArrow) { ToolAnimator.GalleryFacing = ToolAnimator.GalleryFacing.Rotated(RotationDirection.Counterclockwise); Event.current.Use(); }
            }

            var l = new Listing_Standard();
            l.Begin(inRect);

            Text.Font = GameFont.Medium;
            l.Label($"{index + 1} / {tools.Count}  —  {Current.LabelNice}");
            Text.Font = GameFont.Small;
            l.Label($"Swing style: {Current.swingStyle}    (←/→ to step)");
            l.Gap(6f);

            Rect btnRow = l.GetRect(34f);
            if (Widgets.ButtonText(btnRow.LeftHalf().ContractedBy(2f), "◀ Prev")) SelectTool(index - 1);
            if (Widgets.ButtonText(btnRow.RightHalf().ContractedBy(2f), "Next ▶")) SelectTool(index + 1);
            l.Gap(8f);

            // Facing: rotate the model N/E/S/W to inspect each tool from every direction.
            ToolGallery.FacingRow(l);
            l.Gap(6f);

            // Body type: re-render the model with each silhouette to verify the body-type-aware
            // swing + holster fit on every build.
            ToolGallery.BodyTypeRow(l, bodyType, n => { bodyType = n; ToolGallery.SetBodyType(pawn, n); });
            l.Gap(8f);

            l.CheckboxLabeled("Auto-advance", ref autoAdvance,
                "Step to the next tool automatically. Pauses while the game is paused (tools freeze when paused).");
            l.Label($"Interval: {interval:0.0}s");
            interval = l.Slider(interval, 0.5f, 3f);
            l.Gap(4f);

            bool holster = ToolAnimator.GalleryHolster;
            l.CheckboxLabeled("Show holster pose", ref holster,
                "Render the tool tucked on the belt in its idle holster pose instead of the active swing — for inspecting the body-type-aware belt fit. Some tools (medicine kits / scratch poses) draw nothing on the belt by design.");
            ToolAnimator.GalleryHolster = holster;

            bool fx = ToolAnimator.GalleryEffects;
            l.CheckboxLabeled("Effects (debris / glints)", ref fx,
                "Live A/B of the particle layer for this model only — does not change your saved settings.");
            ToolAnimator.GalleryEffects = fx;

            l.End();
        }

        public override void PostClose()
        {
            base.PostClose();
            ToolAnimator.ClearGallery();
            // Remove the throwaway model so it doesn't linger as a stray colonist.
            if (pawn != null && pawn.Spawned && !pawn.Dead) pawn.Destroy();
        }
    }

    /// <summary>
    /// All tools at once: a frozen pawn per tool laid out in a grid. The controls (facing, body
    /// type, holster, effects) apply to the WHOLE row so you can sweep every tool across every
    /// build / pose in one glance. Closing the dialog clears the assignments and despawns the row.
    /// </summary>
    public class Dialog_ToolLineup : Window
    {
        private readonly List<Pawn> galleryPawns;   // frozen posed pawns (one per non-bench tool)
        private readonly int totalToolCount;
        private readonly List<Thing> extra;          // worker pawns + benches + props, for cleanup
        private string bodyType = "Male";

        public Dialog_ToolLineup(List<Pawn> galleryPawns, int totalToolCount, List<Thing> extra)
        {
            this.galleryPawns = galleryPawns ?? new List<Pawn>();
            this.totalToolCount = totalToolCount;
            this.extra = extra ?? new List<Thing>();
            draggable = true;
            doCloseX = true;
            preventCameraMotion = false;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = false;
            forcePause = false;
        }

        public override Vector2 InitialSize => new Vector2(360f, 270f);

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            bool anyPosed = false;
            foreach (Pawn p in galleryPawns)
            {
                if (p == null || !p.Spawned || p.Dead) continue;
                anyPosed = true;
                ToolGallery.HoldPosture(p);   // workers are NOT held — they do the real bench job
            }
            // Keep the window open as long as anything we spawned is still alive.
            if (!anyPosed && !extra.Any(t => t is Pawn wp && wp.Spawned && !wp.Dead)) Close();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.UpArrow) { ToolAnimator.GalleryFacing = ToolAnimator.GalleryFacing.Rotated(RotationDirection.Clockwise); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.DownArrow) { ToolAnimator.GalleryFacing = ToolAnimator.GalleryFacing.Rotated(RotationDirection.Counterclockwise); Event.current.Use(); }
            }

            var l = new Listing_Standard();
            l.Begin(inRect);

            Text.Font = GameFont.Medium;
            l.Label($"Tool lineup — {totalToolCount} tools");
            Text.Font = GameFont.Small;
            l.Label("Bench tools work a real forever-billed bench below the grid.");
            l.Gap(4f);

            // Facing / body type / holster apply to the POSED pawns only; the bench workers do their
            // real job and face their bench.
            ToolGallery.FacingRow(l);
            l.Gap(6f);

            ToolGallery.BodyTypeRow(l, bodyType, n =>
            {
                bodyType = n;
                foreach (Pawn p in galleryPawns)
                {
                    // Keep childcare-prop models as kids — don't swap a child/baby silhouette to an
                    // adult body type when sweeping the row.
                    string bt = p?.story?.bodyType?.defName;
                    if (bt == "Child" || bt == "Baby") continue;
                    ToolGallery.SetBodyType(p, n);
                }
            });
            l.Gap(8f);

            bool holster = ToolAnimator.GalleryHolster;
            l.CheckboxLabeled("Show holster pose", ref holster,
                "Render every posed tool tucked on the belt in its idle holster pose instead of the active swing.");
            ToolAnimator.GalleryHolster = holster;

            bool fx = ToolAnimator.GalleryEffects;
            l.CheckboxLabeled("Effects (debris / glints)", ref fx,
                "Live A/B of the particle layer for the whole row — does not change your saved settings.");
            ToolAnimator.GalleryEffects = fx;

            l.End();
        }

        public override void PostClose()
        {
            base.PostClose();
            ToolAnimator.ClearGallery();
            foreach (Pawn p in galleryPawns)
                if (p != null && p.Spawned && !p.Dead) p.Destroy();
            // Despawn the working pawns, benches, conduits and batteries we spawned for the stations.
            foreach (Thing t in extra)
                if (t != null && t.Spawned && !t.Destroyed) t.Destroy();
        }
    }
}

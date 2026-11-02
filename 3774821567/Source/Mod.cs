using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace JobEffects
{
    public class JobEffectsMod : Mod
    {
        private Vector2 scrollPos;
        private List<JobToolDef> sortedTools;

        public JobEffectsMod(ModContentPack content) : base(content)
        {
            JobEffectsSettings.Instance = GetSettings<JobEffectsSettings>();
        }

        public override string SettingsCategory() => "Show Me Your Tools";

        // Settings just got written (window closed / applied). Force every pawn to re-resolve its tool
        // on the next frame so toggling a tool, the tech-gate, etc. takes effect immediately instead of
        // waiting for the cache's staggered safety refresh.
        public override void WriteSettings()
        {
            base.WriteSettings();
            ToolAnimator.BumpResolveEpoch();
            // Add/remove our delegate from Melee Animation's per-tick list to match the new settings,
            // so a disabled feature costs literally zero rather than a bool check 20k times a second.
            MeleeAnimationCompat.SyncRegistration();
            // Same principle for the Skygaze posture patch: disabling the spyglass UNPATCHES
            // PawnUtility.GetPosture rather than leaving a no-op postfix on a ~1,400-call/frame method.
            SkygazePatch.Sync();
        }

        private List<JobToolDef> SortedTools
        {
            get
            {
                if (sortedTools == null)
                    sortedTools = DefDatabase<JobToolDef>.AllDefs
                        .OrderBy(t => t.LabelNice).ToList();
                return sortedTools;
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            JobEffectsSettings s = JobEffectsSettings.Instance;

            // The per-tool list makes the panel taller than the window; scroll the whole thing.
            // Height: fixed top section (~590) + one row (~28) per tool + headroom.
            float viewHeight = 800f + SortedTools.Count * 28f;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 24f, viewHeight);
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);

            Listing_Standard l = new Listing_Standard();
            l.Begin(viewRect);

            // Reset-to-defaults: right-aligned button at the top of the panel.
            Rect resetRow = l.GetRect(30f);
            if (Widgets.ButtonText(new Rect(resetRow.xMax - 160f, resetRow.y, 160f, resetRow.height),
                    "Reset to defaults"))
                s.ResetToDefaults();
            l.Gap(6f);

            l.CheckboxLabeled("Animated tools (pickaxe, axe, spoon, …)", ref s.animatedTools,
                "Draw a swinging/stirring tool over colonists as they work.");
            l.CheckboxLabeled("Override other tool mods", ref s.overrideToolMods,
                "When our animated tool covers a job, hide other mods' equipped-tool draw for that job "
                + "(Dark Ages: Medieval Tools, Melee Animation, Tools O' Plenty, carry-openly mods) so only "
                + "our tool shows. Jobs we don't cover are left untouched.");
            if (JobEffectsSettings.SmyhActive)
            {
                string handTip = "Draw the colonist's hands holding the animated tool, using Show Me Your Hands' hand art.";
                l.CheckboxLabeled("Hands grip the tools (Show Me Your Hands)", ref s.handsOnTools, handTip);
                l.CheckboxLabeled("Forearms", ref s.drawArms,
                    "Draw a translucent, body-coloured forearm on each hand gripping the tool. It fades out at "
                    + "the elbow, flexes with the swing, and takes the colour of their jacket (then shirt, then "
                    + "bare skin). Requires Show Me Your Hands.");
                if (s.drawArms)
                {
                    l.CheckboxLabeled("   \u2514 Also on weapons & carried items", ref s.forearmsOnHands,
                        "Extend the forearm onto every hand Show Me Your Hands draws \u2014 equipped weapons, "
                        + "carried items and idle hands \u2014 not just the animated work tools.");
                    l.Label($"Forearm opacity: {s.armOpacity * 100f:0}%");
                    s.armOpacity = l.Slider(s.armOpacity, 0f, 1f);
                }
            }
            l.CheckboxLabeled("Completion & event bursts", ref s.completionEffects,
                "Timber fall, harvest pop, research breakthrough, effort puffs.");
            l.Gap();
            l.Label($"Particle intensity: {s.intensity:0.0}x");
            s.intensity = l.Slider(s.intensity, 0.2f, 2f);
            l.Label($"Debris size: {s.debrisScale:0.0}x");
            s.debrisScale = l.Slider(s.debrisScale, 0.1f, 1.5f);

            // --- Tech level gate ---
            l.GapLine();
            Text.Font = GameFont.Small;
            l.Label("Tech level");
            l.CheckboxLabeled("Gate modern tools behind Electricity research", ref s.gateModernTools,
                "Until your colony researches Electricity, tools that don't fit a pre-industrial "
                + "settlement are swapped for period-correct ones:\n"
                + "  \u2022 Metal construction (and repairs) use the hammer instead of the welder.\n"
                + "  \u2022 Smithing at the forge uses a hammer & anvil instead of the welder.\n"
                + "  \u2022 Fires are beaten out with a broom instead of a fire extinguisher.\n"
                + "Stone always uses the chisel and wood the hammer. Once Electricity is researched, "
                + "every tool returns to normal. Turn this off to always use the modern tools.");

            l.GapLine();
            Text.Font = GameFont.Small;
            l.Label("Immersion");
            l.CheckboxLabeled("Seasonal particle tint", ref s.seasonalParticles,
                "Leaves and soil flecks recolor with the season — fresh green in spring, amber in autumn, pale in winter.");
            l.CheckboxLabeled("Material-aware chips", ref s.materialChips,
                "Chips and shards take the color of whatever is being worked (granite, marble, jade, wood species, \u2026).");
            l.CheckboxLabeled("Settling debris", ref s.groundLitter,
                "A few embers and chips land near the work and briefly linger before fading.");
            l.CheckboxLabeled("Tool holstering", ref s.holsterTools,
                "When a colonist stops working, the tool rests at their hip for a moment, then fades away. "
                + "Also rides on their belt while they walk to a job it covers.");
            l.CheckboxLabeled("Tool handling sounds", ref s.toolFoley,
                "A short handle clatter as a colonist pulls a tool out to start work and stows it when done.");
            l.CheckboxLabeled("Tools share the colonist's depth", ref s.toolsMatchPawnDepth,
                "Draw tools and hands at the colonist's own depth (like a held weapon) instead of always "
                + "on top, so anything that covers the colonist — a tall object, an overhead — covers the "
                + "tool too. Turn off to keep tools always drawn on top of everything.");
            l.CheckboxLabeled("Reduce tool detail when zoomed out", ref s.zoomLod,
                "Performance: stop drawing tools entirely when the camera is zoomed far out (the sprites "
                + "are only a few pixels there) and drop the translucent forearms at medium zoom. No "
                + "visible change when you're zoomed in to watch your colonists. Big FPS win on large colonies.");
            l.Gap();
            l.Label($"Max colonists animating tools at once: {s.maxAnimatedPawns}");
            // Perf cap for big colonies: when more than this many colonists are on screen and working at
            // once, only the ones nearest the camera show animated tools; the rest fall back to vanilla
            // (no tool, hands, forearms, effects or motes). Lower = faster. 200 = effectively unlimited.
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            l.Label("Nearest the camera win; the rest fall back to vanilla. Lower = faster. 200 = unlimited.");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            s.maxAnimatedPawns = Mathf.RoundToInt(l.Slider(s.maxAnimatedPawns, 5f, 200f));

            // --- Developer (only shown in dev mode) ---
            if (Prefs.DevMode)
            {
                l.GapLine();
                Text.Font = GameFont.Small;
                l.Label("Developer");
                bool wasDiag = s.diagnosticsOverlay;
                l.CheckboxLabeled("Perf diagnostics overlay", ref s.diagnosticsOverlay,
                    "Draw a small on-screen HUD with this mod's render cost (average AND peak ms/frame), "
                    + "how many colonists were considered / passed the cheap reject / are animating, "
                    + "and the cap / throttle / zoom-LOD / warm-up state. Dev-mode only; costs nothing "
                    + "when off. Toggling it resets the peak-frame reading.");
                if (s.diagnosticsOverlay != wasDiag) Diag.ResetPeak();
            }

            // --- Per-tool toggles ---
            // Two columns per tool: "Tool" = the swinging sprite, "Effects" = its flung debris /
            // dust / glint / emotes. Effects can be muted while the tool keeps animating.
            l.GapLine();
            l.Label("Individual tools");
            l.Label("Tool = the swinging sprite. Effects = the flung debris, dust and glints.");

            Rect btnRow = l.GetRect(28f);
            float qW = (btnRow.width - 24f) / 4f;
            if (Widgets.ButtonText(new Rect(btnRow.x, btnRow.y, qW, btnRow.height), "Tools: all"))
                foreach (JobToolDef t in SortedTools) s.toolEnabled[t.defName] = true;
            if (Widgets.ButtonText(new Rect(btnRow.x + qW + 8f, btnRow.y, qW, btnRow.height), "Tools: none"))
                foreach (JobToolDef t in SortedTools) s.toolEnabled[t.defName] = false;
            if (Widgets.ButtonText(new Rect(btnRow.x + (qW + 8f) * 2f, btnRow.y, qW, btnRow.height), "Effects: all"))
                foreach (JobToolDef t in SortedTools) s.toolEffectsEnabled[t.defName] = true;
            if (Widgets.ButtonText(new Rect(btnRow.x + (qW + 8f) * 3f, btnRow.y, qW, btnRow.height), "Effects: none"))
                foreach (JobToolDef t in SortedTools) s.toolEffectsEnabled[t.defName] = false;
            l.Gap(6f);

            const float cb = 24f;          // checkbox size
            const float colGap = 70f;      // distance between the two checkbox columns

            // Column header row, aligned over the two checkbox columns.
            Rect hdr = l.GetRect(20f);
            float effX = hdr.xMax - cb;
            float toolX = effX - colGap;
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            Widgets.Label(new Rect(toolX - 14f, hdr.y, cb + 28f, 20f), "Tool");
            Widgets.Label(new Rect(effX - 18f, hdr.y, cb + 36f, 20f), "Effects");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            bool alt = false;
            foreach (JobToolDef tool in SortedTools)
            {
                Rect row = l.GetRect(28f);
                if (alt) Widgets.DrawLightHighlight(row);
                alt = !alt;
                Widgets.Label(new Rect(row.x, row.y + 2f, toolX - row.x - 8f, 26f), tool.LabelNice);

                bool on = JobEffectsSettings.IsToolEnabled(tool.defName);
                bool wasOn = on;
                Widgets.Checkbox(new Vector2(toolX, row.y + 2f), ref on, cb);
                if (on != wasOn) s.toolEnabled[tool.defName] = on;

                bool fxOn = JobEffectsSettings.AreToolEffectsEnabled(tool.defName);
                bool wasFx = fxOn;
                // Effects checkbox disabled-looking when the whole tool is off (debris can't show
                // anyway), but still toggleable so the choice persists for when you re-enable it.
                Widgets.Checkbox(new Vector2(effX, row.y + 2f), ref fxOn, cb);
                if (fxOn != wasFx) s.toolEffectsEnabled[tool.defName] = fxOn;
            }

            l.End();
            Widgets.EndScrollView();
        }
    }

    public class JobEffectsGameComponent : GameComponent
    {
        public JobEffectsGameComponent(Game game) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            ResetTransientCaches("game init");
        }

        // Drives the on-demand install/removal of the Skygaze posture patch. One int compare on most
        // ticks; a CurJobDef sweep once every 4s. See SkygazePatch for why the patch isn't permanent.
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            try { SkygazePatch.Tick(); } catch { }
        }

        // Amortized material warm-up: a couple of JobToolDefs per FRAME (not per tick — it must keep
        // running while paused, and it's a render-asset concern) until every tool's materials exist.
        // Costs one bool read per frame once finished. See ToolWarmup.
        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            try { ToolWarmup.Tick(); } catch { }
        }

        private static void ResetTransientCaches(string reason)
        {
            try
            {
                ToolAnimator.ResetTransientState();
                SkygazePatch.ResetTransientState();
                ToolWarmup.Reset();
                Diag.ResetPeak();
            }
            catch (System.Exception e)
            {
                Log.Warning("[Show Me Your Tools] Failed to reset transient caches on " + reason + ": " + e.Message);
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class JobEffectsBootstrap
    {
        static JobEffectsBootstrap()
        {
            Harmony h = new Harmony("meathax.JobEffects");
            h.PatchAll();
            SkygazePatch.Init(h);  // owns Patch_SkygazeStandUp's lifetime; applied on demand, not by PatchAll
            SmyhCompat.Apply(h);   // optional: suppress SMYH's duplicate resting hands during tool work
            SmyhArmHook.Apply(h);  // optional: extend forearms onto every SMYH hand (weapons/carried/idle)
            MeleeAnimationCompat.Apply(); // optional: silence Melee Animation's weapon anim while a tool is in use
        }
    }
}

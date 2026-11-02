using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace JobEffects
{
    public class JobEffectsSettings : ModSettings
    {
        public bool animatedTools = true;

        // Tech-gate: when ON (default), tools that don't fit a pre-industrial colony (the welder,
        // bench-welder, fire extinguisher) are swapped to a period-correct equivalent until the
        // Electricity research is finished. After Electricity, every tool behaves normally. Turn
        // OFF to always use the modern tools regardless of research progress.
        public bool gateModernTools = true;

        // Per-tool enable flags, keyed by JobToolDef.defName. An entry is only stored when the
        // player has toggled a tool OFF (or back on); a tool absent from the dict defaults to
        // enabled, so brand-new tools in future updates light up automatically.
        public Dictionary<string, bool> toolEnabled = new Dictionary<string, bool>();

        // Per-tool PARTICLE-EFFECT flags (the flung wood chips / stone shards / leaves / soil,
        // plus tip glow, sweat/steam emotes and settling litter). Independent of toolEnabled:
        // turning this off keeps the tool swinging but stops its debris. Same default-on /
        // store-on-change semantics as toolEnabled.
        public Dictionary<string, bool> toolEffectsEnabled = new Dictionary<string, bool>();
        public bool completionEffects = true;
        // Hide other tool/animation mods' equipped-tool draw for jobs our animated tool covers.
        public bool overrideToolMods = true;
        public bool handsOnTools = true;
        // Translucent forearms hanging off the SMYH hands on the tool. Default ON, but only ever
        // drawn when Show Me Your Hands is active (see DrawArms accessor).
        public bool drawArms = true;
        // Extend forearms onto every hand Show Me Your Hands draws (equipped weapons, carried items,
        // idle hands), not just our animated work tools. Opt-in, default OFF.
        public bool forearmsOnHands = false;
        public float armOpacity = 1f;   // 0..1, baked into the sleeve material alpha
        public float intensity = 1.0f;
        // Multiplier on the SIZE of thrown impact debris (leaves, stone shards, wood chips, soil,
        // bone dust, …). Defaults to 1.0 = the authored sizes.
        public float debrisScale = 1.0f;

        // --- Immersion layer ---
        public bool seasonalParticles = true;   // leaf/soil flecks tint by season
        public bool materialChips = true;        // chip color keyed to the worked thing
        public bool groundLitter = true;         // settling embers/chips that fade out
        public bool holsterTools = true;         // last tool lingers at the hip, then fades; also rides the belt while walking to a job
        public bool toolFoley = true;            // short handle clatter when a tool is drawn from the belt / stowed

        // Performance LOD: skip drawing tools when zoomed far out (sprites a few px) and drop the
        // translucent forearms at medium zoom. Default ON — big win on large colonies, invisible up close.
        public bool zoomLod = true;

        // Performance cap: at most this many on-screen colonists animate tools at once (the ones nearest
        // the camera win); the rest fall back to vanilla. 5..200, default 60 (200 = effectively unlimited).
        public int maxAnimatedPawns = 60;

        // Draw tools/hands at the colonist's own depth (the vanilla held-weapon altitude) instead of
        // always on top, so whatever occludes the colonist also occludes the tool. Default ON.
        public bool toolsMatchPawnDepth = true;

        // Dev-only perf HUD (Stage 4). Draws a small overlay with render ms/frame, active/holster
        // animator counts, cap/throttle/LOD state. Only has any effect while Prefs.DevMode is on;
        // all instrumentation is gated on Diag.Enabled so it costs nothing when this is off.
        public bool diagnosticsOverlay = false;

        public static JobEffectsSettings Instance;

        // True when Show Me Your Hands is loaded — only then do we draw (its) hands on tools.
        public static readonly bool SmyhActive =
            ModLister.GetActiveModWithIdentifier("Mlie.ShowMeYourHands", true) != null;
        // True when the Nice Hands Retexture is loaded. It swaps SMYH's symmetric disk hand for a
        // DIRECTIONAL fist at the same texture paths, so when active we depth-split the fist around the
        // haft (palm/back behind the tool, fingers in front) instead of drawing it as one flat quad.
        // Detection only — no rotation. See ToolAnimator.DrawHandBillboard.
        public static readonly bool NiceHandsActive =
            ModLister.GetActiveModWithIdentifier("Andromeda.NiceHands", true) != null;
        public static bool AnimatedTools => Instance == null || Instance.animatedTools;
        public static bool GateModernTools => Instance == null || Instance.gateModernTools;

        // True once the Electricity research is finished (or if the project doesn't exist in this
        // modlist -- a total conversion may have no electricity, so default to "unlocked"). The def
        // reference is cached; IsFinished reads live research state, so it flips the instant the
        // colony completes Electricity.
        private static ResearchProjectDef electricityResearch;
        private static bool electricityLookedUp;
        public static bool ElectricityResearched
        {
            get
            {
                if (!electricityLookedUp)
                {
                    electricityResearch = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Electricity");
                    electricityLookedUp = true;
                }
                return electricityResearch == null || electricityResearch.IsFinished;
            }
        }
        public static bool HandsOnTools => SmyhActive && (Instance == null || Instance.handsOnTools);
        // Arms only render when SMYH is active (they hang off the hands we draw), so fold SmyhActive in.
        public static bool DrawArms => SmyhActive && (Instance == null || Instance.drawArms);
        public static float ArmOpacity => Instance == null ? 1f : Mathf.Clamp01(Instance.armOpacity);
        // Forearms on all SMYH hands (weapons/carried/idle). Requires SMYH + the main Forearms toggle.
        public static bool ForearmsOnHands => DrawArms && Instance != null && Instance.forearmsOnHands;
        public static bool CompletionEffects => Instance == null || Instance.completionEffects;
        public static bool OverrideToolMods => Instance == null || Instance.overrideToolMods;
        public static float Intensity => Instance == null ? 1.0f : Instance.intensity;
        public static float DebrisScale => Instance == null ? 1.0f : Instance.debrisScale;

        // A tool is on unless the player explicitly disabled it. Disabling a tool skips its draw
        // AND its impact/particle emissions (both are gated on tool resolution).
        public static bool IsToolEnabled(string defName)
        {
            if (Instance == null || Instance.toolEnabled == null || defName == null) return true;
            return !Instance.toolEnabled.TryGetValue(defName, out bool on) || on;
        }

        // True unless the player muted this tool's debris/particles. Gates SpawnImpact + tip glow
        // + head emote + strike glint in the animator; the tool sprite itself still draws.
        public static bool AreToolEffectsEnabled(string defName)
        {
            if (Instance == null || Instance.toolEffectsEnabled == null || defName == null) return true;
            return !Instance.toolEffectsEnabled.TryGetValue(defName, out bool on) || on;
        }

        public static bool SeasonalParticles => Instance == null || Instance.seasonalParticles;
        public static bool MaterialChips => Instance == null || Instance.materialChips;
        public static bool GroundLitter => Instance == null || Instance.groundLitter;
        public static bool HolsterTools => Instance == null || Instance.holsterTools;
        public static bool ToolFoley => Instance == null || Instance.toolFoley;
        public static bool ZoomLod => Instance == null || Instance.zoomLod;
        public static int MaxAnimatedPawns => Instance == null ? 60 : Mathf.Clamp(Instance.maxAnimatedPawns, 5, 200);
        public static bool ToolsMatchPawnDepth => Instance == null || Instance.toolsMatchPawnDepth;
        public static bool DiagnosticsOverlay => Instance != null && Instance.diagnosticsOverlay;

        // Restore every setting to its authored default and clear all per-tool overrides.
        public void ResetToDefaults()
        {
            animatedTools = true;
            gateModernTools = true;
            completionEffects = true;
            overrideToolMods = true;
            handsOnTools = true;
            drawArms = true;
            forearmsOnHands = false;
            armOpacity = 1f;
            intensity = 1.0f;
            debrisScale = 1.0f;
            seasonalParticles = true;
            materialChips = true;
            groundLitter = true;
            holsterTools = true;
            toolFoley = true;
            zoomLod = true;
            maxAnimatedPawns = 60;
            toolsMatchPawnDepth = true;
            toolEnabled.Clear();
            toolEffectsEnabled.Clear();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref animatedTools, "animatedTools", true);
            Scribe_Values.Look(ref gateModernTools, "gateModernTools", true);
            Scribe_Values.Look(ref completionEffects, "completionEffects", true);
            Scribe_Values.Look(ref overrideToolMods, "overrideToolMods", true);
            Scribe_Values.Look(ref handsOnTools, "handsOnTools", true);
            Scribe_Values.Look(ref drawArms, "drawArms", true);
            Scribe_Values.Look(ref forearmsOnHands, "forearmsOnHands", false);
            Scribe_Values.Look(ref armOpacity, "armOpacity", 1f);
            Scribe_Values.Look(ref intensity, "intensity", 1.0f);
            Scribe_Values.Look(ref debrisScale, "debrisScale", 1.0f);
            Scribe_Values.Look(ref seasonalParticles, "seasonalParticles", true);
            Scribe_Values.Look(ref materialChips, "materialChips", true);
            Scribe_Values.Look(ref groundLitter, "groundLitter", true);
            Scribe_Values.Look(ref holsterTools, "holsterTools", true);
            Scribe_Values.Look(ref toolFoley, "toolFoley", true);
            Scribe_Values.Look(ref zoomLod, "zoomLod", true);
            Scribe_Values.Look(ref maxAnimatedPawns, "maxAnimatedPawns", 60);
            Scribe_Values.Look(ref toolsMatchPawnDepth, "toolsMatchPawnDepth", true);
            Scribe_Values.Look(ref diagnosticsOverlay, "diagnosticsOverlay", false);
            Scribe_Collections.Look(ref toolEnabled, "toolEnabled", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref toolEffectsEnabled, "toolEffectsEnabled", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (toolEnabled == null) toolEnabled = new Dictionary<string, bool>();
                if (toolEffectsEnabled == null) toolEffectsEnabled = new Dictionary<string, bool>();
            }
        }
    }
}

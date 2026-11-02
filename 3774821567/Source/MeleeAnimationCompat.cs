using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Melee Animation (Epicguru, packageId co.uk.epicguru.meleeanimation) interop.
    ///
    /// Melee Animation draws and animates a pawn's equipped melee weapon via its per-pawn
    /// <c>AM.Idle.IdleControllerComp</c> (idle/move/flavour weapon animations). When one of OUR
    /// animated tools is on screen for that pawn, we want Melee Animation to stay out of the way
    /// entirely — otherwise its weapon animation fights / double-draws over our tool.
    ///
    /// Rather than Harmony-patch AM internals, we use AM's OWN public extension point:
    ///   <c>public static readonly List&lt;IdleControllerDrawDelegate&gt; IdleControllerComp.ShouldDrawAdditional</c>
    /// AM invokes every delegate in this list from <c>ShouldBeActive</c>; a delegate that sets
    /// <c>shouldBeActive = false</c> makes AM clear its animation and skip the weapon for that pawn.
    /// We also set <c>doDefaultDraw = false</c> so AM doesn't ask vanilla to draw the weapon instead
    /// (the pawn is mid-work, no weapon should show).
    ///
    /// PERFORMANCE — read before touching this file.
    /// <c>ShouldBeActive</c> is called from <c>IdleControllerComp.CompTick()</c>, i.e. once per
    /// humanlike-with-the-comp PER TICK, on every map, regardless of the camera. On a 120-pawn colony
    /// at 3x that is ~21,600 invocations/second — roughly two orders of magnitude hotter than this
    /// mod's per-frame render postfix. AM also invokes us BEFORE its own <c>GetMeleeWeapon()</c>
    /// check, so unarmed pawns reach us too. Consequences, both handled below:
    ///   * The body must be trivially cheap. It previously recovered the Pawn through a cached
    ///     <c>FieldInfo.GetValue</c> — an unnecessary Mono runtime-invoke, because
    ///     <c>Verse.ThingComp.parent</c> is a PUBLIC field. It's a plain field read now.
    ///   * When our own feature is switched off the delegate is REMOVED from AM's list outright
    ///     (see <see cref="SyncRegistration"/>) rather than early-returning, so a disabled feature
    ///     costs exactly nothing instead of "nearly nothing, 21,600 times a second".
    ///
    /// All reflection, all guarded — a no-op when Melee Animation isn't loaded, and any failure
    /// leaves AM behaving exactly as normal.
    /// </summary>
    public static class MeleeAnimationCompat
    {
        // True when Melee Animation is loaded. The whole hook is skipped otherwise.
        public static readonly bool Active =
            ModLister.GetActiveModWithIdentifier("co.uk.epicguru.meleeanimation", true) != null;

        private static IList hookList;    // AM's public static List<IdleControllerDrawDelegate>
        private static Delegate hook;     // our delegate instance, bound once
        private static bool registered;   // true while `hook` is present in `hookList`
        private static bool resolved;     // one-time reflection has run

        public static void Apply()
        {
            if (!Active || resolved) return;
            resolved = true;
            try
            {
                Type compType = AccessTools.TypeByName("AM.Idle.IdleControllerComp");
                Type delType = AccessTools.TypeByName("AM.Idle.IdleControllerDrawDelegate");
                if (compType == null || delType == null) return;

                FieldInfo listField = AccessTools.Field(compType, "ShouldDrawAdditional");
                if (!(listField?.GetValue(null) is IList list)) return;
                hookList = list;

                // Bind our static method to AM's delegate type. Relaxed delegate binding lets the
                // delegate's IdleControllerComp parameter target our less-derived ThingComp param.
                MethodInfo mi = AccessTools.Method(typeof(MeleeAnimationCompat), nameof(ShouldDrawHook));
                hook = Delegate.CreateDelegate(delType, mi);

                SyncRegistration();
            }
            catch (Exception e)
            {
                Log.Warning("[Show Me Your Tools] Melee Animation compat hook failed: " + e.Message);
            }
        }

        /// <summary>
        /// Add/remove our delegate from AM's list to match the current settings, so that with tool
        /// animation (or the "override other tool mods" option) switched off we are not in AM's
        /// per-tick iteration at all. Idempotent; called at init and from Mod.WriteSettings.
        ///
        /// Mutating AM's list is safe here: AM only enumerates it from CompTick (tick phase) and
        /// PreDraw (render phase), both inside Unity's Update, while settings are written from
        /// OnGUI. The two never interleave, so this can't invalidate an in-flight enumerator.
        /// </summary>
        public static void SyncRegistration()
        {
            if (hookList == null || hook == null) return;
            bool want = JobEffectsSettings.AnimatedTools && JobEffectsSettings.OverrideToolMods;
            if (want == registered) return;
            try
            {
                if (want) hookList.Add(hook);
                else hookList.Remove(hook);
                registered = want;
            }
            catch (Exception e)
            {
                Log.Warning("[Show Me Your Tools] Melee Animation compat (de)registration failed: " + e.Message);
            }
        }

        // Signature-compatible with AM.Idle.IdleControllerDrawDelegate
        // (void(IdleControllerComp, ref bool shouldBeActive, ref bool doDefaultDraw)).
        // Suppress AM's weapon animation/draw for a pawn while we're showing an animated tool.
        //
        // Hot: per humanlike, per tick. Every line here is on that budget — the ladder is ordered
        // static bool -> field read -> type check -> ToolAnimator's array-indexed job gate.
        public static void ShouldDrawHook(ThingComp comp, ref bool shouldBeActive, ref bool doDefaultDraw)
        {
            if (!shouldBeActive) return;   // already suppressed by another delegate
            // Belt-and-braces: SyncRegistration should already have removed us, but a settings change
            // that never routes through WriteSettings must not leave us suppressing weapons.
            if (!JobEffectsSettings.AnimatedTools || !JobEffectsSettings.OverrideToolMods) return;
            try
            {
                // ThingComp.parent is a public field — plain read, no reflection.
                if (!(comp.parent is Pawn pawn)) return;
                if (ToolAnimator.HasActiveTool(pawn))
                {
                    shouldBeActive = false;
                    doDefaultDraw = false;
                }
            }
            catch { }
        }
    }
}

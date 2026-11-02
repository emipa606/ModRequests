using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Show Me Your Hands interop. Two parts:
    ///  • SmyhHands — reflection bridge that reuses SMYH's own per-pawn hand graphics, so our
    ///    tool-hands inherit SMYH's exact colour (gloves / skin / artificial limb, incl. the
    ///    HandClean/OffHand textures) and its missing-hand handling, honouring SMYH's settings.
    ///  • Apply() — a manual prefix on SMYH's private drawHandsAllTheTime so its "show hands at
    ///    all times" resting hands don't double up with the hands we draw on the animated tool.
    /// Everything is guarded behind SMYH being loaded and wrapped in try/catch with fallbacks.
    /// </summary>
    public static class SmyhCompat
    {
        public static void Apply(Harmony h)
        {
            if (!JobEffectsSettings.SmyhActive) return;
            try
            {
                Type drawer = AccessTools.TypeByName("ShowMeYourHands.HandDrawer");
                if (drawer == null) return;
                MethodInfo target = AccessTools.Method(drawer, "drawHandsAllTheTime");
                if (target == null) return;
                MethodInfo prefix = AccessTools.Method(typeof(SmyhCompat), nameof(SuppressAllTimeHands));
                h.Patch(target, prefix: new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Log.Warning("[Show Me Your Tools] SMYH 'hands at all times' compat patch failed: " + e.Message);
            }
        }

        // Prefix on SMYH.HandDrawer.drawHandsAllTheTime(Pawn pawn). Returning false skips SMYH's
        // resting body-hands for this frame, but ONLY while we are actually drawing hands on an
        // animated tool for this pawn — otherwise SMYH behaves exactly as normal.
        public static bool SuppressAllTimeHands(Pawn pawn)
        {
            try
            {
                if (JobEffectsSettings.HandsOnTools && ToolAnimator.HasActiveTool(pawn))
                    return false;
            }
            catch { }
            return true;
        }
    }

    /// <summary>Reflection bridge into SMYH's cached hand graphics + missing-hand state.</summary>
    public static class SmyhHands
    {
        private static bool initialized, ready;
        private static Type drawerType;
        private static FieldInfo fMainGraphics, fOffGraphics, fMissing;
        private static MethodInfo handColorGetter;

        // Per-pawn material cache so we don't reflect into SMYH every frame (the getter boxes a
        // Color and walks AllComps). SMYH itself only recomputes hand colour every ~100 ticks, so
        // refreshing on a similar cadence is visually identical and allocation-free between.
        private struct HandMats { public Material main; public Material off; public bool missing; public int tick; }
        private static readonly Dictionary<int, HandMats> cache = new Dictionary<int, HandMats>();
        private const int RefreshTicks = 90;

        public static void Forget(int thingId) => cache.Remove(thingId);

        public static void ResetTransientState()
        {
            cache.Clear();
        }

        private static void Init()
        {
            if (initialized) return;
            initialized = true;
            try
            {
                Type main = AccessTools.TypeByName("ShowMeYourHands.ShowMeYourHandsMain");
                drawerType = AccessTools.TypeByName("ShowMeYourHands.HandDrawer");
                if (main == null || drawerType == null) return;
                fMainGraphics = AccessTools.Field(main, "mainHandGraphics");
                fOffGraphics = AccessTools.Field(main, "offHandGraphics");
                fMissing = AccessTools.Field(main, "pawnsMissingAHand");
                handColorGetter = AccessTools.PropertyGetter(drawerType, "HandColor");
                ready = fMainGraphics != null && fOffGraphics != null && handColorGetter != null;
            }
            catch { ready = false; }
        }

        /// <summary>
        /// Resolve the materials SMYH would use for this pawn's hands (and whether SMYH would
        /// draw only one). Invokes SMYH's own HandColor getter so its graphics get computed and
        /// cached even though SMYH isn't drawing the pawn during work. Returns false to fall back.
        /// </summary>
        public static bool TryGet(Pawn pawn, out Material main, out Material off, out bool missingHand)
        {
            main = null; off = null; missingHand = false;
            Init();
            if (!ready || pawn == null) return false;

            int id = pawn.thingIDNumber;
            int now = GenTicks.TicksGame;
            if (cache.TryGetValue(id, out HandMats hm) && hm.main != null && now - hm.tick < RefreshTicks)
            {
                main = hm.main; off = hm.off; missingHand = hm.missing;
                return true;
            }

            try
            {
                ThingComp comp = GetDrawer(pawn);
                if (comp == null) return false;

                // Force SMYH to (re)compute + cache its hand graphics / missing-hand flag (at most
                // once per RefreshTicks per pawn, since SMYH isn't drawing this pawn during work).
                handColorGetter.Invoke(comp, null);

                var mainDict = fMainGraphics.GetValue(null) as Dictionary<Pawn, Graphic>;
                var offDict = fOffGraphics.GetValue(null) as Dictionary<Pawn, Graphic>;
                if (mainDict == null || !mainDict.TryGetValue(pawn, out Graphic mg) || mg == null) return false;

                main = mg.MatSingle;
                off = (offDict != null && offDict.TryGetValue(pawn, out Graphic og) && og != null) ? og.MatSingle : main;

                if (fMissing?.GetValue(null) is Dictionary<Pawn, bool> missDict
                    && missDict.TryGetValue(pawn, out bool mm))
                    missingHand = mm;

                cache[id] = new HandMats { main = main, off = off, missing = missingHand, tick = now };
                return main != null;
            }
            catch { return false; }
        }

        private static ThingComp GetDrawer(Pawn pawn)
        {
            List<ThingComp> comps = pawn.AllComps;
            if (comps == null) return null;
            for (int i = 0; i < comps.Count; i++)
                if (comps[i] != null && drawerType.IsInstanceOfType(comps[i]))
                    return comps[i];
            return null;
        }
    }
}

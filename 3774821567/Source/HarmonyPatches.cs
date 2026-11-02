using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace JobEffects
{
    /// <summary>
    /// Override other tool/animation mods (Dark Ages: Medieval Tools' ShowTools, Melee Animation,
    /// Tools O' Plenty, carry-openly mods, SMYH weapon-hands) for any job our animated tool covers.
    /// DrawEquipmentAndApparelExtras is the single vanilla chokepoint that draws the equipped
    /// weapon/tool (the part those mods force-show / pose during work). When our tool is active for
    /// the pawn we skip ONLY the weapon draw here and still draw apparel worn-extras, so their
    /// duplicate tool/effect vanishes and only our animated tool shows.
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
    public static class Patch_DrawEquipment_OverrideToolMods
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn == null) return true;
            if (!JobEffectsSettings.OverrideToolMods) return true;
            bool active;
            try { active = ToolAnimator.HasActiveTool(pawn); } catch { return true; }
            if (!active) return true;   // we're not drawing a tool for this job — let everything draw

            // Our animated tool covers this job: hide the real equipped tool/weapon (and whatever the
            // other mods do with it) but keep apparel worn-extras that this method would have drawn.
            if (pawn.apparel != null)
            {
                List<Apparel> worn = pawn.apparel.WornApparel;
                for (int i = 0; i < worn.Count; i++)
                {
                    try { worn[i].DrawWornExtras(); } catch { }
                }
            }
            return false;
        }
    }
    /// <summary>
    /// Sync the fire-extinguisher foam puff to the pawn's real beat-fire action. Verb_BeatFire
    /// casts once per beat (the forward lunge + Notify_MeleeAttackOn happen inside TryCastShot),
    /// returning true on a successful beat. We bump a per-pawn counter so ToolAnimator emits exactly
    /// one foam puff per lunge instead of on its own free-running clock. TryCastShot is protected,
    /// so it's targeted by string name.
    /// </summary>
    [HarmonyPatch(typeof(Verb_BeatFire), "TryCastShot")]
    public static class Patch_BeatFire_Puff
    {
        public static void Postfix(Verb_BeatFire __instance, bool __result)
        {
            if (!__result) return;
            try { ToolAnimator.NotifyBeatFire(__instance.CasterPawn); } catch { }
        }
    }

    /// <summary>
    /// Suppress the vanilla melee "lunge" while a pawn beats a fire with our animated extinguisher.
    /// The lunge is a draw-position jitter toward the target that Verb_BeatFire.TryCastShot adds via
    /// Pawn_DrawTracker.Notify_MeleeAttackOn (jitterer.AddOffset). The extinguisher is held upright
    /// and discharges foam onto the tile — a forward body lunge looks wrong with it, so we skip the
    /// jitter. Only when OUR tool is actually drawing for this pawn AND Yayo's Animation is NOT
    /// active: if Yayo is running it drives the body animation itself, so we leave the jitter so its
    /// animation plays correctly. Gated to Fire targets so real melee combat is never touched.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.Notify_MeleeAttackOn))]
    public static class Patch_SuppressBeatFireLunge
    {
        public static bool Prefix(Thing Target, Pawn ___pawn)
        {
            try
            {
                if (YayoCompat.Active) return true;                    // Yayo drives the body — leave it
                if (!(Target is Fire)) return true;                   // only the beat-fire case
                if (___pawn == null) return true;
                if (!ToolAnimator.HasActiveTool(___pawn)) return true; // our extinguisher isn't drawing
                return false;                                          // skip the vanilla lunge jitter
            }
            catch { return true; }
        }
    }

    /// <summary>
    /// Hide the vanilla carried medicine sprite while our animated medicine kit is showing.
    /// TendPatient makes the pawn carry the actual Medicine item; without this patch both our
    /// kit and the vanilla item render on top of each other.
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawCarriedThing))]
    public static class Patch_HideCarriedMedicine
    {
        public static bool Prefix(Pawn pawn, Thing carriedThing)
        {
            if (pawn == null || carriedThing == null) return true;
            if (!JobEffectsSettings.OverrideToolMods) return true;
            if (!carriedThing.def.IsMedicine) return true;
            try
            {
                if (ToolAnimator.HasActiveTool(pawn)) return false;
            }
            catch { }
            return true;
        }
    }

    /// <summary>
    /// Work-sound hit sync: a continuous work job's sound is a sustainer that re-fires its grain
    /// back-to-back (one grain == one audible hit/scrape). SubSustainer.StartSample is the per-grain
    /// entry point. Two cases notify the animator so its strike lands on the hit:
    ///  - BUTCHERING: any butcher recipe sound grain (the cleaver's chop).
    ///  - BENCH CHISEL: the PRIMARY grain of whatever recipe sound a sync-enabled chisel's pawn is
    ///    producing (stonecutting "chink", sculpt scrape, etc.) — driven by the bench's own sound,
    ///    so it works for any workbench (vanilla or modded) without hardcoding sound names. Only the
    ///    first subSound is used so stonecutting's secondary debris rattle never disturbs the beat,
    ///    and the WantsBenchSoundSync gate means we only measure intervals for a pawn actually
    ///    showing a chisel — not for every cook/smith/researcher sustainer on the map.
    /// </summary>
    [HarmonyPatch(typeof(SubSustainer), "StartSample")]
    public static class Patch_ButcherHitSync
    {
        public static void Postfix(SubSustainer __instance)
        {
            try
            {
                Sustainer parent = __instance?.parent;
                SoundDef def = parent?.def;
                if (def == null) return;
                if (!(parent.info.Maker.Thing is Pawn pawn)) return;

                string n = def.defName;
                if (n != null && n.IndexOf("Butcher", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ToolAnimator.NotifyWorkHit(pawn);
                    return;
                }

                var subs = def.subSounds;
                if (subs != null && subs.Count > 0 && __instance.subDef == subs[0]
                    && ToolAnimator.WantsBenchSoundSync(pawn))
                {
                    ToolAnimator.NotifyWorkHit(pawn);
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Suppress the vanilla recipe/work effecter's sprayer motes for a pawn while our welder is
    /// drawing. The smithy "Smith" effecter sprays Mote_StoneBit sparks BetweenPositions (biased
    /// ~0.6 toward the worker), so at a bench they erupt from the pawn's feet — visually detached
    /// from the animated welder. We already throw bright sparks at the electrode tip, so cut the
    /// vanilla feet-sparks. A is the effect's source (the working pawn for recipe effecters);
    /// only that pawn's sprayer is gated, and only while ToolAnimator has flagged it this tick.
    /// </summary>
    [HarmonyPatch(typeof(SubEffecter_Sprayer), "MakeMote")]
    public static class Patch_SuppressWelderWorkMotes
    {
        public static bool Prefix(TargetInfo A, SubEffecter_Sprayer __instance)
        {
            try
            {
                if (A.Thing is Pawn pawn)
                {
                    if (ToolAnimator.IsSuppressingWorkMotes(pawn))
                        return false;   // skip this work-effecter spark; the tip sparks cover it
                    // Fishing compat (Option A): drop the vanilla Odyssey / VFE fishing-rod mote
                    // while our animated rod is this pawn's active tool, so only one rod renders.
                    ThingDef mote = __instance?.def?.moteDef;
                    if (mote != null && ToolAnimator.IsSuppressingFishingMote(pawn, mote))
                        return false;
                }
            }
            catch { }
            return true;
        }
    }

    /// <summary>
    /// One-shot completion burst when a construction frame finishes. The frame is
    /// Destroyed mid-method, so capture its footprint in a Prefix and spawn in the Postfix.
    /// </summary>
    /// <summary>
    /// Yayo's Animation compat — WELDER ONLY. Yayo animates the pawn body by postfixing
    /// PawnRenderer.GetBodyPos (adds posOffset) and PawnRenderer.BodyAngle (adds angleOffset).
    /// Our welder + hands are drawn at the raw, un-offset draw position, so under Yayo the body
    /// bobs/leans while the tool stays put — they detach. Rather than chase Yayo's offset for the
    /// welder, the user wants vanilla (still) body animation while welding. These two postfixes run
    /// AFTER Yayo and, for a STANDING pawn whose active animated tool is the welder, restore the
    /// vanilla standing values (GetBodyPos -> drawLoc, BodyAngle -> 0). That removes Yayo's offset
    /// for welding jobs only; every other job keeps Yayo's animation untouched. Both gate on
    /// YayoCompat.Active first, so when Yayo isn't loaded these are pure no-ops.
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    [HarmonyAfter("com.yayo.yayoAni", "com.yayo.yayoAni.continued")]
    public static class Patch_Yayo_StripWelderBodyPos
    {
        // Tier 4: never apply this patch when Yayo's Animation isn't loaded. Evaluated once at
        // PatchAll time, so without Yayo the method is left unpatched and costs nothing per frame
        // (instead of running 56x/frame just to check YayoCompat.Active and return).
        public static bool Prepare() => YayoCompat.Active;

        public static void Postfix(ref Vector3 __result, Vector3 drawLoc, PawnPosture posture, Pawn ___pawn)
        {
            try
            {
                if (posture != PawnPosture.Standing) return;   // welding is always standing
                if (!YayoCompat.Active) return;
                if (___pawn == null) return;
                if (!ToolAnimator.HasActiveWelder(___pawn)) return;
                __result = drawLoc;   // vanilla standing body pos — drops Yayo's posOffset
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
    [HarmonyAfter("com.yayo.yayoAni", "com.yayo.yayoAni.continued")]
    public static class Patch_Yayo_StripWelderBodyAngle
    {
        public static bool Prepare() => YayoCompat.Active;   // see Patch_Yayo_StripWelderBodyPos

        public static void Postfix(ref float __result, Pawn ___pawn)
        {
            try
            {
                if (!YayoCompat.Active) return;
                if (___pawn == null) return;
                if (___pawn.GetPosture() != PawnPosture.Standing) return;
                if (!ToolAnimator.HasActiveWelder(___pawn)) return;
                __result = 0f;   // vanilla standing body angle — drops Yayo's angleOffset
            }
            catch { }
        }
    }

    /// <summary>
    /// Stargazing on your feet. Vanilla JobDriver_Skygaze forces
    /// <c>pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp</c>, so the colonist lies flat on the
    /// ground — which reads wrong with the spyglass held up to the eye. PawnUtility.GetPosture is the
    /// single source the renderer (and our welder-body patches) read, so a postfix that flips the
    /// result back to Standing for a Skygaze pawn makes them stand and sight through the glass. Joy
    /// gain is posture-independent, so the activity is unaffected.
    ///
    /// NOTE THE MISSING [HarmonyPatch] ATTRIBUTE — that is deliberate, do not add it back.
    /// GetPosture is called ~1,400 times per FRAME, and a patch that early-returns is still a patch
    /// that gets invoked: profiling showed this postfix as the most-called thing the mod owns, at
    /// ~0.035ms/frame of pure dispatch overhead that no guard clause can reduce. So it is installed
    /// and removed on demand by <see cref="SkygazePatch"/> — only while the spyglass tool is enabled
    /// AND somebody is actually stargazing. See that class for the full reasoning.
    ///
    /// The guards below still matter, because while the patch IS applied it runs at full frequency.
    /// </summary>
    public static class Patch_SkygazeStandUp
    {
        public static void Postfix(Pawn p, ref PawnPosture __result)
        {
            if (__result == PawnPosture.Standing) return;       // already upright — nothing to do
            if (!JobEffectsSettings.AnimatedTools) return;       // tool animations off — leave vanilla posture
            if (p == null || p.Dead || p.Downed) return;
            JobDef sg = SkygazePatch.SkygazeJob.Value;
            if (sg == null || p.CurJobDef != sg) return;
            __result = PawnPosture.Standing;                     // stand to sight through the spyglass
        }
    }

    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class Patch_Frame_CompleteConstruction
    {
        public struct CompletionState
        {
            public bool valid;
            public Map map;
            public CellRect rect;
        }

        public static void Prefix(Frame __instance, out CompletionState __state)
        {
            __state = default;
            if (__instance.Spawned && __instance.Map != null)
            {
                __state.valid = true;
                __state.map = __instance.Map;
                __state.rect = __instance.OccupiedRect();
            }
        }

        public static void Postfix(CompletionState __state)
        {
            if (!JobEffectsSettings.CompletionEffects || !__state.valid || __state.map == null) return;
            try { CompletionBurst.Spawn(__state.map, __state.rect); } catch { }
        }
    }

}

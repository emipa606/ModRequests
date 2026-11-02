using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JobEffects
{
    /// <summary>
    /// READ-ONLY sync to Yayo's Animation. When Yayo is active it animates the pawn's BODY on slow,
    /// tick-quantized cycles chosen per job archetype, while our tool free-runs on real time — so the
    /// two visibly drift. This module reproduces Yayo's cycle math (we only READ public state:
    /// TicksGame, thingIDNumber, the job def — we never call into or mutate Yayo) and hands back the
    /// normalized position within Yayo's current body cycle, with 0 sitting exactly on Yayo's "lunge"
    /// frame. ToolAnimator then phase-locks the tool so its strike lands on that lunge: one strike per
    /// body cycle, perfectly in step.
    ///
    /// Constants mirror YayoAnimation.AnimationCore.AniStanding (Yayo's Animation Continued, v1.6).
    /// If Yayo restructures its job switch, re-derive cycle lengths / lunge ticks here. Worst case if a
    /// covered job is miscategorized: the tool's cadence is slightly off — never an exception, since we
    /// only read vanilla state.
    /// </summary>
    public static class YayoCompat
    {
        private static bool? active;

        /// <summary>True when Yayo's Animation (original or Continued) is loaded.</summary>
        public static bool Active
        {
            get
            {
                if (active == null)
                {
                    active = ModLister.GetActiveModWithIdentifier("com.yayo.yayoAni.continued", true) != null
                          || ModLister.GetActiveModWithIdentifier("com.yayo.yayoAni", true) != null;
                }
                return active.Value;
            }
        }

        // Yayo "smash": ~133-tick heave that snaps forward (the strike) around tick 71.
        private static readonly HashSet<string> SmashJobs = new HashSet<string>
        { "Mine", "TipOverSewage", "Play_PunchingBag" };

        // Plant work: Yayo treats it as "smash" when the target is a tree, "doSomeThing" otherwise.
        private static readonly HashSet<string> PlantJobs = new HashSet<string>
        { "Harvest", "HarvestDesignated", "CutPlant", "CutPlantDesignated" };

        // Yayo "doSomeThing": ~121-tick gentle lean with a small forward nudge near tick 113.
        // Only the entries our tools actually cover are listed — jobs Yayo does NOT animate are
        // deliberately absent so we don't slow a tool to match a body that isn't moving.
        private static readonly HashSet<string> DoSomethingJobs = new HashSet<string>
        {
            "DoBill", "SmoothWall", "SmoothFloor", "Deconstruct", "FinishFrame", "Repair",
            "FixBrokenDownBuilding", "Research", "Clean", "ClearSnow", "Uninstall", "BuildRoof",
            "RemoveRoof", "PaintBuilding", "PaintFloor", "RemovePaintBuilding", "RemovePaintFloor",
            "FillIn", "Hack", "TendPatient", "Shear", "Slaughter", "ExtractTree", "OperateDeepDrill",
            "RearmTurret", "Ignite", "PlantSeed", "Replant", "ExtractSkull"
        };

        /// <summary>
        /// If Yayo animates the pawn's current job, output the normalized cycle position (0..1) where
        /// 0 == Yayo's body-lunge frame, and return true. Returns false when the job isn't a Yayo
        /// archetype we recognize (tool then free-runs as normal).
        /// </summary>
        public static bool TryGetCyclePos(Pawn pawn, out float pos01)
        {
            pos01 = 0f;
            Job job = pawn?.CurJob;
            string jn = job?.def?.defName;
            if (jn == null) return false;

            int cycle, lunge;
            if (SmashJobs.Contains(jn)) { cycle = 133; lunge = 71; }
            else if (jn == "Sow") { cycle = 50; lunge = 38; }
            else if (PlantJobs.Contains(jn))
            {
                bool tree = job.targetA.Thing?.def?.plant?.IsTree ?? false;
                if (tree) { cycle = 133; lunge = 71; }
                else { cycle = 121; lunge = 113; }
            }
            else if (DoSomethingJobs.Contains(jn)) { cycle = 121; lunge = 113; }
            else return false;

            // Yayo's per-pawn phase seed and tick clock.
            int num = pawn.thingIDNumber * 20;
            int t = ((Find.TickManager.TicksGame + num) % cycle + cycle) % cycle;
            pos01 = Mathf.Repeat((float)(t - lunge) / cycle, 1f);
            return true;
        }
    }
}

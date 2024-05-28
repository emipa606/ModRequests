using System;
using Verse;
using UnityEngine;

namespace PerfectPathing
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content) => GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<Settings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Perfect Pathfinding";
        }
    }

    class Settings : ModSettings
    {
        public static bool heuristicPatch = true;
        public static bool lightPatch = true;
        public static bool weatherPatch = true;
        public static bool dirtAvoidance = false;
        public static float dirtStrength;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref heuristicPatch, "heuristicPatch", true);
            Scribe_Values.Look(ref lightPatch, "lightPatch", true);
            Scribe_Values.Look(ref weatherPatch, "weatherPatch", true);
            Scribe_Values.Look(ref dirtAvoidance, "dirtAvoidance", false);
            Scribe_Values.Look(ref dirtStrength, "dirtStrength", 1f);
        }

        public void DoWindowContents(Rect canvas)
        {
            var options = new Listing_Standard();

            options.Begin(canvas);
            options.Gap();
            options.CheckboxLabeled("Heuristic Patch", checkOn: ref heuristicPatch, tooltip: "Pawns will use the fastest route regardless of target distance.");

            options.Gap();
            options.CheckboxLabeled("Lighting Patch", checkOn: ref lightPatch, tooltip: "Pawns will consider lighting when pathfinding.");

            options.Gap();
            options.CheckboxLabeled("Weather Patch", checkOn: ref weatherPatch, tooltip: "Pawns will consider weather when pathfinding.");

            options.Gap();
            options.CheckboxLabeled("Dirt Avoidance", checkOn: ref dirtAvoidance, tooltip: "Pawns will spend extra time to avoid getting their feet dirty.");
            if (dirtAvoidance)
            {
                options.Label(string.Format("Strength: {0}", Math.Round(dirtStrength, 2).ToString()));
                dirtStrength = options.Slider(dirtStrength, 0f, 10f);
            }

            //options.GapLine();
            options.End();
        }

    }
}

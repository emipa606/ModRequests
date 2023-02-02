using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FixedIdeoStyle
{
    [StaticConstructorOnStartup]
    public static class FixedIdeoStyle
    {
        static FixedIdeoStyle()
        {
            var harmony = new Harmony("me.lubar.FixedIdeoStyle");

            harmony.PatchAll();
        }

        public static Gizmo ChangeStyleGizmo(Thing thing, ThingDef thingDef)
        {
            if (thing == null || thingDef == null || thing.Faction != Faction.OfPlayer || !thingDef.canGenerateDefaultDesignator)
            {
                return null;
            }

            var styles = new List<ThingStyleDef>();

            foreach (var ideo in Find.IdeoManager.IdeosListForReading)
            {
                var style = ideo.GetStyleFor(thingDef);
                if (style != null && !styles.Contains(style))
                {
                    styles.Add(style);
                }
            }

            if (!styles.Any())
            {
                return null;
            }

            return new Command_SetStyle(thing, thingDef, styles);
        }
    }
}

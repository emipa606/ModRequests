using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Clockwork
{
    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef Shocked = HediffDef.Named("Shocked");
    }
    public static class ThingDefOf
    {
        public static ThingDef ClockworkInput = ThingDef.Named("ClockworkInput");
        public static ThingDef ClockworkOutput = ThingDef.Named("ClockworkOutput");
        public static ThingDef SteamPipe = ThingDef.Named("SteamPipe");
    }
    public static class TraitDefOf
    {
        public static TraitDef LivingAutomaton = TraitDef.Named("LivingAutomaton");
        public static TraitDef Alchemist = TraitDef.Named("Alchemist");
        public static TraitDef Clockmaker = TraitDef.Named("Clockmaker");
    }
}

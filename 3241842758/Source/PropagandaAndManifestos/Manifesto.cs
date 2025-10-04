using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace PropagandaAndManifestos
{
    [StaticConstructorOnStartup]
    public static class ResistancePatch
    {
        private static readonly Type patchType = typeof(ResistancePatch);
        static ResistancePatch()
        {
            var harmony = new Harmony("rimworld.Chaoticbloom.PropagandaAndManifestos.Manifesto");
            //harmony.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), "GuestTrackerTick"),
            //   prefix: new HarmonyMethod(patchType, nameof(Prefix)));
        }
        public static bool Prefix(float offset, ref Pawn ___pawn)
        {
            /*if (___pawn.CurJobDef == JobDefOf.Reading)
            {
                ___pawn.guest.resistance += offset;
                return false;
            }
            return true;
            */
            return true;
        }

    }
    public class ReadingOutcomeDoerResistanceModifier : BookOutcomeDoer
    {
        public int lastUpdate = 0;
        public static int hour = 2500;
        public new BookOutcomeProperties_ResistanceModifier Props => (BookOutcomeProperties_ResistanceModifier)props;
        public override bool DoesProvidesOutcome(Pawn reader)
        {
            return reader.guest.IsPrisoner;
        }
        public override void OnReadingTick(Pawn reader, float factor)
        {
            if (reader.CurJobDef == JobDefOf.Reading)
            {
                lastUpdate++;
            }
            else
            {
                lastUpdate = 0;
            }
            if (lastUpdate >= hour)
            {
                factor = GetResistanceQuality(base.Quality);
                if (reader.guest.Resistance > .10)
                    reader.guest.resistance -= factor;
                lastUpdate = 0;
            }
        }
        public float GetResistanceQuality(QualityCategory quality)
        {
            int qual = (int)quality;
            if (qual <= 1) { return 1f; }
            if (qual == 2) { return 2f; }
            if (qual == 3) { return 3f; }
            if (qual == 4) { return 4f; }
            if (qual >= 5) { return 5f; }
            return 0;
        }
        public override string GetBenefitsString(Pawn reader = null)
        {
            // authorIdeo.description needs fix;
            return string.Format(" - {0}: .0{1}", "Resistance Loss per hour", GetResistanceQuality(base.Quality));
        }
    }
    public class BookOutcomeProperties_ResistanceModifier : BookOutcomeProperties
    {
        //public Pawn author;
        public override Type DoerClass => typeof(ReadingOutcomeDoerResistanceModifier);
    }

}

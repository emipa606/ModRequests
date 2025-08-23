using RimWorld;
using UnityEngine;
using Verse;

namespace QuickerAnesthetic
{
    public class AnestheticSettings : ModSettings
    {
        public IntRange tickrange = new IntRange(20, 300);            
        
        public bool stages = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref tickrange, "tickrange", new IntRange(20, 300));
            Scribe_Values.Look(ref stages, "stages", false);
            base.ExposeData();
        }
    }
    public class QuickerAnestheticMod : Mod
    {
        public AnestheticSettings settings;

        public QuickerAnestheticMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<AnestheticSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Recovery Stages", ref settings.stages, "Should anesthetic go through stages of recovery");
            listingStandard.Label("Anaesthetic length range (measured in seconds)");
            listingStandard.IntRange(ref settings.tickrange, 0, 1000);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Quicker Anesthetic";
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
            HediffDef anaesthetic = DefDatabase<HediffDef>.GetNamedSilentFail("Anesthetic");
            if (anaesthetic != null)
            {
                anaesthetic.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks = new IntRange(settings.tickrange.min * 60, settings.tickrange.max * 60);
                HediffCompProperties_SeverityPerDay severitycomp = anaesthetic.CompProps<HediffCompProperties_SeverityPerDay>();
                if (severitycomp != null)
                {
                    if (settings.stages == false)
                    {
                        severitycomp.severityPerDay = 0;
                    }
                    else
                    {
                        severitycomp.severityPerDay = -0.8f;
                    }
                }
                else
                {
                    Log.Warning("[Quicker Anesthetic] Could not find anesthetic severity value, another mod likely removed it.");
                }
            }
            
        }
    }

    [StaticConstructorOnStartup]
    public static class Patches
    {

        static Patches()
        {
            HediffDef anaesthetic = DefDatabase<HediffDef>.GetNamedSilentFail("Anesthetic");
            if (anaesthetic != null)
            {
                AnestheticSettings anestheticSettings = LoadedModManager.GetMod<QuickerAnestheticMod>().GetSettings<AnestheticSettings>();
                anaesthetic.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks = new IntRange(anestheticSettings.tickrange.min * 60, anestheticSettings.tickrange.max * 60);
                HediffCompProperties_SeverityPerDay severitycomp = anaesthetic.CompProps<HediffCompProperties_SeverityPerDay>();
                if (severitycomp != null)
                {
                    if (anestheticSettings.stages == false)
                    {
                        severitycomp.severityPerDay = 0;
                    }
                    else
                    {
                        severitycomp.severityPerDay = -0.8f;
                    }
                }
                else
                {
                    Log.Warning("[Quicker Anesthetic] Could not find anesthetic severity value, another mod likely removed it.");
                }
            }
            else
            {
                Log.Error("[Quicker Anesthetic] Could not find anesthetic hediff, did another mod remove it?");
            }
        }
    }

}

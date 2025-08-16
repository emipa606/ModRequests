using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace PsychicAnimals
{
    public class Settings : ModSettings
    {
        public AbilitiesWindow abilitiesWindow;

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref abilitiesWindow, "abilitiesWindow");
            base.ExposeData();
        }
    }

    

    public class PsychicAnimalsMod : Mod
    {
        private static AbilitiesWindow _selectpsycasts;
        public Settings settings;
        public static AbilitiesWindow Selectpsycasts
        {
            get
            {
                if (_selectpsycasts != null)
                {
                    return _selectpsycasts;
                }
                _selectpsycasts = new AbilitiesWindow();
                return _selectpsycasts;
            }
        }
        public PsychicAnimalsMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(() => this.settings = GetSettings<Settings>());
            LongEventHandler.ExecuteWhenFinished(() => _selectpsycasts = settings.abilitiesWindow);
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            if (listingStandard.ButtonText("SelectPsycastsForAnimals".Translate()))
            {
                Find.WindowStack.Add(Selectpsycasts);
            }
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
            settings.abilitiesWindow = Selectpsycasts;
        }

        public override string SettingsCategory()
        {
            return "PsychicAnimalsModSettings".Translate();
        }
    }
}
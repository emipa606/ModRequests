using RimWorld;
using StuffableCore.SCCaching;
using StuffableCore.Settings.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings.BulkEditor
{
    internal class EditorModule : BaseModule
    {
        private EditorSettings settings;
        private StuffableCategorySettings selectedSettings;
        private ThingDef selectedDef;

        private Rect recButtonL;
        private Rect recButtonC;
        private Rect recButtonR;

        protected ISettings innerSettingsWindow;

        public ThingDef SelectedDef { get => selectedDef; set => selectedDef = value; }
        public StuffableCategorySettings SelectedSettings { get => selectedSettings; set => selectedSettings = value; }

        public EditorModule(EditorSettings settings)
        {
            this.settings = settings;
        }

        public static EditorModule GetDefaultEditor(EditorSettings settings)
        {
            return new EditorModule(settings);
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            settings.GetSettingsHeaderCustom(listingStandard);
            listingStandard.Gap();
            listingStandard.Gap();
            Rect rect = listingStandard.GetRect(500);
            listingStandard.Begin(rect);

            Rect innerTopRect = rect;
            innerTopRect.x = 0;
            innerTopRect.y = 0;
            innerTopRect.height = 50;
            Listing_Standard innerTopLs = new Listing_Standard();
            innerTopLs.Begin(innerTopRect);
            float width = innerTopRect.width / 3;

            recButtonL = new Rect(innerTopRect.x, innerTopRect.y, width, innerTopRect.height);
            recButtonC = new Rect(innerTopRect.x + width, innerTopRect.y, width, innerTopRect.height);
            recButtonR = new Rect(innerTopRect.x + (width * 2), innerTopRect.y, width, innerTopRect.height);

            GetButton("Choose clothing/armor to edit.", StuffTagCache.ClothingArmorCache, innerTopLs, recButtonL, true);
            GetButton("Choose weapon to edit.", StuffTagCache.WeaponsCache, innerTopLs, recButtonC);
            GetButton("Choose implant/prosthetic to edit.", StuffTagCache.ImplantsProstheticsCache, innerTopLs, recButtonR);
            innerTopLs.End();

            Rect innerBotRect = new Rect(rect.x, innerTopRect.height, rect.width, rect.height);
            Listing_Standard innerListingStandard = new Listing_Standard();
            innerListingStandard.Begin(innerBotRect);
            if (innerSettingsWindow != null)
                innerSettingsWindow.GetSettings(innerListingStandard);
            innerListingStandard.End();
            listingStandard.End();
        }

        private void GetButton(string text, Dictionary<string, ThingDef> cache, Listing_Standard listing_Standard, Rect rect, bool usesAdditionalStuffMultiplierArmor = false, bool usesAltSearch = false)
        {
            listing_Standard.Begin(rect);
            if (listing_Standard.ButtonText(text) && !cache.NullOrEmpty())
                GetSelectionForButton(cache, usesAdditionalStuffMultiplierArmor, usesAltSearch);
            listing_Standard.End();
        }

        private void GetSelectionForButton(Dictionary<string, ThingDef> cache, bool usesAdditionalStuffMultiplierArmor, bool usesAltSearch)
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();

            foreach (ThingDef shownItemForIcon in cache.Values)
            {
                opts.Add(new FloatMenuOption((string)shownItemForIcon.LabelCap, () =>
                {
                    SelectedDef = shownItemForIcon;
                    string key = SelectedDef.defName;
                    settings.ThingDefSettingsCache.TryGetValue(key, out StuffableCategorySettings value);
                    SelectedSettings = value;
                    SelectedSettings.usesAdditionalStuffMultiplierArmor = usesAdditionalStuffMultiplierArmor && SelectedSettings.usesAdditionalStuffMultiplierArmor;
                    SelectedSettings.usesAltSearch = usesAltSearch;
                    innerSettingsWindow = MainEditorModule.GetDefaultEditor(SelectedSettings);
                }, shownItemForIcon));
            }
            Find.WindowStack.Add(new FloatMenu(opts));
        }
    }
}

using StuffableCore.SCCaching;
using StuffableCore.Settings.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;
using Verse.Noise;
using RimWorld;

namespace StuffableCore.Settings.BulkEditor
{
    internal class BulkEditorModule : BaseModule
    {

        private int columns;
        private int height;
        private int size;
        private List<StuffableCategorySettings> settingsCollection;
        private List<Rect> header;
        private List<ISettings> selectedWindows;
        private ISettings selectedWindow;

        public BulkEditorModule(List<StuffableCategorySettings> settingsCollection, int columns, int height, int size)
        {
            this.columns = columns;
            this.height = height;
            this.size = size;
            header = new List<Rect>();
            this.settingsCollection = settingsCollection;
            selectedWindows = new List<ISettings>();
            foreach (StuffableCategorySettings settings in settingsCollection)
                selectedWindows.Add(MainEditorModule.GetDefaultEditor(settings));
        }

        public static BulkEditorModule GetDefaultEditor(List<StuffableCategorySettings> settingsCollection, int height = 50, int size = 500)
        {
            return new BulkEditorModule(settingsCollection, settingsCollection.Count, height, size);
        }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            listingStandard.Label("Click on a button to get started.");
            if (selectedWindow != null) { listingStandard.Label("Currently displayed settings ► {0}".Formatted(Selected.SettingsLabel)); }
            else{ listingStandard.Gap(); listingStandard.Gap(); }
            listingStandard.Gap();
            Rect rect = listingStandard.GetRect(size);
            listingStandard.Begin(rect);

            Rect innerTopRect = rect;
            innerTopRect.x = 0;
            innerTopRect.y = 0;
            innerTopRect.height = height;
            Listing_Standard innerTopLs = new Listing_Standard();
            innerTopLs.Begin(innerTopRect);
            float width = innerTopRect.width / columns;

            if(header.Count == 0)
                for (int i = 0; i < columns; i++)
                    header.Add(new Rect(innerTopRect.x + (width * i), innerTopRect.y, width, innerTopRect.height));

            foreach (Rect item in header)
                GetHeaderItem(listingStandard, item, header.IndexOf(item));

            innerTopLs.End();

            Rect innerBotRect = new Rect(rect.x, innerTopRect.height, rect.width, rect.height);
            Listing_Standard innerListingStandard = new Listing_Standard();
            innerListingStandard.Begin(innerBotRect);
            if (selectedWindow != null)
                selectedWindow.GetSettings(innerListingStandard);
            innerListingStandard.End();
            listingStandard.End();
        }

        public virtual void GetHeaderItem(Listing_Standard listingStandard, Rect rect, int index, string label = "Test")
        {
            listingStandard.Begin(rect);
            if (listingStandard.ButtonText(settingsCollection.ElementAt(index).SettingsLabel))
            {
                Selected = settingsCollection.ElementAt(index);
                selectedWindow = selectedWindows.ElementAt(index);
            }
            listingStandard.End();
        }
    }
}

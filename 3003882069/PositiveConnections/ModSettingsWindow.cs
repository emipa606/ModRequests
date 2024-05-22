using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnectionsModSettingsWindow : Window
    {
        private Vector2 scrollPosition;
        public PositiveConnectionsModSettings modSettings; // Updated the type here

        public PositiveConnectionsModSettingsWindow(PositiveConnectionsModSettings settings) // Updated the parameter type here
        {
            // Set the window properties
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            // Store the ModSettings instance
            modSettings = settings;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Define the rect for the content area
            Rect contentRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 30f);

            // Begin a scroll view if needed
            Widgets.BeginScrollView(contentRect, ref scrollPosition, contentRect);

            // Create the mod settings GUI elements
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(contentRect);

            // Add a checkbox for the gender adjustment setting
            listing.CheckboxLabeled("Enable Gender Adjustment", ref modSettings.EnableGenderAdjustment);

            // Add a checkbox for disabling all messages
            listing.CheckboxLabeled("Hardcore: Disable all messages (player notifications) from this mod", ref modSettings.DisableAllMessages);

            // Add a slider control for the base interaction frequency
            listing.Label("Base Interaction Frequency");

            float minValue = 0.25f;
            float maxValue = 10f;
            float sliderValue = Mathf.InverseLerp(minValue, maxValue, modSettings.BaseInteractionFrequency);

            float newSliderValue = listing.Slider(sliderValue, 0f, 1f);
            float newBaseInteractionFrequency = Mathf.Lerp(minValue, maxValue, newSliderValue);

            modSettings.BaseInteractionFrequency = newBaseInteractionFrequency;

            listing.Label(newBaseInteractionFrequency.ToString("F2"));


            listing.End();
            Widgets.EndScrollView();

            // Save the mod settings when the window is closed
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 25f, inRect.width, 25f), "Save"))
            {
                modSettings.Write();
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            // Set the window size and position
            windowRect = new Rect(0f, 0f, 400f, 300f);
            windowRect.center = new Vector2(UI.screenWidth / 2f, UI.screenHeight / 2f);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            // Save the mod settings when the window is closed
            modSettings.Write();
        }
    }
}

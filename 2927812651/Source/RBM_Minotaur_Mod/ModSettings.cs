using System;
using UnityEngine;
using Verse;

namespace RBM_Minotaur
{
    public class MinotaurSettings : ModSettings
    {
        // Default Mod Settings
        public const bool milkableFemalesDefault = true;
        public const bool milkableMalesDefault = true;
        public const bool midasDespawnDontDestroyDefault = false;
        public const bool midasRemovesCorpseDefault = true;
        public const int midasGoldAmountDefault = 10;
        public const float TaurailFearRadiusDefault = 3.5f;
        public const float SeeRedFearRadiusDefault = 8.5f;
        public const float SeeRedFleeRadiusDefault = 10.5f;
        public const int SeeRedFearDurationDefault = 120;
        public const int lactateMilkAmountDefault = 25;
        public const bool debugMessagesDefault = false;
        public const bool disableChunksDefault = false;
        public const bool regenChunksDefault = true;
        public const bool canEquipHeavyDefault = true;



        // setting variables to defaults
        public static bool milkableFemales = milkableFemalesDefault;
        public static bool milkableMales = milkableMalesDefault;
        public static bool midasDespawnDontDestroy = midasDespawnDontDestroyDefault;
        public static bool midasRemovesCorpse = midasRemovesCorpseDefault;
        public static int midasGoldAmount = midasGoldAmountDefault;
        public static float TaurailFearRadius = TaurailFearRadiusDefault;
        public static float SeeRedFearRadius = SeeRedFearRadiusDefault;
        public static float SeeRedFleeRadius = SeeRedFleeRadiusDefault;
        public static int SeeRedFearDuration = SeeRedFearDurationDefault;
        public static int lactateMilkAmount = lactateMilkAmountDefault;
        public static bool debugMessages = debugMessagesDefault;
        public static bool disableChunks = disableChunksDefault;
        public static bool regenChunks = regenChunksDefault;
        public static bool canEquipHeavy = canEquipHeavyDefault;

        // Writes settings to file. Note that saving is by ref.
        public override void ExposeData()
        {
            Scribe_Values.Look(ref milkableFemales, "milkableFemales", milkableFemalesDefault);
            Scribe_Values.Look(ref milkableMales, "milkableMales", milkableMalesDefault);
            Scribe_Values.Look(ref midasDespawnDontDestroy, "midasDestroysOrDespawns", midasDespawnDontDestroyDefault);
            Scribe_Values.Look(ref midasRemovesCorpse, "midasRemovesCorpse", midasRemovesCorpseDefault);
            Scribe_Values.Look(ref midasGoldAmount, "midasGoldAmount", midasGoldAmountDefault);
            Scribe_Values.Look(ref TaurailFearRadius, "TaurailFearRadius", TaurailFearRadiusDefault);
            Scribe_Values.Look(ref SeeRedFearRadius, "SeeRedFearRadius", SeeRedFearRadiusDefault);
            Scribe_Values.Look(ref SeeRedFleeRadius, "SeeRedFleeRadius", SeeRedFleeRadiusDefault);
            Scribe_Values.Look(ref SeeRedFearDuration, "SeeRedFearDuration", SeeRedFearDurationDefault);
            Scribe_Values.Look(ref lactateMilkAmount, "lactateMilkAmount", lactateMilkAmountDefault);
            Scribe_Values.Look(ref debugMessages, "debugMessages", debugMessagesDefault);
            Scribe_Values.Look(ref disableChunks, "disableChunks", disableChunksDefault);
            Scribe_Values.Look(ref regenChunks, "regenChunks", regenChunksDefault);
            Scribe_Values.Look(ref canEquipHeavy, "canEquipHeavy", canEquipHeavyDefault);
            base.ExposeData();
        }
    }


    public class RBM_Minotaur_Mod : Mod
    {
        // reference to our settings.
        public MinotaurSettings settings;

        // constructor which resolves the reference to our settings.
        public RBM_Minotaur_Mod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<MinotaurSettings>();
        }

        // The GUI part to set our settings.
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            //listingStandard.Label("Lactation Ability Settings");
            listingStandard.CheckboxLabeled("Lactation active for female pawns", ref MinotaurSettings.milkableFemales);
            listingStandard.CheckboxLabeled("Lactation active for male pawns", ref MinotaurSettings.milkableMales);
            listingStandard.Label("Lactation milk amount: " + MinotaurSettings.lactateMilkAmount.ToString());
            listingStandard.IntAdjuster(ref MinotaurSettings.lactateMilkAmount, 5, 5);

            //listingStandard.Label("See Red Ability Settings");
            MinotaurSettings.SeeRedFearRadius = listingStandard.SliderLabeled("See Red ability radius: " + MinotaurSettings.SeeRedFearRadius.ToString(), MinotaurSettings.SeeRedFearRadius, 1f, 50f, 0.5f, "The radius within which pawns will flee from a pawn using the See Red ability.");
            MinotaurSettings.SeeRedFearRadius = (float)Math.Round(MinotaurSettings.SeeRedFearRadius * 2.0) / 2;
            MinotaurSettings.SeeRedFleeRadius = listingStandard.SliderLabeled("See Red flee radius: " + MinotaurSettings.SeeRedFleeRadius.ToString(), MinotaurSettings.SeeRedFleeRadius, 1f, 50f, 0.5f, "The distance which pawns will flee from a pawn using the See Red ability. Lowest value of distance and time wins.");
            MinotaurSettings.SeeRedFleeRadius = (float)Math.Round(MinotaurSettings.SeeRedFleeRadius * 2.0) / 2;
            float SeeRedFearDurationSeconds = (float)Math.Round((float)MinotaurSettings.SeeRedFearDuration / 60.0f, 1);
            MinotaurSettings.SeeRedFearDuration = (int)listingStandard.SliderLabeled("See Red fear duration: " + SeeRedFearDurationSeconds.ToString() + "s (" + MinotaurSettings.SeeRedFearDuration.ToString() + " ticks)", MinotaurSettings.SeeRedFearDuration, 1f, 600f, 0.5f, "The time period which pawns will flee from a pawn using the See Red ability. Lowest value of distance and time wins.");

            listingStandard.Label(" ");

            //listingStandard.Label("Taurail Gun Weapon Settings");
            MinotaurSettings.TaurailFearRadius = listingStandard.SliderLabeled("Taurail Gun projectile blast radius: " + MinotaurSettings.TaurailFearRadius.ToString(), MinotaurSettings.TaurailFearRadius, 1f, 10f, 0.5f, "The radius in which pawns will flee from a Taurail Gun projectile explosion.");
            MinotaurSettings.TaurailFearRadius = (float)Math.Round(MinotaurSettings.TaurailFearRadius * 2.0) / 2;

            listingStandard.Label(" ");

            listingStandard.Label("Midaspear Weapon Settings");
            listingStandard.CheckboxLabeled("Removes corpses after killing", ref MinotaurSettings.midasRemovesCorpse, "More fun, but makes pawns killed by the Midaspear impossible to resurrect.");
            listingStandard.CheckboxLabeled("Despawns corpses rather than destroy", ref MinotaurSettings.midasDespawnDontDestroy, "Destroying a corpse is a volatile action, and can cause occasional errors. Despawning a pawn will may not trigger some on on-death events.");
            listingStandard.Label("Midaspear gold amount: " + MinotaurSettings.midasGoldAmount.ToString());
            listingStandard.IntAdjuster(ref MinotaurSettings.midasGoldAmount, 5);
            listingStandard.CheckboxLabeled("Heculean pawns can equip heavy weapons", ref MinotaurSettings.canEquipHeavy, "Grants herculean pawns a trait allowing them to equip heavy weapons. May take a few seconds to apply. Does not work on pawns that had the herculean gene before this update - use dev mode to give them the gene again to fix this.");
            if (listingStandard.ButtonText("Reset to Default"))
            {
                MinotaurSettings.milkableFemales = MinotaurSettings.milkableFemalesDefault;
                MinotaurSettings.milkableMales = MinotaurSettings.milkableMalesDefault;
                MinotaurSettings.midasDespawnDontDestroy = MinotaurSettings.midasDespawnDontDestroyDefault;
                MinotaurSettings.midasRemovesCorpse = MinotaurSettings.midasRemovesCorpseDefault;
                MinotaurSettings.midasGoldAmount = MinotaurSettings.midasGoldAmountDefault;
                MinotaurSettings.TaurailFearRadius = MinotaurSettings.TaurailFearRadiusDefault;
                MinotaurSettings.SeeRedFearRadius = MinotaurSettings.SeeRedFearRadiusDefault;
                MinotaurSettings.SeeRedFleeRadius = MinotaurSettings.SeeRedFleeRadiusDefault;
                MinotaurSettings.SeeRedFearDuration = MinotaurSettings.SeeRedFearDurationDefault;
                MinotaurSettings.lactateMilkAmount = MinotaurSettings.lactateMilkAmountDefault;
                MinotaurSettings.canEquipHeavy = MinotaurSettings.canEquipHeavyDefault;
            }

            listingStandard.CheckboxLabeled("Minotaur Debug Messages Active", ref MinotaurSettings.debugMessages, "Degrades performance and spams the log with every function call this mod adds - don't use this during normal gameplay");
            listingStandard.CheckboxLabeled("Disable Weaponised Chunks", ref MinotaurSettings.disableChunks, "Disables all of this mod's chunk changes - you must regenerate chunks after changing this. Always requires restart.");
            listingStandard.CheckboxLabeled("Regenerate Chunks", ref MinotaurSettings.regenChunks, "Destroys and remakes all chunks on the map - useful for fixing saves with invalid chunk properties. This setting disables itself once finished.");

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        // Override SettingsCategory to show up in the list of settings.
        public override string SettingsCategory()
        {
            return "Roo's Minotaur Xenotype";
        }
    }
}

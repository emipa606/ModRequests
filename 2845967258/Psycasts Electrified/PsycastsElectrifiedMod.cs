using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Psycasts_Electrified
{
    public class PsycastsElectrifiedMod : Mod
    {
        Settings settings;

        public PsycastsElectrifiedMod(ModContentPack content) : base(content)
        {
            try
            {
                var harmony = new Harmony("rain.PEM");
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Log.Error("Psycasts Electrified could not patch properly!" + e.ToString());
            }

            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();

            listing.Begin(inRect);
            listing.Label("PEM_Ear_Charge_Rate".Translate() + Settings.earChargeRate.ToString(), -1, "PEM_Ear_Charge_Rate_Tooltip".Translate());
            Settings.earChargeRate = (float)Math.Truncate(listing.Slider(Settings.earChargeRate, 0f, 50f));
            listing.Label("PEM_Ear_Max".Translate() + Settings.earMaxCharge.ToString(), -1, "PEM_Ear_Max_Tooltip".Translate());
            Settings.earMaxCharge = (float)Math.Truncate(listing.Slider(Settings.earMaxCharge, 1000f, 8000f));
            listing.Label("PEM_Ear_Power_Drain".Translate() + Settings.earPowerUsage.ToString(), -1, "PEM_Ear_Power_Drain_Tooltip".Translate());
            Settings.earPowerUsage = (float)Math.Truncate(listing.Slider(Settings.earPowerUsage, 0f, 300f));

            listing.Label("PEM_Overload_Cost".Translate() + Settings.electricalOverloadCost.ToString(), -1, "PEM_Overload_Cost_Tooltip".Translate());
            Settings.electricalOverloadCost = (float)Math.Truncate(listing.Slider(Settings.electricalOverloadCost, 0f, 200f));

            listing.Label("PEM_Superload_Base_Cost".Translate() + Settings.electricalSuperloadBaseCost.ToString(), -1, "PEM_Superload_Base_Cost_Tooltip".Translate());
            Settings.electricalSuperloadBaseCost = (float)Math.Truncate(listing.Slider(Settings.electricalSuperloadBaseCost, 1000f, 10000f));

            listing.Label("PEM_Superload_Increase".Translate() + Settings.electricalSuperloadIncreasePerLevel.ToString(), -1, "PEM_Superload_Increase_Tooltip".Translate());
            Settings.electricalSuperloadIncreasePerLevel = (float)Math.Truncate(listing.Slider(Settings.electricalSuperloadIncreasePerLevel, 1000f, 10000f));

            listing.Label("PEM_Neuralheatsink".Translate() + Settings.neuralHeatsink.ToString(), -1, "PEM_Neuralheatsink_Tooltip".Translate());

            Settings.neuralHeatsink = (float)Math.Truncate(listing.Slider(Settings.neuralHeatsink, 0f, 100f));
            if (listing.ButtonText("PEM_Reset".Translate()))
            {
                Settings.earChargeRate = 60f;
                Settings.earMaxCharge = 6000f;
                Settings.earPowerUsage = 3f;
                Settings.electricalOverloadCost = 50f;
                Settings.electricalSuperloadIncreasePerLevel = 1000f;
                Settings.electricalSuperloadBaseCost = 1000f;
                Settings.neuralHeatsink = 25f;
            }
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Psycasts Electrified";
        }
    }
}

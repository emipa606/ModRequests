﻿/*
 * Faster Aging Mod by Verdiss
 * Mod Version 2.0.0, last updated 2022-11-02, tested for Rimworld 1.4
 * You are free to redistribute this mod and use its code for non-commercial purposes, just give credit to Verdiss in descriptions and in source code.
 */

using Verse;
using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace FasterAging
{
    /// <summary>
    /// Mod Core. Handles settings and setting-values, and provides some key methods for the aging patches.
    /// </summary>
    public class FasterAgingMod : ModBase
    {
        public override string ModIdentifier => "FasterAging";

        //HugsLib Settings
        public static SettingHandle<int> pawnAgingMult;
        public static SettingHandle<int> pawnAgingMultAfterCutoff;
        public static SettingHandle<int> pawnCutoffAge;

        public static SettingHandle<int> animalAgingMult;
        public static SettingHandle<int> animalAgingMultAfterCutoff;
        public static SettingHandle<int> animalCutoffAge;

        public static SettingHandle<bool> enableAgeCutoffs;

        //private SettingHandle<bool> enablePerPawnRateSetting; //Disabled due to not working


        public static SettingHandle<int> biosculptAgeReversalYears;

        public static SettingHandle<int> growthVatAgeTicksPerTick;


        //Vars pulled from the HugsLib Settings
        //public static int pawnAgingMult = 1; //Multiplier to human pawn aging speed (before the age cutoff if that system is enabled)
        //public static int pawnAgingMultAfterCutoff = 1; //Multiplier to human pawn aging speed after the age cutoff
        //public static int pawnCutoffAge = 18; //Human aging rate age cutoff
        public static long pawnCutoffAgeTicks => ((long)pawnCutoffAge * 3600000L) + 1000L; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        //public static int animalAgingMult = 1; //Multiplier to animal pawn aging speed (before the age cutoff if that system is enabled)
        //public static int animalAgingMultAfterCutoff = 1; //Multiplier to animal pawn aging speed after the age cutoff
        //public static int animalCutoffAge = 18; //Animal aging rate age cutoff
        public static long animalCutoffAgeTicks => ((long)animalCutoffAge * 3600000L) + 1000L; //Cutoff age converted to ticks. 1000 ticks are added to this value as a buffer around birthdays to prevent repeatedly calling birthday code when aging is disabled.

        //public static bool enableAgeCutoffs = false; //Whether the age cutoffs system is enabled

        //public static bool enablePerPawnRate = false; //Whether the per-pawn rate system is enabled and its control button shown.


        //public static int biosculptAgeReversalYears = 1; //Number of years that are taken off a pawn's age at the completion of an age-reversal biosculpting process


        //public static int growthVatAgeTicksPerTick = 20; //Numer of biological age ticks gained per tick in a growth vat


        //Misc
        public static Dictionary<string, int> perPawnRates; //Stores any per-pawn custom selected aging rates. Key is pawn's LoadID, accessed via pawn.GetUniqueLoadID(). Value is aging multiplier for that pawn.
        



        /// <summary>
        /// HugsLib runs this during RimWorld executable startup.
        /// Initializes settings and setting values.
        /// </summary>
        public override void DefsLoaded()
        {
            //Setup and get settings values
            pawnAgingMult = Settings.GetHandle<int>(settingName: "fa_pawnAgingMult", title: "fa_settings_pawnAgingMult_title".Translate(), description: "fa_settings_pawnAgingMult_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            
            pawnAgingMultAfterCutoff = Settings.GetHandle<int>(settingName: "fa_pawnAgingMultAfter", title: "fa_settings_pawnAgingMultAfter_title".Translate(), description: "fa_settings_pawnAgingMultAfter_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            pawnAgingMultAfterCutoff.VisibilityPredicate = delegate () { return enableAgeCutoffs.Value; }; //Hide if the system is disabled
            
            pawnCutoffAge = Settings.GetHandle<int>(settingName: "fa_pawnCutoffAge", title: "fa_settings_pawnCutoffAge_title".Translate(), description: "fa_settings_pawnCutoffAge_description".Translate(), defaultValue: 18, Validators.IntRangeValidator(0, 99999));
            pawnCutoffAge.VisibilityPredicate = delegate () { return enableAgeCutoffs.Value; }; //Hide if the system is disabled
            


            animalAgingMult = Settings.GetHandle<int>(settingName: "fa_animalAgingMult", title: "fa_settings_animalAgingMult_title".Translate(), description: "fa_settings_animalAgingMult_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            
            animalAgingMultAfterCutoff = Settings.GetHandle<int>(settingName: "fa_animalAgingMultAfter", title: "fa_settings_animalAgingMultAfter_title".Translate(), description: "fa_settings_animalAgingMultAfter_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(0, 99999));
            animalAgingMultAfterCutoff.VisibilityPredicate = delegate () { return enableAgeCutoffs.Value; }; //Hide if the system is disabled
            
            animalCutoffAge = Settings.GetHandle<int>(settingName: "fa_animalCutoffAge", title: "fa_settings_animalCutoffAge_title".Translate(), description: "fa_settings_animalCutoffAge_description".Translate(), defaultValue: 5, Validators.IntRangeValidator(0, 99999));
            animalCutoffAge.VisibilityPredicate = delegate () { return enableAgeCutoffs.Value; }; //Hide if the system is disabled
            


            enableAgeCutoffs = Settings.GetHandle<bool>(settingName: "fa_enableAgeCutoffs", title: "fa_settings_enableAgeCutoffs_title".Translate(), description: "fa_settings_enableAgeCutoffs_description".Translate(), defaultValue: false);
            


            //Disabled due to not working
            //enablePerPawnRateSetting = Settings.GetHandle<bool>(settingName: "enablePerPawnRate", title: "fa_settings_enablePerPawnRate_title".Translate(), description: "fa_settings_enablePerPawnRate_description".Translate(), defaultValue: false);
            //enablePerPawnRate = enablePerPawnRateSetting.Value;
            



            biosculptAgeReversalYears = Settings.GetHandle<int>(settingName: "fa_biosculptAgeReversalYears", title: "fa_settings_biosculptAgeReversalYears_title".Translate(), description: "fa_settings_biosculptAgeReversalYears_description".Translate(), defaultValue: 1, Validators.IntRangeValidator(1, 100));
            biosculptAgeReversalYears.VisibilityPredicate = delegate () { return ModsConfig.IdeologyActive; }; //Only show when the user has Ideology
            

            growthVatAgeTicksPerTick = Settings.GetHandle<int>(settingName: "fa_growthVatAgeTicksPerTick", title: "fa_settings_growthVatAgeTicksPerTick_title".Translate(), description: "fa_settings_growthVatAgeTicksPerTick_description".Translate(), defaultValue: 20, Validators.IntRangeValidator(1, 99999));
            growthVatAgeTicksPerTick.VisibilityPredicate = delegate () { return ModsConfig.BiotechActive; }; //Only show when the user has Biotech
        }

        /// <summary>
        /// HugsLib runs this when a game world is finished loading, i.e. after loading a save or starting a new game
        /// Handles loading game save data
        /// </summary>
        public override void WorldLoaded()
        {
            //Set the per-pawn rates store to be saved/loaded
            Scribe_Collections.Look(ref perPawnRates, "perPawnRatesFA", LookMode.Value, LookMode.Value);
            if (perPawnRates == null)
            {
                perPawnRates = new Dictionary<string, int>(); //Initialize if the dic wasn't loaded
            }
        }

        /// <summary>
        /// Gets the aging rate multiplier value for the input pawn, based on the mod's settings, and the pawn's type and age, and whether the pawn has a per-pawn rate set
        /// </summary>
        /// <param name="pawn">Pawn to determine the age rate multiplier for</param>
        /// <returns></returns>
        public static int GetPawnAgingMultiplier(Pawn pawn)
        {
            //First check if the pawn has a per-pawn rate selected and use it if they do
            //Disabled as it is not working
            //if (enablePerPawnRate)
            //{
            //    int perPawnRate = GetPerPawnAgingMultiplier(pawn);
            //    if (perPawnRate != -1)
            //    {
            //        return perPawnRate;
            //    }
            //}

            if (!pawn.RaceProps.Humanlike)
            {
                //It's an animal
                if (enableAgeCutoffs && pawn.ageTracker.AgeBiologicalTicks >= animalCutoffAgeTicks)
                {
                    //It's after the cutoff age and that system is enabled
                    //Log.Message("Animal pawn after cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + animalCutoffAgeTicks);
                    return animalAgingMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age or the cutoff system is disabled
                    //Log.Message("Animal pawn before cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + animalCutoffAgeTicks);
                    return animalAgingMult;
                }
            }
            else
            {
                //It's a humanlike
                if (enableAgeCutoffs && pawn.ageTracker.AgeBiologicalTicks >= pawnCutoffAgeTicks)
                {
                    //It's after the cutoff age and that system is enabled
                    //Log.Message("Human pawn after cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + pawnCutoffAgeTicks);
                    return pawnAgingMultAfterCutoff;
                }
                else
                {
                    //It's before the cutoff age or the cutoff system is disabled
                    //Log.Message("Human pawn before cutoff, name: " + pawn.Name + ", age ticks: " + pawn.ageTracker.AgeBiologicalTicks + ", cutoff ticks: " + pawnCutoffAgeTicks);
                    return pawnAgingMult; //This is intentionally the default case - if I add more conditions I should move this to the last else
                }
            }
        }

        //Below are disabled due to per pawn system not working
        ///// <summary>
        ///// Returns the per-pawn custom aging rate associated with the input Pawn, if it exists. If it is not found, returns -1.
        ///// </summary>
        ///// <param name="pawn"></param>
        ///// <returns></returns>
        //public static int GetPerPawnAgingMultiplier(Pawn pawn)
        //{
        //    if (perPawnRates.ContainsKey(pawn.GetUniqueLoadID())) return perPawnRates[pawn.GetUniqueLoadID()];

        //    return -1;
        //}

        ///// <summary>
        ///// Sets the per-pawn custom aging rate for the input pawn to the input multiplier.
        ///// </summary>
        ///// <param name="pawn"></param>
        ///// <param name="mult"></param>
        //public static void SetPerPawnAgingMultiplier(Pawn pawn, int mult)
        //{
        //    perPawnRates[pawn.GetUniqueLoadID()] = mult;
        //}
    }
}

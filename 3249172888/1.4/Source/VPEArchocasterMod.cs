using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VPEArchocaster
{
    [StaticConstructorOnStartup]
    public class VPEArchocasterMod : Mod
    {

        VPEArchocasterMod_Settings settings;
        public bool appliedThrallPatch = false;

        public VPEArchocasterMod(ModContentPack pack) : base(pack)
        {
            LongEventHandler.ExecuteWhenFinished(OnInitialized);
            settings = GetSettings<VPEArchocasterMod_Settings>();
            

            Harmony harmony = new Harmony("VPEArchocaster");


            harmony.PatchAll();
            

        }

        public void OnInitialized()
        {
            VPEACUtils.applySettings();
        }



        public override void DoSettingsWindowContents(Rect inRect)
        {
            float conf = 30f;

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label( "VPEAC.HonourWillDecreaseGoodwill".Translate() + " " + 
                VPEArchocasterMod_Settings.honorToGoodWillRatio);
            Rect rectHonour = listingStandard.GetRect(conf);
            Rect honourSlider = new Rect(rectHonour.xMin, rectHonour.yMin, rectHonour.width / 2 - conf, conf);
            Rect honourText = new Rect(rectHonour.xMin + rectHonour.width / 2 + conf, rectHonour.yMin, 
                rectHonour.width / 2 - conf, conf);
            Widgets.HorizontalSlider(honourSlider, ref VPEArchocasterMod_Settings.honorToGoodWillRatio,
                new FloatRange(0f, 40f), "VPEAC.HonourGoodwillRatio".Translate(), 0.1f);
            string bufferHonour = VPEArchocasterMod_Settings.honorToGoodWillRatio.ToString();
            Widgets.TextFieldNumeric(honourText, ref VPEArchocasterMod_Settings.honorToGoodWillRatio, 
                ref bufferHonour, -1000f, 1000f);
            
            listingStandard.Label(TranslatorFormattedStringExtensions.Translate("VPEAC.ThrallsMultiplierInt", 
                new NamedArgument(VPEArchocasterMod_Settings.allowThrallsLearning.ToString(), "multiplier")));
            Rect rectLearning = listingStandard.GetRect(conf);
            Rect learningSlider = new Rect(rectLearning.xMin, rectLearning.yMin, rectLearning.width / 2 - conf, conf);
            Rect learningText = new Rect(rectLearning.xMin + rectLearning.width / 2 + conf, 
                rectLearning.yMin, rectLearning.width / 2 - conf, conf);
            Widgets.HorizontalSlider(learningSlider, ref VPEArchocasterMod_Settings.allowThrallsLearning,
                new FloatRange(0f, VPEArchocasterMod_Settings.maxThrallLearningMultiplier), "VPEAC.ThrallsMultiplier".Translate(), 0.005f);
            string bufferLearning = VPEArchocasterMod_Settings.allowThrallsLearning.ToString();
            Widgets.TextFieldNumeric(learningText, ref VPEArchocasterMod_Settings.allowThrallsLearning,
                ref bufferLearning, 0f);




            listingStandard.End();


            base.DoSettingsWindowContents(inRect);
        }


        public override string SettingsCategory()
        {
            return "Archocaster (VPE)";
        }

        public override void WriteSettings()
        {
            VPEACUtils.applySettings();
            base.WriteSettings();
        }


        


    }





}

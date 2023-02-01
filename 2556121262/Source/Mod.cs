using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using Verse.Sound;
using HarmonyLib;
using Verse;
using UnityEngine;
using AlienRace;

namespace HalfDragons
{
    public class HalfDragonsMod : Mod
    {
        ToggleSettings settings;
        public HalfDragonsMod(ModContentPack content) : base(content)
        {

         


            this.settings = GetSettings<ToggleSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            this.settings = GetSettings<ToggleSettings>();
            listingStandard.CheckboxLabeled("Use vanilla eyes (requires reloading the save to take effect)", ref settings.eyes, "Use vanilla eyes (requires reloading the save)");
            if (settings.eyes)
            {
               
               
                DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.ToList().Find(A => A.defName == "HalfDragon").alienRace.generalSettings.alienPartGenerator.bodyAddons.Find(J => J.bodyPart == "right eye").path = "pawn/eyes/vanilla/REye";
                DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.ToList().Find(A => A.defName == "HalfDragon").alienRace.generalSettings.alienPartGenerator.bodyAddons.Find(J => J.bodyPart == "left eye").path = "pawn/eyes/vanilla/LEye";
               
            }
            else
            {
              

                DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.ToList().Find(A => A.defName == "HalfDragon").alienRace.generalSettings.alienPartGenerator.bodyAddons.Find(J => J.bodyPart == "right eye").path = "pawn/eyes/REye";
                DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.ToList().Find(A => A.defName == "HalfDragon").alienRace.generalSettings.alienPartGenerator.bodyAddons.Find(J => J.bodyPart == "left eye").path = "pawn/eyes/LEye";
                

            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);

        }
        public override string SettingsCategory()
        {
            return "Half Dragons eyes settings";
        }
    }
    public class ToggleSettings : ModSettings
    {

        public bool eyes;
       




        public override void ExposeData()
        {
            Scribe_Values.Look(ref eyes, "eyes");
        



            base.ExposeData();
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace USESetApparelDrawOrder
{
    class USESetApparelDrawOrderSettings : Mod
    {
        public USESetApparelDrawOrderSettings(ModContentPack content) : base(content)
        {
            GetSettings<USESetApparelDrawOrderModSettings>();
        }

        public override string SettingsCategory() => "USE Set Apparel Draw Order";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            USESetApparelDrawOrderModSettings.DoWindowContents(inRect);
        }
    }

    public class USESetApparelDrawOrderModSettings : ModSettings
    {
        public static bool USESetApparelDrawOrderSettings_WorkEveryPawn = false;
        public static bool USESetApparelDrawOrderSettings_DrawHeadApparelWhenInBed = false;
        public static bool USESetApparelDrawOrderSettings_EnableFreeMode= false;
        public static float USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider = 2500f;
        public static string USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider_buffer = "";

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();

            ls.Begin(inRect);
            ls.Gap(12f);
            ls.CheckboxLabeled("Functional on every pawn? (Colonist alwasy functional)", ref USESetApparelDrawOrderSettings_WorkEveryPawn);
            ls.Gap(12f);

            ls.Gap(12f);
            ls.CheckboxLabeled("Disable Draw Head apparel when pawn in bed", ref USESetApparelDrawOrderSettings_DrawHeadApparelWhenInBed);
            ls.Gap(12f);

            ls.Gap(12f);
            ls.CheckboxLabeled("Enable Free mode", ref USESetApparelDrawOrderSettings_EnableFreeMode);
            ls.Gap(12f);

            ls.Gap(12f);
            ls.TextFieldNumericLabeled("DrawOffsetDivider", ref USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider, ref USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider_buffer);
            ls.Gap(12f);

            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref USESetApparelDrawOrderSettings_WorkEveryPawn, "USESetApparelDrawOrderSettings_WorkEveryPawn");
            Scribe_Values.Look(ref USESetApparelDrawOrderSettings_DrawHeadApparelWhenInBed, "USESetApparelDrawOrderSettings_DrawHeadApparelWhenInBed");
            Scribe_Values.Look(ref USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider, "USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider");
            Scribe_Values.Look(ref USESetApparelDrawOrderSettings_EnableFreeMode, "USESetApparelDrawOrderSettings_EnableFreeMode");
            USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider_buffer = USESetApparelDrawOrderSettings_ApparelDrawOffsetDivider.ToString();
        }
    }
}

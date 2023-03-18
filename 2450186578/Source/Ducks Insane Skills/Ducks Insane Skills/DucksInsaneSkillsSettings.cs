using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace DucksInsaneSkills
{
    public class DucksInsaneSkillsSettings : ModSettings
    {
        public int MaxLevel = 0;

        public bool PatchSkillCap = true;
        public int ValueSkillCap = 2000;

        public bool MuteCraftNotifications = true;

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = 250f;
            listing_Standard.Begin(canvas);
            listing_Standard.Label(Translator.Translate("DucksSkillsOptionHeader"));
            listing_Standard.Gap(12f);
            //listing_Standard.Label(Translator.Translate("DucksSkillsComingSoon"));

            //string MaxLevel_str = MaxLevel.ToString();
            //listing_Standard.Label(Translator.Translate("MaxLevelOption"), tooltip: Translator.Translate("MaxLevelLabel"));
            //listing_Standard.TextFieldNumeric(ref MaxLevel, ref MaxLevel_str, 0f, 1000f);
            //listing_Standard.TextFieldNumericLabeled(Translator.Translate("MaxLevelOption"), ref MaxLevel, ref MaxLevel_str, 0f, 1000f);

            //listing_Standard.Gap(6f);

            string ValueSkillCap_str = ValueSkillCap.ToString();
            listing_Standard.Label(Translator.Translate("XpCapOption"), tooltip: Translator.Translate("XpCapLabel"));
            listing_Standard.TextFieldNumeric(ref ValueSkillCap, ref ValueSkillCap_str, 0f, 100000f);
            //listing_Standard.TextFieldNumericLabeled(Translator.Translate("XpCapOption"), ref ValueSkillCap, ref ValueSkillCap_str, 0f, 100000f);

            listing_Standard.Gap(6f);

            listing_Standard.CheckboxLabeled(Translator.Translate("MuteCraftOption"), ref MuteCraftNotifications, Translator.Translate("MuteCraftLabel"));

            listing_Standard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MaxLevel, "MaxLevel", 0);

            Scribe_Values.Look(ref PatchSkillCap, "PatchSkillCap", true);
            Scribe_Values.Look(ref ValueSkillCap, "ValueSkillCap", 2000);

            Scribe_Values.Look(ref MuteCraftNotifications, "MuteCraftNotifications", true);
            base.ExposeData();
        }
    }
}
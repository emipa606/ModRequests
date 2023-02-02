using UnityEngine;
using Verse;

namespace ArchotechPlus
{
    public class ArchotechPlus : Mod
    {
        private readonly ArchotechPlusSettings _settings;

        public ArchotechPlus(ModContentPack content) : base(content)
        {
            _settings = GetSettings<ArchotechPlusSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var targetAgeBuffer = ArchotechPlusSettings.TargetAge.ToString();
            var healingChargesBuffer = ArchotechPlusSettings.MaxHealingCharges.ToString();
            var resurrectorChargesBuffer = ArchotechPlusSettings.MaxResurrectionCharges.ToString();

            var listingStandard = new Listing_Standard(GameFont.Small) { ColumnWidth = inRect.width / 3f };
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Regenerator can de-age", ref ArchotechPlusSettings.RegeneratorDeAge, "The regenerator will change the age of the pawn when used.\nDefault: Enabled");

            if (ArchotechPlusSettings.RegeneratorDeAge)
            {
                listingStandard.Label("Regenerator's Target Age in Years:", -1,
                "This is the average age that the regenerator implant will return the pawn to over time. \nDefault: 28");
            }
            listingStandard.Label("Maximum Healing Charges:", -1,
                "This is the maximum number of charges a level 2 or level 3 implant can accumulate over time. More charges will result in the implant being able to heal multiple parts with only an in-game hour between healing events. \nDefault: 1");
            listingStandard.Gap(Text.LineHeight / 2);
            listingStandard.Label("Healing Time Range(Hours): ", -1,
                "The regenerator will regain a healing charge somewhere in this time range when above level 1.\nDefault: 120 - 240 hours");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Regenerator Can Resurrect", ref ArchotechPlusSettings.RegeneratorResurrects, "The level 3 regenerator implant will attempt to resurrect the user if this is enabled.\nDefault: Enabled");
            if (ArchotechPlusSettings.RegeneratorResurrects)
            {
                listingStandard.Label("\tMax Resurrection Charges:", -1, "The number of resurrecting charges that the level 3 regenerator will be able to accumulate over time.\nDefault: 1");
                listingStandard.Gap(Text.LineHeight / 2);
                listingStandard.Label("\tResurrection Time Range (Hours):", -1,
                    "The pawn will be resurrected somewhere in the hour range set here, should the pawns body and head not be destroyed and the resurrector have a charge.\nDefault: 12 - 96 hours.");
            }
            listingStandard.NewColumn();
            listingStandard.Gap(Text.LineHeight + 3f);
            if (ArchotechPlusSettings.RegeneratorDeAge)
            {
                listingStandard.IntEntry(ref ArchotechPlusSettings.TargetAge, ref targetAgeBuffer);
            }
            listingStandard.IntEntry(ref ArchotechPlusSettings.MaxHealingCharges, ref healingChargesBuffer);
            listingStandard.IntRange(ref ArchotechPlusSettings.HealingRange, 1, 360);
            listingStandard.Gap();
            if (ArchotechPlusSettings.RegeneratorResurrects)
            {
                listingStandard.Gap(Text.LineHeight + 3f);
                listingStandard.IntEntry(ref ArchotechPlusSettings.MaxResurrectionCharges, ref resurrectorChargesBuffer);
                listingStandard.IntRange(ref ArchotechPlusSettings.ResurrectionRange, 1, 360);
            }
            else
            {

                listingStandard.Gap(Text.LineHeight);
            }
            listingStandard.NewColumn();
            listingStandard.Gap(Text.LineHeight * 2.75f);
            listingStandard.Label(
                (ArchotechPlusSettings.HealingRange.min / 24 != 0 ? $"{ArchotechPlusSettings.HealingRange.min / 24} Days" : "")
                + (ArchotechPlusSettings.HealingRange.min / 24 != 0 && ArchotechPlusSettings.HealingRange.min % 24 != 0 ? ", " : "")
                + (ArchotechPlusSettings.HealingRange.min % 24 != 0 ? $"{ArchotechPlusSettings.HealingRange.min % 24} Hours" : "")
                + " - "
                + (ArchotechPlusSettings.HealingRange.max / 24 != 0 ? $"{ArchotechPlusSettings.HealingRange.max / 24} Days" : "")
                + (ArchotechPlusSettings.HealingRange.max / 24 != 0 && ArchotechPlusSettings.HealingRange.max % 24 != 0 ? ", " : "")
                + (ArchotechPlusSettings.HealingRange.max % 24 != 0 ? $"{ArchotechPlusSettings.HealingRange.max % 24} Hours" : ""));
            if (ArchotechPlusSettings.RegeneratorResurrects)
            {
                listingStandard.Gap(Text.LineHeight * 3.25f);
                listingStandard.Label(
                    (ArchotechPlusSettings.ResurrectionRange.min / 24 != 0 ? $"{ArchotechPlusSettings.ResurrectionRange.min / 24} Days" : "")
                    + (ArchotechPlusSettings.ResurrectionRange.min / 24 != 0 && ArchotechPlusSettings.ResurrectionRange.min % 24 != 0 ? ", " : "")
                    + (ArchotechPlusSettings.ResurrectionRange.min % 24 != 0 ? $"{ArchotechPlusSettings.ResurrectionRange.min % 24} Hours" : "")
                    + " - "
                    + (ArchotechPlusSettings.ResurrectionRange.max / 24 != 0 ? $"{ArchotechPlusSettings.ResurrectionRange.max / 24} Days" : "")
                    + (ArchotechPlusSettings.ResurrectionRange.max / 24 != 0 && ArchotechPlusSettings.ResurrectionRange.max % 24 != 0 ? ", " : "")
                    + (ArchotechPlusSettings.ResurrectionRange.max % 24 != 0 ? $"{ArchotechPlusSettings.ResurrectionRange.max % 24} Hours" : ""));
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            Log.Message($"[Archotech+] Regenerator Target Age: {ArchotechPlusSettings.TargetAge}");
            Log.Message($"[Archotech+] Maximum Healing Charges: {ArchotechPlusSettings.MaxHealingCharges} charges");
            Log.Message($"[Archotech+] Regenerator Resurrection is enabled with {ArchotechPlusSettings.MaxResurrectionCharges} maximum charges");
            base.WriteSettings();
        }

        public override string SettingsCategory()
        {
            return "Archotech+";
        }
    }
}
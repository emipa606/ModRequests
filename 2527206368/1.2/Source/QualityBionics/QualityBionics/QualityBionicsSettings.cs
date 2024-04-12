using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace QualityBionics
{
    class QualityBionicsSettings : ModSettings
    {
        public Dictionary<QualityCategory, float> qualityMultipliers = new Dictionary<QualityCategory, float>
        {
            {QualityCategory.Awful, 0.50f},
            {QualityCategory.Poor, 0.75f},
            {QualityCategory.Normal, 1f},
            {QualityCategory.Good, 1.25f},
            {QualityCategory.Excellent, 1.5f},
            {QualityCategory.Masterwork, 1.7f},
            {QualityCategory.Legendary, 2f},
        };

        public Dictionary<QualityCategory, float> hpQualityMultipliers = new Dictionary<QualityCategory, float>
        {
            {QualityCategory.Awful, 0.50f},
            {QualityCategory.Poor, 0.75f},
            {QualityCategory.Normal, 1f},
            {QualityCategory.Good, 1.25f},
            {QualityCategory.Excellent, 1.5f},
            {QualityCategory.Masterwork, 1.7f},
            {QualityCategory.Legendary, 2f},
        };
        public float GetQualityMultipliers(QualityCategory quality)
        {
            return qualityMultipliers[quality];
        }
        public float GetQualityMultipliersForHP(QualityCategory quality)
        {
            return hpQualityMultipliers[quality];
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref qualityMultipliers, "qualityMultipliers", LookMode.Value, LookMode.Value, ref qualityKeys, ref floatValues);
            Scribe_Collections.Look(ref hpQualityMultipliers, "hpQualityMultipliers", LookMode.Value, LookMode.Value, ref qualityKeys2, ref floatValues2);
        }

        private List<QualityCategory> qualityKeys;
        private List<float> floatValues;

        private List<QualityCategory> qualityKeys2;
        private List<float> floatValues2;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            var num = rect.width * 0.5f;
            listingStandard.ColumnWidth = num;
            listingStandard.Begin(rect);
            listingStandard.Label("QB.BodyPartEfficiencyMultipliers".Translate());
            foreach (QualityCategory quality in Enum.GetValues(typeof(QualityCategory)))
            {
                var value = qualityMultipliers[quality];
                listingStandard.SliderLabeled(Enum.GetName(typeof(QualityCategory), quality), ref value, (value * 100f).ToStringDecimalIfSmall() + "%", 0.01f, 5f);
                qualityMultipliers[quality] = value;
            }
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                qualityMultipliers = new Dictionary<QualityCategory, float>
                {
                    {QualityCategory.Awful, 0.50f},
                    {QualityCategory.Poor, 0.75f},
                    {QualityCategory.Normal, 1f},
                    {QualityCategory.Good, 1.25f},
                    {QualityCategory.Excellent, 1.5f},
                    {QualityCategory.Masterwork, 1.7f},
                    {QualityCategory.Legendary, 2f},
                };
            }

            listingStandard.NewColumn();
            listingStandard.Label("QB.BodyPartHPMultipliers".Translate());
            listingStandard.ColumnWidth = rect.width - num;
            foreach (QualityCategory quality in Enum.GetValues(typeof(QualityCategory)))
            {
                var value = hpQualityMultipliers[quality];
                listingStandard.ColumnWidth -= 30;
                listingStandard.SliderLabeled(Enum.GetName(typeof(QualityCategory), quality), ref value, (value * 100f).ToStringDecimalIfSmall() + "%", 0.01f, 5f);
                listingStandard.ColumnWidth += 30;
                hpQualityMultipliers[quality] = value;
            }
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                hpQualityMultipliers = new Dictionary<QualityCategory, float>
                {
                    {QualityCategory.Awful, 0.50f},
                    {QualityCategory.Poor, 0.75f},
                    {QualityCategory.Normal, 1f},
                    {QualityCategory.Good, 1.25f},
                    {QualityCategory.Excellent, 1.5f},
                    {QualityCategory.Masterwork, 1.7f},
                    {QualityCategory.Legendary, 2f},
                };
            }

            listingStandard.End();
            base.Write();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BeautyStatMod
{
    public enum StatFormula
    {
        SquareRoot = 0,
        Linear = 1
    }
    public class BeautyStatSettings : ModSettings
    {
        public float NegotiationStatFactor = 20f;
        public float ConversionStatFactor = 20f;
        public float TradePriceStatFactor = 10f;
        public float SuppressionStatFactor = 5f;
        public float SocialImpactStatFactor = 20f;
        //public bool exampleBool = true;
        //public StatFormula StatBeautyFormula = StatFormula.Linear;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref NegotiationStatFactor, "NegotiationStatFactor", 20f);
            Scribe_Values.Look(ref ConversionStatFactor, "ConversionStatFactor", 20f);
            Scribe_Values.Look(ref TradePriceStatFactor, "TradePriceStatFactor", 10f);
            Scribe_Values.Look(ref SuppressionStatFactor, "SuppressionStatFactor", 5f);
            Scribe_Values.Look(ref SocialImpactStatFactor, "SocialImpactStatFactor", 20f);
            //Scribe_Values.Look(ref StatBeautyFormula, "StatBeautyFormula", StatFormula.Linear);

            //Scribe_Values.Look(ref ManipulationAffects, "ManipulationAffects", true);
            //Scribe_Values.Look(ref ManipulationMoreForDamage, "ManipulationMoreForDamage", true);
        }

        public class BeautyStatSettingsMod : Mod
        {
            public BeautyStatSettings settings;

            public BeautyStatSettingsMod(ModContentPack content) : base(content)
            {
                this.settings = GetSettings<BeautyStatSettings>();
            }

            public override void DoSettingsWindowContents(Rect inRect)
            {
                Listing_Standard list = new Listing_Standard();

                list.Begin(inRect);
                list.Label("NegotiationStatFactor".Translate() + ": " + settings.NegotiationStatFactor + "%");
                settings.NegotiationStatFactor = list.Slider(settings.NegotiationStatFactor, 0, 40f);
                list.Label("ConversionStatFactor".Translate() + ": " + settings.ConversionStatFactor + "%");
                settings.ConversionStatFactor = list.Slider(settings.ConversionStatFactor, 0, 40f);
                list.Label("TradePriceStatFactor".Translate() + ": " + settings.TradePriceStatFactor + "%");
                settings.TradePriceStatFactor = list.Slider(settings.TradePriceStatFactor, 0, 40f);
                list.Label("SuppressionStatFactor".Translate() + ": " + settings.SuppressionStatFactor + "%");
                settings.SuppressionStatFactor = list.Slider(settings.SuppressionStatFactor, 0, 40f);
                list.Label("SocialImpactStatFactor".Translate() + ": " + settings.SocialImpactStatFactor + "%");
                settings.SocialImpactStatFactor = list.Slider(settings.SocialImpactStatFactor, 0, 40f);
                /*list.CheckboxLabeled("TypeofScaling", ref settings.exampleBool);
                foreach (var label in Enum.GetNames(typeof(StatFormula)))
                {
                    var val = (StatFormula)Enum.Parse(typeof(StatFormula), label);
                    if (list.RadioButton(label.Translate(), settings.StatBeautyFormula == val, 8f)) settings.StatBeautyFormula = val;
                }*/
                list.Gap(28f);

                if (list.ButtonText("RestoreToDefaultSettings".Translate()))
                {
                    settings.NegotiationStatFactor = 20f;
                    settings.ConversionStatFactor = 20f;
                    settings.TradePriceStatFactor = 10f;
                    settings.SuppressionStatFactor = 5f;
                    settings.SocialImpactStatFactor = 20f;
                }
                list.End();
                //base.DoSettingsWindowContents(inRect);
            }

            public override string SettingsCategory()
            {
                return "Beauty Stat Settings";
            }
        }
        }
}

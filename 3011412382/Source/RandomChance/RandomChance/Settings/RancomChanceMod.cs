using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class RancomChanceMod : Mod
    {
        RandomChanceSettings settings;
        public static RancomChanceMod mod;
        private Vector2 scrollPos = Vector2.zero;

        public RancomChanceMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<RandomChanceSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Listing_Standard list1 = new();
            Rect viewRect1 = new(inRect.x, inRect.y, inRect.width / 2 - 20, inRect.height - 40); // 1st rect, left-side panel
            Rect vROffset1 = new(0, 0, inRect.width / 2 - 40, inRect.height + 100); // 2nd panel inside 1st, "100", update as more settings are added to list
            //Widgets.BeginScrollView(viewRect1, ref scrollPos, vROffset1, true);

            list1.Begin(vROffset1);
            list1.Gap(48.00f);

            list1.CheckboxLabeled("RC_AllowMessages".Translate(), ref settings._allowMessages, "RC_AllowMessagesDesc".Translate());
            list1.Gap(6.0f);

            list1.End();
            //Widgets.EndScrollView();

            // list 1 end, list 2 begin

            Listing_Standard list2 = new ();
            Rect viewRect2 = new(inRect.x + 450, inRect.y, inRect.width / 2 - 20, inRect.height - 40); // 2nd rect, right-side panel
            Rect vROffset2 = new(450, 0, inRect.width / 2 - 40, inRect.height + 100); // 2nd panel inside 1st, "100", update as more settings are added to list
            Widgets.BeginScrollView(viewRect2, ref scrollPos, vROffset2, true);

            list2.Begin(vROffset2);
            list2.Gap(6.00f);

            // Cooking
            list2.Label("<color=#ff6666>Cooking</color>");
            
            list2.Gap(3.00f);
            float cookingFailureChanceSlider = settings._cookingFailureChance;
            string cookingFailureChanceSliderText = cookingFailureChanceSlider.ToString("F2");
            list2.Label(label: "RC_CookingFailureChance".Translate(cookingFailureChanceSliderText), tooltip: "RC_CookingFailureChanceDesc".Translate());
            settings._cookingFailureChance = list2.Slider(settings._cookingFailureChance, 0.0f, 1.0f);

            float failedCookingFireSizeSlider = settings._failedCookingFireSize;
            string failedCookingFireSizeSliderText = failedCookingFireSizeSlider.ToString("F2");
            list2.Label(label: "RC_FailedCookingFireSize".Translate(failedCookingFireSizeSliderText), tooltip: "RC_FailedCookingFireSizeDesc".Translate());
            settings._failedCookingFireSize = list2.Slider(settings._failedCookingFireSize, 1.0f, 15.0f);

            float cookingBetterMealChanceSlider = settings._cookingBetterMealChance;
            string cookingBetterMealChanceSliderText = cookingBetterMealChanceSlider.ToString("F2");
            list2.Label(label: "RC_CookingBetterMealChance".Translate(cookingBetterMealChanceSliderText), tooltip: "RC_CookingBetterMealChanceDesc".Translate());
            settings._cookingBetterMealChance = list2.Slider(settings._cookingBetterMealChance, 0.0f, 1.0f);
            list2.Gap(6.0f);

            // Butchering
            list2.Label("<color=#ff6666>Butchering</color>");
            
            list2.Gap(3.00f);
            float butcheringFailureChanceSlider = settings._butcheringFailureChance;
            string butcheringFailureChanceSliderText = butcheringFailureChanceSlider.ToString("F2");
            list2.Label(label: "RC_ButcheringFailureChance".Translate(butcheringFailureChanceSliderText), tooltip: "RC_ButcheringFailureChanceDesc".Translate());
            settings._butcheringFailureChance = list2.Slider(settings._butcheringFailureChance, 0.0f, 1.0f);

            float butcherMessRadiusSlider = settings._butcherMessRadius;
            string butcherMessRadiusSliderText = butcherMessRadiusSlider.ToString("F0");
            list2.Label(label: "RC_ButcherMessRadius".Translate(butcherMessRadiusSliderText), tooltip: "RC_ButcherMessRadiusDesc".Translate());
            settings._butcherMessRadius = (int)list2.Slider(settings._butcherMessRadius, 1, 5);

            float bonusButcherProductChanceSlider = settings._bonusButcherProductChance;
            string bonusButcherProductChanceSliderText = bonusButcherProductChanceSlider.ToString("F2");
            list2.Label(label: "RC_BonusButcherProductChance".Translate(bonusButcherProductChanceSliderText), tooltip: "RC_BonusButcherProductChanceDesc".Translate());
            settings._bonusButcherProductChance = list2.Slider(settings._bonusButcherProductChance, 0.0f, 1.0f);
            list2.Gap(6.0f);

            // Cremating
            list2.Label("<color=#ff6666>Cremating</color>");
            
            list2.Gap(3.00f);
            float crematingInjuryChanceSlider = settings._crematingInjuryChance;
            string crematingInjuryChanceSliderText = crematingInjuryChanceSlider.ToString("F2");
            list2.Label(label: "RC_CrematingInjuryChance".Translate(crematingInjuryChanceSliderText), tooltip: "RC_CrematingInjuryChanceDesc".Translate());
            settings._crematingInjuryChance = list2.Slider(settings._crematingInjuryChance, 0.0f, 1.0f);
            list2.Gap(6.0f);

            // Repairing
            list2.Label("<color=#ff6666>Repairing</color>");
            
            list2.Gap(3.00f);
            float electricalRepairFailureChanceSlider = settings._electricalRepairFailureChance;
            string electricalRepairFailureChanceSliderText = electricalRepairFailureChanceSlider.ToString("F2");
            list2.Label(label: "RC_ElectricalRepairFailureChance".Translate(electricalRepairFailureChanceSliderText), tooltip: "RC_ElectricalRepairFailureChanceDesc".Translate());
            settings._electricalRepairFailureChance = list2.Slider(settings._electricalRepairFailureChance, 0.0f, 1.0f);
            list2.Gap(6.0f);

            // Plant work
            list2.Label("<color=#ff6666>Plant work</color>");
            
            list2.Gap(3.00f);
            float plantHarvestingFindEggsChanceSlider = settings._plantHarvestingFindEggsChance;
            string plantHarvestingFindEggsChanceSliderText = plantHarvestingFindEggsChanceSlider.ToString("F2");
            list2.Label(label: "RC_PlantHarvestingFindEggsChance".Translate(plantHarvestingFindEggsChanceSliderText), tooltip: "RC_PlantHarvestingFindEggsChanceDesc".Translate());
            settings._plantHarvestingFindEggsChance = list2.Slider(settings._plantHarvestingFindEggsChance, 0.0f, 1.0f);
            list2.Gap(6.0f);

            // Animal work
            list2.Label("<color=#ff6666>Animal work</color>");
            
            list2.Gap(3.00f);
            float hurtByFarmAnimalChanceSlider = settings._hurtByFarmAnimalChance;
            string hurtByFarmAnimalChanceSliderText = hurtByFarmAnimalChanceSlider.ToString("F2");
            list2.Label(label: "RC_HurtByFarmAnimalChance".Translate(hurtByFarmAnimalChanceSliderText), tooltip: "RC_HurtByFarmAnimalChanceDesc".Translate());
            settings._hurtByFarmAnimalChance = list2.Slider(settings._hurtByFarmAnimalChance, 0.0f, 1.0f);
            list2.Gap(6.0f);

            list2.End();
            Widgets.EndScrollView();
        }

        public override string SettingsCategory()
        {
            return "RC_ModName".Translate();
        }
    }
}

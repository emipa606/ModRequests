using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Desperate_Hunger
{
    public class DesperateHungerModSettings : ModSettings
    {
        public bool featureEnabled = true;

        public float minimumMalnutritionToHuntWoundedTarget = 0.0f;
        public float minimumMalnutritionToHuntHealthyTarget = 0.2f;

        public bool desperatePredatorsIgnoreSize = true;
        public bool desperatePreyIgnoreSize = true;
        public bool desperateAnimalsIgnoreCombatPower = true;
        public bool desperateHerdAnimalsEatOwnKind = true;
        public bool desperateAnimalsAttackAllies = false;
        public bool desperateHumans = false;

        public bool ignoreFertilizedEggs = false;
        public bool desperatePreyTargetRandomly = false;

        public float preyTargetMaxRelativeSize = 1.0f;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref featureEnabled, "featureEnabled", true);

            Scribe_Values.Look(ref desperatePredatorsIgnoreSize, "desperatePredatorsIgnoreSize", true);
            Scribe_Values.Look(ref desperatePreyIgnoreSize, "desperatePreyIgnoreSize", true);
            
            Scribe_Values.Look(ref desperateAnimalsIgnoreCombatPower, "desperateAnimalsIgnoreCombatPower", true);
            Scribe_Values.Look(ref desperateHerdAnimalsEatOwnKind, "desperateHerdAnimalsEatOwnKind", true);

            Scribe_Values.Look(ref desperateAnimalsAttackAllies, "desperateAnimalsAttackAllies", false);
            Scribe_Values.Look(ref desperateHumans, "desperateHumans", false);

            Scribe_Values.Look(ref minimumMalnutritionToHuntWoundedTarget, "minimumMalnutritionToHuntWoundedTarget", 0.0f);
            Scribe_Values.Look(ref minimumMalnutritionToHuntHealthyTarget, "minimumMalnutritionToHuntHealthyTarget", 0.2f);

            Scribe_Values.Look(ref ignoreFertilizedEggs, "ignoreFertilizedEggs", false);
            Scribe_Values.Look(ref desperatePreyTargetRandomly, "desperatePreyTargetRandomly", false);

            Scribe_Values.Look(ref preyTargetMaxRelativeSize, "preyTargetMaxRelativeSize", 1.0f);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 34f) / 2f;

            listing_Standard.Begin(inRect);

            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Feature_Enabled_Label".Translate(), ref featureEnabled, "CM_Desperate_Hunger_Settings_Feature_Enabled_Tooltip".Translate());

            listing_Standard.GapLine();

            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Predators_Ignore_Size_Label".Translate(), ref desperatePredatorsIgnoreSize, "CM_Desperate_Hunger_Settings_Desperate_Predators_Ignore_Size_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Prey_Ignore_Size_Label".Translate(), ref desperatePreyIgnoreSize, "CM_Desperate_Hunger_Settings_Desperate_Prey_Ignore_Size_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Animals_Ignore_Combat_Power_Label".Translate(), ref desperateAnimalsIgnoreCombatPower, "CM_Desperate_Hunger_Settings_Desperate_Animals_Ignore_Combat_Power_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Herd_Animals_Eat_Own_Kind_Label".Translate(), ref desperateHerdAnimalsEatOwnKind, "CM_Desperate_Hunger_Settings_Desperate_Herd_Animals_Eat_Own_Kind_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Animals_Attack_Allies_Label".Translate(), ref desperateAnimalsAttackAllies, "CM_Desperate_Hunger_Settings_Desperate_Animals_Attack_Allies_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Humans_Label".Translate(), ref desperateHumans, "CM_Desperate_Hunger_Settings_Desperate_Humans_Tooltip".Translate());
            
            listing_Standard.GapLine();

            listing_Standard.Label("CM_Desperate_Hunger_Settings_Prey_Target_Max_Relative_Size_Label".Translate(), -1, "CM_Desperate_Hunger_Settings_Prey_Target_Max_Relative_Size_Tooltip".Translate());
            listing_Standard.Label(preyTargetMaxRelativeSize.ToString("F2") + "x");
            preyTargetMaxRelativeSize = listing_Standard.Slider(preyTargetMaxRelativeSize, 0.0f, 20.0f);

            listing_Standard.Label("CM_Desperate_Hunger_Settings_Minimum_Malnutrition_Wounded_Label".Translate(), -1, "CM_Desperate_Hunger_Settings_Minimum_Malnutrition_Wounded_Tooltip".Translate());
            listing_Standard.Label(minimumMalnutritionToHuntWoundedTarget.ToString("P0"));
            minimumMalnutritionToHuntWoundedTarget = listing_Standard.Slider(minimumMalnutritionToHuntWoundedTarget, 0.0f, 1.0f);

            listing_Standard.Label("CM_Desperate_Hunger_Settings_Minimum_Malnutrition_Healthy_Label".Translate(), -1, "CM_Desperate_Hunger_Settings_Minimum_Malnutrition_Healthy_Tooltip".Translate());
            listing_Standard.Label(minimumMalnutritionToHuntHealthyTarget.ToString("P0"));
            minimumMalnutritionToHuntHealthyTarget = listing_Standard.Slider(minimumMalnutritionToHuntHealthyTarget, 0.0f, 1.0f);

            listing_Standard.GapLine();

            listing_Standard.Label("CM_Desperate_Hunger_Settings_Special_Label".Translate());

            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Ignore_Fertilized_Eggs_Label".Translate(), ref ignoreFertilizedEggs, "CM_Desperate_Hunger_Settings_Ignore_Fertilized_Eggs_Tooltip".Translate());
            listing_Standard.CheckboxLabeled("CM_Desperate_Hunger_Settings_Desperate_Prey_Select_Random_Label".Translate(), ref desperatePreyTargetRandomly, "CM_Desperate_Hunger_Settings_Desperate_Prey_Select_Random_Tooltip".Translate());

            listing_Standard.End();
        }

        public void UpdateSettings()
        {

        }
    }
}

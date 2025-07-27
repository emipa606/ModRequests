using System;
using CM_Callouts.PendingCallouts;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000006 RID: 6
	public class CalloutModSettings : ModSettings
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000020F0 File Offset: 0x000002F0
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.enableCalloutsCombat, "enableCalloutsCombat", true, false);
			Scribe_Values.Look<bool>(ref this.enableCalloutsAnimal, "enableCalloutsAnimal", true, false);
			Scribe_Values.Look<bool>(ref this.queueTextMotes, "queueTextMotes", true, false);
			Scribe_Values.Look<bool>(ref this.attachCalloutText, "attachCalloutText", true, false);
			Scribe_Values.Look<bool>(ref this.drawLabelBackgroundForTextMotes, "drawLabelBackgroundForTextMotes", true, false);
			Scribe_Values.Look<float>(ref this.baseCalloutChance, "baseCalloutChance", 0.15f, false);
			Scribe_Values.Look<ShowWoundLevel>(ref this.showWoundLevel, "showWoundLevel", ShowWoundLevel.All, false);
			Scribe_Values.Look<bool>(ref this.allowCalloutsWhenTargetingAnimals, "allowCalloutsWhenTargetingAnimals", false, false);
			Scribe_Values.Look<bool>(ref this.allowCalloutsForAnimals, "allowCalloutsForAnimals", false, false);
			Scribe_Values.Look<bool>(ref this.showDebugLogMessages, "showDebugLogMessages", false, false);
			Scribe_Values.Look<bool>(ref this.hyperNuzzling, "hyperNuzzling", false, false);
			Scribe_Values.Look<bool>(ref this.forceInitiatorCallouts, "forceInitiatorCallouts", false, false);
			Scribe_Values.Look<bool>(ref this.forceRecipientCallouts, "forceRecipientCallouts", false, false);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002200 File Offset: 0x00000400
		public void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = (inRect.width - 34f) / 2f;
			listing_Standard.Begin(inRect);
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Do_Callouts_Combat_Label".Translate(), ref this.enableCalloutsCombat, "CM_Callouts_Settings_Do_Callouts_Combat_Description".Translate());
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Do_Callouts_Animal_Label".Translate(), ref this.enableCalloutsAnimal, "CM_Callouts_Settings_Do_Callouts_Animal_Description".Translate());
			listing_Standard.GapLine(12f);
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Queue_Text_Motes_Label".Translate(), ref this.queueTextMotes, "CM_Callouts_Settings_Queue_Text_Motes_Description".Translate());
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Attach_Callout_Text_Label".Translate(), ref this.attachCalloutText, "CM_Callouts_Settings_Attach_Callout_Text_Description".Translate());
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Draw_Label_Background_For_Text_Motes_Label".Translate(), ref this.drawLabelBackgroundForTextMotes, "CM_Callouts_Settings_Draw_Label_Background_For_Text_Motes_Description".Translate());
			listing_Standard.GapLine(12f);
			listing_Standard.Label("CM_Callouts_Settings_Show_Wound_Level_Label".Translate(), -1f, "CM_Callouts_Settings_Show_Wound_Level_Description".Translate());
			bool flag = listing_Standard.RadioButton("CM_Callouts_Settings_Show_Wounds_None_Label".Translate(), this.showWoundLevel == ShowWoundLevel.None, 8f, "CM_Callouts_Settings_Show_Wounds_None_Description".Translate(), null);
			if (flag)
			{
				this.showWoundLevel = ShowWoundLevel.None;
			}
			bool flag2 = listing_Standard.RadioButton("CM_Callouts_Settings_Show_Wounds_Destroyed_Label".Translate(), this.showWoundLevel == ShowWoundLevel.Destroyed, 8f, "CM_Callouts_Settings_Show_Wounds_Destroyed_Description".Translate(), null);
			if (flag2)
			{
				this.showWoundLevel = ShowWoundLevel.Destroyed;
			}
			bool flag3 = listing_Standard.RadioButton("CM_Callouts_Settings_Show_Wounds_Major_Label".Translate(), this.showWoundLevel == ShowWoundLevel.Major, 8f, "CM_Callouts_Settings_Show_Wounds_Major_Description".Translate(), null);
			if (flag3)
			{
				this.showWoundLevel = ShowWoundLevel.Major;
			}
			bool flag4 = listing_Standard.RadioButton("CM_Callouts_Settings_Show_Wounds_Serious_Label".Translate(), this.showWoundLevel == ShowWoundLevel.Serious, 8f, "CM_Callouts_Settings_Show_Wounds_Serious_Description".Translate(), null);
			if (flag4)
			{
				this.showWoundLevel = ShowWoundLevel.Serious;
			}
			bool flag5 = listing_Standard.RadioButton("CM_Callouts_Settings_Show_Wounds_All_Label".Translate(), this.showWoundLevel == ShowWoundLevel.All, 8f, "CM_Callouts_Settings_Show_Wounds_All_Description".Translate(), null);
			if (flag5)
			{
				this.showWoundLevel = ShowWoundLevel.All;
			}
			listing_Standard.GapLine(12f);
			listing_Standard.Label("CM_Callouts_Settings_Base_Callout_Chance_Label".Translate(), -1f, "CM_Callouts_Settings_Base_Callout_Chance_Description".Translate());
			listing_Standard.Label(this.baseCalloutChance.ToString("P0"), -1f, null);
			this.baseCalloutChance = listing_Standard.Slider(this.baseCalloutChance, 0f, 1f);
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Callout_When_Targeting_Animals_Label".Translate(), ref this.allowCalloutsWhenTargetingAnimals, "CM_Callouts_Settings_Callout_When_Targeting_Animals_Description".Translate());
			listing_Standard.CheckboxLabeled("CM_Callouts_Settings_Animal_Callouts_Label".Translate(), ref this.allowCalloutsForAnimals, "CM_Callouts_Settings_Animal_Callouts_Description".Translate());
			bool devMode = Prefs.DevMode;
			if (devMode)
			{
				listing_Standard.Gap(24f);
				listing_Standard.Label("Debug settings", -1f, null);
				listing_Standard.GapLine(12f);
				listing_Standard.CheckboxLabeled("showDebugLogMessages", ref this.showDebugLogMessages, null);
				listing_Standard.CheckboxLabeled("hyperNuzzling", ref this.hyperNuzzling, null);
				listing_Standard.CheckboxLabeled("forceInitiatorCallouts", ref this.forceInitiatorCallouts, null);
				listing_Standard.CheckboxLabeled("forceRecipientCallouts", ref this.forceRecipientCallouts, null);
			}
			listing_Standard.End();
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000025F8 File Offset: 0x000007F8
		public void UpdateSettings()
		{
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000025FC File Offset: 0x000007FC
		public bool CalloutCategoryEnabled(CalloutCategory category)
		{
			bool result;
			if (category != CalloutCategory.Combat)
			{
				result = (category != CalloutCategory.Animal || this.enableCalloutsAnimal);
			}
			else
			{
				result = this.enableCalloutsCombat;
			}
			return result;
		}

		// Token: 0x04000024 RID: 36
		public bool enableCalloutsCombat = true;

		// Token: 0x04000025 RID: 37
		public bool enableCalloutsAnimal = true;

		// Token: 0x04000026 RID: 38
		public bool queueTextMotes = true;

		// Token: 0x04000027 RID: 39
		public bool attachCalloutText = true;

		// Token: 0x04000028 RID: 40
		public ShowWoundLevel showWoundLevel = ShowWoundLevel.Major;

		// Token: 0x04000029 RID: 41
		public bool drawLabelBackgroundForTextMotes = true;

		// Token: 0x0400002A RID: 42
		public float baseCalloutChance = 0.15f;

		// Token: 0x0400002B RID: 43
		public bool allowCalloutsWhenTargetingAnimals = false;

		// Token: 0x0400002C RID: 44
		public bool allowCalloutsForAnimals = false;

		// Token: 0x0400002D RID: 45
		public bool showDebugLogMessages = false;

		// Token: 0x0400002E RID: 46
		public bool hyperNuzzling = false;

		// Token: 0x0400002F RID: 47
		public bool forceInitiatorCallouts = false;

		// Token: 0x04000030 RID: 48
		public bool forceRecipientCallouts = false;
	}
}

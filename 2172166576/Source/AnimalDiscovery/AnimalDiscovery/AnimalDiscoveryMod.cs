using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnimalDiscovery
{
	[StaticConstructorOnStartup]
    public class AnimalDiscoveryMod : Mod
    {
		public static AnimalDiscoverySettings Settings
		{
			get
			{
				AnimalDiscoverySettings animalDiscoverySettings = LoadedModManager.GetMod<AnimalDiscoveryMod>().settings;
				animalDiscoverySettings.ResolveItemDef();
				return animalDiscoverySettings;
			}
		}

		public AnimalDiscoveryMod(ModContentPack content) : base(content)
		{
			this.settings = base.GetSettings<AnimalDiscoverySettings>();
			this.settings.ResolveItemDef();
			this.scrollPosition = new Vector2(0f, 0f);
			//this.floatFieldSettingBodySize = new FloatField(this.settings.settingBodySize);
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			float num = inRect.y;
			num += 2f;
			Rect rect = new Rect(inRect.x, num, 320f, 28f);
			Widgets.CheckboxLabeled(new Rect(0f, num, 440f, 24f), "AnimalDiscovery.notOnlyPlayerHome".Translate(), ref this.settings.notOnlyPlayerHome, false, null, null, false);
			num += rect.height + 10f;
			Widgets.ListSeparator(ref num, inRect.width, "AnimalDiscovery.AlertsToNotify".Translate());
			
			num += 2f;
			Rect rect2 = new Rect(rect.xMax, num, 640f, rect.height);
			Widgets.CheckboxLabeled(new Rect(0f, num, 520f, 24f), "AnimalDiscovery.predatorAlert".Translate(), ref this.settings.predatorAlert, false, null, null, false);
			//num += rect.height;
			//Widgets.Label(new Rect(0f, num, 640f, 18f), "AnimalDiscovery.predatorAlertDesc".Translate());
			/*num += rect.height + 2f;
			Widgets.CheckboxLabeled(new Rect(0f, num, 640f, 24f), "AnimalDiscovery.detailAlert".Translate(), ref this.settings.detailAlert, false, null, null, false);*/
			num += rect.height + 2f;
			Widgets.CheckboxLabeled(new Rect(0f, num, 520f, 24f), "AnimalDiscovery.customAlert".Translate(), ref this.settings.customAlert, false, null, null, false);
			//num += rect.height;
			//Widgets.Label(new Rect(0f, num, 640f, 18f), "AnimalDiscovery.customAlertDesc".Translate());

			/*if (this.floatFieldSettingBodySize.Update(rect2))
			{
				this.settings.settingBodySize = this.floatFieldSettingBodySize.Value;
			}*/
			Rect rect3 = new Rect(rect.xMax, num, 320f, rect.height);
			Listing_Standard listing_Standard = new Listing_Standard();
			
			num += rect.height + 4f;
			if (AnimalDiscoveryMod.Settings.alertTargetAnimal == AnimalDiscoverySettings.AlertTargetAnimal.Custom)
			{
				Widgets.ListSeparator(ref num, inRect.width, "AnimalDiscovery.TitleAnimalDefs".Translate());
				num += 2f;
				Text.Font = GameFont.Small;
				Rect rect5 = new Rect(inRect.x, num, 300f, 30f);
				this.DrawAddAnimalDefButton(rect5);
				num += rect5.height + 4f;
				Rect outRect = new Rect(inRect.x, num, 336f, inRect.height - num - 10f);
				float width = outRect.width - 16f;
				Rect rect6 = new Rect(0f, 0f, width, this.CalcHeight());
				Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect6, true);
				listing_Standard.Begin(rect6);
				this.DrawAnimals(listing_Standard);
				listing_Standard.End();
				Widgets.EndScrollView();
			}
		}

		private float CalcHeight()
		{
			float num = 0f;
			Text.Font = GameFont.Small;
			return num + 30f * (float)AnimalDiscoveryMod.Settings.SettingItems.Count<AnimalAlertSettingItem>();
		}

		/*private void DrawAddAnimalDefButtonPredator(Rect rect)
		{
			List<ThingDef> list = new List<ThingDef>();
			list.AddRange(from thingDef in DefDatabase<ThingDef>.AllDefsListForReading
						  where !this.settings.settingItems.Exists((AnimalAlertSettingItem item) => item.def == thingDef) && thingDef.race != null && thingDef.race.Animal == true && thingDef.race.predator == true
						  select thingDef);
			if (!list.NullOrEmpty<ThingDef>())
			{
				this.settings.settingItems.Add(new AnimalAlertSettingItem(list));
				return;
			}
			List<FloatMenuOption> list3 = new List<FloatMenuOption>();
			list3.Add(new FloatMenuOption("AnimalDiscovery.NoAnimalDef".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
			Find.WindowStack.Add(new FloatMenu(list3));
		}*/


		private void DrawAddAnimalDefButton(Rect rect)
		{
			if (Widgets.ButtonText(rect, "AnimalDiscovery.AddAnimalDef".Translate(), true, true, true))
			{
				List<ThingDef> list = new List<ThingDef>();
				list.AddRange(from thingDef in DefDatabase<ThingDef>.AllDefsListForReading
							  where !this.settings.settingItems.Exists((AnimalAlertSettingItem item) => item.def == thingDef) && thingDef.race != null && thingDef.race.Animal == true
							  select thingDef);
				if (!list.NullOrEmpty<ThingDef>())
				{
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					using (List<ThingDef>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ThingDef animalDef = enumerator.Current;
							list2.Add(new FloatMenuOption(animalDef.LabelCap, delegate ()
							{
								this.settings.settingItems.Add(new AnimalAlertSettingItem(animalDef));
							}, MenuOptionPriority.Default, null, null, 0f, null, null));
						}
					}
					Find.WindowStack.Add(new FloatMenu(list2));
					return;
				}
				List<FloatMenuOption> list3 = new List<FloatMenuOption>();
				list3.Add(new FloatMenuOption("AnimalDiscovery.NoAnimalDef".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
				Find.WindowStack.Add(new FloatMenu(list3));
			}
		}

		private void DrawAnimals(Listing_Standard list)
		{
			this.settings.ResolveItemDef();
			Text.Font = GameFont.Small;
			list.ColumnWidth = 330f;
			string deleteDefName = null;
			for (int i = 0; i < AnimalDiscoveryMod.Settings.SettingItems.Count; i++)
			{
				Rect rect = list.GetRect(24f);
				if (this.DoAnimalRow(rect, i))
				{
					deleteDefName = AnimalDiscoveryMod.Settings.SettingItems[i].defName;
				}
				list.Gap(6f);
			}
			if (deleteDefName != null)
			{
				AnimalDiscoveryMod.Settings.settingItems.RemoveAll((AnimalAlertSettingItem item) => item.defName == deleteDefName);
			}
		}

		private bool DoAnimalRow(Rect rect, int index)
		{
			bool result = false;
			AnimalAlertSettingItem animalAlertSettingItem = AnimalDiscoveryMod.Settings.SettingItems[index];
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			GUI.BeginGroup(rect);
			WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
			Color color = GUI.color;
			if (!animalAlertSettingItem.IsAvailable)
			{
				GUI.color = Color.red;
			}
			Rect rect2 = widgetRow.Label(animalAlertSettingItem.Label, 260f);
			if (!animalAlertSettingItem.IsAvailable)
			{
				TooltipHandler.TipRegion(rect2, "AnimalDiscovery.AddedByUnloadMod".Translate());
				GUI.color = color;
			}
			//widgetRow.ToggleableIcon(ref animalAlertSettingItem.onlyGrowingZone, ContentFinder<Texture2D>.Get("UI/Buttons/ShowZones", true), "AnimalDiscovery.ToggleOnlyGrowingZone".Translate(), null, null);
			if (widgetRow.ButtonIcon(ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true), null, null, true))
			{
				result = true;
			}
			GUI.EndGroup();
			return result;
		}

		public override string SettingsCategory()
		{
			return "Animal Discovery";
		}

		private Vector2 scrollPosition;

		public AnimalDiscoverySettings settings;

		private FloatField floatFieldSettingBodySize;

		private bool settingsNotOnlyPlayerHome;
	}
}

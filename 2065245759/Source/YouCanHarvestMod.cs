using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YouCanHarvest {
    [StaticConstructorOnStartup]
    public class YouCanHarvestMod : Mod {
        private Vector2 scrollPosition;
        public YouCanHarvestSettings settings;

        public static YouCanHarvestSettings Settings {
            get {
                YouCanHarvestSettings settings = LoadedModManager.GetMod<YouCanHarvestMod>().settings;
                settings.ResolveItemDef();
                return settings;
            }
        }

        public YouCanHarvestMod(ModContentPack content) : base(content) {
            this.settings = GetSettings<YouCanHarvestSettings>();
            this.settings.ResolveItemDef();
            this.scrollPosition = new Vector2(0f, 0f);
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            float num = inRect.y;

            {
                num += 2f;

                Rect rectLabel = new Rect(inRect.x, num, 320f, 28f);
                Widgets.Label(rectLabel, "YouCanHarvest.AlertTargetPlant".Translate());
                {
                    Rect rectRadioButton = new Rect(rectLabel.xMax, num, 320f, rectLabel.height);
                    bool isSelected = (settings.alertTargetPlant == YouCanHarvestSettings.AlertTargetPlant.All);
                    if(Widgets.RadioButtonLabeled(rectRadioButton, "YouCanHarvest.AlertTargetPlantAll".Translate(), isSelected)) {
                        settings.alertTargetPlant = YouCanHarvestSettings.AlertTargetPlant.All;
                    }
                }
                num += rectLabel.height + 4f;

                {
                    Rect rectRadioButton = new Rect(rectLabel.xMax, num, 320f, rectLabel.height);
                    bool isSelected = (settings.alertTargetPlant == YouCanHarvestSettings.AlertTargetPlant.InGrowingArea);
                    if (Widgets.RadioButtonLabeled(rectRadioButton, "YouCanHarvest.AlertTargetPlantInGrowingArea".Translate(), isSelected)) {
                        settings.alertTargetPlant = YouCanHarvestSettings.AlertTargetPlant.InGrowingArea;
                    }
                }
                num += rectLabel.height + 4f;

                {
                    Rect rectRadioButton = new Rect(rectLabel.xMax, num, 320f, rectLabel.height);
                    bool isSelected = (settings.alertTargetPlant == YouCanHarvestSettings.AlertTargetPlant.Custom);
                    if (Widgets.RadioButtonLabeled(rectRadioButton, "YouCanHarvest.AlertTargetPlantCustom".Translate(), isSelected)) {
                        settings.alertTargetPlant = YouCanHarvestSettings.AlertTargetPlant.Custom;
                    }
                }
                num += rectLabel.height + 4f;
            }

            if(Settings.alertTargetPlant == YouCanHarvestSettings.AlertTargetPlant.Custom){
                Widgets.ListSeparator(ref num, inRect.width, "YouCanHarvest.TitlePlantDefs".Translate());
                num += 2f;

                Text.Font = GameFont.Small;
                Rect buttonRect = new Rect(inRect.x, num, 300, 30);
                DrawAddPlantDefButton(buttonRect);
                num += buttonRect.height + 4f;

                Rect outRect = new Rect(inRect.x, num, 336, inRect.height - num - 10f);
                float width = outRect.width - 16f;
                Rect viewRect = new Rect(0f, 0f, width, CalcHeight());
                Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(viewRect);
                DrawPlants(listing_Standard);
                listing_Standard.End();

                Widgets.EndScrollView();
            }
        }

        private float CalcHeight() {
            float height = 0f;

            Text.Font = GameFont.Small;
            height += (30f) * Settings.SettingItems.Count();

            return height;
        }

        private void DrawAddPlantDefButton(Rect rect) {
            if (Widgets.ButtonText(rect,"YouCanHarvest.AddPlantDef".Translate())) {
                List<ThingDef> plantDefs = new List<ThingDef>();
                plantDefs.AddRange(from thingDef in DefDatabase<ThingDef>.AllDefsListForReading
                                                where !settings.settingItems.Exists(item => item.def == thingDef) && thingDef.plant != null && thingDef.plant.Harvestable
                                   select thingDef);

                if (!plantDefs.NullOrEmpty()) {
                    List<FloatMenuOption> listFloatMenu = new List<FloatMenuOption>();
                    foreach (ThingDef plantDef in plantDefs) {
                        listFloatMenu.Add(new FloatMenuOption(plantDef.LabelCap, delegate {
                            settings.settingItems.Add(new PlantAlertSettingItem(plantDef, false));
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(listFloatMenu));
                } else {
                    List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                    list2.Add(new FloatMenuOption("YouCanHarvest.NoPlantDef".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                    Find.WindowStack.Add(new FloatMenu(list2));
                }
            }
        }

        private void DrawPlants(Listing_Standard list) {
            this.settings.ResolveItemDef();

            Text.Font = GameFont.Small;
            list.ColumnWidth = 330f;
            string deleteDefName = null;
            for (int i = 0; i < Settings.SettingItems.Count; i++) {
                Rect rect = list.GetRect(24f);
                if(DoPlantRow(rect, i)) {
                    deleteDefName = Settings.SettingItems[i].defName;
                }
                list.Gap(6f);
            }
            if (deleteDefName != null) {
                Settings.settingItems.RemoveAll(item => item.defName == deleteDefName);
            }
        }

        private bool DoPlantRow(Rect rect, int index) {
            bool deleteFlag = false;

            PlantAlertSettingItem item = Settings.SettingItems[index];
            if (Mouse.IsOver(rect)) {
                Widgets.DrawHighlight(rect);
            }

            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
            Color color = GUI.color;
            if (!item.IsAvailable) {
                GUI.color = Color.red;
            }
            Rect rectLabel = widgetRow.Label(item.Label, 260f);
            if (!item.IsAvailable) {
                TooltipHandler.TipRegion(rectLabel, "YouCanHarvest.AddedByUnloadMod".Translate());
                GUI.color = color;
            }
            widgetRow.ToggleableIcon(ref item.onlyGrowingZone, ContentFinder<Texture2D>.Get("UI/Buttons/ShowZones", true), "YouCanHarvest.ToggleOnlyGrowingZone".Translate());
            if (widgetRow.ButtonIcon(ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true))) {
                deleteFlag = true;
            }
            GUI.EndGroup();

            return deleteFlag;
        }

        public override string SettingsCategory() {
            return "You Can Harvest!";
        }
    }
}

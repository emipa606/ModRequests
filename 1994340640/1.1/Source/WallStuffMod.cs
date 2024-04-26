using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace WallStuff
{
    class WallStuffMod : Mod
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 0f;

        public WallStuffMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(GetSettings);
            LongEventHandler.ExecuteWhenFinished(BuildDictionary);
        }

        public void GetSettings()
        {
            GetSettings<WallStuffSettings>();
        }

        public static IEnumerable<ThingDef> PossibleThingDefs()
        {
            return from d in DefDatabase<ThingDef>.AllDefs
                   where (d.category == ThingCategory.Item && d.scatterableOnMapGen && !d.destroyOnDrop && !d.MadeFromStuff)
                   select d;
        }

        private void BuildDictionary()
        {
            if (WallStuffSettings.listOfSpawnableThings == null)
            {
                ThingCountExposable gWorldMedicine = new ThingCountExposable
                {
                    count = 2,
                    thingDef = ThingDef.Named("MedicineUltratech")
                };
                WallStuffSettings.listOfSpawnableThings = new List<ThingCountExposable>
                {
                    gWorldMedicine
                };
            }
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard list = new Listing_Standard()
            {
                ColumnWidth = rect.width
            };

            list.Begin(rect);

            list.Label("Wall Heater Heating Power: " + WallStuffSettings.heaterPower);
            WallStuffSettings.heaterPower = list.Slider(WallStuffSettings.heaterPower, 0f, 100f).RoundToAsInt(1);

            list.Label("Wall Cooler Cooling Power: " + WallStuffSettings.coolerPower);
            WallStuffSettings.coolerPower = list.Slider(WallStuffSettings.coolerPower, 0f, -100f).RoundToAsInt(1);

            list.Gap(15);
            {
                Rect labelRect = list.GetRect(32).Rounded();
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRect, "Replicatable Items");
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            list.Gap(1);

            {
                Rect buttonsRect = list.GetRect(Text.LineHeight).Rounded();
                Rect addRect = buttonsRect.LeftThird();
                Rect rmvRect = buttonsRect.MiddleThird();
                Rect resRect = buttonsRect.RightThird();

                // Add an entry to the dictionary
                if (Widgets.ButtonText(addRect, "Add Thing"))
                {
                    List<FloatMenuOption> thingList = new List<FloatMenuOption>();
                    foreach (ThingDef current in from t in PossibleThingDefs()
                                                 orderby t.label
                                                 select t)
                    {

                        bool skip = false;
                        for (int i = 0; i < WallStuffSettings.listOfSpawnableThings.Count; i++)
                        {
                            if (WallStuffSettings.listOfSpawnableThings[i].thingDef == current)
                            {
                                skip = true;
                                break;
                            }
                        };
                        if (skip) continue;

                        thingList.Add(new FloatMenuOption(current.LabelCap, delegate {
                            WallStuffSettings.listOfSpawnableThings.Add(new ThingCountExposable(current, 1));
                        }));
                    }
                    FloatMenu menu = new FloatMenu(thingList);
                    Find.WindowStack.Add(menu);
                }

                // Remove an entry from the dictionary
                if (Widgets.ButtonText(rmvRect, "Remove Thing") && WallStuffSettings.listOfSpawnableThings.Count >= 2)
                {
                    List<FloatMenuOption> thingList = new List<FloatMenuOption>();
                    foreach (ThingCountExposable current in from t in WallStuffSettings.listOfSpawnableThings
                                                            orderby t.thingDef.label
                                                            select t)
                    {
                        ThingDef localTd = current.thingDef;
                        thingList.Add(new FloatMenuOption(localTd.LabelCap, delegate {
                            for (int i = 0; i < WallStuffSettings.listOfSpawnableThings.Count; i++)
                            {
                                if (WallStuffSettings.listOfSpawnableThings[i].thingDef == localTd)
                                {
                                    WallStuffSettings.listOfSpawnableThings.Remove(WallStuffSettings.listOfSpawnableThings[i]);
                                    break;
                                }
                            };
                        }));
                    }
                    FloatMenu menu = new FloatMenu(thingList);
                    Find.WindowStack.Add(menu);
                }

                // Reset the dictionary
                if (Widgets.ButtonText(resRect, "Reset List"))
                {
                    //OreDictionary.Build();
                }
            }

            list.Gap(5);
            {
                Rect listRect = list.GetRect(300f).Rounded();
                Rect cRect = listRect.ContractedBy(10f);
                Rect position = new Rect(cRect.x, cRect.y, cRect.width, cRect.height);
                Rect outRect = new Rect(0f, 0f, position.width, position.height);
                Rect viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);

                float num = 0f;
                List<ThingCountExposable> dict = new List<ThingCountExposable>(WallStuffSettings.listOfSpawnableThings);

                GUI.BeginGroup(position);
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

                foreach (var tc in dict.Select((value, index) => new { index, value }))
                {
                    Rect entryRect = new Rect(0f, num, viewRect.width, 32);                    
                    Rect iconRect = entryRect.LeftPartPixels(32);
                    Rect labelRect = new Rect(entryRect.LeftThird().x + 33f, entryRect.y, entryRect.LeftThird().width - 33f, entryRect.height);
                    Rect pctRect = new Rect(entryRect.LeftHalf().RightPartPixels(41).x, entryRect.y, 40f, entryRect.height);
                    Rect sliderRect = new Rect(entryRect.RightHalf().x, entryRect.y, entryRect.RightHalf().width, entryRect.height);


                    Widgets.ThingIcon(iconRect, tc.value.thingDef);
                    Widgets.Label(labelRect, tc.value.thingDef.LabelCap + " - BMV - " + tc.value.thingDef.BaseMarketValue);
                    Widgets.Label(pctRect, $"{WallStuffSettings.listOfSpawnableThings[tc.index].count}");
                    int val = tc.value.count;
                    val = Widgets.HorizontalSlider(
                            sliderRect,
                            WallStuffSettings.listOfSpawnableThings[tc.index].count, 0f, 100f, true
                        ).RoundToAsInt(1);
                    if (val != WallStuffSettings.listOfSpawnableThings[tc.index].count)
                    {
                        WallStuffSettings.listOfSpawnableThings[tc.index].count = val;
                    }

                    if (Mouse.IsOver(entryRect))
                    {
                        Widgets.DrawHighlight(entryRect);
                    }
                    TooltipHandler.TipRegion(entryRect.LeftThird(), tc.value.thingDef.description);

                    num += 32f;
                    scrollViewHeight = num;
                }

                Widgets.EndScrollView();
                GUI.EndGroup();
            }

            list.End();
        }

        public override string SettingsCategory()
        {
            return "Wall Stuff";
        }
    }
}

using System;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace WealthHere
{

    [StaticConstructorOnStartup]
    public static class WealthHereData
    {
        public readonly static Texture2D ShowWealth= ContentFinder<Texture2D>.Get("UI/Buttons/ShowWealth");
    }



    public class WealthHereMod : Mod
    {
        public static bool showWealth=false;

        

        public WealthHereMod(ModContentPack content) : base(content)
        {
            new Harmony(nameof(WealthHereMod)).PatchAll();
        }


        


        [HarmonyPatch(typeof(PlaySettings))]
        [HarmonyPatch("DoPlaySettingsGlobalControls")]
        public static class Patch_PlaySettings_DoPlaySettingsGlobalControls
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                if(!worldView)
                {
                    row.ToggleableIcon(ref showWealth, WealthHereData.ShowWealth, "ShowWealthToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
                }
            }
        }

    }

    [HarmonyPatch(typeof(Verse.EnvironmentStatsDrawer))]
    [HarmonyPatch("EnvironmentStatsOnGUI")]
    public static class Patch_EnvironmentStatsDrawer_EnvironmentStatsOnGUI
    {
        public static void Postfix()
        {
            WealthHereDrawer.DrawWealthInfo();
        }
    }


    [HarmonyPatch(typeof(MapInterface))]
    [HarmonyPatch("MapInterfaceOnGUI_BeforeMainTabs")]
    public static class Patch_MapInterface_MapInterfaceOnGUI_BeforeMainTabs
    {
        public static void Postfix()
        {
            WealthHereDrawer.DrawWealthAroundMouse();
        }
    }


    public static class WealthHereDrawer
    {
        private static List<Thing> wealthCountedThings = new List<Thing>();
        private static Color ColorPoor = Color.grey;
        private static Color ColorRich = Color.green;

        private static HashSet<Thing> buildingsArtificial = new HashSet<Thing>();
        private static HashSet<Thing> haulableEverThings = new HashSet<Thing>();
        private static HashSet<Pawn> playerFactionPawns = new HashSet<Pawn>();
        private static float currentAreaWealth = 0;
        


        public static void DrawWealthInfo()
        {
            if (!WealthHereMod.showWealth) return;
            DrawWealthAroundMouse();
            Text.Font = GameFont.Small;
            Rect windowRect = GetRect();
            Find.WindowStack.ImmediateWindow(74975, windowRect, WindowLayer.Super, () => FillWindow(windowRect));
            
        }

        private static Rect GetRect()
        {
            Rect rect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 414f, 24f);
            rect.height += 25f;
            rect.x += 26f;
            rect.y += 26f;
            if ((double)rect.xMax > (double)UI.screenWidth)
                rect.x -= rect.width + 52f;
            if ((double)rect.yMax > (double)UI.screenHeight)
                rect.y -= rect.height + 52f;
            return rect;
        }

        public static Color WealthColor(float wealth, float scale)
        {
            float value = wealth / scale ;
            value = Mathf.Clamp01(value);
            return Color.Lerp(Color.Lerp(ColorPoor, ColorRich, value), Color.white, 0.1f);
        }


        private static void FillWindow(Rect windowRect)
        {
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TileInspector, KnowledgeAmount.FrameDisplayed);
            Text.Font = GameFont.Small;
            float curY = 12f;
            int dividingLinesSeen = 0;
            if (true)
            {
                DrawDividingLineIfNecessary();
                Rect rect = new Rect(22f, curY, (float)((double)windowRect.width - 24.0 - 10.0), 100f);
                GUI.color = WealthColor(currentAreaWealth, 80000f);
                TaggedString label = "WealthHere".Translate() + ": " + currentAreaWealth.ToString("F1");
                Widgets.Label(rect, label);
                curY += 25f;
            }
            GUI.color = Color.white;

            void DrawDividingLineIfNecessary()
            {
                dividingLinesSeen++;
                if (dividingLinesSeen <= 1)
                    return;
                curY += 5f;
                GUI.color = new Color(1f, 1f, 1f, 0.4f);
                Widgets.DrawLineHorizontal(12f, curY, windowRect.width - 24f);
                GUI.color = Color.white;
                curY += 8f;
            }
        }

        public static void DrawWealthAroundMouse()
        {
            if (!WealthHereMod.showWealth) return;
            UpdateThingList(Find.CurrentMap);
            BeautyUtility.FillBeautyRelevantCells(UI.MouseCell(), Find.CurrentMap);
            currentAreaWealth = 0;
            for (int index = 0; index < BeautyUtility.beautyRelevantCells.Count; ++index)
            {
                IntVec3 beautyRelevantCell = BeautyUtility.beautyRelevantCells[index];
                float wealth = CellWealth(beautyRelevantCell, Find.CurrentMap, wealthCountedThings);
                currentAreaWealth += wealth;
                if ((double)wealth != 0.0)
                    GenMapUI.DrawThingLabel((Vector2)(Vector3)GenMapUI.LabelDrawPosFor(beautyRelevantCell), $"${Mathf.RoundToInt(wealth).ToStringCached()}" , WealthColor(wealth, 2000f));
            }
            wealthCountedThings.Clear();
        }



        private static void UpdateThingList(Map map)
        {
            buildingsArtificial.Clear();
            haulableEverThings.Clear();
            playerFactionPawns.Clear();
            buildingsArtificial.AddRange(  map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            haulableEverThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver));
            playerFactionPawns.AddRange(map.mapPawns.PawnsInFaction(Faction.OfPlayer));


        }

        //public static bool BeautyRelevant(ThingCategory cat)
        //{
        //    if (cat != ThingCategory.Building && cat != ThingCategory.Item && cat != ThingCategory.Plant)
        //    {
        //        return cat == ThingCategory.Filth;
        //    }

        //    return true;
        //}



        public static float GetThingWealth(Map map, Thing thing)
        {
            float wealthItems=0;
            float wealthBuildings = 0;
            float wealthPawns = 0;

            if (haulableEverThings.Contains(thing) && thing.SpawnedOrAnyParentSpawned && !thing.PositionHeld.Fogged(map))
            {
                wealthItems += thing.MarketValue * thing.stackCount;
            }
            if (buildingsArtificial.Contains(thing) && thing.Faction == Faction.OfPlayer)
            {
                wealthBuildings += thing.GetStatValue(StatDefOf.MarketValueIgnoreHp);
            }
            Pawn p = thing as Pawn;
            if (p!=null && playerFactionPawns.Contains(p))
            {
                if (!p.IsQuestLodger())
                {
                    wealthPawns += p.MarketValue;
                }
            }
            return wealthItems + wealthBuildings + wealthPawns;
        }

        public static float CellWealth(IntVec3 c, Map map, List<Thing> countedThings = null)
        {
            float thingWealth = 0;

            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                
                if (countedThings != null)
                {
                    if (!countedThings.Contains(thing))
                        countedThings.Add(thing);
                    else
                        continue;
                }
                thingWealth += GetThingWealth(map, thing);
            }


            //int index = map.cellIndices.CellToIndex(c);
            //if (index > 0 && index < map.cellIndices.NumGridCells)
            //{
            //    TerrainDef top = map.terrainGrid.topGrid[index];
            //    bool fog = map.fogGrid.fogGrid[index];
            //    if (!fog)
            //    {
            //        thingWealth += top.GetStatValueAbstract(StatDefOf.MarketValue);
            //    }
            //}


            return thingWealth + map.terrainGrid.TerrainAt(c).GetStatValueAbstract(StatDefOf.MarketValue);
        }




    }



}

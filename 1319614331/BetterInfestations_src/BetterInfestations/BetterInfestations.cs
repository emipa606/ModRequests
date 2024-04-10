using System;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using System.Collections.Generic;
using UnityEngine;

namespace BetterInfestations
{
    public class BetterInfestationsSettings : ModSettings
    {
        public bool infestationsAllowed = true;
        public bool raidsAllowed = true;
        public bool initialHunterSpawnsAllowed = false;
        public bool swarmingHordeEventAllowed = true;
        public bool queensAllowed = true;
        public bool newbornInsects = false;
        public bool notificationInf = false;
        public bool produceLeather = true;
        public int hiveLevel = 1;
        public int maxHivesPerMap = 30;
        public float initialPawnsPoints = 350f;
        public float jellyMultiplier = 0.25f;
        public List<float> minSpawnInDays = new List<float> { 0.75f, 0.85f, 0.85f, 0.85f };
        public List<float> maxSpawnInDays = new List<float> { 1.25f, 1.4f, 1.4f, 1.4f };
        public float queenHiveMinSpawnInDays = 1.6f;
        public float queenHiveMaxSpawnInDays = 2.8f;
        public float hiveReproductionMinSpawnInDays = 3.8f;
        public float hiveReproductionMaxSpawnInDays = 5.2f;

        public override void ExposeData()
        {
            base.ExposeData();

            float defenderMinSpawnInDays = minSpawnInDays[0];
            float defenderMaxSpawnInDays = maxSpawnInDays[0];
            float hunter1MinSpawnInDays = minSpawnInDays[1];
            float hunter1MaxSpawnInDays = maxSpawnInDays[1];
            float hunter2MinSpawnInDays = minSpawnInDays[2];
            float hunter2MaxSpawnInDays = maxSpawnInDays[2];
            float hunter3MinSpawnInDays = minSpawnInDays[3];
            float hunter3MaxSpawnInDays = maxSpawnInDays[3];

            Scribe_Values.Look(ref infestationsAllowed, "infestationsAllowed", true);
            Scribe_Values.Look(ref raidsAllowed, "raidsAllowed", true);
            Scribe_Values.Look(ref initialHunterSpawnsAllowed, "initialHunterSpawnsAllowed", false);
            Scribe_Values.Look(ref swarmingHordeEventAllowed, "swarmingHordeEventAllowed", true);
            Scribe_Values.Look(ref queensAllowed, "queensAllowed", true);
            Scribe_Values.Look(ref newbornInsects, "newbornInsects", false);
            Scribe_Values.Look(ref notificationInf, "notificationInf", false);
            Scribe_Values.Look(ref produceLeather, "produceLeather", true);
            Scribe_Values.Look(ref hiveLevel, "hiveLevel", 1);
            Scribe_Values.Look(ref maxHivesPerMap, "maxHivesPerMap", 30);
            Scribe_Values.Look(ref initialPawnsPoints, "initialPawnsPoints", 350f);
            Scribe_Values.Look(ref jellyMultiplier, "jellyMultiplier", 0.25f);
            Scribe_Values.Look(ref defenderMinSpawnInDays, "defenderMinSpawnInDays", 0.75f);
            Scribe_Values.Look(ref defenderMaxSpawnInDays, "defenderMaxSpawnInDays", 1.25f);
            Scribe_Values.Look(ref hunter1MinSpawnInDays, "hunter1MinSpawnInDays", 0.85f);
            Scribe_Values.Look(ref hunter1MaxSpawnInDays, "hunter1MaxSpawnInDays", 1.4f);
            Scribe_Values.Look(ref hunter2MinSpawnInDays, "hunter2MinSpawnInDays", 0.85f);
            Scribe_Values.Look(ref hunter2MaxSpawnInDays, "hunter2MaxSpawnInDays", 1.4f);
            Scribe_Values.Look(ref hunter3MinSpawnInDays, "hunter3MinSpawnInDays", 0.85f);
            Scribe_Values.Look(ref hunter3MaxSpawnInDays, "hunter3MaxSpawnInDays", 1.4f);
            Scribe_Values.Look(ref queenHiveMinSpawnInDays, "queenHiveMinSpawnInDays", 1.6f);
            Scribe_Values.Look(ref queenHiveMaxSpawnInDays, "queenHiveMaxSpawnInDays", 2.8f);
            Scribe_Values.Look(ref hiveReproductionMinSpawnInDays, "hiveReproductionMinSpawnInDays", 3.8f);
            Scribe_Values.Look(ref hiveReproductionMaxSpawnInDays, "hiveReproductionMaxSpawnInDays", 5.2f);

            minSpawnInDays[0] = defenderMinSpawnInDays;
            maxSpawnInDays[0] = defenderMaxSpawnInDays;
            minSpawnInDays[1] = hunter1MinSpawnInDays;
            maxSpawnInDays[1] = hunter1MaxSpawnInDays;
            minSpawnInDays[2] = hunter2MinSpawnInDays;
            maxSpawnInDays[2] = hunter2MaxSpawnInDays;
            minSpawnInDays[3] = hunter3MinSpawnInDays;
            maxSpawnInDays[3] = hunter3MaxSpawnInDays;
        }
    }

    [StaticConstructorOnStartup]
    public class BetterInfestationsMod : Mod
    {
        public static BetterInfestationsSettings settings;
        public static Vector2 scrollPosition = Vector2.zero;

        public BetterInfestationsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<BetterInfestationsSettings>();
        }
        static void EasyDefaults()
        {
            settings.initialHunterSpawnsAllowed = false;
            settings.queensAllowed = false;
            settings.newbornInsects = true;
            settings.notificationInf = true;
            settings.maxHivesPerMap = 15;
            settings.initialPawnsPoints = 200f;
            settings.minSpawnInDays[0] = 1.5f;
            settings.maxSpawnInDays[0] = 2.25f;
            settings.minSpawnInDays[1] = 1.8f;
            settings.maxSpawnInDays[1] = 2.5f;
            settings.minSpawnInDays[2] = 1.8f;
            settings.maxSpawnInDays[2] = 2.5f;
            settings.minSpawnInDays[3] = 1.8f;
            settings.maxSpawnInDays[3] = 2.5f;
            settings.queenHiveMinSpawnInDays = 2.8f;
            settings.queenHiveMaxSpawnInDays = 4.2f;
            settings.hiveReproductionMinSpawnInDays = 5.2f;
            settings.hiveReproductionMaxSpawnInDays = 7.4f;
            settings.Write();
        }
        static void NormalDefaults()
        {
            settings.initialHunterSpawnsAllowed = false;
            settings.queensAllowed = true;
            settings.newbornInsects = false;
            settings.notificationInf = false;
            settings.maxHivesPerMap = 20;
            settings.initialPawnsPoints = 350f;
            settings.minSpawnInDays[0] = 0.75f;
            settings.maxSpawnInDays[0] = 1.25f;
            settings.minSpawnInDays[1] = 0.85f;
            settings.maxSpawnInDays[1] = 1.4f;
            settings.minSpawnInDays[2] = 0.85f;
            settings.maxSpawnInDays[2] = 1.4f;
            settings.minSpawnInDays[3] = 0.85f;
            settings.maxSpawnInDays[3] = 1.4f;
            settings.queenHiveMinSpawnInDays = 1.6f;
            settings.queenHiveMaxSpawnInDays = 2.8f;
            settings.hiveReproductionMinSpawnInDays = 3.8f;
            settings.hiveReproductionMaxSpawnInDays = 5.2f;
            settings.Write();
        }
        static void HardDefaults()
        {
            settings.initialHunterSpawnsAllowed = false;
            settings.queensAllowed = true;
            settings.newbornInsects = false;
            settings.notificationInf = false;
            settings.maxHivesPerMap = 25;
            settings.initialPawnsPoints = 500f;
            settings.minSpawnInDays[0] = 0.5f;
            settings.maxSpawnInDays[0] = 0.9f;
            settings.minSpawnInDays[1] = 0.65f;
            settings.maxSpawnInDays[1] = 1.15f;
            settings.minSpawnInDays[2] = 0.65f;
            settings.maxSpawnInDays[2] = 1.15f;
            settings.minSpawnInDays[3] = 0.65f;
            settings.maxSpawnInDays[3] = 1.15f;
            settings.queenHiveMinSpawnInDays = 1.2f;
            settings.queenHiveMaxSpawnInDays = 2.2f;
            settings.hiveReproductionMinSpawnInDays = 3f;
            settings.hiveReproductionMaxSpawnInDays = 4.4f;
            settings.Write();
        }
        static void NightmareDefaults()
        {
            settings.initialHunterSpawnsAllowed = true;
            settings.queensAllowed = true;
            settings.newbornInsects = false;
            settings.notificationInf = false;
            settings.maxHivesPerMap = 30;
            settings.initialPawnsPoints = 650f;
            settings.minSpawnInDays[0] = 0.35f;
            settings.maxSpawnInDays[0] = 0.75f;
            settings.minSpawnInDays[1] = 0.45f;
            settings.maxSpawnInDays[1] = 0.95f;
            settings.minSpawnInDays[2] = 0.45f;
            settings.maxSpawnInDays[2] = 0.95f;
            settings.minSpawnInDays[3] = 0.45f;
            settings.maxSpawnInDays[3] = 0.95f;
            settings.queenHiveMinSpawnInDays = 0.8f;
            settings.queenHiveMaxSpawnInDays = 1.5f;
            settings.hiveReproductionMinSpawnInDays = 2.2f;
            settings.hiveReproductionMaxSpawnInDays = 3.5f;
            settings.Write();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, inRect.height + 192f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

            float width = (outRect.width / 2) - 32f;
            float height = outRect.height - 560f;

            Color darkerWhite = new Color(0.75f, 0.75f, 0.75f, 1f);

            //Left side
            float x = outRect.position.x;
            float y = outRect.position.y - 24f;
            Color backgroundColor = new Color(0.07f, 0.07f, 0.07f, 1f);
            Rect drawTexRect1 = new Rect(0f, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect1, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect1 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect1, "General");

            y += 32f;
            Rect checkboxRect4 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_4 = new Vector2(checkboxRect4.x, checkboxRect4.y);
            Widgets.Checkbox(checkboxVec2_4, ref settings.infestationsAllowed);
            GUI.color = darkerWhite;
            Rect labelRect12 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect12, "Allow infestations");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect12, "BI_InfestationsTip");

            y += 32f;
            Rect checkboxRect5 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_5 = new Vector2(checkboxRect5.x, checkboxRect5.y);
            Widgets.Checkbox(checkboxVec2_5, ref settings.raidsAllowed);
            GUI.color = darkerWhite;
            Rect labelRect13 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect13, "Allow raiding insects");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect13, "BI_RaidingInsectsTip");

            y += 32f;
            Rect checkboxRect7 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_7 = new Vector2(checkboxRect7.x, checkboxRect7.y);
            Widgets.Checkbox(checkboxVec2_7, ref settings.swarmingHordeEventAllowed);
            GUI.color = darkerWhite;
            Rect labelRect16 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect16, "Allow swarming horde event");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect16, "BI_SwarmingHordeTip");

            y += 32f;
            Rect checkboxRect8 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_8 = new Vector2(checkboxRect8.x, checkboxRect8.y);
            Widgets.Checkbox(checkboxVec2_8, ref settings.produceLeather);
            GUI.color = darkerWhite;
            Rect labelRect17 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect17, "Butchered corpses produce leather");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect17, "BI_ProduceLeatherTip");

            y += 32f;
            Rect checkboxRect2 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_2 = new Vector2(checkboxRect2.x, checkboxRect2.y);
            Widgets.Checkbox(checkboxVec2_2, ref settings.notificationInf);
            GUI.color = darkerWhite;
            Rect labelRect9 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect9, "Notification of Infestation");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect9, "BI_NotificationTip");

            y += 48f;
            Rect sliderRect15 = new Rect(x + 8f, y, width - 4f, height);
            settings.jellyMultiplier = HorizontalSliderFraction(sliderRect15, settings.jellyMultiplier, 0.05f, 1f, false, null, "Jelly conversion multiplier", (settings.jellyMultiplier * 100).ToString() + " %", 0.05f, 2);

            y += 48f;
            Rect drawTexRect6 = new Rect(0f, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect6, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect7 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect7, "Queen: Interval to spawn additional hives");

            y += 48f;
            Rect sliderRect9 = new Rect(x + 8f, y, width - 4f, height);
            settings.queenHiveMinSpawnInDays = HorizontalSliderFraction(sliderRect9, settings.queenHiveMinSpawnInDays, 0.05f, 5f, false, null, "Minimum interval", settings.queenHiveMinSpawnInDays.ToString() + " Days", 0.05f, 2);
            if (settings.queenHiveMinSpawnInDays > settings.queenHiveMaxSpawnInDays)
            {
                settings.queenHiveMaxSpawnInDays = settings.queenHiveMinSpawnInDays;
            }

            y += 32f;
            Rect sliderRect10 = new Rect(x + 8f, y, width - 4f, height);
            settings.queenHiveMaxSpawnInDays = HorizontalSliderFraction(sliderRect10, settings.queenHiveMaxSpawnInDays, 0.05f, 5f, false, null, "Maximum interval", settings.queenHiveMaxSpawnInDays.ToString() + " Days", 0.05f, 2);
            if (settings.queenHiveMaxSpawnInDays < settings.queenHiveMinSpawnInDays)
            {
                settings.queenHiveMinSpawnInDays = settings.queenHiveMaxSpawnInDays;
            }

            y += 48f;
            Rect drawTexRect8 = new Rect(0f, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect8, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect10 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect10, "Hive: Interval to reproduce additional hives");

            y += 48f;
            Rect sliderRect11 = new Rect(x + 8f, y, width - 4f, height);
            settings.hiveReproductionMinSpawnInDays = HorizontalSliderFraction(sliderRect11, settings.hiveReproductionMinSpawnInDays, 0.05f, 10f, false, null, "Minimum interval", settings.hiveReproductionMinSpawnInDays.ToString() + " Days", 0.05f, 2);
            if (settings.hiveReproductionMinSpawnInDays > settings.hiveReproductionMaxSpawnInDays)
            {
                settings.hiveReproductionMaxSpawnInDays = settings.hiveReproductionMinSpawnInDays;
            }

            y += 32f;
            Rect sliderRect12 = new Rect(x + 8f, y, width - 4f, height);
            settings.hiveReproductionMaxSpawnInDays = HorizontalSliderFraction(sliderRect12, settings.hiveReproductionMaxSpawnInDays, 0.05f, 10f, false, null, "Maximum interval", settings.hiveReproductionMaxSpawnInDays.ToString() + " Days", 0.05f, 2);
            if (settings.hiveReproductionMaxSpawnInDays < settings.hiveReproductionMinSpawnInDays)
            {
                settings.hiveReproductionMinSpawnInDays = settings.hiveReproductionMaxSpawnInDays;
            }

            y += 48f;
            Rect drawTexRect9 = new Rect(0f, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect9, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect15 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect15, "Hive settings");

            y += 32f;
            Rect checkboxRect6 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_6 = new Vector2(checkboxRect6.x, checkboxRect6.y);
            Widgets.Checkbox(checkboxVec2_6, ref settings.initialHunterSpawnsAllowed);
            GUI.color = darkerWhite;
            Rect labelRect14 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect14, "Spawn initial hunters");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect14, "BI_SpawnInitialHuntersTip");

            y += 32f;
            Rect checkboxRect1 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_1 = new Vector2(checkboxRect1.x, checkboxRect1.y);
            Widgets.Checkbox(checkboxVec2_1, ref settings.queensAllowed);
            GUI.color = darkerWhite;
            Rect labelRect2 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect2, "Spawn queens");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect2, "BI_SpawnQueensTip");

            y += 32f;
            Rect checkboxRect3 = new Rect(x + 8f, y, width, height);
            Vector2 checkboxVec2_3 = new Vector2(checkboxRect3.x, checkboxRect3.y);
            Widgets.Checkbox(checkboxVec2_3, ref settings.newbornInsects);
            GUI.color = darkerWhite;
            Rect labelRect11 = new Rect(x + 40f, y, width, height);
            Widgets.Label(labelRect11, "Spawn newborns");
            GUI.color = Color.white;
            TooltipHandler.TipRegionByKey(labelRect11, "BI_SpawnNewbornsTip");

            y += 48f;
            Rect sliderRect13 = new Rect(x + 8f, y, width, height);
            settings.maxHivesPerMap = (int)Widgets.HorizontalSlider(sliderRect13, settings.maxHivesPerMap, 1f, 50f, false, null, "Maximum hives per map", settings.maxHivesPerMap.ToString(), 1f);

            y += 32f;
            Rect sliderRect16 = new Rect(x + 8f, y, width - 4f, height);
            settings.hiveLevel = (int)Widgets.HorizontalSlider(sliderRect16, settings.hiveLevel, 1f, 3f, false, null, "Initial number of hunting parties", settings.hiveLevel.ToString(), 1f);

            y += 32f;
            Rect sliderRect14 = new Rect(x + 8f, y, width - 4f, height);
            settings.initialPawnsPoints = (int)Widgets.HorizontalSlider(sliderRect14, settings.initialPawnsPoints, 150f, 1000f, false, null, "Initial spawn points", settings.initialPawnsPoints.ToString(), 50f);

            //Right side
            x = outRect.position.x + 432f;
            y = outRect.position.y - 24f;
            Rect drawTexRect2 = new Rect(x, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect2, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect3 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect3, "Hive: Defense group spawn intervals");

            y += 48f;
            Rect sliderRect1 = new Rect(x, y, width - 4f, height);
            settings.minSpawnInDays[0] = HorizontalSliderFraction(sliderRect1, settings.minSpawnInDays[0], 0.05f, 3f, false, null, "Minimum interval", settings.minSpawnInDays[0].ToString() + " Days", 0.05f, 2);
            if (settings.minSpawnInDays[0] > settings.maxSpawnInDays[0])
            {
                settings.maxSpawnInDays[0] = settings.minSpawnInDays[0];
            }

            y += 32f;
            Rect sliderRect2 = new Rect(x, y, width - 4f, height);
            settings.maxSpawnInDays[0] = HorizontalSliderFraction(sliderRect2, settings.maxSpawnInDays[0], 0.05f, 3f, false, null, "Maximum interval", settings.maxSpawnInDays[0].ToString() + " Days", 0.05f, 2);
            if (settings.maxSpawnInDays[0] < settings.minSpawnInDays[0])
            {
                settings.minSpawnInDays[0] = settings.maxSpawnInDays[0];
            }

            y += 48f;
            Rect drawTexRect3 = new Rect(x, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect3, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect4 = new Rect(x + 8f, outRect.position.y + 104f, width, height);
            Widgets.Label(labelRect4, "Hive: Hunter group 1 spawn intervals");

            y += 48f;
            Rect sliderRect3 = new Rect(x, y, width - 4f, height);
            settings.minSpawnInDays[1] = HorizontalSliderFraction(sliderRect3, settings.minSpawnInDays[1], 0.05f, 3f, false, null, "Minimum interval", settings.minSpawnInDays[1].ToString() + " Days", 0.05f, 2);
            if (settings.minSpawnInDays[1] > settings.maxSpawnInDays[1])
            {
                settings.maxSpawnInDays[1] = settings.minSpawnInDays[1];
            }

            y += 32f;
            Rect sliderRect4 = new Rect(x, y, width - 4f, height);
            settings.maxSpawnInDays[1] = HorizontalSliderFraction(sliderRect4, settings.maxSpawnInDays[1], 0.05f, 3f, false, null, "Maximum interval", settings.maxSpawnInDays[1].ToString() + " Days", 0.05f, 2);
            if (settings.maxSpawnInDays[1] < settings.minSpawnInDays[1])
            {
                settings.minSpawnInDays[1] = settings.maxSpawnInDays[1];
            }

            y += 48f;
            Rect drawTexRect4 = new Rect(x, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect4, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect5 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect5, "Hive: Hunter group 2 spawn intervals");

            y += 48f;
            Rect sliderRect5 = new Rect(x, y, width - 4f, height);
            settings.minSpawnInDays[2] = HorizontalSliderFraction(sliderRect5, settings.minSpawnInDays[2], 0.05f, 3f, false, null, "Minimum interval", settings.minSpawnInDays[2].ToString() + " Days", 0.05f, 2);
            if (settings.minSpawnInDays[2] > settings.maxSpawnInDays[2])
            {
                settings.maxSpawnInDays[2] = settings.minSpawnInDays[2];
            }

            y += 32f;
            Rect sliderRect6 = new Rect(x, y, width - 4f, height);
            settings.maxSpawnInDays[2] = HorizontalSliderFraction(sliderRect6, settings.maxSpawnInDays[2], 0.05f, 3f, false, null, "Maximum interval", settings.maxSpawnInDays[2].ToString() + " Days", 0.05f, 2);
            if (settings.maxSpawnInDays[2] < settings.minSpawnInDays[2])
            {
                settings.minSpawnInDays[2] = settings.maxSpawnInDays[2];
            }

            y += 48f;
            Rect drawTexRect5 = new Rect(x, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect5, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect6 = new Rect(x + 8f, outRect.position.y + 360f, width, height);
            Widgets.Label(labelRect6, "Hive: Hunter group 3 spawn intervals");

            y += 48f;
            Rect sliderRect7 = new Rect(x, y, width - 4f, height);
            settings.minSpawnInDays[3] = HorizontalSliderFraction(sliderRect7, settings.minSpawnInDays[3], 0.05f, 3f, false, null, "Minimum interval", settings.minSpawnInDays[3].ToString() + " Days", 0.05f, 2);
            if (settings.minSpawnInDays[3] > settings.maxSpawnInDays[3])
            {
                settings.maxSpawnInDays[3] = settings.minSpawnInDays[3];
            }

            y += 32f;
            Rect sliderRect8 = new Rect(x, y, width - 4f, height);
            settings.maxSpawnInDays[3] = HorizontalSliderFraction(sliderRect8, settings.maxSpawnInDays[3], 0.05f, 3f, false, null, "Maximum interval", settings.maxSpawnInDays[3].ToString() + " Days", 0.05f, 2);
            if (settings.maxSpawnInDays[3] < settings.minSpawnInDays[3])
            {
                settings.minSpawnInDays[3] = settings.maxSpawnInDays[3];
            }

            y += 48f;
            Rect drawTexRect7 = new Rect(x, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect7, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect8 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect8, "Difficulty settings");

            y += 32f;
            Rect buttonTextRect1 = new Rect(x, y, width - 208f, height);
            bool bEasy = Widgets.ButtonText(buttonTextRect1, "Easy", true);
            if (bEasy)
            {
                EasyDefaults();
            }

            y += 32f;
            Rect buttonTextRect2 = new Rect(x, y, width - 208f, height);
            bool bNormal = Widgets.ButtonText(buttonTextRect2, "Normal", true);
            if (bNormal)
            {
                NormalDefaults();
            }

            y += 32f;
            Rect buttonTextRect3 = new Rect(x, y, width - 208f, height);
            bool bHard = Widgets.ButtonText(buttonTextRect3, "Hard", true);
            if (bHard)
            {
                HardDefaults();
            }

            y += 32f;
            Rect buttonTextRect4 = new Rect(x, y, width - 208f, height);
            bool bNightmare = Widgets.ButtonText(buttonTextRect4, "Nightmare", true);
            if (bNightmare)
            {
                NightmareDefaults();
            }

            y += 48f;
            Rect drawTexRect10 = new Rect(x, y, width + 8f, height);
            GUI.DrawTexture(drawTexRect10, SolidColorMaterials.NewSolidColorTexture(backgroundColor));
            Rect labelRect18 = new Rect(x + 8f, y, width, height);
            Widgets.Label(labelRect18, "Warning: Uninstall options below !!!");

            y += 32f;
            Rect buttonTextRect5 = new Rect(x, y, width - 208f, height);
            bool bUninstall = Widgets.ButtonText(buttonTextRect5, "Uninstall", true);
            TooltipHandler.TipRegionByKey(buttonTextRect5, "BI_UninstallTip");
            if (bUninstall)
            {
                UninstallMod();
            }

            Widgets.EndScrollView();
            settings.Write();
        }
        public static void UninstallMod()
        {
            if (Current.Game != null)
            {
                for (int i = 0; i < Find.Maps.Count; i++)
                {
                    List<Thing> corpses = Find.Maps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                    List<Pawn> pawns = Find.Maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfInsects);
                    List<Thing> spawners = Find.Maps[i].listerThings.ThingsOfDef(ThingDefOf.BI_TunnelHiveSpawner);
                    List<Thing> hives = Find.Maps[i].listerThings.ThingsOfDef(ThingDefOf.BI_Hive);
                    List<Site> sites = Find.WorldObjects.Sites;
                    if (!corpses.NullOrEmpty())
                    {
                        for (int c = corpses.Count; c-- > 0;)
                        {
                            Corpse corpse = corpses[c] as Corpse;
                            if (corpse != null && corpse.InnerPawn != null && corpse.InnerPawn.def == ThingDefOf.BI_Queen)
                            {
                                corpses[c].Destroy(DestroyMode.Vanish);
                            }
                        }
                    }
                    if (!pawns.NullOrEmpty())
                    {
                        for (int p = pawns.Count; p-- > 0;)
                        {
                            pawns[p].Destroy(DestroyMode.Vanish);
                        }
                    }
                    if (!spawners.NullOrEmpty())
                    {
                        for (int s = spawners.Count; s-- > 0;)
                        {
                            spawners[s].Destroy(DestroyMode.Vanish);
                        }
                    }
                    if (!hives.NullOrEmpty())
                    {
                        for (int h = hives.Count; h-- > 0;)
                        {
                            Hive hive = hives[h] as Hive;
                            if (hive.CompSpawnerPawns != null)
                            {
                                if (hive.CompSpawnerPawns.Lord[0] != null)
                                {
                                    Find.Maps[i].lordManager.RemoveLord(hive.CompSpawnerPawns.Lord[0]);
                                }
                                if (hive.CompSpawnerPawns.Lord[1] != null)
                                {
                                    Find.Maps[i].lordManager.RemoveLord(hive.CompSpawnerPawns.Lord[1]);
                                }
                                if (hive.CompSpawnerPawns.Lord[2] != null)
                                {
                                    Find.Maps[i].lordManager.RemoveLord(hive.CompSpawnerPawns.Lord[2]);
                                }
                                if (hive.CompSpawnerPawns.Lord[3] != null)
                                {
                                    Find.Maps[i].lordManager.RemoveLord(hive.CompSpawnerPawns.Lord[3]);
                                }
                            }
                            hives[h].Destroy(DestroyMode.Vanish);
                        }
                    }
                    if (!sites.NullOrEmpty())
                    {
                        for (int t = sites.Count; t-- > 0;)
                        {
                            if (sites[t].def.defName == "BI_InfestationWorldObject")
                            {
                                Find.WorldObjects.Remove(sites[t]);
                            }
                        }
                    }
                }
                Messages.Message("Better Infestations uninstalled successfully!", MessageTypeDefOf.PositiveEvent);
            }
        }
        public static float HorizontalSliderFraction(Rect rect, float value, float leftValue, float rightValue, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f, int fractionalDigit = 1)
        {
            if (middleAlignment || !label.NullOrEmpty())
            {
                rect.y += Mathf.Round((rect.height - 16f) / 2f);
            }
            if (!label.NullOrEmpty())
            {
                rect.y += 5f;
            }
            float num = GUI.HorizontalSlider(rect, value, leftValue, rightValue);
            if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
            {
                TextAnchor anchor = Text.Anchor;
                GameFont font = Text.Font;
                Text.Font = GameFont.Tiny;
                float num2 = label.NullOrEmpty() ? 18f : Text.CalcSize(label).y;
                rect.y = rect.y - num2 + 3f;
                if (!leftAlignedLabel.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperLeft;
                    Widgets.Label(rect, leftAlignedLabel);
                }
                if (!rightAlignedLabel.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    Widgets.Label(rect, rightAlignedLabel);
                }
                if (!label.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(rect, label);
                }
                Text.Anchor = anchor;
                Text.Font = font;
            }
            if (roundTo > 0)
            {
                num = (float)Math.Round(Mathf.RoundToInt(num / roundTo) * roundTo, fractionalDigit);
            }
            if (value != num)
            {
                SoundDefOf.DragSlider.PlayOneShot(SoundInfo.OnCamera());
            }
            return num;
        }

        public override string SettingsCategory() => "Better Infestations";
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DrugScheduleWhileWorking;

//Expose object needed to store key value pair properly.
public class DrugEnabled : IExposable
{
    public bool drugEnabledBool = true;

    public void ExposeData()
    {
        Scribe_Values.Look(ref drugEnabledBool, "drugEnabledBool");
    }
}

//Settings data container
public class DrugScheduleWhileWorkingSettings : ModSettings
{
    public static Dictionary<string, DrugEnabled> drugsEnabledDuringWorkSettingsDictionary = new();

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref drugsEnabledDuringWorkSettingsDictionary, "drugsEnabledDuringWorkSettingsDictionary", LookMode.Value, LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            if (drugsEnabledDuringWorkSettingsDictionary == null) new Dictionary<string, DrugEnabled>();
        }

        base.ExposeData();
    }

    public static DrugEnabled GetOrCreateEnabledSetting(string defName)
    {
        DrugEnabled settingsContainsKey = drugsEnabledDuringWorkSettingsDictionary.TryGetValue(defName);

        if (settingsContainsKey != null) return settingsContainsKey;

        Log.Message("new settings made for drug: " + defName);
        settingsContainsKey = new DrugEnabled();
        drugsEnabledDuringWorkSettingsDictionary[defName] = settingsContainsKey;
        return settingsContainsKey;
    }
}

//Settings window draw class
public static class SettingsDrawUtil
{
    private static Vector2 _scrollPosition = Vector2.zero;

    public static void DoWindowContents(Rect inRect)
    {
        var settingsList = new Listing_Standard { ColumnWidth = inRect.width - 20f };
        settingsList.Begin(inRect);

        DrawSettingsDynamic(settingsList, inRect);

        settingsList.End();
    }

    private static void DrawSettingsDynamic(Listing_Standard listingStandard, Rect inRect)
    {
        Text.Font = GameFont.Small;

        var rowCount = DrugScheduleUtils.AllDrugs.Count();
        var scrollRect = new Rect(0f, 0f, inRect.width - 16f, CalculateScrollHeight(listingStandard, rowCount));

        Widgets.BeginScrollView(inRect, ref _scrollPosition, scrollRect);
        listingStandard.ColumnWidth -= 16f;

        listingStandard.Begin(scrollRect);

        foreach (var drug in DrugScheduleUtils.AllDrugs)
        {
            var storytellerEnabledSetting = DrugScheduleWhileWorkingSettings.GetOrCreateEnabledSetting(drug.defName);
            listingStandard.CheckboxLabeled(drug.label, ref storytellerEnabledSetting.drugEnabledBool, drug.description);

            listingStandard.Gap(5f);
        }

        listingStandard.End();
        Widgets.EndScrollView();
    }

    private static float CalculateScrollHeight(Listing_Standard listingStandard, int rowCount)
    {
        return (Text.LineHeight + listingStandard.verticalSpacing + 5f) * rowCount;
    }
}
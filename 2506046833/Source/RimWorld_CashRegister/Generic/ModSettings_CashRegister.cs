using UnityEngine;
using Verse;

namespace CashRegister;

public class ModSettings_CashRegister : ModSettings
{
    public static bool inactiveIfEveryoneIsSleeping = true;

    private Vector2 scrollPosition;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref inactiveIfEveryoneIsSleeping, "inactiveIfEveryoneIsSleeping", true);
        base.ExposeData();
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var rect = new Rect(inRect.x, inRect.y, inRect.width - 20f, inRect.height);
        var contentHeight = 150f;
        Widgets.BeginScrollView(inRect, ref scrollPosition, new Rect(0f, 0f, rect.width, contentHeight));
        var options = new Listing_CashRegister();
        options.Begin(rect);
        options.GapLine();
        Text.Font = GameFont.Medium;
        Text.Font = GameFont.Small;
        options.CustomCheckboxLabeled("InactiveIfEveryoneIsSleeping".Translate(), ref inactiveIfEveryoneIsSleeping, "InactiveIfEveryoneIsSleepingDesc".Translate());
        options.GapLine();
        options.End();
        Widgets.EndScrollView();
    }

    // Make sure that it still works when referenced settings are null!
    //private static SettingHandle.ValueIsValid WorkSkillLimits { get { return AtLeast(() => disableLimits?.Value != false ? 0 : 6); } }
    //private static SettingHandle.ValueIsValid MaxIncidentsPer3DaysLimitsMin { get { return AtLeast(() => 1); } }
    //private static SettingHandle.ValueIsValid GroupSizeLimitsMin { get { return Between(() => 1, () => maxGuestGroupSize?.Value ?? DefaultMaxGroupSize); } }
    //private static SettingHandle.ValueIsValid GroupSizeLimitsMax { get { return AtLeast(() => Mathf.Max(minGuestGroupSize?.Value ?? 0, disableLimits?.Value != false ? 1 : 8)); } }
    //
    //private static SettingHandle.ValueIsValid Between(Func<int> amountMin, Func<int> amountMax)
    //{
    //	return value => int.TryParse(value, out var actual) && actual >= amountMin() && actual <= amountMax();
    //}
    //
    //private static SettingHandle.ValueIsValid AtLeast(Func<int> amount)
    //{
    //	return value => int.TryParse(value, out var actual) && actual >= amount();
    //}
    //
    //private static SettingHandle.ValueIsValid AtMost(Func<int> amount)
    //{
    //	return value => int.TryParse(value, out var actual) && actual <= amount();
    //}
}
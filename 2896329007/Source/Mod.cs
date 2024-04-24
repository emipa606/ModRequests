using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rachek128.RitualAttenuation
{
  public class Mod : Verse.Mod
  {
    internal static Settings settings;

    public Mod(ModContentPack content) : base(content)
    {
      settings = GetSettings<Settings>();
      var harmony = new Harmony("Rachek128.RitualAttenuation");
      harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      var list = new Listing_Standard();
      list.Begin(inRect);

      settings.requiredPopulationPercentage = list.SliderLabeled("Rachek_RSA_PopulationPercentageLabel".Translate(settings.requiredPopulationPercentage.ToStringPercent()),
        settings.requiredPopulationPercentage, 0f, 1f, 0.5f, "Rachek_RSA_PopulationPercentageTooltip".Translate())
        .ClampToFixed(2);

      bool zeroPercent = settings.requiredPopulationPercentage < 0.009f;

      if (zeroPercent)
      {
        list.Gap();
        GUI.color = Color.Lerp(Color.red, Color.white, 0.25f);
        list.Label("Rachek_RSA_ZeroPercentWarningLabel".Translate());
        GUI.color = Color.white;
        list.Gap();
      }

      string buttonText = Tools.UncapModeNameKey(settings.EffectiveUncapMode).Translate();

      if (zeroPercent)
      {
        buttonText += $" ({"ApparelForcedLower".Translate()})";
      }

      if (list.ButtonTextLabeled("Rachek_RSA_UncappingModeLabel".Translate(), buttonText,
        TextAnchor.UpperLeft, null, "Rachek_RSA_UncappingModeTooltip".Translate(Tools.UncapModeDescriptionKey(settings.EffectiveUncapMode).Translate())))
      {
        ShowUncapModeMenu(settings);
      }

      if (settings.EffectiveUncapMode == UncapMode.Bonus)
      {
        settings.maximumBonusPercentage = list.SliderLabeled("Rachek_RSA_BonusLabel".Translate(settings.maximumBonusPercentage.ToStringPercent()),
          settings.maximumBonusPercentage, 0f, 1f, 0.5f, "Rachek_RSA_BonusTooltip".Translate())
          .ClampToFixed(2);

        DrawAdvancedSettings(list);
      }

      list.End();
      base.DoSettingsWindowContents(inRect);
    }

    private void DrawAdvancedSettings(Listing_Standard list)
    {
      list.Gap();

      list.CheckboxLabeled("Rachek_RSA_ShowAdvancedLabel".Translate(), ref settings.showAdvancedSettings);

      if (!settings.showAdvancedSettings)
        return;

      list.Gap();
      GUI.color = Color.Lerp(Color.red, Color.white, 0.25f);
      list.Label("Rachek_RSA_AdvancedWarningLabel".Translate());
      GUI.color = Color.white;
      list.Gap();
      settings.bonusExponent = list.SliderLabeled("Rachek_RSA_BonusExponentLabel".Translate(settings.bonusExponent.ToString("F1")),
        settings.bonusExponent, 0.3f, 5.0f, 0.5f, "Rachek_RSA_BonusExponentTooltip".Translate())
        .ClampToFixed(1);

      Text.Font = GameFont.Tiny;
      list.Label("Rachek_RSA_InterpolationFunction".Translate(settings.bonusExponent.ToString("F1")));
      Text.Font = GameFont.Medium;

      DrawCurvePreview(list);
    }

    private void DrawCurvePreview(Listing_Standard list)
    {
      Tools.RefreshInterpolationCurve();
      var rect = list.GetRect(128f);
      rect.width = 128f;
      Tools.interpolationCurve.View.SetViewRectAround(Tools.interpolationCurve);
      SimpleCurveDrawerStyle style = new SimpleCurveDrawerStyle();
      style.DrawPoints = false;
      style.UseAntiAliasedLines = true;
      SimpleCurveDrawer.DrawCurve(rect, Tools.interpolationCurve, style);
      var oldColor = GUI.color;
      GUI.color = Color.gray;
      Widgets.DrawBox(rect);
      GUI.color = oldColor;
    }

    public override string SettingsCategory()
    {
      return "Rachek_RSA".Translate();
    }

    private static void ShowUncapModeMenu(Settings settings)
    {
      var items = new List<FloatMenuOption>();

      bool zeroPercent = settings.requiredPopulationPercentage < 0.009f;

      var none = new FloatMenuOption(Tools.UncapModeDescriptionKey(UncapMode.None).Translate(), () =>
      {
        if (!zeroPercent)
          settings.uncapMode = UncapMode.None;
      });

      items.Add(none);

      var scaled = new FloatMenuOption(Tools.UncapModeDescriptionKey(UncapMode.Scaled).Translate(), () =>
      {
        if (!zeroPercent)
          settings.uncapMode = UncapMode.Scaled;
      });

      items.Add(scaled);

      var bonus = new FloatMenuOption(Tools.UncapModeDescriptionKey(UncapMode.Bonus).Translate(), () =>
      {
        if (!zeroPercent)
          settings.uncapMode = UncapMode.Bonus;
      });

      bonus.Disabled = none.Disabled = zeroPercent;
      if (zeroPercent)
      {
        bonus.tooltip = none.tooltip = "Rachek_RSA_ZeroPercentForcedModeTooltip".Translate();
      }

      items.Add(bonus);

      Find.WindowStack.Add(new FloatMenu(items));
    }
  }

}
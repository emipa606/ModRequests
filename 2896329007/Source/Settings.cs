using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Rachek128.RitualAttenuation
{
  public class Settings : Verse.ModSettings
  {
    public UncapMode uncapMode = UncapMode.None;
    public float requiredPopulationPercentage = 0.75f;
    public float bonusExponent = 2.0f;
    public float maximumBonusPercentage = 0.25f;
    public bool showAdvancedSettings = false;
    
    public UncapMode EffectiveUncapMode => requiredPopulationPercentage < 0.009 ? UncapMode.Scaled : uncapMode;

    public override void ExposeData()
    {
      Scribe_Values.Look(ref uncapMode, nameof(uncapMode));
      Scribe_Values.Look(ref requiredPopulationPercentage, nameof(requiredPopulationPercentage));
      Scribe_Values.Look(ref bonusExponent, nameof(bonusExponent));
      Scribe_Values.Look(ref maximumBonusPercentage, nameof(maximumBonusPercentage));
      Scribe_Values.Look(ref showAdvancedSettings, nameof(showAdvancedSettings));
      base.ExposeData();
    }
  }
}
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DeepScannerSettings
{
  [StaticConstructorOnStartup]
  public static class StartUp
  {
    static StartUp()
    {
      // Loads right before main menu
      SetOriginalValues();

      DSSMod.UpdateChanges();
    }

    public static void SetOriginalValues()
    {
      List<ThingDef> oreList = DefDatabase<ThingDef>.AllDefs.Where(o => o.deepCommonality > 0).ToList();

      if (oreList.Count() != DSSSettings.Commonality.Count() || oreList.Count() != DSSSettings.MinedAmountPerChunk.Count() || oreList.Count() != DSSSettings.VeinSizeRange.Count())
      {
        Log.Warning("WarningSettingsMismatchReset".Translate());
        DSSSettings.ClearSettings();
      }

      if (oreList.Count() < DSSSettings.DeepOreDefNames.Count())
      {
        Log.Warning("WarningLostOresReset".Translate());
        DSSSettings.ClearSettings();
      }

      if (oreList.Count() > 0) 
      { 
        for (int i = 0; i < oreList.Count(); ++i)
        {
          if (!DSSSettings.DeepOreDefNames.Contains(oreList[i].defName))
          {
            DSSSettings.DeepOreDefNames.Add(oreList[i].defName);
            DSSSettings.Commonality.Add(oreList[i].deepCommonality);
            DSSSettings.MinedAmountPerChunk.Add(oreList[i].deepCountPerPortion);
            DSSSettings.VeinSizeRange.Add(oreList[i].deepLumpSizeRange);
          }
        }
      }
    }
  }
}

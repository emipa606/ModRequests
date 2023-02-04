using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace CeilingLights
{
  public class CeilingLightsSettings : ModSettings
  {
    public static int GrowLightPowerConsumption => growLPC;
    public static bool ForceRoofRequired => forceRR;

    public static int growLPC;
    public static bool forceRR = true;
    public static string growLPCTooltip = ""; // Cant translate CLSettingsGrowLPCTooltip for some reason

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref growLPC, "growLightPowerConsumption");
      Scribe_Values.Look(ref forceRR, "forceRoofRequired");
    }
  }

  public class CeilingLightsMod : Mod
  {
    CeilingLightsSettings settings;
    public CeilingLightsMod(ModContentPack con) : base(con)
    {
      this.settings = GetSettings<CeilingLightsSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      Listing_Standard listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.TextFieldNumericLabeled<int>("CLSettingsGrowLPC".Translate(), ref CeilingLightsSettings.growLPC, ref CeilingLightsSettings.growLPCTooltip);
      listing.End();
      base.DoSettingsWindowContents(inRect);
      WriteSettings();
    }

    public override void WriteSettings()
    {
      // DefDatabase<ThingDef>.GetNamed("Lighting_CeilingLight").comps.First<CompProperties_PowerConsume>().basePowerConsumption => CeilingLightsSettings.GrowLightPowerConsumption;
    }

    public override string SettingsCategory()
    {
      // This is the title on the mod screen
      return "CLSettingsCategory".Translate();
    }
  }
}

using UnityEngine;
using Verse;

namespace GiveMeMeat
{
  public class GiveMeMeatSettings : ModSettings
  {
    public static int MeatAmountToMultiply => meatATM;

    // DON'T reference these following var(s) except from inside this class; Reference the above variables instead. ExposeData requires them to be public
    public static int meatATM = 2;
    // End warning

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref meatATM, "GMMAmountToMultiply");
    }
  }

  public class GiveMeMeatMod : Mod
  {
    GiveMeMeatSettings settings;
    public GiveMeMeatMod(ModContentPack con) : base(con)
    {
      this.settings = GetSettings<GiveMeMeatSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      Listing_Standard listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.Label("GMMATMLabel".Translate() + " " + (GiveMeMeatSettings.meatATM * 100).ToString() + "%");
      GiveMeMeatSettings.meatATM = (int)listing.Slider(GiveMeMeatSettings.meatATM, 0f, 10f);
      listing.Label("GMMAdviseRestart".Translate());
      listing.End();
      base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
      return "GMMATMTitle".Translate();
    }
  }
}

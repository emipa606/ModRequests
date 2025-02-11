using System;
using UnityEngine;
using Verse;

namespace LeafLikeLife
{
  public class LLLModSettings : ModSettings
  {
    public static float amountPenaltyConsciousness = -0.15f;
    public static float amountPenaltyMovement = -0.1f;
    public static float amountPenaltyRest = -0.1f;
    public static float amountHungerRate = 0.3f;

    public static float amountAddictiveness = 0.01f;
    public static float amountToleranceToAddict = 0.50f;

    public static int amountAsthmaDaysChance = 360;
    public static int amountCancerDaysChance = 720;

    public static int amountPenaltyWithdrawal = -8;


    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref amountPenaltyConsciousness, "LLLamountPenaltyConsciousness");
      Scribe_Values.Look(ref amountPenaltyMovement, "LLLamountPenaltyMovement");
      Scribe_Values.Look(ref amountPenaltyRest, "LLLamountPenaltyRest");
      Scribe_Values.Look(ref amountHungerRate, "LLLamountHungerRate");
      Scribe_Values.Look(ref amountAddictiveness, "LLLamountAddictiveness");
      Scribe_Values.Look(ref amountToleranceToAddict, "LLLamountToleranceToAddict");
      Scribe_Values.Look(ref amountAsthmaDaysChance, "LLLamountAsthmaDaysChance");
      Scribe_Values.Look(ref amountCancerDaysChance, "LLLamountCancerDaysChance");
      Scribe_Values.Look(ref amountPenaltyWithdrawal, "LLLamountPenaltyWithdrawal");
    }
  }

  public class LLLMod : Mod
  {
    LLLModSettings settings;
    public LLLMod(ModContentPack con) : base(con)
    {
      this.settings = GetSettings<LLLModSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      Listing_Standard listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.Label("LLLConsciousnessLabel".Translate() + ": " + (LLLModSettings.amountPenaltyConsciousness * 100) + "%", tooltip: "LLLConsciousnessTooltip".Translate());
      LLLModSettings.amountPenaltyConsciousness = listing.Slider(RoundToNearestHundredth(LLLModSettings.amountPenaltyConsciousness), -0f, -1f);
      listing.Label("LLLMovementLabel".Translate() + ": " + (LLLModSettings.amountPenaltyMovement * 100) + "%", tooltip: "LLLMovementTooltip".Translate());
      LLLModSettings.amountPenaltyMovement = listing.Slider(RoundToNearestHundredth(LLLModSettings.amountPenaltyMovement), -0f, -1f);
      listing.Label("LLLRestLabel".Translate() + ": " + (LLLModSettings.amountPenaltyRest * 100) + "%", tooltip: "LLLRestTooltip".Translate());
      LLLModSettings.amountPenaltyRest = listing.Slider(RoundToNearestHundredth(LLLModSettings.amountPenaltyRest), -0f, -1f);
      listing.Label("LLLHungerLabel".Translate() + ": +" + (LLLModSettings.amountHungerRate * 100) + "%", tooltip: "LLLHungerTooltip".Translate());
      LLLModSettings.amountHungerRate = listing.Slider(RoundToNearestHundredth(LLLModSettings.amountHungerRate), 0f, 1f);
      listing.Gap();
      listing.Label("LLLAddictivenessLabel".Translate() + ": " + (LLLModSettings.amountAddictiveness * 100) + "%", tooltip: "LLLAddictivenessTooltip".Translate());
      LLLModSettings.amountAddictiveness = listing.Slider(RoundToNearestHundredth(LLLModSettings.amountAddictiveness), 0f, 0.40f);
      listing.Label("LLLToleranceLabel".Translate() + ": " + (LLLModSettings.amountToleranceToAddict * 100) + "%", tooltip: "LLLToleranceTooltip".Translate());
      LLLModSettings.amountToleranceToAddict = listing.Slider(RoundToNearestHundredth(LLLModSettings.amountToleranceToAddict), 0f, 1f);
      listing.Gap();
      listing.Label("LLLAsthmaLabel".Translate() + ": " + LLLModSettings.amountAsthmaDaysChance, tooltip: "LLLAsthmaTooltip".Translate());
      LLLModSettings.amountAsthmaDaysChance = (int)listing.Slider(LLLModSettings.amountAsthmaDaysChance, 5, 2000);
      listing.Label("LLLCancerLabel".Translate() + ": " + LLLModSettings.amountCancerDaysChance, tooltip: "LLLCancerTooltip".Translate());
      LLLModSettings.amountCancerDaysChance = (int)listing.Slider(LLLModSettings.amountCancerDaysChance, 5, 2000);
      listing.Gap();
      listing.Label("LLLWithdrawalLabel".Translate() + ": " + LLLModSettings.amountPenaltyWithdrawal, tooltip: "LLLWithdrawalTooltip".Translate());
      LLLModSettings.amountPenaltyWithdrawal = (int)listing.Slider(LLLModSettings.amountPenaltyWithdrawal, -30, 0);
      listing.End();
      base.DoSettingsWindowContents(inRect);
    }

    public override void WriteSettings()
    {
      LLLUtility.UpdateAllChanges();

      base.WriteSettings();
    }

    public override string SettingsCategory()
    {
      return "LLLTitle".Translate();
    }

    private float RoundToNearestHalf(float val)
    { 
      return (float)Math.Round(val * 2, MidpointRounding.AwayFromZero) / 2;
    }

    private float RoundToNearestHundredth(float val)
    {
      return (float)Math.Round(val * 100, MidpointRounding.AwayFromZero) / 100;
    }
  }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DeepScannerSettings
{
  public class DSSSettings : ModSettings
  {
    public static List<string> DeepOreDefNames = new List<string>();
    public static List<float> Commonality = new List<float>();
    public static List<int> MinedAmountPerChunk = new List<int>();
    public static List<IntRange> VeinSizeRange = new List<IntRange>();

    public static void ClearSettings()
    {
      DeepOreDefNames.Clear();
      Commonality.Clear();
      MinedAmountPerChunk.Clear();
      VeinSizeRange.Clear();
    }

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Collections.Look(ref DeepOreDefNames, "DeepOreDefNames");
      Scribe_Collections.Look(ref Commonality, "Commonality");
      Scribe_Collections.Look(ref MinedAmountPerChunk, "MinedAmountPerChunk");
      Scribe_Collections.Look(ref VeinSizeRange, "VeinSizeRange");
    }
  }

  public class DSSMod : Mod
  {
    public bool resetDefaults;
    public Vector2 scrollPosition;

    DSSSettings settings;
    public DSSMod(ModContentPack con) : base(con)
    {
      this.settings = GetSettings<DSSSettings>();
    }

    public override void DoSettingsWindowContents(Rect canvas)
    {
      Listing_Standard lister = new Listing_Standard();

      float numOfItems = DSSSettings.DeepOreDefNames.Count() * 220f;
      float height = canvas.y + numOfItems;
      Rect viewRect = new Rect(0f, 0f, canvas.width - 260f, height);

      lister.Begin(canvas);
      lister.BeginScrollView(new Rect(120f, 0f, canvas.width - 240f, canvas.height), ref scrollPosition, ref viewRect);
      lister.Gap();

      for (int i = 0; i < DSSSettings.DeepOreDefNames.Count(); ++i)
      {
        string oreLabel = ThingDef.Named(DSSSettings.DeepOreDefNames[i]).label;

        lister.Settings_Header(Utils.StringToTitleCase(oreLabel), Color.clear, GameFont.Medium, TextAnchor.MiddleLeft);
        lister.GapLine();

        float c = DSSSettings.Commonality[i];
        int m = DSSSettings.MinedAmountPerChunk[i];
        int r1 = DSSSettings.VeinSizeRange[i].min;
        int r2 = DSSSettings.VeinSizeRange[i].max;

        lister.Settings_Numericbox("DSSCommonality".Translate() + " " + oreLabel, ref c, 420, 12f);
        lister.Settings_IntegerBox("DSSMinedAmountPerChunk".Translate(), ref m, 420, 12);
        lister.Settings_IntegerBox("DSSVeinSizeRangeMin".Translate(), ref r1, 420, 12f);
        lister.Settings_IntegerBox("DSSVeinSizeRangeMax".Translate(), ref r2, 420, 12f);

        DSSSettings.Commonality[i] = c;
        DSSSettings.MinedAmountPerChunk[i] = m;
        DSSSettings.VeinSizeRange[i] = new IntRange(r1, r2);

        lister.Gap(32f);
      }

      lister.End();
      lister.EndScrollView(ref viewRect);

      base.DoSettingsWindowContents(canvas);
    }

    public override void WriteSettings()
    {
      UpdateChanges();

      base.WriteSettings();
    }

    public override string SettingsCategory()
    {
      return "DSSMenuTitle".Translate();
    }

    public static void UpdateChanges()
    {
      // *My own references, ignore*
      // DefDatabase<HediffDef>.GetNamed("SmokeleafHigh").stages[0].capMods[0].offset = LLLModSettings.amountPenaltyConsciousness;
      // HediffDef.Named("SmokeleafHigh").stages.Where((HediffStage stage) => stage.capMods.Any((PawnCapacityModifier mod) => mod.capacity == PawnCapacityDefOf.Consciousness)).First().capMods.Where((PawnCapacityModifier mod) => mod.capacity == PawnCapacityDefOf.Consciousness).First().offset = RSModSettings.amountCramped;
      // ThingDef.Named("NEC_ReinforcedWall").statBases.Where((StatModifier statBase) => statBase.stat == StatDefOf.MaxHitPoints).First().value = RWModSettings.WallHitPoints;
      // SRWSettings.ReinforcedWall.statBases.Where((StatModifier statBase) => statBase.stat == StatDefOf.MaxHitPoints).First().value = SRWSettings.WallHitPoints;
      if (DSSSettings.DeepOreDefNames.Count() > 0)
      {
        for (int i = 0; i < DSSSettings.DeepOreDefNames.Count(); ++i)
        {
          ThingDef oreDef = ThingDef.Named(DSSSettings.DeepOreDefNames[i]);
          oreDef.deepCommonality = DSSSettings.Commonality[i];
          oreDef.deepCountPerPortion = DSSSettings.MinedAmountPerChunk[i];
          oreDef.deepLumpSizeRange = DSSSettings.VeinSizeRange[i];
        }
      }
    }
  }
}

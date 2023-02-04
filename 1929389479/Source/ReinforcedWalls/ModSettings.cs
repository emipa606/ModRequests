using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReinforcedWalls
{
  public class RWModSettings : ModSettings
  {
    public int EmbrasureCover = 65;
    public int WallHitPoints = 900;

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref EmbrasureCover, "EmbrasureCover");
      Scribe_Values.Look(ref WallHitPoints, "WallHitPoints");
    }
  }

  public class RWMod : Mod
  {
    public RWModSettings settings;
    public static RWMod mod;

    public RWMod(ModContentPack con) : base(con)
    {
      settings = GetSettings<RWModSettings>();
      mod = this;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      Listing_Standard listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.Gap(24f);

      listing.Settings_SliderLabeled("EmbrasureCoverValueLabel".Translate(), "%", ref mod.settings.EmbrasureCover, 1, 75);
      //listing.Settings_SliderLabeled("WallHitPointLabel".Translate(), "%", ref mod.settings.WallHitPoints, 300, 3000);
      listing.Settings_IntegerBox("WallHitPointLabel".Translate(), ref mod.settings.WallHitPoints, 500f, 24f, min: 1, max: 999999);
      listing.End();

      base.DoSettingsWindowContents(inRect);
    }

    public override void WriteSettings()
    {
      UpdateChanges();

      base.WriteSettings();
    }

    public override string SettingsCategory()
    {
      return "MenuTitle".Translate();
    }

    public static void UpdateChanges()
    {

      DefDatabase<ThingDef>.GetNamed("NEC_ReinforcedEmbrasure").fillPercent = RWMod.mod.settings.EmbrasureCover / 100f;

      DefDatabase<ThingDef>.GetNamed("NEC_ReinforcedWall").statBases.Where((StatModifier statBase) => statBase.stat == StatDefOf.MaxHitPoints).First().value = RWMod.mod.settings.WallHitPoints;
      DefDatabase<ThingDef>.GetNamed("NEC_ReinforcedEmbrasure").statBases.Where((StatModifier statBase) => statBase.stat == StatDefOf.MaxHitPoints).First().value = RWMod.mod.settings.WallHitPoints;
    }
  }
}

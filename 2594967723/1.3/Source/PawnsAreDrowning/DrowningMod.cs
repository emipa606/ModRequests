using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnsAreDrowning
{
    class DrowningMod : Mod
    {
        public static PawnsAreDrowningSettings settings;
        public DrowningMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<PawnsAreDrowningSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public static void ResetTerrainSettings()
        {
            var terrainDefs = DefDatabase<TerrainDef>.AllDefsListForReading.Where(x => x.IsWater);
            foreach (var waterTerrain in terrainDefs)
            {
                if (settings.drowningStates == null) settings.drowningStates = new Dictionary<string, float>();
                if (!settings.drowningStates.ContainsKey(waterTerrain.defName))
                {
                    settings.drowningStates[waterTerrain.defName] = 0.01f;
                }
            }
        }

        public static void ResetRaceSettings()
        {
            var raceDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race != null);
            foreach (var raceDef in raceDefs)
            {
                if (settings.raceStates == null) settings.raceStates = new Dictionary<string, bool>();
                if (!settings.raceStates.ContainsKey(raceDef.defName))
                {
                    settings.raceStates[raceDef.defName] = true;
                }
            }
        }


        public static void ResetApparelSettings()
        {
            var apparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel);
            foreach (var apparelDef in apparelDefs)
            {
                if (settings.apparelStates == null) settings.apparelStates = new Dictionary<string, bool>();
                if (!settings.apparelStates.ContainsKey(apparelDef.defName))
                {
                    if (apparelDef.apparel.tags.Contains("PreventDrowning"))
                    {
                        settings.apparelStates[apparelDef.defName] = true;
                    }
                    else
                    {
                        settings.apparelStates[apparelDef.defName] = false;
                    }
                }
            }
        }

        public override string SettingsCategory()
        {
            return "Pawns are drowning";
        }
    }

    [StaticConstructorOnStartup]
    public static class SettingsInit 
    {
        static SettingsInit()
        {
            if (!(DrowningMod.settings.raceStates?.Count() > 0))
            {
                DrowningMod.ResetRaceSettings();
            }
            if (!(DrowningMod.settings.apparelStates?.Count() > 0))
            {
                DrowningMod.ResetApparelSettings();
            }
            if (!(DrowningMod.settings.drowningStates?.Count() > 0))
            {
                DrowningMod.ResetTerrainSettings();
            }
        }
    }
}

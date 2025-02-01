using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LostTechnology
{
    public class LostTechnologyMod : Mod
    {
        public static LostTechnologySettings settings;
        public LostTechnologyMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<LostTechnologySettings>();
            new Harmony("LostTechnology").PatchAll();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return this.Content.Name;
        }

    }
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Reset();
        }

        public static void Reset()
        {
            if (LostTechnologySettings.spawnIntervalsByTechLevels is null)
            {
                LostTechnologySettings.spawnIntervalsByTechLevels = new Dictionary<TechLevel, IntRange>
                {
                    { TechLevel.Neolithic, new IntRange( GenDate.TicksPerDay / 5,  GenDate.TicksPerDay * 5) },
                    { TechLevel.Medieval, new IntRange( GenDate.TicksPerDay / 4,  GenDate.TicksPerDay * 6) },
                    { TechLevel.Industrial, new IntRange( GenDate.TicksPerDay / 3,  GenDate.TicksPerDay * 7) },
                    { TechLevel.Spacer, new IntRange( GenDate.TicksPerDay / 2,  GenDate.TicksPerDay * 8) },
                    { TechLevel.Ultra, new IntRange( GenDate.TicksPerDay,  GenDate.TicksPerDay * 9) },
                };
            }
            if (LostTechnologySettings.distanceRangeByTechLevels is null)
            {
                LostTechnologySettings.distanceRangeByTechLevels = new Dictionary<TechLevel, IntRange>
                {
                    { TechLevel.Neolithic, new IntRange(7,  15) },
                    { TechLevel.Medieval, new IntRange(8,  18) },
                    { TechLevel.Industrial, new IntRange(10,  20) },
                    { TechLevel.Spacer, new IntRange(15,  25) },
                    { TechLevel.Ultra, new IntRange(20,  30) },
                };
            }
        }
    }
}
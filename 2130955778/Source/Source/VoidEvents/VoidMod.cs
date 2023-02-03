using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace VoidEvents
{
    class VoidMod : Mod
    {
        public static VoidSettings settings;
        public VoidMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<VoidSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        // Return the name of the mod in the settings tab in game
        public override string SettingsCategory()
        {
            return "ReGrowth Core";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

        }
    }
}
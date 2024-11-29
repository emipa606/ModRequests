using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DireWolves
{
    class DireWolvesMod : Mod
    {
        public static DireWolvesSettings settings;
        public DireWolvesMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<DireWolvesSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "Dire Wolves";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }


}
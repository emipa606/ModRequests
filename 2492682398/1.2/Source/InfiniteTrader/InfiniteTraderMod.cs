﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace InfiniteTrader
{
    public class InfiniteTraderMod : Mod
    {
        public static InfiniteTraderSettings settings;
        public InfiniteTraderMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<InfiniteTraderSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Ultimate Capitalist Trader";
        }
    }
}

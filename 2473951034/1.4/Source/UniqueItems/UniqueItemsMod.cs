using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UniqueItems
{
    public class UniqueItemsMod : Mod
    {
        public static UniqueItemsSettings settings;

        public static string ModName = "Unique Items";
        public UniqueItemsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<UniqueItemsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return ModName;
        }

        public override void WriteSettings()
        {
            Core.ApplySettings(true);
            base.WriteSettings();
        }
    }
}

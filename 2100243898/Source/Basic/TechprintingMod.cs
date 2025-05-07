using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace DTechprinting
{
    class TechprintingMod : Mod
    {

        TechprintingSettings settings;

        public TechprintingMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<TechprintingSettings>();

            if (TechprintingSettings.lateLoad)
                LongEventHandler.QueueLongEvent(Base.Initialize, "DTechprinting.BuildingDatabase", false, null);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            TechprintingSettings.DrawSettings(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Techprinting";
        }


        public override void WriteSettings() // called when settings window closes
        {
            TechprintingSettings.WriteAll();
            base.WriteSettings();
        }
    }
}

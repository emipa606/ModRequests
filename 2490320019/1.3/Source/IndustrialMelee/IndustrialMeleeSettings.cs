using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IndustrialMelee
{
    class IndustrialMeleeSettings : ModSettings
    {
        public bool makeWeaponsAndPowerArmorNonStuffable = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref makeWeaponsAndPowerArmorNonStuffable, "makeWeaponsAndPowerArmorNonStuffable", false);
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("IM.MakeWeaponsAndPowerArmorNonStuffable".Translate(), ref makeWeaponsAndPowerArmorNonStuffable);
            listingStandard.End();
            base.Write();
        }
    }
}

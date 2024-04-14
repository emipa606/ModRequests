using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace USESetApparelDrawOrder
{
    class USESetApparelDrawOrderSettings : Mod
    {
        public USESetApparelDrawOrderSettings(ModContentPack content) : base(content)
        {
            GetSettings<USESetApparelDrawOrderModSettings>();
        }

        public override string SettingsCategory() => "USE Set Apparel Draw Order";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            USESetApparelDrawOrderModSettings.DoWindowContents(inRect);
        }
    }

    public class USESetApparelDrawOrderModSettings : ModSettings
    {
        public static bool USESetApparelDrawOrderSettings_WorkEveryPawn = false;

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();

            ls.Begin(inRect);
            ls.Gap(12f);
            ls.CheckboxLabeled("Functional on every pawn? (Colonist alwasy functional)", ref USESetApparelDrawOrderSettings_WorkEveryPawn);

            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref USESetApparelDrawOrderSettings_WorkEveryPawn, "USESetApparelDrawOrderSettings_WorkEveryPawn");
        }
    }
}

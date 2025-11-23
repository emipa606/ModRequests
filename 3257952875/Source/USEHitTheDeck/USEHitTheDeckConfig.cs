using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace USEHitTheDeck
{
    class USEHitTheDeckConfig : Mod
    {
        public USEHitTheDeckConfig(ModContentPack content) : base(content)
        {
            GetSettings<USEHitTheDeckConfigModSetting>();
        }

        public override string SettingsCategory() => "USE Hit The Deck Setting";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            USEHitTheDeckConfigModSetting.DoWindowContents(inRect);
        }
    }

    public class USEHitTheDeckConfigModSetting : ModSettings
    {
        public static bool onlyAppliedPlayerFactionPawn = true;
        public static bool excludeAnimalAndMech = false;
        public static bool canNotShootWhileThereIsObstacle = false;


        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();

            ls.Begin(inRect);
            ls.Gap(12f);
            ls.CheckboxLabeled("Only show gizmo on player controlled pawn?", ref onlyAppliedPlayerFactionPawn);

            ls.Begin(inRect);
            ls.Gap(12f);
            ls.CheckboxLabeled("Exclude animal and mechanoid?", ref excludeAnimalAndMech);

            //ls.Gap(12f);
            //ls.CheckboxLabeled("Can not shoot while crawl when there is obstacle in the sight?", ref canNotShootWhileThereIsObstacle);

            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref onlyAppliedPlayerFactionPawn, "onlyAppliedPlayerFactionPawn");
            Scribe_Values.Look(ref excludeAnimalAndMech, "excludeAnimalAndMech");
            //Scribe_Values.Look(ref canNotShootWhileThereIsObstacle, "canNotShootWhileThereIsObstacle");
        }
    }
}

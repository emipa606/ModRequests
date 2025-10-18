using System;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using Verse.AI;

namespace Umbrellas {
    public class RimbrellasSettings : ModSettings {
        public bool showUmbrellas = true;
        public bool showUmbrellasInRain = true;
        public bool showUmbrellasInSnow = true;
        public bool showUmbrellasWhenInside = false;
        public bool cowboyHatsPreventSoakingWet = false;

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref showUmbrellas, "showUmbrellas", true);
            Scribe_Values.Look(ref showUmbrellasInRain, "showUmbrellasInRain", true);
            Scribe_Values.Look(ref showUmbrellasInSnow, "showUmbrellasInSnow", true);
            Scribe_Values.Look(ref showUmbrellasWhenInside, "showUmbrellasWhenInside", false);
            Scribe_Values.Look(ref cowboyHatsPreventSoakingWet, "cowboyHatsPreventSoakingWet", false);
        }
    }
    public class RimbrellasPatcher {
        public static void DoPatching() {
            var harmony = new Harmony("com.battlemage64.rimbrellas");
            harmony.PatchAll();
        }
    }
    class RimbrellasMod : Mod {
        //private bool recheckDefList = true;
        //public static Vector2 scrollPosition = new Vector2();
        public static RimbrellasSettings settings;
        public RimbrellasMod(ModContentPack content) : base(content) {
            RimbrellasPatcher.DoPatching();
            settings = GetSettings<RimbrellasSettings>();

        }
    public override string SettingsCategory() => "RB_ModTitle".Translate();
        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard ls = new Listing_Standard();
            /*Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 16f, inRect.height + DefDatabase<TraitDef>.AllDefs.Count() * 55); //  + DefDatabase<TraitDef>.AllDefs.Count() * 55
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);*/
            //ls.Begin(rect2);
            ls.Begin(inRect);
            ls.CheckboxLabeled("RB_ShowEver".Translate(), ref settings.showUmbrellas);
            if (settings.showUmbrellas) {
                ls.CheckboxLabeled("RB_ShowInRain".Translate(), ref settings.showUmbrellasInRain);
                ls.CheckboxLabeled("RB_ShowInSnow".Translate(), ref settings.showUmbrellasInSnow);
                ls.CheckboxLabeled("RB_ShowInside".Translate(), ref settings.showUmbrellasWhenInside);
            }
            ls.Label("RB_AlwaysBonus".Translate()+"\n\n\n\n");
            ls.CheckboxLabeled("RB_CowboyHats".Translate(), ref settings.cowboyHatsPreventSoakingWet);
            settings.Write();
            ls.End();
            //Widgets.EndScrollView();
        }

    }
}
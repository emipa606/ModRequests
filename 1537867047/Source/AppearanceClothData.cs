using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace AppearanceClothes {
    public class AppearanceClothData : IExposable {
        public ThingDef apparelDef;
        public ThingDef stuffDef;
        public string apparelDefName;
        public Color apparelColor;

        public ThingStyleDef styleDef;
        public string styleDefName;

        public bool IsValid {
            get {
                return (apparelDef != null);
            }
        }

        public AppearanceClothData() {}

        public AppearanceClothData(Thing apparel) {
            this.apparelDef = apparel.def;
            this.stuffDef = apparel.Stuff;
            this.apparelColor = apparel.DrawColor;
            this.styleDef = apparel.GetStyleDef();
        }

        public Apparel MakeApparel() {
            if (apparelDef == null) {
                Log.Warning("apparelDef is null.");
                return null;
            }
            
            ThingDef stuff = stuffDef;
            if (apparelDef.MadeFromStuff && stuff == null) {
                stuff = GenStuff.DefaultStuffFor(apparelDef);
            }

            Apparel apparel = (Apparel)ThingMaker.MakeThing(apparelDef, stuff);
            apparel.SetColor(apparelColor,false);
            if (this.styleDef != null) {
                apparel.SetStyleDef(this.styleDef);
            }
            return apparel;
        }

        public void ExposeData() {
            string stuffDefName = "";
            if (Scribe.mode == LoadSaveMode.Saving) {
                if (this.apparelDef != null) {
                    this.apparelDefName = this.apparelDef.defName;
                }
                if (this.stuffDef != null) {
                    stuffDefName = this.stuffDef.defName;
                }
                if (this.styleDef != null) {
                    this.styleDefName = this.styleDef.defName;
                }
            }

            Scribe_Values.Look<string>(ref this.apparelDefName, "apparelDef");
            Scribe_Values.Look<string>(ref stuffDefName, "stuffDef", "");
            Scribe_Values.Look<Color>(ref this.apparelColor, "apparelColor");
            Scribe_Values.Look<string>(ref this.styleDefName, "styleDef", "");

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                this.apparelDef = DefDatabase<ThingDef>.GetNamed(this.apparelDefName, false);
                this.stuffDef = DefDatabase<ThingDef>.GetNamed(stuffDefName, false);
                if (this.styleDefName != null) {
                    this.styleDef = DefDatabase<ThingStyleDef>.GetNamed(this.styleDefName, false);
                }
                if (this.apparelDef == null) {
                    Log.Message("[Appearance clothes] apparelDef \"" + apparelDefName + "\" is not found.");
                }
                if (this.styleDef == null) {
                    Log.Message("[Appearance clothes] styleDef \"" + apparelDefName + "\" is not found.");
                }
            }
        }
    }
}

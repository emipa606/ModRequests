using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace WhereIsMyWeapon {
    public class WhereIsMyWeaponMod : Mod {
        public static WhereIsMyWeaponSettings Settings {
            get {
                return LoadedModManager.GetMod<WhereIsMyWeaponMod>().settings;
            }
        }
        
        public WhereIsMyWeaponSettings settings;
        private string bufferSearchDistance;

        public WhereIsMyWeaponMod(ModContentPack content) : base(content) {
            this.settings = GetSettings<WhereIsMyWeaponSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            float num = inRect.y;

            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, num, 400f, 24f);
            Widgets.CheckboxLabeled(rect, "WhereIsMyWeapon.LabelReequip".Translate(), ref settings.reequip);
            num += 30f;

            rect = new Rect(0f, num, 400f, 24f);
            Widgets.CheckboxLabeled(rect, "WhereIsMyWeapon.LabelRetake".Translate(), ref settings.retake);
            num += 30f;

            rect = new Rect(0f, num, 300f, 24f);
            Widgets.Label(rect, "WhereIsMyWeapon.LabelSearchRange".Translate());
            rect = new Rect(350f, num, 50f, 24f);
            Widgets.TextFieldNumeric<int>(rect, ref settings.searchDistance, ref bufferSearchDistance, 0, 9999);
            num += 30f;

            Text.Font = GameFont.Medium;
        }

        public override string SettingsCategory() {
            return "Where Is My Weapon";
        }
    }
}

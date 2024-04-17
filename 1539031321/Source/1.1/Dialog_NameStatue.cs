using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class Dialog_NameStatue : Window {
        private Building_StatueOfColonist statue;

        private string curName;

        public override Vector2 InitialSize {
            get {
                return new Vector2(500f, 120f);
            }
        }

        public Dialog_NameStatue(Building_StatueOfColonist statue) {
            this.statue = statue;
            if (statue.name == null) {
                statue.name = "";
            }
            this.optionalTitle = "StatueOfColonist.CommandRenameLabel".Translate();
            this.curName = statue.name;

            this.forcePause = true;
            this.closeOnAccept = false;
            this.closeOnCancel = false;
            this.absorbInputAroundWindow = true;
        }


        public override void DoWindowContents(Rect rect) {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
                flag = true;
                Event.current.Use();
            }
            Rect rect2;
            this.curName = Widgets.TextField(new Rect(0f, 0f, rect.width / 2f - 20f, 35f), this.curName);
            rect2 = new Rect(rect.width / 2f + 20f, 0f, rect.width / 2f - 20f, 35f);

            if (Widgets.ButtonText(rect2, "OK".Translate(), true, false, true) || flag) {
                this.statue.name = this.curName;
                Find.WindowStack.TryRemove(this, true);
                Event.current.Use();
            }
        }
    }
}

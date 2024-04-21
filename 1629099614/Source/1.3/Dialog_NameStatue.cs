using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public class Dialog_NameStatue : Window {
        private Building_StatueOfAnimal statue;

        private string curName;

        public override Vector2 InitialSize {
            get {
                return new Vector2(600f, 120f);
            }
        }

        public Dialog_NameStatue(Building_StatueOfAnimal statue) {
            this.statue = statue;
            if (statue.name == null) {
                statue.name = "";
            }
            this.optionalTitle = "StatueOfAnimal.CommandRenameLabel".Translate();
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
            this.curName = Widgets.TextField(new Rect(0f, 0f, 180f, 35f), this.curName);

            Rect rect2 = new Rect(190f, 0f, 120f, 35f);
            if (Widgets.ButtonText(rect2, "OK".Translate(), true, false, true) || flag) {
                this.statue.name = this.curName;
                Find.WindowStack.TryRemove(this, true);
                Event.current.Use();
            }
            rect2 = new Rect(320f, 0f, 200f, 35f);
            if (Widgets.ButtonText(rect2, "StatueOfAnimal.SetKindName".Translate(), true, false, true) || flag) {
                this.curName = this.statue.Data.pawnKindDef.label;
            }
        }
    }
}

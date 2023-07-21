using AlienRace;
using StatueOfColonist;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace StatueOfColonistAlienRacePatch {
    public class Dialog_EditBodyAddons : Window {
        private Building_StatueOfColonist statue;

        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        public override Vector2 InitialSize {
            get {
                return new Vector2(300f, 600f);
            }
        }

        public Dialog_EditBodyAddons(Building_StatueOfColonist statue) {
            this.statue = statue;

            this.forcePause = true;
            this.closeOnAccept = true;
            this.closeOnCancel = true;
            this.doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect) {
            Rect rect = inRect;
            Rect rect2 = rect.ContractedBy(10f);
            Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float num = 0f;

            ThingDef_AlienRace alienRace = statue.Data.raceDef as ThingDef_AlienRace;
            AlienComp alienComp = statue.GetComp<AlienComp>();

            List<BodyAddon> bodyAddons = alienRace.alienRace.generalSettings.alienPartGenerator.bodyAddons;
            for (int i=0;i< bodyAddons.Count;i++) {
                BodyAddon ba = bodyAddons[i];
                Rect rectIntRange = new Rect(0f, num, 240f, 40f);
                int val = alienComp.addonVariants[i];
                int oldVal = val;
                val = (int)Widgets.HorizontalSlider(rectIntRange, val, 0, ba.variantCount - 1, false, ba.path);
                alienComp.addonVariants[i] = val;
                if (oldVal != val) {
                    statue.ResolveGraphics();
                }
                num += 40f;
            }

            if (Event.current.type == EventType.Layout) {
                this.scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}

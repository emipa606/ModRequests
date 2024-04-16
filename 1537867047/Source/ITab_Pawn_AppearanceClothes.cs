using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AppearanceClothes {
    [StaticConstructorOnStartup]
    public class ITab_Pawn_AppearanceClothes : ITab {
        private const float TopPadding = 20f;

        private const float ThingIconSize = 28f;

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;

        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static Texture2D textureNoStyle = null;

        public override bool IsVisible {
            get {
                return (this.SelPawnForGear.RaceProps.ToolUser || this.SelPawnForGear.inventory.innerContainer.Count > 0) && (this.AppearanceClothes != null) && this.SelPawnForGear.IsColonist;
            }
        }

        private bool CanControl {
            get {
                return this.SelPawnForGear.IsColonistPlayerControlled && !this.SelPawnForGear.Downed;
            }
        }

        private Pawn SelPawnForGear {
            get {
                if (base.SelPawn != null) {
                    return base.SelPawn;
                }
                Corpse corpse = base.SelThing as Corpse;
                if (corpse != null) {
                    return corpse.InnerPawn;
                }
                throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + base.SelThing);
            }
        }

        private CompAppearanceClothes AppearanceClothes {
            get {
                return this.SelPawnForGear.GetComp<CompAppearanceClothes>();
            }
        }

        public ITab_Pawn_AppearanceClothes() {
            this.size = new Vector2(460f, 450f);
            this.labelKey = "AppearanceClothes.TabAppearanceClothes";
            this.tutorTag = "AppearanceClothes";
            LongEventHandler.ExecuteWhenFinished(delegate {
                ITab_Pawn_AppearanceClothes.textureNoStyle = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare", true);
            });
        }

        protected override void FillTab() {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
            Rect rect2 = rect.ContractedBy(10f);
            Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float num = 0f;

            Rect rect3 = new Rect(0f, num, viewRect.width - 36f, 28f);
            bool showAppearanceClothes = AppearanceClothes.ShowAppearanceClothes;
            bool oldShowApparentClothes = showAppearanceClothes;
            Widgets.CheckboxLabeled(rect3, "AppearanceClothes.ShowAppearanceClothes".Translate(), ref showAppearanceClothes);
            num += 32f;
            if (oldShowApparentClothes != showAppearanceClothes) {
                AppearanceClothes.ToggleShowAppearanceClothes();
            }

            if (this.SelPawnForGear.apparel != null && showAppearanceClothes) {
                if(this.SelPawnForGear.EnableAppearanceBody()) {
                    Rect rect4 = new Rect(0f, num, 100f, 28f);
                    Widgets.Label(rect4, "AppearanceClothes.AppearanceBodyType".Translate());
                    Rect rect5 = new Rect(100f, num, 200f, 28f);
                    if (Widgets.ButtonText(rect5, AppearanceClothes.appearanceBodyTypeDef.GetName())) {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach (BodyTypeDef bodyTypeDef in GetBodyTypeDefs(this.SelPawnForGear)) {
                            BodyTypeDef bodyType = bodyTypeDef;
                            Action action = delegate {
                                AppearanceClothes.appearanceBodyTypeDef = bodyType;
                                AppearanceClothes.UpdateAppearanceClothes();
                            };
                            FloatMenuOption item = new FloatMenuOption(bodyType.GetName(), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                            list.Add(item);
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                    num += 28f;
                }

                Widgets.ListSeparator(ref num, viewRect.width, "AppearanceClothes.AppearanceClothes".Translate());
                Rect rect6 = new Rect(0f, num, 140f, 28f);
                if (Widgets.ButtonText(rect6, "AppearanceClothes.AddAppearanceClothes".Translate())) {
                    AddAppearenceClothes();
                }
                Rect rect7 = new Rect(145f, num, 140f, 28f);
                if (Widgets.ButtonText(rect7, "AppearanceClothes.CopyAppearanceClothesFromApparel".Translate())) {
                    CopyAppearenceClothesFromApparel();
                }
                Rect rect8 = new Rect(290f, num, 140f, 28f);
                if (Widgets.ButtonText(rect8, "AppearanceClothes.OpenPresetListDialog".Translate())) {
                    OpenPresetListDialog();
                }
                num += 32f;

                List<Thing> apparentClothes = AppearanceClothes.AppearanceClothes;
                if (apparentClothes != null) {
                    for (int i=0;i<apparentClothes.Count;i++) {
                        Thing apparel = apparentClothes[i];
                        this.DrawThingRow(ref num, viewRect.width, apparel,i);
                    }
                }
            }

            if (Event.current.type == EventType.Layout) {
                this.scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private List<BodyTypeDef> GetBodyTypeDefs(Pawn pawn) {
            Func<BodyTypeDef, bool> validator = delegate (BodyTypeDef b) {
                bool result = !ContentFinder<Texture2D>.Get(b.bodyNakedGraphicPath + "_north", false).NullOrBad() && !ContentFinder<Texture2D>.Get(b.bodyNakedGraphicPath + "_south", false).NullOrBad();
                return result;
            };
            return DefDatabase<BodyTypeDef>.AllDefsListForReading.Where(b => validator(b)).ToList();
        }

        private void DrawThingRow(ref float y, float width, Thing thing,int index, bool showDropButtonIfPrisoner = false) {
            Rect rect = new Rect(0f, y, width, 28f);

            //ボタン「見た目装備の解除」
            {
                Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegion(rect2, "AppearanceClothes.RemoveApparentClothes".Translate());
                if (Widgets.ButtonImage(rect2, MyTexButton.Delete)) {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    RemoveAppearenceClothes(thing);
                }
                rect.width -= 24f;
            }

            //ボタン「色変更」
            {
                Apparel apparel = thing as Apparel;
                if (apparel != null && apparel.GetComp<CompColorable>() != null) {
                    Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                    TooltipHandler.TipRegion(rect2, "AppearanceClothes.ChangeApparelColor".Translate());
                    if (Widgets.ButtonImage(rect2, MyTexButton.ChangeColor)) {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        Find.WindowStack.Add(new Dialog_ChangeApparelColor(SelPawnForGear, apparel));
                    }
                    rect.width -= 24f;
                }
            }

            //ボタン「スタイル変更」
            if (thing.def.CanBeStyled()) {
                Apparel apparel = thing as Apparel;
                Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                Texture2D texture = textureNoStyle;
                if (apparel.StyleDef != null) {
                    texture = apparel.StyleDef.UIIcon;
                }
                List<ThingStyleDef> thingCategories = Util.GetThingStyles(thing.def);
                if (!thingCategories.NullOrEmpty() && Widgets.ButtonImage(rect2, texture)) {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    {
                        Action action = delegate {
                            thing.SetStyleDef(null);
                            AppearanceClothes.UpdateAppearanceClothes();
                        };
                        FloatMenuOption item = new FloatMenuOption("AppearanceClothes.StyleNone".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                        list.Add(item);
                    }
                    foreach (ThingStyleDef thingStyleDef in thingCategories) {
                        Action action = delegate {
                            thing.SetStyleDef(thingStyleDef);
                            AppearanceClothes.UpdateAppearanceClothes();
                        };
                        FloatMenuOption item = new FloatMenuOption(thingStyleDef.Category.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                        list.Add(item);
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                rect.width -= 24f;
            }


            if (Mouse.IsOver(rect)) {
                GUI.color = ITab_Pawn_AppearanceClothes.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }

            //見た目装備のアイコン
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null) {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f);
            }

            //見た目装備のラベル
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_AppearanceClothes.ThingLabelColor;
            Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect5, thing.def.LabelCap);

            //ホバー時
            Text.WordWrap = true;
            TooltipHandler.TipRegion(rect, thing.def.LabelCap);
            y += 28f;
        }

        private void AddAppearenceClothes() {
            List<AddMenuOption> list = new List<AddMenuOption>();
            foreach (ThingDef current in from def in DefDatabase<ThingDef>.AllDefs
                                         where CanAdd(def)
                                         select def) {
                ThingDef localDef = current;
                list.Add(new AddMenuOption(localDef, delegate {
                    ThingDef stuff = GenStuff.RandomStuffFor(localDef);
                    Thing thing = ThingMaker.MakeThing(localDef, stuff);
                    this.AppearanceClothes.AddAppearanceClothes(thing);
                }));
            }
            Find.WindowStack.Add(new Dialog_AddOptionLister(list,this.SelPawnForGear.GetAppearanceBodyTypeDef()));
        }

        private bool CanAdd(ThingDef def) {
            bool allowDuplicationOfClothes = AppearanceClothesMod.Settings.allowDuplicationOfClothes;
            return def.IsApparelAC() && (this.AppearanceClothes.CanWearWithoutDroppingAnything(def) || allowDuplicationOfClothes) && def.IsAvailableForBody(this.SelPawnForGear.GetAppearanceBodyTypeDef());
        }

        private void CopyAppearenceClothesFromApparel() {
            string text = "AppearanceClothes.ConfirmationCopyApparentClothes".Translate();
            Action action = delegate {
                this.AppearanceClothes.CopyAppearanceClothesFromApparel();
            };

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, action, false, null));
        }

        private void OpenPresetListDialog() {
            CompAppearanceClothes comp = this.SelPawn.GetComp<CompAppearanceClothes>();
            Find.WindowStack.Add(new Dialog_PresetList(comp));
        }

        private void RemoveAppearenceClothes(Thing apparel) {
            this.AppearanceClothes.RemoveAppearanceClothes(apparel);
        }
    }
}

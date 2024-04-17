using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace StatueOfColonist {
    public class Window_EditStatue : Window {
        private const float TopPadding = 20f;

        private const float ThingIconSize = 28f;

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;

        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private Building_StatueOfColonist statue;

        private string raceName = "";
        private string lifeStageName = "";
        private string hairName = "";
        private string headName = "";

        public List<ThingDef> raceDefs;

        public Building_StatueOfColonist Statue {
            get {
                return statue;
            }
        }

        public override Vector2 InitialSize {
            get {
                return new Vector2(460f, 600f);
            }
        }

        public Window_EditStatue(Building_StatueOfColonist statue) {
            this.optionalTitle = "StatueOfColonist.WindowEditStatue".Translate();
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;

            this.statue = statue;

            InitRaceDefs();

            UpdateButtonLabel();
        }

        public void InitRaceDefs() {
            this.raceDefs = new List<ThingDef>();
            this.raceDefs.Add(ThingDefOf.Human);
        }

        public void UpdateButtonLabel() {
            raceName = Statue.Data.raceDef.LabelCap;
            lifeStageName = Statue.Data.lifeStageDef.LabelCap;

            HairDef hair = Util.GetHairDefFromGraphicPath(statue.Data.hairGraphicPath);
            if (hair != null) {
                hairName = hair.LabelCap;
            }

            headName = statue.Data.headGraphicPath.Split('/').LastOrDefault();
        }

        public override void DoWindowContents(Rect inRect) {
            Text.Font = GameFont.Small;
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

            Rect rectCopyFromColonist = new Rect(0f, num, 140f, 28f);
            if (Widgets.ButtonText(rectCopyFromColonist, "StatueOfColonist.CopyFromColonist".Translate())) {
                CopyFromColonist();
            }
            Rect rectOpenPresetListDialog = new Rect(145f, num, 140f, 28f);
            if (Widgets.ButtonText(rectOpenPresetListDialog, "StatueOfColonist.OpenPresetListDialog".Translate())) {
                OpenPresetListDialog();
            }
            num += 40f;
            Widgets.DrawLineHorizontal(0f, num, viewRect.width);
            num += 8f;

            if (raceDefs.Count >= 2) {
                Rect rectLabelRaceOfStatue = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabelRaceOfStatue, "StatueOfColonist.RaceOfStatue".Translate());
                Rect rectButtonRaceOfStatue = new Rect(100f, num, 200f, 28f);
                if (Widgets.ButtonText(rectButtonRaceOfStatue, raceName)) {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (ThingDef raceDef in raceDefs) {
                        ThingDef localRaceDef = raceDef;
                        Action action = delegate {
                            raceName = localRaceDef.LabelCap;
                            Statue.Data.raceDef = localRaceDef;
                            UpdateRace();
                        };
                        FloatMenuOption item = new FloatMenuOption(localRaceDef.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                        list.Add(item);
                    }
                    if (!list.NullOrEmpty()) {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                num += 28f;


                Rect rectLabelGenderOfStatue = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabelGenderOfStatue, "StatueOfColonist.GenderOfStatue".Translate());
                Rect rectButtonGenderOfStatue = new Rect(100f, num, 200f, 28f);
                if (Widgets.ButtonText(rectButtonGenderOfStatue, Statue.Data.gender.GetLabel(false))) {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    for (int i = 1; i < 3; i++) {
                        Gender gender = (Gender)Enum.ToObject(typeof(Gender), i);
                        if (IsAvailableGender(gender, Statue.Data.raceDef)) {
                            Action action = delegate {
                                Statue.Data.gender = gender;
                                Statue.ResolveGraphics();
                            };
                            FloatMenuOption item = new FloatMenuOption(gender.GetLabel(false), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                            list.Add(item);
                        }
                    }
                    if (!list.NullOrEmpty()) {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                num += 28f;
            }

            Rect rectLabelLifeStageOfStatue = new Rect(0f, num + 2, 100f, 28f);
            Widgets.Label(rectLabelLifeStageOfStatue, "StatueOfColonist.LifeStageOfStatue".Translate());
            Rect rectButtonLifeStageOfStatue = new Rect(100f, num, 200f, 28f);
            if (Widgets.ButtonText(rectButtonLifeStageOfStatue, lifeStageName)) {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (LifeStageDef lifeStageDef in Statue.Data.raceDef.race.lifeStageAges.ConvertAll(ls => ls.def)) {
                    LifeStageDef localLifeStageDef = lifeStageDef;
                    Action action = delegate {
                        lifeStageName = localLifeStageDef.LabelCap;
                        Statue.Data.lifeStageDef = localLifeStageDef;
                        Statue.ResolveGraphics();
                    };
                    FloatMenuOption item = new FloatMenuOption(localLifeStageDef.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                    list.Add(item);
                }
                if (!list.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            num += 28f;

            Rect rectLabelHairTypeOfStatue = new Rect(0f, num + 2, 100f, 28f);
            Widgets.Label(rectLabelHairTypeOfStatue, "StatueOfColonist.HairTypeOfStatue".Translate());
            Rect rectButtonHairTypeOfStatue = new Rect(100f, num, 200f, 28f);
            if (Widgets.ButtonText(rectButtonHairTypeOfStatue, hairName)) {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                List<HairDef> hairDefs = DefDatabase<HairDef>.AllDefsListForReading;
                foreach (HairDef hairDef in hairDefs) {
                    if (IsAvailableHair(hairDef,Statue.Data.raceDef)) {
                        HairDef localHairDef = hairDef;
                        Action action = delegate {
                            hairName = localHairDef.LabelCap;
                            Statue.Data.hairGraphicPath = localHairDef.texPath;
                            Statue.ResolveGraphics();
                        };
                        FloatMenuOption item = new FloatMenuOption(localHairDef.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                        list.Add(item);
                    }
                }
                if (!list.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            num += 28f;

            Rect rectLabelHeadTypeOfStatue = new Rect(0f, num + 2, 100f, 28f);
            Widgets.Label(rectLabelHeadTypeOfStatue, "StatueOfColonist.HeadTypeOfStatue".Translate());
            Rect rectButtonHeadTypeOfStatue = new Rect(100f, num, 200f, 28f);
            if (Widgets.ButtonText(rectButtonHeadTypeOfStatue, headName)) {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                List<object> headTypes = GetHeadTypes(Statue.Data.raceDef);
                foreach (object headType in headTypes) {
                    list.Add(GetHeadTypeItem(headType, Statue.Data.raceDef));
                }
                if (!list.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            num += 28f;

            Rect rectLabelBodyTypeDefOfStatue = new Rect(0f, num + 2, 100f, 28f);
            Widgets.Label(rectLabelBodyTypeDefOfStatue, "StatueOfColonist.BodyTypeOfStatue".Translate());
            Rect rectButtonBodyTypeDefOfStatue = new Rect(100f, num, 200f, 28f);
            if (Widgets.ButtonText(rectButtonBodyTypeDefOfStatue, Statue.Data.bodyType.GetName())) {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (BodyTypeDef bodyType in GetBodyTypes(Statue.Data.raceDef)) {
                    Action action = delegate {
                        Statue.Data.bodyType = bodyType;
                        Statue.ResolveGraphics();
                    };
                    FloatMenuOption item = new FloatMenuOption(bodyType.GetName(), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                    list.Add(item);
                }
                if (!list.NullOrEmpty()) {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            num += 28f;

            if (Statue.Data.raceDef != ThingDefOf.Human) {
                EditAddons(ref num);
            }

            {
                Rect rectLabel = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabel, "StatueOfColonist.OffsetXOfStatue".Translate());
                Rect rectButtonInit = new Rect(305f, num, 40f, 28f);
                if (Widgets.ButtonText(rectButtonInit, "0")) {
                    this.Statue.Data.offset.x = 0f;
                }
                Rect rectSlider = new Rect(100f, num, 200f, 28f);
                this.Statue.Data.offset.x = Widgets.HorizontalSlider(rectSlider, this.Statue.Data.offset.x, -2f, 2f, false, this.Statue.Data.offset.x.ToString(), null, null, 0.025f);
                num += 32f;
            }

            {
                Rect rectLabel = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabel, "StatueOfColonist.OffsetZOfStatue".Translate());
                Rect rectButtonInit = new Rect(305f, num, 40f, 28f);
                if (Widgets.ButtonText(rectButtonInit, "0")) {
                    this.Statue.Data.offset.y = 0f;
                }
                Rect rectSlider = new Rect(100f, num, 200f, 28f);
                this.Statue.Data.offset.y = Widgets.HorizontalSlider(rectSlider, this.Statue.Data.offset.y, -2f, 2f, false, this.Statue.Data.offset.y.ToString(), null, null, 0.025f);
                num += 32f;
            }

            Widgets.ListSeparator(ref num, viewRect.width, "StatueOfColonist.Apparel".Translate());
            Rect rectAddApparel = new Rect(0f, num, 140f, 28f);
            if (Widgets.ButtonText(rectAddApparel, "StatueOfColonist.AddApparel".Translate())) {
                AddAppearenceClothes();
            }
            num += 32f;

            List<ThingDef> apparels = Statue.Data.wornApparelDefs;
            if (apparels != null) {
                for (int i = 0; i < apparels.Count; i++) {
                    ThingDef apparel = apparels[i];
                    this.DrawThingRow(ref num, viewRect.width, apparel, i);
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

        public virtual bool IsAvailableGender(Gender gender, ThingDef raceDef) {
            return true;
        }

        public virtual bool IsAvailableHair(HairDef hairDef,ThingDef raceDef) {
            return true;
        }

        public void UpdateRace() {
            Statue.ResolveGraphics();
        }

        public List<object> GetHeadTypes(ThingDef raceDef) {
            List<object> list = new List<object>();
            for (int i = 0; i < 12; i++) {
                HeadType headType = i.ToHeadType();
                list.Add(headType);
            }
            return list;
        }

        public FloatMenuOption GetHeadTypeItem(object src,ThingDef raceDef) {
            if(raceDef != ThingDefOf.Human) {
                return null;
            }
            HeadType headType = (HeadType)src;
            Action action = delegate {
                Statue.Data.headGraphicPath = headType.GetHeadGraphicPath();
                Statue.Data.crownType = headType.GetCrownType();
                headName = headType.GetName();
                Statue.ResolveGraphics();
            };
            FloatMenuOption item = new FloatMenuOption(headType.GetName(), action, MenuOptionPriority.Default, null, null, 0f, null, null);
            return item;
        }

        public List<BodyTypeDef> GetBodyTypes(ThingDef raceDef) {
            List<BodyTypeDef> list = new List<BodyTypeDef>();
            for (int i = 0; i < 5; i++) {
                BodyTypeDef bodyType = i.ToBodyTypeDef();
                list.Add(bodyType);
            }
            return list;
        }

        public virtual void EditAddons(ref float y) {
        }

        private void DrawThingRow(ref float y, float width, ThingDef thingdef,int index, bool showDropButtonIfPrisoner = false) {
            Rect rect = new Rect(0f, y, width, 28f);

            //ボタン「装備の解除」
            {
                Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegion(rect2, "StatueOfColonist.RemoveApparel".Translate());
                if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true))) {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                    Statue.RemoveApparel(thingdef);
                }
                rect.width -= 24f;
            }

            if (Mouse.IsOver(rect)) {
                GUI.color = Window_EditStatue.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }

            //装備のアイコン
            if (thingdef.DrawMatSingle != null && thingdef.DrawMatSingle.mainTexture != null) {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thingdef);
            }

            //装備のラベル
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Window_EditStatue.ThingLabelColor;
            Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect5, thingdef.LabelCap);

            //ホバー時
            Text.WordWrap = true;
            TooltipHandler.TipRegion(rect, thingdef.LabelCap);
            y += 28f;
        }

        private void AddAppearenceClothes() {
            List<AddMenuOption> list = new List<AddMenuOption>();
            foreach (ThingDef current in from def in DefDatabase<ThingDef>.AllDefs
                                         where def.thingClass != null && (def.thingClass == typeof(Apparel) || def.thingClass.IsSubclassOf(typeof(Apparel))) && CanWearWithoutDroppingAnything(def) && Util.CanWear(def,Statue.Data.bodyType)
                                         select def) {
                ThingDef localDef = current;
                list.Add(new AddMenuOption(localDef, delegate {
                    Statue.AddApparel(localDef);
                }));
            }
            Find.WindowStack.Add(new Dialog_AddOptionLister(list,Statue.Data.bodyType));
        }

        private bool CanWearWithoutDroppingAnything(ThingDef apDef) {
            for (int i = 0; i < Statue.Data.wornApparelDefs.Count; i++) {
                if (!ApparelUtility.CanWearTogether(apDef, Statue.Data.wornApparelDefs[i], Statue.Data.raceDef.race.body)) {
                    return false;
                }
            }
            return true;
        }

        

        private void CopyFromColonist() {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Pawn> colonists = getAllColonists();
            colonists.SortBy((Pawn x) => x.Dead);
            foreach (Pawn colonist in colonists) {
                Pawn localColonist = colonist;
                Action action = delegate {
                    string text = "StatueOfColonist.ConfirmationCopyFromColonist".Translate();
                    Action act = delegate {
                        CopyFromColonistInternal(localColonist);
                    };

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, act, false, null));
                };
                string label = localColonist.LabelShort;
                if (localColonist.Dead) {
                    label += "StatueOfColonist.PostLabelDeadColonist".Translate();
                }
                FloatMenuOption item = new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                list.Add(item);
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void CopyFromColonistInternal(Pawn p) {
            Statue.Data = new StatueOfColonistData(p, Statue.DrawColor, "Map/Cutout");
            Statue.ResolveGraphics();
            Statue.ResolveArt(p);
            UpdateButtonLabel();
            if (StatueOfColonistMod.Settings.autoName) {
                Statue.name = p.LabelShort;
            }
        }

        private List<Pawn> getAllColonists() {
            List<Map> tmpMaps = new List<Map>();
            tmpMaps.AddRange(Find.Maps);
            tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);

            List<Pawn> colonists = new List<Pawn>();
            for (int i = 0; i < tmpMaps.Count; i++) {
                colonists.AddRange(tmpMaps[i].mapPawns.FreeColonists);
                List<Thing> corpses = tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                for (int j = 0; j < corpses.Count; j++) {
                    if (!corpses[j].IsDessicated()) {
                        Corpse corpse = corpses[j] as Corpse;
                        if (corpse != null) {
                            Pawn innerPawn = corpse.InnerPawn;
                            if (innerPawn != null) {
                                if (innerPawn.IsColonist) {
                                    colonists.Add(innerPawn);
                                }
                            }
                        }
                    }
                }
                List<Pawn> allPawnsSpawned = tmpMaps[i].mapPawns.AllPawnsSpawned;
                for (int k = 0; k < allPawnsSpawned.Count; k++) {
                    Corpse corpse = allPawnsSpawned[k].carryTracker.CarriedThing as Corpse;
                    if (corpse != null && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist) {
                        colonists.Add(corpse.InnerPawn);
                    }
                }
                List<Thing> graves = tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Grave);
                for (int j = 0; j < graves.Count; j++) {
                    if (!graves[j].IsDessicated()) {
                        Building_Grave grave = graves[j] as Building_Grave;
                        if (grave != null && grave.Corpse != null) {
                            Pawn innerPawn = grave.Corpse.InnerPawn;
                            if (innerPawn != null) {
                                if (innerPawn.IsColonist) {
                                    colonists.Add(innerPawn);
                                }
                            }
                        }
                    }
                }
            }
            return colonists;
        }

        private void OpenPresetListDialog() {
            Find.WindowStack.Add(new Dialog_PresetList(statue,this));
        }
    }
}

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace StatueOfAnimal {
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

        private Building_StatueOfAnimal statue;

        private string pawnKindName = "";

        private string bufferScale = "";

        private Building_StatueOfAnimal Statue {
            get {
                return statue;
            }
        }

        public override Vector2 InitialSize {
            get {
                return new Vector2(460f, 450f);
            }
        }

        public Window_EditStatue(Building_StatueOfAnimal statue) {
            this.optionalTitle = "StatueOfAnimal.WindowEditStatue".Translate();
            this.doCloseButton = true;
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;

            this.statue = statue;
            this.bufferScale = statue.Data.scale.ToString();
            UpdateButtonLabel();
        }

        public void UpdateButtonLabel() {
            PawnKindDef pawnKindDef = statue.Data.pawnKindDef;
            if (pawnKindDef != null) {
                pawnKindName = pawnKindDef.LabelCap;
            }
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

            {
                Rect rectCopyFromPet = new Rect(0f, num, 140f, 28f);
                if (Widgets.ButtonText(rectCopyFromPet, "StatueOfAnimal.CopyFromPet".Translate())) {
                    CopyFromPet();
                }
                Rect rectOpenPresetListDialog = new Rect(145f, num, 140f, 28f);
                if (Widgets.ButtonText(rectOpenPresetListDialog, "StatueOfAnimal.OpenPresetListDialog".Translate())) {
                    OpenPresetListDialog();
                }
                num += 40f;
                Widgets.DrawLineHorizontal(0f, num, viewRect.width);
                num += 12f;
            }

            {
                Rect rectLabelPawnKindOfStatue = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabelPawnKindOfStatue, "StatueOfAnimal.PawnKindOfStatue".Translate());
                Rect rectButtonHairTypeOfStatue = new Rect(100f, num, 200f, 28f);
                if (Widgets.ButtonText(rectButtonHairTypeOfStatue, pawnKindName)) {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();

                    List<PawnKindDef> pawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.Animal);
                    foreach (PawnKindDef pawnKindDef in pawnKindDefs) {
                        PawnKindDef localPawnKindDef = pawnKindDef;
                        Action action = delegate {
                            pawnKindName = localPawnKindDef.LabelCap;
                            if (Statue.Data.pawnKindDef != localPawnKindDef) {
                                bool hasGenderOld = Statue.Data.pawnKindDef.RaceProps.hasGenders;
                                Statue.Data.pawnKindDef = localPawnKindDef;
                                Statue.Data.lifeStageIndex = Statue.Data.pawnKindDef.lifeStages.Count - 1;

                                bool hasGender = Statue.Data.pawnKindDef.RaceProps.hasGenders;
                                if (hasGender != hasGenderOld) {
                                    if (hasGender) {
                                        Statue.Data.gender = Gender.Male;
                                    } else {
                                        Statue.Data.gender = Gender.None;
                                    }
                                }
                                Statue.ResolveGraphics();
                            }
                        };
                        FloatMenuOption item = new FloatMenuOption(localPawnKindDef.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                        list.Add(item);
                    }
                    if (!list.NullOrEmpty()) {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                }
                num += 32f;
            }

            {
                Rect rectLabelLifeStageOfStatue = new Rect(0f, num + 4, 100f, 28f);
                Widgets.Label(rectLabelLifeStageOfStatue, "StatueOfAnimal.LifeStageOfStatue".Translate());
                Rect rectSliderLifeStageOfStatue = new Rect(100f, num + 2, 200f, 28f);
                float lifeStageIndex = this.Statue.Data.lifeStageIndex;
                int rightValue = this.Statue.Data.pawnKindDef.lifeStages.Count - 1;
                string lifeStageLabel = this.Statue.Data.CurKindLifeStage.label;
                string labelValue = (lifeStageLabel != null) ? lifeStageLabel : " ";
                lifeStageIndex = Widgets.HorizontalSlider(rectSliderLifeStageOfStatue, lifeStageIndex, 0, rightValue, false, labelValue, null, null, 1f);
                if (this.Statue.Data.lifeStageIndex != (int)lifeStageIndex) {
                    Log.Message(this.statue.Data.pawnKindDef.lifeStages[(int)lifeStageIndex].bodyGraphicData.drawSize.ToString());
                    if (this.Statue.Data.lifeStageIndex != (int)lifeStageIndex) {
                        this.Statue.Data.lifeStageIndex = (int)lifeStageIndex;
                        Statue.ResolveGraphics();
                    }
                }
                num += 36f;
            }

            {
                Rect rectLabelGenderOfStatue = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabelGenderOfStatue, "StatueOfAnimal.GenderOfStatue".Translate());
                Rect rectButtonGenderOfStatue = new Rect(100f, num, 200f, 28f);
                if (Widgets.ButtonText(rectButtonGenderOfStatue, Statue.Data.gender.GetLabel(true))) {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    bool hasGender = Statue.Data.pawnKindDef.RaceProps.hasGenders;
                    for (int i = 0; i < 3; i++) {
                        Gender gender = (Gender)Enum.ToObject(typeof(Gender), i);
                        if ((hasGender && gender != Gender.None) || (!hasGender && gender == Gender.None)) {
                            Action action = delegate {
                                Statue.Data.gender = gender;
                                Statue.ResolveGraphics();
                            };
                            FloatMenuOption item = new FloatMenuOption(gender.GetLabel(true), action, MenuOptionPriority.Default, null, null, 0f, null, null);
                            list.Add(item);
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                num += 32f;
            }

            {
                Rect rectCheckbox = new Rect(0f, num + 2, 124f, 28f);
                Widgets.CheckboxLabeled(rectCheckbox, "StatueOfAnimal.PackedOfStatue".Translate(), ref Statue.Data.packed, !Statue.HasPackedGraphic);
                num += 32f;
            }

            {
                Rect rectLabel = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabel, "StatueOfAnimal.OffsetXOfStatue".Translate());
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
                Widgets.Label(rectLabel, "StatueOfAnimal.OffsetZOfStatue".Translate());
                Rect rectButtonInit = new Rect(305f, num, 40f, 28f);
                if (Widgets.ButtonText(rectButtonInit, "0")) {
                    this.Statue.Data.offset.y = 0f;
                }
                Rect rectSlider = new Rect(100f, num, 200f, 28f);
                this.Statue.Data.offset.y = Widgets.HorizontalSlider(rectSlider, this.Statue.Data.offset.y, -2f, 2f, false, this.Statue.Data.offset.y.ToString(), null, null, 0.025f);
                num += 32f;
            }

            {
                Rect rectLabel = new Rect(0f, num + 2, 100f, 28f);
                Widgets.Label(rectLabel, "StatueOfAnimal.ScaleOfStatue".Translate());
                Rect rectButtonInit = new Rect(305f, num, 40f, 28f);
                if (Widgets.ButtonText(rectButtonInit, "1")) {
                    this.Statue.Data.scale = 1f;
                }
                Rect rectSlider = new Rect(100f, num, 200f, 28f);
                this.Statue.Data.scale = Widgets.HorizontalSlider(rectSlider, this.Statue.Data.scale, 0f, 2f, false, this.Statue.Data.scale.ToString(), null, null, 0.01f);
                num += 32f;
            }

            if (Event.current.type == EventType.Layout) {
                this.scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void CopyFromPet() {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Pawn> pets = GetAllPets();
            pets.SortBy((Pawn x) => x.Dead);
            foreach (Pawn pet in pets) {
                Pawn localPet = pet;
                Action action = delegate {
                    string text = "StatueOfAnimal.ConfirmationCopyFromPet".Translate();
                    Action act = delegate {
                        Statue.Data = new StatueOfAnimalData(localPet, Statue.DrawColor);
                        Statue.name = localPet.Name.ToStringShort;
                        Statue.ResolveGraphics();

                        UpdateButtonLabel();
                    };

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, act, false, null));
                };
                string label = localPet.LabelShort;
                if (localPet.Dead) {
                    label += "StatueOfAnimal.PostLabelDeadPet".Translate();
                }
                FloatMenuOption item = new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null);
                list.Add(item);
            }
            if (!list.NullOrEmpty()) {
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        private List<Pawn> GetAllPets() {
            List<Map> tmpMaps = new List<Map>();
            tmpMaps.AddRange(Find.Maps);
            tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);

            List<Pawn> pets = new List<Pawn>();
            for (int i = 0; i < tmpMaps.Count; i++) {
                pets.AddRange(from p in tmpMaps[i].mapPawns.PawnsInFaction(Faction.OfPlayer)
                                   where p.RaceProps.Animal
                                   select p);
                List<Thing> corpses = tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                for (int j = 0; j < corpses.Count; j++) {
                    if (!corpses[j].IsDessicated()) {
                        Corpse corpse = corpses[j] as Corpse;
                        if (corpse != null) {
                            Pawn innerPawn = corpse.InnerPawn;
                            if (innerPawn != null) {
                                if (innerPawn.Faction == Faction.OfPlayer && innerPawn.RaceProps.Animal) {
                                    pets.Add(innerPawn);
                                }
                            }
                        }
                    }
                }
                List<Pawn> allPawnsSpawned = tmpMaps[i].mapPawns.AllPawnsSpawned;
                for (int k = 0; k < allPawnsSpawned.Count; k++) {
                    Corpse corpse = allPawnsSpawned[k].carryTracker.CarriedThing as Corpse;
                    if (corpse != null && !corpse.IsDessicated() && corpse.InnerPawn.Faction == Faction.OfPlayer && corpse.InnerPawn.RaceProps.Animal) {
                        pets.Add(corpse.InnerPawn);
                    }
                }
                List<Thing> graves = tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Grave);
                for (int j = 0; j < graves.Count; j++) {
                    if (!graves[j].IsDessicated()) {
                        Building_Grave grave = graves[j] as Building_Grave;
                        if (grave != null && grave.Corpse != null) {
                            Pawn innerPawn = grave.Corpse.InnerPawn;
                            if (innerPawn != null) {
                                if (innerPawn.Faction == Faction.OfPlayer && innerPawn.RaceProps.Animal) {
                                    pets.Add(innerPawn);
                                }
                            }
                        }
                    }
                }
            }
            return pets;
        }

        private void OpenPresetListDialog() {
            Find.WindowStack.Add(new Dialog_PresetList(statue,this));
        }
    }
}

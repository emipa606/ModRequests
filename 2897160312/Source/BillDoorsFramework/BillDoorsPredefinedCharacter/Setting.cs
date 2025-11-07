using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace BillDoorsPredefinedCharacter
{
    public class Setting : ModSettings
    {
        //Used to store settings for disabled mods.
        public Dictionary<string, List<string>> AppearModes = new Dictionary<string, List<string>>();
        public Dictionary<PredefinedCharacterParmDef, List<string>> LoadedAppearModes = new Dictionary<PredefinedCharacterParmDef, List<string>>();

        List<string> defNameWorkingList;
        List<List<string>> appearModeWorkingList;

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving) foreach (var v in LoadedAppearModes) AppearModes.SetOrAdd(v.Key.defName, v.Value);
            Scribe_Collections.Look(ref AppearModes, "AppearModes", LookMode.Value, LookMode.Value, ref defNameWorkingList, ref appearModeWorkingList);
        }

        public void LoadDefs(bool purge = false)
        {
            foreach (var def in DefDatabase<PredefinedCharacterParmDef>.AllDefsListForReading)
            {
                var defName = def.defName;
                if (!purge && AppearModes.ContainsKey(defName))
                {
                    if (!def.appearMethods.Any()) AppearModes.Remove(defName);
                    else LoadedAppearModes.Add(def, AppearModes[defName]);
                }
                else
                {
                    InitDefAppMode(def);
                }
            }
        }

        public void InitDefAppMode(PredefinedCharacterParmDef def)
        {
            if (!def.appearMethods.Any())
            {
                if (AppearModes.ContainsKey(def.defName)) AppearModes.Remove(def.defName);
                return;
            }

            LoadedAppearModes.SetOrAdd(def, new List<string>());
            foreach (var mode in def.appearMethods)
            {
                if (mode.defaultEnabled) LoadedAppearModes[def].Add(mode.identifier);
            }
            AppearModes.SetOrAdd(def.defName, LoadedAppearModes[def]);
        }

        public void TryAddOrRemove(CharAppearParm d, PredefinedCharacterParmDef def)
        {
            if (!LoadedAppearModes.ContainsKey(def))
                TryAddOrRemove(d, def, LoadedAppearModes[def]);
        }

        public void TryAddOrRemove(CharAppearParm d, PredefinedCharacterParmDef def, List<string> modes)
        {
            if (modes.Contains(d.identifier))
            {
                modes.Remove(d.identifier);
            }
            else
            {
                modes.Add(d.identifier);
                if (d.tag != null)
                {
                    foreach (var d2 in def.appearMethods)
                    {
                        if (d2 != d && d2.tag == d.tag)
                        {
                            modes.Remove(d2.identifier);
                        }
                    }
                }
            }
        }
    }

    public class BDPDC_Mod : Mod
    {
        public static Setting settings;

        Scroll defScroll = new Scroll();

        public static Dictionary<PredefinedCharacterParmDef, List<string>> LoadedAppearModes => settings.LoadedAppearModes;
        public static Dictionary<PredefinedCharacterParmDef, Pawn> Tracker => GameComponent_CharacterTracker.TrackedPawns;
        public static Dictionary<Pawn, PredefinedCharacterParmDef> PawnPDCPair => GameComponent_CharacterTracker.PawnPDCPair;

        public BDPDC_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Setting>();
            LongEventHandler.QueueLongEvent(LoadDefs, "BDPDC_LoadDefs", false, null);
        }

        public override string SettingsCategory()
        {
            return "BDPDC_Setting".Translate();
        }

        void LoadDefs()
        {
            if (settings.LoadedAppearModes.NullOrEmpty()) settings.LoadDefs();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            inRect.SplitVertically(inRect.width * 0.6f, out Rect L, out Rect R);

            if (LoadedAppearModes.Any())
            {
                defScroll.Draw(L, settings.LoadedAppearModes.Keys.Reverse(), DrawButtons, "BDPDC_Appearance".Translate());
                string str = "BDPDC_Reset".Translate();
                if (Widgets.ButtonText(L.TopPartPixels(Text.LineHeight).LeftPartPixels(Text.CalcSize(str).x), str, overrideTextAnchor: TextAnchor.MiddleCenter)) settings.LoadDefs(true);
            }
            var corners = R.Corners();
            Widgets.DrawLine(corners[0], corners[3], Color.white, 1);


            if (Current.Game != null)
            {
                var trackPawns = DefDatabase<PredefinedCharacterParmDef>.AllDefsListForReading.Where(d => d.tracked);
                if (trackPawns.Any())
                {
                    defScroll.Draw(R, trackPawns.Reverse(), DrawTracker, "BDPDC_Tracker".Translate());
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(R, "BDPDC_NoTrackedPawns".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(R, "BDPDC_NotInGame".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public float DrawButtons(Rect rect, PredefinedCharacterParmDef def, bool highlight)
        {
            rect.height = def.appearMethods.Count * (Text.LineHeight + 2) + 6;
            rect.SplitVertically(rect.width / 2, out Rect a, out Rect b);

            a = a.TopPart((Text.LineHeight + 2) / rect.height);
            a.SplitVertically(Text.LineHeight + 2, out Rect iconRect, out Rect nameRect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(nameRect, LabelForDef(def));
            if (def.description != null) TooltipHandler.TipRegion(a, def.description);
            if (def.iconPath != null)
            {
                Widgets.DrawTextureFitted(iconRect, def.Icon, 1);
            }

            Listing_Standard listing_buttons = new Listing_Standard();
            listing_buttons.ColumnWidth = b.width;
            listing_buttons.Begin(b);
            foreach (var d in def.appearMethods)
            {
                var modes = LoadedAppearModes[def];
                if (listing_buttons.RadioButton(d.name, modes.Contains(d.identifier), tooltip: d.description)) settings.TryAddOrRemove(d, def, modes);
            }
            listing_buttons.Gap();
            listing_buttons.End();
            Widgets.DrawHighlightIfMouseover(rect);
            if (highlight) Widgets.DrawLightHighlight(rect);
            Text.Anchor = TextAnchor.UpperLeft;
            return rect.height;
        }

        public float DrawTracker(Rect rect, PredefinedCharacterParmDef def, bool highlight)
        {
            Tracker.TryGetValue(def, out Pawn p);
            rect.x += 10;
            rect.width -= 10;

            Text.Font = GameFont.Small;

            var label = LabelForDef(def);
            var halfW = rect.width / 2;
            var name = p == null ? "none" : $"{p.Name.ToStringFull}({p.ThingID})";

            var h = Mathf.Max(Text.CalcHeight(label, halfW), Text.CalcHeight(name, halfW));
            rect.height = h;

            rect.SplitVertically(halfW, out Rect a, out Rect b);
            a.SplitVertically(Text.LineHeight + 2, out Rect iconRect, out Rect nameRect);
            Widgets.Label(nameRect, label);
            Widgets.Label(b, name);
            if (def.iconPath != null)
            {
                Widgets.DrawTextureFitted(iconRect, def.Icon, 1);
            }

            var desc = def.description == null ? "" : def.description + "\n\n";

            if (p != null)
            {
                TooltipHandler.TipRegion(b, desc + "BDPDC_FindChar".Translate());
                if (Widgets.ButtonInvisible(b))
                {
                    GlobalTargetInfo target = p;
                    if (CameraJumper.CanJump(target))
                    {
                        CameraJumper.TryJumpAndSelect(target);
                        Find.WindowStack.TryRemoveAssignableFromType(typeof(Dialog_ModSettings));
                        Find.WindowStack.TryRemoveAssignableFromType(typeof(Dialog_Options));
                    }
                }
            }
            if (!WorldRendererUtility.WorldRenderedNow)
            {
                TooltipHandler.TipRegion(a, desc + "BDPDC_RedesignateChar".Translate());

                if (Widgets.ButtonInvisible(a))
                {
                    Targeter targeter = Find.Targeter;
                    var pram = new TargetingParameters
                    {
                        canTargetBuildings = false,
                        canTargetAnimals = false,
                        canTargetMechs = false
                    };
                    Find.Targeter.BeginTargeting(pram, DebugRedesignatePawn, DrawLabel);

                    Find.WindowStack.TryRemoveAssignableFromType(typeof(Dialog_ModSettings));
                    Find.WindowStack.TryRemoveAssignableFromType(typeof(Dialog_Options));
                }


            }

            Widgets.DrawHighlightIfMouseover(a);
            Widgets.DrawHighlightIfMouseover(b);
            Widgets.DrawHighlightIfMouseover(rect);
            if (highlight) Widgets.DrawLightHighlight(rect);
            return h;

            void DebugRedesignatePawn(LocalTargetInfo t)
            {
                if (t.Pawn == null) return;
                Tracker.SetOrAdd(def, t.Pawn);
                PawnPDCPair.SetOrAdd(t.Pawn, def);
            }

            void DrawLabel(LocalTargetInfo t)
            {
                var position = Event.current.mousePosition;
                var text = "BDPDC_DesignateChar".Translate(def.label);

                Color color = GUI.color;
                GUI.color = Color.white;
                Rect r = new Rect(position.x + 30, position.y - 18, 9999f, 100f);
                Vector2 vector = Text.CalcSize(text);
                GUI.DrawTexture(new Rect(r.x - vector.x * 0.1f, r.y, vector.x * 1.2f, vector.y), TexUI.GrayTextBG);
                GUI.color = color;
                Widgets.Label(r, text);
            }
        }

        string LabelForDef(PredefinedCharacterParmDef def)
        {
            return def.label ?? (def.firstName ?? def.defName);
        }
    }
}

using System;
using RimWorld;
using UINotIncluded.Widget.Configs;
using UINotIncluded.Windows;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UINotIncluded.Widget.Workers
{
    public class Button_Worker : WidgetWorker
    {
        private const float CompactModeMargin = 2f;
        public readonly ButtonConfig config;
        public MainButtonDef def;

        public Action<Rect> OnRepaint;

        public Button_Worker(ButtonConfig config)
        {
            this.config = config;
            def = (MainButtonDef)config.Def;
            config.RefreshCache();
            config.RefreshIcon();

            if (def.workerClass == typeof(MainButtonWorker_ToggleMechTab))
            {
                OnRepaint = _OnRepaint;
                Visible = () => config.forceShow || !def.Worker.Disabled;
            }
            else if (Settings.forceCustomTab || def.workerClass == typeof(MainButtonWorker) ||
                     def.workerClass == typeof(MainButtonWorker_ToggleTab) ||
                     def.workerClass == typeof(MainButtonWorker_ToggleWorld) ||
                     def.workerClass == typeof(MainButtonWorker_ToggleResearchTab))
            {
                OnRepaint = _OnRepaint;
                Visible = () => config.forceShow || def.Worker.Visible;
            }
            else
            {
                OnRepaint = def.Worker.DoButton;
                Visible = () => config.forceShow || def.buttonVisible;
            }
        }

        private new float IconSize => ExtendedToolbar.Height - 3f;
        private float HalfIconSize => IconSize / 2;

        public override bool FixedWidth => config.minimized;

        public override float Width => 60f;

        public override void OpenConfigWindow()
        {
            Find.WindowStack.Add(new EditMainButton_Window(config));
        }

        public virtual void InterfaceTryActivate()
        {
            if (TutorSystem.TutorialMode && def.canBeTutorDenied && Find.MainTabsRoot.OpenTab != def &&
                !TutorSystem.AllowAction((EventPack)("MainTab-" + def.defName + "-Open")))
                return;
            if (def.closesWorldView && Find.TilePicker.Active && !Find.TilePicker.AllowEscape)
                Messages.Message("MessagePlayerMustSelectTile".Translate(), MessageTypeDefOf.RejectInput, false);
            else
                def.Worker.Activate();
        }

        public override void OnGUI(Rect rect)
        {
#if DEBUG
            string key = "DrawButton " + this.def.defName;
            Analyzer.Profiling.Profiler profiler = Analyzer.Profiling.ProfileController.Start(key);
#endif
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    OnRepaint(rect);
                    break;

                default:
                    OnInteraction(rect);
                    break;
            }
#if DEBUG
            Analyzer.Profiling.ProfileController.Stop(key);
#endif
        }

        private void _OnRepaint(Rect rect)
        {
            Text.Font = GameFont.Small;
            var label = config.hideLabel ? "" : config.Label;
            var labelWidth = config.hideLabel ? 0 : config.LabelWidth;
            var overButton = Mouse.IsOver(rect);
            var leftMargin = rect.width * 0.1f;
            var drawIcon = config.Icon != null;
            var maxWidth = drawIcon ? rect.width - leftMargin - IconSize - 8f : rect.width - leftMargin - 4f;

            if (labelWidth > maxWidth)
            {
                label = config.ShortenedLabel;
                labelWidth = config.ShortenedLabelWidth;
                if (labelWidth > maxWidth) label = "";
            }

            if (def.Worker.Disabled)
            {
                Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            }
            else
            {
                label = config.minimized ? "" : label;
                double buttonBarPercent = def.Worker.ButtonBarPercent;

                if (drawIcon)
                {
                    var iconPos = rect.center;
                    iconPos -= new Vector2(HalfIconSize, HalfIconSize);
                    if (label != "") iconPos.x = rect.x + leftMargin;
                    if (overButton) iconPos += new Vector2(2f, -2f);

                    var iconSpace = new Rect(iconPos.x, iconPos.y, IconSize, IconSize);
                    leftMargin += IconSize + 4f;
                    DrawButtonTextSubtle(rect, label, SoundDefOf.Mouseover_Category, overButton,
                        (float)buttonBarPercent, leftMargin);
                    GUI.DrawTexture(iconSpace, config.Icon);
                }
                else
                {
                    DrawButtonTextSubtle(rect, label, SoundDefOf.Mouseover_Category, overButton,
                        (float)buttonBarPercent, leftMargin);
                }

                if (Find.MainTabsRoot.OpenTab != def && !Find.WindowStack.NonImmediateDialogWindowOpen)
                    UIHighlighter.HighlightOpportunity(rect, def.cachedHighlightTagClosed);
                if (def.description.NullOrEmpty())
                    return;
                TooltipHandler.TipRegion(rect, (TipSignal)(config.Label + "\n\n" + def.description));
            }
        }

        private void OnInteraction(Rect rect)
        {
            if (!Mouse.IsOver(rect)) return;
            if (def.Worker.Disabled)
            {
                if (Event.current.type == EventType.MouseDown)
                    return;
                Event.current.Use();
            }
            else
            {
                if (Widgets.ButtonInvisible(rect, false)) InterfaceTryActivate();
            }
        }

        private void DrawButtonTextSubtle(
            Rect rect,
            string label,
            SoundDef mouseoverSound,
            bool OverButton = false,
            float barPercent = 0.0f,
            float textLeftMargin = 0f)
        {
            if (OverButton) GUI.color = GenUI.MouseoverColor;
            MouseoverSounds.DoRegion(rect, mouseoverSound);
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            if (barPercent > 1.0 / 1000.0)
                Widgets.FillableBar(rect.ContractedBy(1f), barPercent, ModTextures.ButtonBarTex, null, false);
            var labelSpace = new Rect(rect);
            labelSpace.x += textLeftMargin;
            if (OverButton)
            {
                labelSpace.x += 2f;
                labelSpace.y -= 2f;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(labelSpace, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
        }
    }
}
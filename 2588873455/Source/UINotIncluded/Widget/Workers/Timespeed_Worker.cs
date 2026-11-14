using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UINotIncluded.Widget.Workers
{
    public class Timespeed_Worker : WidgetWorker
    {
        private static Action<Rect> cached_DoTimeControlsGUI;
        private static float extraWidth;

        private static readonly float _width = 140f;

        public Timespeed_Worker()
        {
            cached_DoTimeControlsGUI = cached_DoTimeControlsGUI ?? CustomTimeControls.DoSmartTimeControlsGUI2;
        }

        public override bool FixedWidth => true;
        public override float Width => _width + extraWidth;

        public static void SetSmartspeedMode()
        {
            cached_DoTimeControlsGUI = CustomTimeControls.DoSmartTimeControlsGUI;
            extraWidth = 8f;
        }

        public override void OnGUI(Rect rect)
        {
            Margins(ref rect);
            ExtendedToolbar.DoWidgetBackground(rect);
            Padding(ref rect);

            var timeRect = new Rect(rect.x, rect.center.y - 12f, rect.width, 24f);
            cached_DoTimeControlsGUI(timeRect);
        }

        private static class CustomTimeControls
        {
            public static readonly Vector2 TimeButSize = TimeControls.TimeButSize;
            private static readonly TimeSpeed[] CachedTimeSpeedValues = (TimeSpeed[])Enum.GetValues(typeof(TimeSpeed));

            private static void PlaySoundOf(TimeSpeed speed)
            {
                var soundDef = (SoundDef)null;
                switch (speed)
                {
                    case TimeSpeed.Paused:
                        soundDef = SoundDefOf.Clock_Stop;
                        break;

                    case TimeSpeed.Normal:
                        soundDef = SoundDefOf.Clock_Normal;
                        break;

                    case TimeSpeed.Fast:
                        soundDef = SoundDefOf.Clock_Fast;
                        break;

                    case TimeSpeed.Superfast:
                        soundDef = SoundDefOf.Clock_Superfast;
                        break;

                    case TimeSpeed.Ultrafast:
                        soundDef = SoundDefOf.Clock_Superfast;
                        break;
                }

                if (soundDef == null)
                    return;
                soundDef.PlayOneShotOnCamera();
            }

            //public static void DoTimeControlsGUI(Rect timerRect)
            //{
            //    TickManager tickManager = Find.TickManager;
            //    GUI.BeginGroup(timerRect);
            //    Rect rect = new Rect(0.0f, 0.0f, TimeControls.TimeButSize.x, TimeControls.TimeButSize.y);
            //    for (int index = 0; index < CustomTimeControls.CachedTimeSpeedValues.Length; ++index)
            //    {
            //        TimeSpeed cachedTimeSpeedValue = CustomTimeControls.CachedTimeSpeedValues[index];
            //        if (cachedTimeSpeedValue != TimeSpeed.Ultrafast)
            //        {
            //            bool selected = tickManager.CurTimeSpeed == cachedTimeSpeedValue;
            //            if (Widgets.ButtonImage(rect, cachedTimeSpeedValue.GetTexture(selected)))
            //            {
            //                if (cachedTimeSpeedValue == TimeSpeed.Paused)
            //                {
            //                    tickManager.TogglePaused();
            //                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
            //                }
            //                else
            //                {
            //                    tickManager.CurTimeSpeed = cachedTimeSpeedValue;
            //                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
            //                }
            //                CustomTimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
            //            }
            //            rect.x += rect.width;
            //        }
            //    }
            //    if (Find.TickManager.slower.ForcedNormalSpeed)
            //        Widgets.DrawLineHorizontal(rect.width * 2f, rect.height / 2f, rect.width * 2f);
            //    GUI.EndGroup();
            //    GenUI.AbsorbClicksInRect(timerRect);
            //    UIHighlighter.HighlightOpportunity(timerRect, nameof(TimeControls));
            //    if (Event.current.type != EventType.KeyDown)
            //        return;
            //    if (KeyBindingDefOf.TogglePause.KeyDownEvent)
            //    {
            //        Find.TickManager.TogglePaused();
            //        CustomTimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
            //        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
            //        Event.current.Use();
            //    }
            //    if (!Find.WindowStack.WindowsForcePause)
            //    {
            //        if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
            //        {
            //            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            //            CustomTimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
            //            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
            //            Event.current.Use();
            //        }
            //        if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
            //        {
            //            Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
            //            CustomTimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
            //            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
            //            Event.current.Use();
            //        }
            //        if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
            //        {
            //            Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
            //            CustomTimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
            //            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
            //            Event.current.Use();
            //        }
            //    }
            //    if (!Prefs.DevMode)
            //        return;
            //    if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
            //    {
            //        Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
            //        CustomTimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
            //        Event.current.Use();
            //    }
            //    if (!KeyBindingDefOf.Dev_TickOnce.KeyDownEvent || tickManager.CurTimeSpeed != TimeSpeed.Paused)
            //        return;
            //    tickManager.DoSingleTick();
            //    SoundDefOf.Clock_Stop.PlayOneShotOnCamera();
            //}

            public static void DoSmartTimeControlsGUI(Rect timerRect)
            {
                timerRect.x += 14;
                TimeControls.DoTimeControlsGUI(timerRect);
            }

            public static void DoSmartTimeControlsGUI2(Rect timerRect)
            {
                TimeControls.DoTimeControlsGUI(timerRect);
            }
        }
    }
}
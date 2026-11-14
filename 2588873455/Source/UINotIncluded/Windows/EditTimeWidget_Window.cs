using System;
using System.Collections.Generic;
using UINotIncluded.Widget.Configs;
using UnityEngine;
using Verse;

namespace UINotIncluded.Windows
{
    internal class EditTimeWidget_Window : Window
    {
        private static readonly Vector2 ButSize = new Vector2(150f, 38f);
        public TimeConfig config;

        public EditTimeWidget_Window(TimeConfig config)
        {
            this.config = config;
        }

        public override Vector2 InitialSize => new Vector2(400f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);

            if (list.ButtonTextLabeled("UINotIncluded.Setting.dateFormat".Translate(),
                    config.dateFormat.ToStringHuman()))
            {
                var options = new List<FloatMenuOption>();
                foreach (DateFormat dateFormat in Enum.GetValues(typeof(DateFormat)))
                {
                    var localFormat = dateFormat;
                    options.Add(new FloatMenuOption(localFormat.ToStringHuman(), () => config.dateFormat = dateFormat));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (list.ButtonTextLabeled("UINotIncluded.Setting.roundHour".Translate(), config.roundHour.ToString()))
            {
                var options = new List<FloatMenuOption>();
                foreach (RoundHour roundHour in Enum.GetValues(typeof(RoundHour)))
                    options.Add(new FloatMenuOption(roundHour.ToString(), () => config.roundHour = roundHour));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (list.ButtonTextLabeled("UINotIncluded.Setting.clockFormat".Translate(), config.clockFormat.ToString()))
            {
                var options = new List<FloatMenuOption>();
                foreach (ClockFormat clockFormat in Enum.GetValues(typeof(ClockFormat)))
                    options.Add(new FloatMenuOption(clockFormat.ToString(), () => config.clockFormat = clockFormat));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            list.End();
            if (Widgets.ButtonText(
                    new Rect(inRect.width / 2f - ButSize.x / 2f, inRect.height - ButSize.y, ButSize.x, ButSize.y),
                    "DoneButton".Translate())) Close();
        }
    }
}
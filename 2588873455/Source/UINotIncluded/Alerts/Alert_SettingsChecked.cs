using RimWorld;
using Verse;

namespace UINotIncluded.Alerts
{
    internal class Alert_SettingsChecked : Alert
    {
        public Alert_SettingsChecked()
        {
            defaultLabel = "UINotIncluded.Alerts.SettingsChecked.Label".Translate();
            defaultExplanation = "UINotIncluded.Alerts.SettingsChecked.Description".Translate();
            defaultPriority = AlertPriority.High;
        }

        public override AlertReport GetReport()
        {
            return !Settings.settingsChecked;
        }
    }
}
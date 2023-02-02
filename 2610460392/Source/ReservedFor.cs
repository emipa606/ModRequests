using HugsLib;
using HugsLib.Settings;
using Verse;

namespace ReservedFor
{
    public class ReservedFor : ModBase
    {
        internal static ReservedFor Instance;

        public override string ModIdentifier => "me.lubar.ReservedFor";

        public ReservedFor()
        {
            Instance = this;
        }

        private SettingHandle<bool> showOtherFactions;

        internal bool ShowOtherFactions => showOtherFactions;

        public override void DefsLoaded()
        {
            showOtherFactions = Settings.GetHandle<bool>("showOtherFactions",
                "ReservedFor_showOtherFactionReservations_title".Translate(),
                "ReservedFor_showOtherFactionReservations_description".Translate(),
                false);
        }
    }
}

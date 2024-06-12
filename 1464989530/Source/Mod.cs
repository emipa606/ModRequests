// Nightvision NightVision Mod.cs
// 
// 10 03 2018
// 
// 21 07 2018

using JetBrains.Annotations;

using UnityEngine;

using Verse;


namespace NightVision
{
    public class Mod : Verse.Mod
    {
        public static Mod Instance;

        [UsedImplicitly]
        public static Settings Settings => Settings.Instance;

        public static Storage Store => Settings.Store;

        public static SettingsCache Cache => Settings.Cache;

        public static Storage_Combat CombatStore => Settings.CombatStore;


        [UsedImplicitly]
        public Mod(
            ModContentPack content
        ) : base(content)
        {
            Instance = this;

            LongEventHandler.QueueLongEvent(
                InitSettings,
                "Initialising Night Vision Settings",
                false,
                null
            );
        }

        public override string SettingsCategory() => "Night Vision";

        public override void DoSettingsWindowContents(
            Rect inRect
        )
        {
            Settings.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        ///     Called by Rimworld before displaying the list of mod settings & after closing this mods settings window
        /// </summary>
        public override void WriteSettings()
        {
            Log.Message("Nightvision: WriteSettings called");
            //Cache.DoPreWriteTasks();
            base.WriteSettings();
        }

        public void InitSettings()
        {
            GetSettings<Settings>();
            Settings.Initialise();
            /*var initialise = new Initialiser();
            initialise.Startup();

            if (Settings.Store.NullRefWhenLoading)
            {
                WriteSettings();
            }*/
        }
    }
}
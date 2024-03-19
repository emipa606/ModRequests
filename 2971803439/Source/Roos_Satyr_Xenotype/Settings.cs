using Roos_Satyr_Xenotype;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Roos_Satyr_Xenotype
{
    public class SatyrSettings : ModSettings
    {
        // Default Mod Settings
        public const float SatyrSongVolumeDefault = 100.0f;
        public const int SatyrInstrumentTickrateDefault = 6;


        // setting variables to defaults
        public static float SatyrSongVolume = SatyrSongVolumeDefault;
        public static int SatyrInstrumentTickrate = SatyrInstrumentTickrateDefault;
        public static int SatyrInstrumentTickrateTicks = SatyrInstrumentTickrateDefault * 60;

        // Writes settings to file. Note that saving is by ref.
        public override void ExposeData()
        {
            Scribe_Values.Look(ref SatyrSongVolume, "SatyrSongVolume", SatyrSongVolumeDefault);
            Scribe_Values.Look(ref SatyrInstrumentTickrate, "SatyrInstrumentTickrate", SatyrInstrumentTickrateDefault);
            base.ExposeData();
        }
    }


    public class Roos_Satyr_Xenotype : Mod
    {
        // reference to our settings.
        public SatyrSettings settings;

        // constructor which resolves the reference to our settings.
        public Roos_Satyr_Xenotype(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SatyrSettings>();
        }

        // The GUI part to set our settings.
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            //listingStandard.Label("Lactation Ability Settings");
            SatyrSettings.SatyrSongVolume = listingStandard.SliderLabeled("Satyr Song Volume: " + SatyrSettings.SatyrSongVolume + "%", SatyrSettings.SatyrSongVolume, 0f, 100f);
            adjustMusicVolume(SatyrSettings.SatyrSongVolume);
            SatyrSettings.SatyrInstrumentTickrate = (int)listingStandard.SliderLabeled("Instrument Skillgain Tickrate: " + SatyrSettings.SatyrInstrumentTickrate + " sec/tick", SatyrSettings.SatyrInstrumentTickrate, 1, 25);
            SatyrSettings.SatyrInstrumentTickrateTicks = 60 * SatyrSettings.SatyrInstrumentTickrate;
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        // Override SettingsCategory to show up in the list of settings.
        public override string SettingsCategory()
        {
            return "Roo's Satyr Xenotype";
        }

        public static void adjustMusicVolume(float SatyrSongVolume)
        {
            foreach (SoundDef soundDef in RBSF_DefLists.RBSF_SongSounds)
            {
                foreach (SubSoundDef subSoundDef in soundDef.subSounds)
                {
                    float PushVolume = 0.3f * SatyrSongVolume;
                    subSoundDef.volumeRange.max = PushVolume;
                    subSoundDef.volumeRange.min = PushVolume;
                    //Log.Message("Subsound " + subSoundDef.name + " of sound " + soundDef.defName + " adjusted to " + PushVolume + "/30.0");
                }
            }
        }
    }

    

}

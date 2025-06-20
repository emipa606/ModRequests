using HarmonyLib;
using UnityEngine;
using Verse;

namespace Therapy
{
    [StaticConstructorOnStartup]
    public class Mod_Therapy : Mod
    {
        public Mod_Therapy(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony(this.Content.PackageIdPlayerFacing);
            harmony.PatchAll();
            PostLoad();
        }

        public void PostLoad()
        {
            LongEventHandler.QueueLongEvent(() => MapLoaded(), "Loading Therapy Resources", false, null);
        }

        public static void MapLoaded()
        {
            symbolTherapist1 = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Therapist1");
            symbolTherapist2 = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Therapist2");
            symbolPatient1 = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Patient1");
            symbolPatient2 = ContentFinder<Texture2D>.Get("Things/Mote/SpeechSymbols/Patient2");
            therapyIcon = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowRight");
        }

        public static Texture2D symbolTherapist1;
        public static Texture2D symbolTherapist2;
        public static Texture2D symbolPatient1;
        public static Texture2D symbolPatient2;
        public static Texture2D therapyIcon;
    }
}
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArmoredUp
{
    public class ArmoredUp : Mod
    {
        public static AUSettings settings;
        
        public ArmoredUp(ModContentPack content) : base(content)
        {
            settings = GetSettings<AUSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "Armored Up";
        }
    }
    
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //Harmony.DEBUG = true;
            new Harmony("ArmoredUp").PatchAllUncategorized();
        }
    }
    
}

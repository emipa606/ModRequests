using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FriendlyFireTweaks
{
    public class Main : Mod
    {
        private static readonly string ModName = ((AssemblyTitleAttribute) Attribute.GetCustomAttribute(typeof(Main).Assembly, typeof(AssemblyTitleAttribute), false)).Title;
        internal static Config config;
    
        public Main(ModContentPack contentPack) : base(contentPack)
        {
            config = GetSettings<Config>();
            new Harmony($"alx.{ModName.ToLower().Replace(" ", "")}").PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory()
        {
            return ModName;
        }
        
        public override void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(canvas);
            listingStandard.Label($"Version {((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(typeof(Main).Assembly, typeof(AssemblyFileVersionAttribute), false)).Version}");
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("Friendly fire by skill level.", ref config.ShootingLevelAffectsFriendlyFireChance, "If checked changes friendly fire chance to be based on the shooters shooting skill level, instead of disabling it completely. The higher the shooting level the less likely to hit a friendly target. Things without a shooting skill, like turrets, are treated as level 20.");
            if (config.ShootingLevelAffectsFriendlyFireChance)
            {
                listingStandard.Label($"Each level reduces chance by {config.PercentBonus}%", -1f,$"Chance of friendly fire at level...\n 0 = {Utility.GetHitChance(0, true)}%\n 5 = {Utility.GetHitChance(5, true)}%\n10 = {Utility.GetHitChance(10, true)}%\n15 = {Utility.GetHitChance(15, true)}%\n20 = {Utility.GetHitChance(20, true)}%");
                config.PercentBonus = (int)Math.Round(listingStandard.Slider(config.PercentBonus, 1, 20));
            }
            listingStandard.GapLine();
            
            listingStandard.Label("Protect from friendly fire for");
            listingStandard.CheckboxLabeled("Pawns.", ref config.ProtectPawns);
            listingStandard.CheckboxLabeled("Animals.", ref config.ProtectAnimals);
            listingStandard.CheckboxLabeled("Buildings.", ref config.ProtectBuildings);
            listingStandard.CheckboxLabeled("Mountain Stones/Resources.", ref config.ProtectMineables);
            
            listingStandard.End();
            base.DoSettingsWindowContents(canvas);
        }
    }
}
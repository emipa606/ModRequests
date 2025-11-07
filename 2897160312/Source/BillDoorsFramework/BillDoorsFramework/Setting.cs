using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Setting : ModSettings
    {
        public bool CustomProjectileColor = true;

        public bool useFactionColor = false;

        public bool discoLightMode = false;

        public bool isRGB = true;

        public bool enableProjectileColoring = true;

        public Color CustomPlayerProjectileColor;
        public Color CustomPirateProjectileColor;
        public Color CustomEmpireProjectileColor;
        public Color CustomMechanoidProjectileColor;
        public Color CustomHostilesProjectileColor;
        public Color CustomOthersProjectileColor;



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref CustomProjectileColor, "CustomProjectileColor", defaultValue: true);
            Scribe_Values.Look(ref useFactionColor, "useFactionColor", defaultValue: false);
            Scribe_Values.Look(ref discoLightMode, "discoLightMode", defaultValue: false);
            Scribe_Values.Look(ref isRGB, "isRGB", defaultValue: true);
            Scribe_Values.Look(ref enableProjectileColoring, "enableProjectileColoring", defaultValue: true);

            Scribe_Values.Look(ref CustomPlayerProjectileColor, "CustomPlayerProjectileColor", defaultValue: Color.cyan);
            Scribe_Values.Look(ref CustomPirateProjectileColor, "CustomPirateProjectileColor", defaultValue: Color.yellow);
            Scribe_Values.Look(ref CustomEmpireProjectileColor, "CustomEmpireProjectileColor", defaultValue: Color.green);
            Scribe_Values.Look(ref CustomMechanoidProjectileColor, "CustomMechanoidProjectileColor", defaultValue: Color.magenta);
            Scribe_Values.Look(ref CustomHostilesProjectileColor, "CustomHostilesProjectileColor", defaultValue: Color.red);
            Scribe_Values.Look(ref CustomOthersProjectileColor, "CustomOthersProjectileColor", defaultValue: Color.cyan);

        }
    }

    public class BDPMod : Mod
    {
        public static Setting settings;

        public static bool CustomProjectileColor => settings.CustomProjectileColor;

        public static bool useFactionColor => settings.useFactionColor;

        public static bool discoLightMode => settings.discoLightMode;

        public static bool enableProjectileColoring => settings.enableProjectileColoring;

        public static Color CustomPlayerProjectileColor => settings.CustomPlayerProjectileColor;
        public static Color CustomPirateProjectileColor => settings.CustomPirateProjectileColor;
        public static Color CustomEmpireProjectileColor => settings.CustomEmpireProjectileColor;
        public static Color CustomMechanoidProjectileColor => settings.CustomMechanoidProjectileColor;
        public static Color CustomHostilesProjectileColor => settings.CustomHostilesProjectileColor;
        public static Color CustomOthersProjectileColor => settings.CustomOthersProjectileColor;

        Color colorCache = Color.clear;

        public static bool isRGB => settings.isRGB;

        public FactionTypes selectedFaction = FactionTypes.Player;

        public BDPMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<Setting>();

            PatchMain.HarmonyInstance.Patch(PostLoad_PreFix.PostLoadMethod, null, new HarmonyMethod(typeof(PostLoad_PreFix).GetMethod("Prefix")));
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 17f) / 2f;
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Small;
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("BDP_enableProjectileColoring_Title".Translate(), ref settings.enableProjectileColoring, "BDP_enableProjectileColoring_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_CustomPlayerProjectileColor_Title".Translate(), ref settings.CustomProjectileColor, "BDP_CustomPlayerProjectileColor_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_useFactionColor_Title".Translate(), ref settings.useFactionColor, "BDP_useFactionColor_Desc".Translate());
            listing_Standard.CheckboxLabeled("BDP_discoLightMode_Title".Translate(), ref settings.discoLightMode, "BDP_discoLightMode_Desc".Translate());
            if (CustomProjectileColor && !useFactionColor && enableProjectileColoring)
            {
                switch (selectedFaction)
                {
                    case FactionTypes.Player:
                        colorCache = CustomPlayerProjectileColor;
                        break;
                    case FactionTypes.Pirate:
                        colorCache = CustomPirateProjectileColor;
                        break;
                    case FactionTypes.Empire:
                        colorCache = CustomEmpireProjectileColor;
                        break;
                    case FactionTypes.Mechanoid:
                        colorCache = CustomMechanoidProjectileColor;
                        break;
                    case FactionTypes.Hostiles:
                        colorCache = CustomHostilesProjectileColor;
                        break;
                    case FactionTypes.Others:
                        colorCache = CustomOthersProjectileColor;
                        break;
                }
                listing_Standard.GapLine();
                if (listing_Standard.RadioButton("BDP_SettingPlayer".Translate(), selectedFaction == FactionTypes.Player))
                {
                    colorCache = CustomPlayerProjectileColor;
                    selectedFaction = FactionTypes.Player;
                }
                if (listing_Standard.RadioButton("BDP_SettingPirate".Translate(), selectedFaction == FactionTypes.Pirate))
                {
                    colorCache = CustomPirateProjectileColor;
                    selectedFaction = FactionTypes.Pirate;
                }
                if (listing_Standard.RadioButton("BDP_SettingEmpire".Translate(), selectedFaction == FactionTypes.Empire))
                {
                    colorCache = CustomEmpireProjectileColor;
                    selectedFaction = FactionTypes.Empire;
                }
                if (listing_Standard.RadioButton("BDP_SettingMechanoid".Translate(), selectedFaction == FactionTypes.Mechanoid))
                {
                    colorCache = CustomMechanoidProjectileColor;
                    selectedFaction = FactionTypes.Mechanoid;
                }
                if (listing_Standard.RadioButton("BDP_SettingHostile".Translate(), selectedFaction == FactionTypes.Hostiles))
                {
                    colorCache = CustomHostilesProjectileColor;
                    selectedFaction = FactionTypes.Hostiles;
                }
                if (listing_Standard.RadioButton("BDP_SettingOthers".Translate(), selectedFaction == FactionTypes.Others))
                {
                    colorCache = CustomOthersProjectileColor;
                    selectedFaction = FactionTypes.Others;
                }
                listing_Standard.GapLine();
                if (listing_Standard.RadioButton("BDP_SettingRGB".Translate(), isRGB))
                {
                    settings.isRGB = true;
                }
                if (listing_Standard.RadioButton("BDP_SettingHSV".Translate(), !isRGB))
                {
                    settings.isRGB = false;
                }
                if (isRGB)
                {
                    listing_Standard.Label("R: " + Math.Round(colorCache.r * 255).ToString());
                    colorCache.r = listing_Standard.Slider(colorCache.r, 0, 1);

                    listing_Standard.Label("G: " + Math.Round(colorCache.g * 255).ToString());
                    colorCache.g = listing_Standard.Slider(colorCache.g, 0, 1);

                    listing_Standard.Label("B: " + Math.Round(colorCache.b * 255).ToString());
                    colorCache.b = listing_Standard.Slider(colorCache.b, 0, 1);
                }
                else
                {
                    Color.RGBToHSV(colorCache, out float H, out float S, out float V);

                    listing_Standard.Label("H: " + Math.Round(H * 360).ToString());
                    float Hadj = listing_Standard.Slider(H, 0, 1);

                    listing_Standard.Label("S: " + Math.Round(S * 100).ToString() + "%");
                    float Sadj = listing_Standard.Slider(S, 0, 1);

                    listing_Standard.Label("V: " + Math.Round(V * 100).ToString() + "%");
                    float Vadj = listing_Standard.Slider(V, 0, 1);

                    colorCache = Color.HSVToRGB(Hadj, Sadj, Vadj);
                }

                Rect ColorPreview = new Rect(200, 190, 100, 100);
                Texture2D bullet = ContentFinder<Texture2D>.Get("Things/Projectile/Bullet_Small");
                GUI.DrawTexture(ColorPreview, bullet, ScaleMode.ScaleToFit, true, 0, colorCache, 0, 0);

                switch (selectedFaction)
                {
                    case FactionTypes.Player:
                        settings.CustomPlayerProjectileColor = colorCache;
                        break;
                    case FactionTypes.Pirate:
                        settings.CustomPirateProjectileColor = colorCache;
                        break;
                    case FactionTypes.Empire:
                        settings.CustomEmpireProjectileColor = colorCache;
                        break;
                    case FactionTypes.Mechanoid:
                        settings.CustomMechanoidProjectileColor = colorCache;
                        break;
                    case FactionTypes.Hostiles:
                        settings.CustomHostilesProjectileColor = colorCache;
                        break;
                    case FactionTypes.Others:
                        settings.CustomOthersProjectileColor = colorCache;
                        break;
                }

                if (listing_Standard.ButtonText("BDP_SettingReset".Translate()))
                {
                    settings.CustomPlayerProjectileColor = Color.cyan;
                    settings.CustomPirateProjectileColor = Color.yellow;
                    settings.CustomEmpireProjectileColor = Color.green;
                    settings.CustomMechanoidProjectileColor = Color.magenta;
                    settings.CustomHostilesProjectileColor = Color.red;
                    settings.CustomOthersProjectileColor = Color.cyan;
                }

            }
            listing_Standard.End();
        }

        public override string SettingsCategory()
        {
            return "BDF_Setting".Translate();
        }
    }

    public enum FactionTypes
    {
        Player,
        Pirate,
        Empire,
        Mechanoid,
        Hostiles,
        Others,
    }
}

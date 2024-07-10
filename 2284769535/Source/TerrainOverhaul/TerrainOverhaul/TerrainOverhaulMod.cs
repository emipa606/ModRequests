using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace TerrainOverhaul
{
    public class TerrainOverhaulMod : Mod
    {
        public const string DISABLED_TAG = "[X]";
        public static TerrainOverhaulMod Instance { get; private set; }

        public static void Log(string msg)
        {
            Verse.Log.Message("<color=magenta>[TerrainOverhaul] " + (msg ?? "<null>") + "</color>");
        }
        public static void Error(string msg)
        {
            Verse.Log.Error("[TerrainOverhaul] " + (msg ?? "<null>"));
        }

        public TerrainOverhaulMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Log("Init");

            // Required to ensure that settings are loaded.
            var settings = GetSettings<TO_Settings>();

            Harmony h = new Harmony("Bickley.LegacyTerrainOverhaul");
            h.PatchAll();
            var number = h.GetPatchedMethods().EnumerableCount();
            Log($"Patched {number} methods.");
        }

        public override string SettingsCategory()
        {
            return "Legacy Terrain Overhaul";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var settings = GetSettings<TO_Settings>();
            settings?.Draw(inRect);
        }

        public void OnDefsLoaded()
        {
            var settings = GetSettings<TO_Settings>();

            settings.CreateTextureList(out bool reqRestart1);
            string errors = settings.EnsureAll(out bool requiresRestart);
            if (errors != null)
            {
                Log($"[ERROR] {errors}");
                PopupWindow.Open((r) =>
                {
                    Widgets.Label(r,
                        $"<size=34>[Legacy Terrain Overhaul]</size>\n\n{"LTO.LoadErrorMessage".Translate()}\n" +
                        errors);
                });
            }

            if (requiresRestart || reqRestart1)
            {
                Log("Requires restart");
                PopupWindow.Open((r) =>
                {
                    Widgets.Label(r,
                        $"<size=34>[Legacy Terrain Overhaul]</size>\n\n{"LTO.RequireRestartMessage".Translate()}");
                });
            }

            Log("Replacing snow texture...");
            ReplaceInternalTexture(MatBases.Snow, "Terrain/Surfaces/Snow");

            // These method calls come from legacy code, but aren't necessary.
            //Log("Replacing world map textures...");
            //ReplaceInternalTexture(WorldMaterials.WorldTerrain, "World/WorldTerrain");
            //ReplaceInternalTexture(WorldMaterials.WorldIce, "World/WorldIce");
            //ReplaceInternalTexture(WorldMaterials.WorldOcean, "World/WorldOcean");
            //ReplaceInternalTexture(WorldMaterials.UngeneratedPlanetParts, "World/UngeneratedPlanetParts");
            //ReplaceInternalTexture(WorldMaterials.Rivers, "World/Rivers");
            //ReplaceInternalTexture(WorldMaterials.RiversBorder, "World/RiversBorder");
            //ReplaceInternalTexture(WorldMaterials.Roads, "World/Roads");
            //ReplaceInternalTexture(WorldMaterials.MouseTile, "World/MouseTile");
            //ReplaceInternalTexture(WorldMaterials.SelectedTile, "World/SelectedTile");
            //ReplaceInternalTexture(WorldMaterials.CurrentMapTile, "World/CurrentMapTile");
            //ReplaceInternalTexture(WorldMaterials.Stars, "World/Stars");
            //ReplaceInternalTexture(WorldMaterials.Sun, "World/Sun");
            //ReplaceInternalTexture(WorldMaterials.PlanetGlow, "World/PlanetGlow");
            //ReplaceInternalTexture(WorldMaterials.SmallHills, "World/SmallHills");
            //ReplaceInternalTexture(WorldMaterials.LargeHills, "World/LargeHills");
            //ReplaceInternalTexture(WorldMaterials.Mountains, "World/Mountains");
            //ReplaceInternalTexture(WorldMaterials.ImpassableMountains, "World/ImpassableMountains");
        }

        /// <summary>
        /// Replaces an internal material's texture with a texture from this mod.
        /// Only required by some materials that are not created from defs.
        /// </summary>
        private void ReplaceInternalTexture(Material material, string path)
        {
            var texture = Content.GetContentHolder<Texture2D>().Get(path);

            if (texture != null)
                material.mainTexture = texture;
        }
    }

    [StaticConstructorOnStartup]
    internal static class StaticLoader
    {
        static StaticLoader()
        {
            TerrainOverhaulMod.Log("Static constructor!");
            TerrainOverhaulMod.Instance.OnDefsLoaded();
        }
    }
}

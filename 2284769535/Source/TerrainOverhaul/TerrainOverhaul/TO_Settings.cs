using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace TerrainOverhaul
{
    public class TO_Settings : ModSettings
    {
        private void Log(string msg) => TerrainOverhaulMod.Log(msg);
        private void Error(string msg) => TerrainOverhaulMod.Error(msg);

        private HashSet<string> disabledTextures = new HashSet<string>();
        //public bool DoWorldMapTextures = true;

        private Vector2 scroll;
        private float lastHeight;
        private List<TextureItem> items;
        private string search;
        private byte toShow = 0; // 0: all, 1: only disabled, 2: only enabled

        public float FarmlandFertilityScale = 1f;
        public float FarmlandXPScale = 1f;

        private int tabIndex;
        private List<(string, Action<Rect>)> tabs;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref disabledTextures, "DisabledTextures", LookMode.Value);
            if (disabledTextures == null)
                disabledTextures = new HashSet<string>();
            Scribe_Values.Look(ref FarmlandFertilityScale, "FarmlandFertilityScale", 1f);
            Scribe_Values.Look(ref FarmlandXPScale, "FarmlandXPScale", 1f);
        }

        public void Draw(Rect rect)
        {
            if (tabs == null)
                CreateTabs();

            float x = rect.x;
            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                bool active = tabIndex == i;

                float w = Text.CalcSize(tab.Item1).x + 20;
                Rect buttonRect = new Rect(x, rect.y, w, 32);
                x += w + 10;

                bool clicked = Widgets.CustomButtonText(ref buttonRect, tab.Item1, Color.black, Color.white, active ? Color.cyan : Color.white, active: !active);
                if (clicked)
                    tabIndex = i;

                if (active)
                {
                    var drawRect = new Rect(rect.x, rect.y + 40, rect.width, rect.height - 40);
                    Widgets.DrawWindowBackground(drawRect.ExpandedBy(4));
                    tab.Item2?.Invoke(drawRect);
                }
            }
        }

        private static Rect AdjustRectToUIScaling(Rect rect)
        {
            Rect result = rect;
            result.xMin = Widgets.AdjustCoordToUIScalingFloor(rect.xMin);
            result.yMin = Widgets.AdjustCoordToUIScalingFloor(rect.yMin);
            result.xMax = Widgets.AdjustCoordToUIScalingCeil(rect.xMax);
            result.yMax = Widgets.AdjustCoordToUIScalingCeil(rect.yMax);
            return result;
        }
        private static void DrawBox(Rect rect, int thickness = 1, Texture2D lineTexture = null)
        {
            try
            {
                Widgets.DrawBox(rect, thickness, lineTexture);
            }
            catch (MissingMethodException) // not exactly sure why this happens during runtime
            {
                Vector2 vector = new Vector2(rect.x, rect.y);
                Vector2 vector2 = new Vector2(rect.x + rect.width, rect.y + rect.height);
                if (vector.x > vector2.x)
                {
                    float x = vector.x;
                    vector.x = vector2.x;
                    vector2.x = x;
                }
                if (vector.y > vector2.y)
                {
                    float y = vector.y;
                    vector.y = vector2.y;
                    vector2.y = y;
                }
                Vector3 vector3 = vector2 - vector;
                GUI.DrawTexture(AdjustRectToUIScaling(new Rect(vector.x, vector.y, (float)thickness, vector3.y)), lineTexture ?? BaseContent.WhiteTex);
                GUI.DrawTexture(AdjustRectToUIScaling(new Rect(vector2.x - (float)thickness, vector.y, (float)thickness, vector3.y)), lineTexture ?? BaseContent.WhiteTex);
                GUI.DrawTexture(AdjustRectToUIScaling(new Rect(vector.x + (float)thickness, vector.y, vector3.x - (float)(thickness * 2), (float)thickness)), lineTexture ?? BaseContent.WhiteTex);
                GUI.DrawTexture(AdjustRectToUIScaling(new Rect(vector.x + (float)thickness, vector2.y - (float)thickness, vector3.x - (float)(thickness * 2), (float)thickness)), lineTexture ?? BaseContent.WhiteTex);
            }
            
        }

        private void CreateTabs()
        {
            tabs = new List<(string, Action<Rect>)>();

            tabs.Add(("LTO.Textures".Translate(), DrawTextureToggles));
            tabs.Add(("LTO.Other".Translate(), DrawOtherTab));
        }

        private void DrawTextureToggles(Rect rect)
        {
            const int SEARCH_HEIGHT = 60;

            Widgets.Label(new Rect(rect.x, rect.y, 300, 20), $"{"LTO.Filter".Translate()}:");
            search = Widgets.TextField(new Rect(rect.x, rect.y + 20, 360, 28), search);

            string buttonText = $"{"LTO.Show".Translate()}: {(toShow == 0 ? "LTO.All".Translate() : toShow == 1 ? "LTO.Disabled".Translate() : "LTO.Enabled".Translate())}";
            Rect buttonRect = new Rect(rect.x + 370, rect.y + 20, 120, 28);
            Rect buttonRect2 = new Rect(rect.xMax - 110, rect.y + 20, 100, 28);
            Rect buttonRect3 = new Rect(rect.xMax - 220, rect.y + 20, 100, 28);
            bool cycle = Widgets.ButtonText(buttonRect, buttonText);
            bool enableAll = Widgets.ButtonText(buttonRect2, "LTO.EnableAll".Translate());
            bool disableAll = Widgets.ButtonText(buttonRect3, "LTO.DisableAll".Translate());
            if (cycle)
            {
                toShow++;
                toShow %= 3;
            }

            Rect outRect = new Rect(rect.x, rect.y + SEARCH_HEIGHT, rect.width, rect.height - SEARCH_HEIGHT);
            Rect viewRect = new Rect(0, 0, rect.width - 30, lastHeight);

            Widgets.BeginScrollView(outRect, ref scroll, viewRect);
            lastHeight = DrawTextureList(viewRect, enableAll, disableAll);
            Widgets.EndScrollView();
        }

        private void DrawOtherTab(Rect rect)
        {
            Listing_Standard ui = new Listing_Standard();
            ui.Begin(new Rect(rect.x, rect.y, rect.width * 0.4f, rect.height));

            //ui.CheckboxLabeled("Replace World Map Textures", ref DoWorldMapTextures, "When enabled, the world map textures are replaced. Requires a restart to take effect.");
            ui.Label($"{"LTO.FarmlandFertilityLabel".Translate()}: {FarmlandFertilityScale*100:F0}%");
            FarmlandFertilityScale = ui.Slider(FarmlandFertilityScale, 0f, 10f);
            ui.Label($"{"LTO.FarmlandXPLabel".Translate()}: {FarmlandXPScale * 100:F0}%");
            FarmlandXPScale = ui.Slider(FarmlandXPScale, 0f, 10f);

            ui.End();
        }

        private bool ShouldDraw(TextureItem item)
        {
            if (item.RelativePath.StartsWith("LTO"))
                return false;

            switch (toShow)
            {
                case 1:
                    // Only disabled.
                    if (item.IsEnabled)
                        return false;
                    break;
                case 2:
                    // Only enabled.
                    if (!item.IsEnabled)
                        return false;
                    break;
            }

            if (string.IsNullOrWhiteSpace(search))
                return true;

            string s = search.ToLowerInvariant().Trim();

            if (item.NiceName.ToLowerInvariant().Contains(s))
                return true;
            if (s.Contains(item.NiceName))
                return true;

            if (item.NiceFolderFull.ToLowerInvariant().Contains(s))
                return true;
            if (s.Contains(item.NiceFolderFull))
                return true;

            if (item.RelativePath.ToLowerInvariant().Contains(s))
                return true;
            if (s.Contains(item.RelativePath))
                return true;

            return false;
        }

        private float DrawTextureList(Rect rect, bool enableAll, bool disableAll)
        {
            void MoveDown(float amount)
            {
                rect.y += amount;
                rect.height -= amount;
            }

            const float TITLE_HEIGHT = 34;
            const float IMG_HEIGHT = 74;
            const float PAD = 5;

            var modContent = TerrainOverhaulMod.Instance.Content;

            foreach (var item in items)
            {
                if (!ShouldDraw(item))
                    continue;

                var drawPos = new Rect(rect.x, rect.y, rect.width, TITLE_HEIGHT + IMG_HEIGHT);
                var drawPos2 = new Rect(rect.x + rect.width - 130, rect.y + 10, 120, TITLE_HEIGHT);

                Widgets.DrawBoxSolid(drawPos, Color.white * 0.25f);
                Text.Font = GameFont.Medium;
                string label = $"<color=grey><size=14>{item.NiceFolderFull}</size></color> <color=white>{item.NiceName}</color>";
                if (!item.IsEnabled)
                    label += $" <color=red>[{"LTO.Disabled".Translate().ToString().ToUpper()}]</color>";
                if (item.IsChangePending)
                    label += $" <color=yellow>[{"LTO.Restart".Translate().ToString().ToUpper()}]</color>";

                var images = LoadedTextureHelper.GetTexturesInLoadOrder(item.RelativePath, TerrainOverhaulMod.Instance.Content, item.RelativePathDisabled);
                
                drawPos.x += 10;
                Widgets.Label(drawPos, label);
                bool canClick = images.Count > 1 || !item.IsEnabled || item.RelativePath.StartsWith("World/"); // Can only toggle of there are at least 2 textures, or if it's already disabled.
                if (!canClick)
                    GUI.color = new Color(0.7f, 0.6f, 0.6f, 1f);
                bool clicked = Widgets.ButtonText(drawPos2, item.IsEnabled ? "LTO.Disable".Translate() : "LTO.Enable".Translate(), active: canClick);
                GUI.color = Color.white;
                if (!canClick)
                {
                    TooltipHandler.TipRegion(drawPos2, "LTO.NoAltTextureError".Translate());
                }

                bool toggle = clicked || (item.IsEnabled && disableAll) || (!item.IsEnabled && enableAll);
                if (toggle && canClick)
                {
                    item.IsEnabled = !item.IsEnabled;
                    string error = item.ApplyState(disabledTextures, out bool _);
                    if (error != null)
                        Verse.Log.Error($"Error: {error}");
                }
                Text.Font = GameFont.Small;
                MoveDown(TITLE_HEIGHT);

                int activeIndex = -1;
                string activeName = null;
                for (int i = images.Count - 1; i >= 0; i--)
                {
                    var img = images[i];
                    bool isThisMods = img.modName == "[LTO] Terrain Overhaul";
                    if (isThisMods && !item.IsEnabled)
                        continue;
                    activeIndex = i;
                    activeName = img.modName;
                    break;
                }

                for (int i = 0; i < images.Count; i++)
                {
                    var img = images[i];
                    Rect buttonRect = new Rect(10 + rect.x + i * (64 + 16), rect.y, 64, 64);

                    bool isThisMods = img.modName == "[LTO] Terrain Overhaul";
                    bool isUsed = activeIndex == i;

                    Color color;
                    if (isUsed)
                    {
                        color = new Color(0.5f, 1, 0.5f, 1);
                    }
                    else
                    {
                        if (isThisMods)
                        {
                            color = item.IsEnabled ? Color.yellow : Color.grey;
                        }
                        else
                        {
                            color = Color.white;
                        }
                    }
                    GUI.color = color;
                    DrawBox(buttonRect.ExpandedBy(2));
                    GUI.color = Color.white;

                    bool clickImg = Widgets.ButtonImageFitted(buttonRect, img.texture, Color.white);
                    TooltipHandler.TipRegion(buttonRect, img.modName);

                    // Draw warning(s)
                    if (!isUsed && isThisMods && item.IsEnabled)
                    {
                        // Legacy texture is being forcefully overwritten by another mod...
                        Rect iconRect = new Rect(0, 0, 32, 32).CenteredOnXIn(buttonRect).CenteredOnYIn(buttonRect);
                        Widgets.ButtonImage(iconRect, modContent.GetContentHolder<Texture2D>().Get("LTO/IconExclaim"), Color.yellow, false);
                        TooltipHandler.TipRegion(iconRect, "LTO.OverwriteError".Translate(activeName));
                    }
                }

                MoveDown(IMG_HEIGHT + PAD);
            }

            return rect.y;
        }

        public bool IsDisabled(string relativePath)
        {
            return disabledTextures?.Contains(relativePath) ?? false;
        }

        public void CreateTextureList(out bool changed)
        {
            var list = new List<TextureItem>();

            // Edge case:
            // The mod updates, and Steam restores all textures.
            // Now we have the disabled texture

            string rootPath = TerrainOverhaulMod.Instance.Content.RootDir + "/Textures/";

            bool Contains(string relativePath, out TextureItem found)
            {
                foreach (var item in list)
                {
                    if (item.RelativePath == relativePath)
                    {
                        found = item;
                        return true;
                    }
                }
                found = null;
                return false;
            }

            changed = false;
            var content = TerrainOverhaulMod.Instance.Content.GetContentHolder<Texture2D>().contentList;
            foreach (var pair in content)
            {
                string rpRaw = pair.Key.Replace("\\", "/");

                try
                {
                    var rpRawInfo = new FileInfo(rpRaw);
                    bool isInRoot = rpRaw.LastIndexOf('/') == -1;
                    string dirName = isInRoot ? "" : rpRaw.Substring(0, rpRaw.LastIndexOf('/'));

                    bool isDisabled = rpRawInfo.Name.StartsWith(TerrainOverhaulMod.DISABLED_TAG);
                    string fileName = isDisabled
                        ? rpRawInfo.Name.Substring(TerrainOverhaulMod.DISABLED_TAG.Length)
                        : rpRawInfo.Name;

                    string relativePath = dirName + '/' + fileName;
                    string relativePathDis = dirName + '/' + TerrainOverhaulMod.DISABLED_TAG + fileName;
                    string enabledPath = rootPath + dirName + '/' + fileName + ".png";

                    string disabledPath;
                    if (isDisabled)
                    {
                        disabledPath = rootPath + dirName + '/' + rpRawInfo.Name + ".png";
                    }
                    else
                    {
                        disabledPath = rootPath + dirName + '/' + TerrainOverhaulMod.DISABLED_TAG + fileName + ".png";
                    }

                    var item = new TextureItem(!isDisabled);
                    item.EnabledPath = enabledPath;
                    item.DisabledPath = disabledPath;
                    item.RelativePath = relativePath;
                    item.RelativePathDisabled = relativePathDis;
                    item.CreateNiceNames();

                    if (Contains(relativePath, out var existing))
                    {
                        // Duplicate texture! Only add one!
                        bool shouldBeThere = isDisabled == IsDisabled(relativePath);
                        Log($"[WARN] Duplicate texture: {item.RelativePath} ({rpRawInfo.Name}) Should be there: {shouldBeThere}");

                        changed = true;
                        if (!shouldBeThere)
                        {
                            // Delete this item, it shouldn't be there.
                            item.Delete();
                            continue;
                        }
                        else
                        {
                            // If this one should be there, then the other one shouldn't!
                            list.Remove(existing);
                            existing.Delete();
                        }
                    }

                    list.Add(item);
                }
                catch (Exception e)
                {
                    Error($"Failed creating settings for texture '{rpRaw}'. See exception:\n" + e.Message + "\n" + e.StackTrace);
                }
            }

            this.items = list;
        }

        public string EnsureAll(out bool changed)
        {
            List<string> errors = new List<string>();
            changed = false;
            foreach (var item in items)
            {
                try
                {
                    item.IsEnabled = !IsDisabled(item.RelativePath);
                    string error = item.ApplyState(disabledTextures, out bool change);
                    if (error != null)
                        errors.Add($"[{item.RelativePath}] {error}");
                    if (change)
                        changed = true;
                }
                catch (Exception e)
                {
                    errors.Add($"[{item.RelativePath}] Exception '{e.Message}'");
                }
            }

            return errors.Count == 0 ? null : string.Join(",\n", errors);
        }
    }
}

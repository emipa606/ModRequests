using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Reload.Components;
using System;
using Reload.Defs;
using Reload.Data;

namespace Reload
{
    public class Setting : ModSettings
    {
        static bool _enableAmmo = false;

        static bool _useDynamicReloadTime = false;

        static bool _useDynamicReloadTime_Shooting = false;

        static bool _useDynamicReloadTime_Manipulation = false;

        static bool _useDynamicReloadTime_Consciousness = false;

        static bool _useDynamicReloadTime_Quality = false;

        static List<float> _reloadSpeedModifier_Shooting = new List<float>()
        {
            1.5f, // 0
            1.43f,
            1.34f,
            1.25f,
            1.16f,
            1.08f,
            1f, // 6
            0.95f,
            0.9f,
            0.87f,
            0.83f,
            0.79f,
            0.75f,
            0.72f,
            0.7f,
            0.67f,
            0.64f,
            0.61f,
            0.58f,
            0.55f,
            0.5f // 20
        };

        static Dictionary<QualityCategory, float> _reloadSpeedModifier_Quality = new Dictionary<QualityCategory, float>()
        {
            {QualityCategory.Awful,  1.2f},
            {QualityCategory.Poor,  1.1f},
            {QualityCategory.Normal,  1f},
            {QualityCategory.Good,  0.95f},
            {QualityCategory.Excellent,  0.9f},
            {QualityCategory.Masterwork,  0.85f},
            {QualityCategory.Legendary,  0.8f},
        };

        static Dictionary<string, ReloadPresetDef> _preset = new Dictionary<string, ReloadPresetDef>();

        static Dictionary<string, ReloadData> _reloadDataDict = new Dictionary<string, ReloadData>();

        static Dictionary<string, ReloadData> _reloadDataDictLocal = new Dictionary<string, ReloadData>();

        static List<string> _reloadDataKey = new List<string>();

        static List<ReloadData> _reloadDataValue = new List<ReloadData>();

        static List<ThingDef> _reloadableWeapons = new List<ThingDef>();

        static SettingPage _page = SettingPage.Customize;

        static ThingDef _selectedWeapon = null;

        static string _nameFilter = string.Empty;

        static ModContentPack _modFilter = null;

        static Vector2 _scrollPos = Vector2.zero;

        static float _overflow = 0f;

        public static bool EnableAmmo => _enableAmmo;

        public static bool UseDynamicReloadTime => _useDynamicReloadTime;

        public static bool UseDynamicReloadTime_Shooting => _useDynamicReloadTime_Shooting;

        public static bool UseDynamicReloadTime_Manipulation => _useDynamicReloadTime_Manipulation;

        public static bool UseDynamicReloadTime_Consciousness => _useDynamicReloadTime_Consciousness;

        public static bool UseDynamicReloadTime_Quality => _useDynamicReloadTime_Quality;

        public static List<float> ReloadSpeedModifier_Shooting => _reloadSpeedModifier_Shooting;

        public static Dictionary<QualityCategory, float> ReloadSpeedModifier_Quality => _reloadSpeedModifier_Quality;

        public static List<ThingDef> ReloadableWeapons => _reloadableWeapons;

        public static List<AmmoDef> AmmoDefs => DefDatabase<AmmoDef>.AllDefsListForReading;

        public static Dictionary<string, ReloadData> ReloadDataDict => _reloadDataDict;

        static readonly float GAP = 5f;

        static readonly float CELLHEIGHT = Text.LineHeight * 1.5f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref _enableAmmo, "usingAmmoSystem", false);
            Scribe_Values.Look<bool>(ref _useDynamicReloadTime, "useDynamicReloadTime", false);
            Scribe_Values.Look<bool>(ref _useDynamicReloadTime_Shooting, "useDynamicReloadTime_Shooting", false);
            Scribe_Values.Look<bool>(ref _useDynamicReloadTime_Manipulation, "useDynamicReloadTime_Manipulation", false);
            Scribe_Values.Look<bool>(ref _useDynamicReloadTime_Consciousness, "useDynamicReloadTime_Consciousness", false);
            Scribe_Values.Look<bool>(ref _useDynamicReloadTime_Quality, "useDynamicReloadTime_Quality", false);
            Scribe_Collections.Look(ref _reloadDataDict, "reloadDataDict", LookMode.Value, LookMode.Value, ref _reloadDataKey, ref _reloadDataValue);
        }
        public static void Init()
        {
            foreach (var def in DefDatabase<ReloadPresetDef>.AllDefs)
            {
                _preset.Add(def.defName, def);
            }
            _reloadableWeapons = DefDatabase<ThingDef>.AllDefs.Where(
                delegate (ThingDef def)
                {
                    _preset.TryGetValue(def.defName, out ReloadPresetDef presetDef);
                    bool ignore = presetDef?.ignore ?? false;

                    if (!ignore
                    && def.IsRangedWeapon
                    && !def.destroyOnDrop
                    && def.weaponTags != null
                    && !def.weaponTags.Contains("GunSingleUse")
                    && (!def.thingSetMakerTags?.Contains("SingleUseWeapon") ?? true)
                    && def.GetCompProperties<CompProperties_Reloadable>() == null
                    && def.GetCompProperties<CompProperties_Refuelable>() == null)
                        return true;

                    return false;
                }).OrderBy(x => x.techLevel).ToList();

            foreach (var def in _reloadableWeapons)
            {
                ReloadData data = new ReloadData(def);

                if (_reloadDataDict.ContainsKey(def.defName))
                {
                    data.Copy(_reloadDataDict[def.defName]);
                }
                else
                {
                    if (_preset.ContainsKey(def.defName))
                    {
                        data.Copy(_preset[def.defName]);
                        if (!DefDatabase<AmmoDef>.AllDefsListForReading.Contains(_preset[def.defName].ammoDef))
                            data.ammoDef = AmmoUtils.GetProperAmmo(def).defName;
                    }
                    _reloadDataDict.Add(def.defName, data);
                }
                _reloadDataDictLocal.Add(def.defName, data);

                CompProperties_Reload comp = new CompProperties_Reload(data);

                if (_preset.ContainsKey(def.defName))
                {
                    comp.beginSound = _preset[def.defName].beginSound;
                    comp.endSound = _preset[def.defName].endSound;
                    comp.reloadSound = _preset[def.defName].reloadSound;
                }
                else
                {
                    SoundDef beginSound = null;
                    SoundDef reloadSound = SoundDefOf.Standard_Reload;
                    SoundDef endSound = SoundDefOf.Standard_Reload;

                    string defName = def.defName.ToLower();
                    if(defName.Contains("sniper"))
                    {
                        beginSound = SoundDef.Named("SniperRifle_Begin");
                        reloadSound = SoundDef.Named("SniperRifle_End");
                        endSound = reloadSound;
                    }
                    else if (defName.Contains("launcher"))
                    {
                        beginSound = SoundDef.Named("Launcher_Begin");
                        reloadSound = SoundDef.Named("Launcher_Reload");
                        endSound = SoundDef.Named("Launcher_End");
                    }
                    else if(defName.Contains("rifle"))
                    {
                        beginSound = SoundDef.Named("AssaultRifle_Begin");
                        reloadSound = SoundDef.Named("AssaultRifle_End");
                        endSound = reloadSound;
                    }
                    else if(defName.Contains("revolver"))
                    {
                        beginSound = SoundDef.Named("Revolver_Begin");
                        reloadSound = SoundDef.Named("Revolver_ShellInject");
                        endSound = SoundDef.Named("Revolver_End");
                    }
                    else if(defName.Contains("pistol"))
                    {
                        beginSound = SoundDef.Named("AutoPistol_Begin");
                        reloadSound = SoundDef.Named("AutoPistol_End");
                        endSound = reloadSound;
                    }
                    else if(defName.Contains("shotgun"))
                    {
                        beginSound = SoundDef.Named("PumpShotgun_Begin");
                        reloadSound = SoundDef.Named("PumpShotgun_ShellInject");
                        endSound = SoundDef.Named("PumpShotgun_End");
                    }
                    else if(defName.Contains("grenade"))
                    {
                        reloadSound = SoundDef.Named("Grenade_Reload");
                        endSound = beginSound;
                    }
                    else if(defName.Contains("bow"))
                    {
                        reloadSound = SoundDef.Named("Grenade_Reload");
                        endSound = beginSound;
                    }
                    comp.beginSound = beginSound;
                    comp.reloadSound = reloadSound;
                    comp.endSound = endSound;
                }
                def.comps.Add(comp);
            }
        }
        public void DrawWindow(Rect inRect)
        {
            Rect rectPageButton, rectMain;
            GenUI.SplitHorizontallyWithMargin(inRect, out rectPageButton, out rectMain, out _overflow, CELLHEIGHT, new float?(inRect.height / 12f));
            GenUI.SplitHorizontallyWithMargin(rectMain, out rectMain, out Rect rectBottom, out _overflow, CELLHEIGHT, rectMain.height * 0.95f - CELLHEIGHT);
            DrawPageButtonsWindow(rectPageButton);

            if (_page == SettingPage.Customize)
            {
                DrawCustomizeWindow(rectMain, rectBottom);
            }
            if (_page == SettingPage.Options)
            {
                DrawOptionsWindow(rectMain);
            }
        }
        void DrawPageButtonsWindow(Rect rect)
        {
            Rect rect_left = rect.LeftHalf();

            if (Widgets.ButtonText(GenUI.ContractedBy(rect_left.LeftHalf(), 0f, 4f), "CustomizePage".Translate()))
                _page = SettingPage.Customize;
            if (Widgets.ButtonText(GenUI.ContractedBy(rect_left.RightHalf(), 4f, 4f), "OptionsPage".Translate()))
                _page = SettingPage.Options;
        }
        void DrawCustomizeWindow(Rect rectWeaponsList, Rect rectBottom)
        {
            GenUI.SplitHorizontallyWithMargin(rectWeaponsList, out Rect rectTableLabel, out rectWeaponsList, out _overflow, 0, CELLHEIGHT);

            // Filter
            List<ThingDef> result = _reloadableWeapons;

            Widgets.Dropdown(rectBottom.LeftHalf().LeftHalf().LeftHalf(), _modFilter, x => x, new Func<ModContentPack, IEnumerable<Widgets.DropdownMenuElement<ModContentPack>>>(GenerateModContentCategoryMenu), _modFilter == null ? "All" : _modFilter.Name);
            if (_modFilter != null)
                result = result.Where(x => x.modContentPack == _modFilter).ToList();

            Rect rectFilter = rectBottom.LeftHalf().LeftHalf().RightHalf();
            _nameFilter = Widgets.TextField(rectFilter, _nameFilter);
            result = result.Where(x => x.label.Contains(_nameFilter)).ToList();

            {
                // Label
                rectTableLabel.x -= 8f;
                rectTableLabel.height *= 2f;
                rectTableLabel.y -= rectTableLabel.height/3f;
                GenUI.SplitVertically(rectTableLabel, rectTableLabel.width / 4f, out Rect left, out Rect rectCustomizable);
                rectCustomizable = rectCustomizable.ContractedBy(rectCustomizable.height * 0.1f);

                Rect rectAmmoType, rectMagazineCap, rectLoadsAmountPerReload, rectReloadTicks, rectMovementPenalty;

                float x = rectCustomizable.x;
                float y = rectCustomizable.y;
                float width = rectCustomizable.LeftHalf().LeftHalf().LeftHalf().width;
                float height = rectCustomizable.height;

                rectAmmoType = new Rect(x, y, width, height);
                x += width + GAP;
                rectMagazineCap = new Rect(x, y, width, height);
                x += width + GAP;
                rectLoadsAmountPerReload = new Rect(x,y, width, height);
                x += width + GAP;
                rectReloadTicks = new Rect(x,y, width, height);
                x += width * 1.5f + GAP;
                rectMovementPenalty = new Rect(x,y, width, height);

                Text.Anchor = TextAnchor.MiddleCenter;
                if(EnableAmmo)
                    Widgets.Label(rectAmmoType, "AmmoType".Translate());
                Widgets.Label(rectMagazineCap, "MagazineCapacity".Translate());
                Widgets.Label(rectLoadsAmountPerReload, "LoadsAmountPerReload".Translate());
                Widgets.Label(rectReloadTicks, "ReloadTicks".Translate());
                Widgets.Label(rectMovementPenalty, "MoveSpeedPenalty".Translate());
                Text.Anchor = TextAnchor.MiddleLeft;
            }

            // Data
            Rect rectScrollView = new Rect(0f, 0f, rectWeaponsList.width - 16f, result.Count * CELLHEIGHT);
            Widgets.BeginScrollView(rectWeaponsList, ref _scrollPos, rectScrollView, true);
            Listing_Standard listingStandard = new Listing_Standard(rectWeaponsList, () => _scrollPos);
            listingStandard.Begin(rectScrollView);
            for (int i = 0; i < result.Count; i++)
            {
                ThingDef item = result[i];
                ReloadData reloadData = _reloadDataDictLocal[item.defName];
                ThingDef selectedCategory = ThingDef.Named(reloadData.ammoDef); Rect rectItem = listingStandard.GetRect(CELLHEIGHT, 1f);

                Rect rectIcon, rectAmmoType, rectMagazineCap, rectLoadsAmountPerReload, rectReloadTicks, rectReloadTicksDesc, rectMovementPenalty;

                GenUI.SplitVertically(rectItem, rectItem.width / 4f, out Rect rectIconAndName, out Rect rectCustomizable);
                GenUI.SplitVertically(rectIconAndName, CELLHEIGHT, out rectIcon, out Rect rectName);
                rectCustomizable = rectCustomizable.ContractedBy(rectCustomizable.height * 0.1f);
                float x = rectCustomizable.x;
                float y = rectCustomizable.y;
                float width = rectCustomizable.LeftHalf().LeftHalf().LeftHalf().width;
                float height = rectCustomizable.height;

                rectAmmoType = new Rect(x, y, width, height);
                x += width + GAP;
                rectMagazineCap = new Rect(x, y, width, height);
                x += width + GAP;
                rectLoadsAmountPerReload = new Rect(x, y, width, height);
                x += width + GAP;
                rectReloadTicks = new Rect(x, y, width, height);
                x += width;
                rectReloadTicksDesc = new Rect(x, y - 0.1f, width, height * 0.8f);
                x += width/2f + GAP;
                rectMovementPenalty = new Rect(x, y, width, height);

                Widgets.DrawTextureFitted(rectIcon, Widgets.GetIconFor(item), 0.85f); // Icon
                Widgets.Label(rectName, item.LabelCap); // Name
                if(EnableAmmo)
                    Widgets.Dropdown(rectAmmoType, item, t => t, new Func<ThingDef, IEnumerable<Widgets.DropdownMenuElement<ThingDef>>>(GenerateAmmoCategoryMenu), null, Widgets.GetIconFor(selectedCategory));
                CustomWidgets.TextFieldInt(rectMagazineCap, ref reloadData.magazineCapacity, 1, 9999);
                CustomWidgets.TextFieldInt(rectReloadTicks, ref reloadData.reloadTicks, 0, 9999);
                CustomWidgets.TextFieldInt(rectLoadsAmountPerReload, ref reloadData.loadsAmountPerReload, 0, 9999);
                Widgets.Label(rectReloadTicksDesc, $"{reloadData.reloadTicks / 60f:0.00}s");
                CustomWidgets.TextFieldInt(rectMovementPenalty, ref reloadData.moveSpeedPenalty, 0, 100);

                Widgets.DrawHighlightIfMouseover(rectItem);
                if (Mouse.IsOver(rectIcon))
                    _selectedWeapon = item;

                _reloadDataDictLocal[item.defName].magazineCapacity = reloadData.magazineCapacity;
                _reloadDataDictLocal[item.defName].loadsAmountPerReload = reloadData.loadsAmountPerReload;
                _reloadDataDictLocal[item.defName].reloadTicks = reloadData.reloadTicks;
                _reloadDataDictLocal[item.defName].moveSpeedPenalty = reloadData.moveSpeedPenalty;
            }

            listingStandard.End();
            Widgets.EndScrollView();

            // Bottom Buttons
            if (Widgets.ButtonText(rectBottom.RightHalf().LeftHalf().RightHalf(), "LoadPreset".Translate(), true, true, true, null))
            {
                foreach(var item in _reloadDataDictLocal)
                {
                    if(_preset.ContainsKey(item.Key))
                    {
                        if (!_preset[item.Key].ignore)
                            item.Value.Copy(_preset[item.Key]);
                    }
                }
            }
            if (Widgets.ButtonText(rectBottom.RightHalf().RightHalf().RightHalf(), "Save".Translate(), true, true, true, null))
            {
                foreach (var item in _reloadDataDictLocal)
                {
                    _reloadDataDict[item.Key].Copy(item.Value);
                }
                _reloadableWeapons.OrderBy(x => x.techLevel).ToList();
                foreach (var item in _reloadableWeapons)
                {
                    item.GetCompProperties<CompProperties_Reload>()?.CopyFrom(_reloadDataDict[item.defName]);
                }
                Write();
            }
            if (_selectedWeapon != null)
            {
                DrawWeaponInfoWindow(rectWeaponsList, rectBottom);
            }

            _selectedWeapon = null;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        void DrawOptionsWindow(Rect rect)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            Rect rect2 = listingStandard.GetRect(CELLHEIGHT).LeftHalf();
            Widgets.DrawHighlightIfMouseover(rect2);
            Widgets.CheckboxLabeled(rect2, "EnableAmmo".Translate(), ref _enableAmmo);
            listingStandard.Gap(2f);

            Rect rect3 = listingStandard.GetRect(CELLHEIGHT).LeftHalf();
            Widgets.DrawHighlightIfMouseover(rect3);
            Widgets.CheckboxLabeled(rect3, "EnableDynamicReloadTime".Translate(), ref _useDynamicReloadTime);
            listingStandard.Gap(2f);
            if (_useDynamicReloadTime)
            {
                Rect rect4 = listingStandard.GetRect(CELLHEIGHT).LeftHalf();
                Widgets.DrawHighlightIfMouseover(rect4);
                Widgets.CheckboxLabeled(rect4, $" - {SkillDefOf.Shooting.LabelCap}", ref _useDynamicReloadTime_Shooting);
                listingStandard.Gap(2f);

                Rect rect5 = listingStandard.GetRect(CELLHEIGHT).LeftHalf();
                Widgets.DrawHighlightIfMouseover(rect5);
                Widgets.CheckboxLabeled(rect5, $" - {PawnCapacityDefOf.Manipulation.LabelCap}", ref _useDynamicReloadTime_Manipulation);
                listingStandard.Gap(2f);

                Rect rect6 = listingStandard.GetRect(CELLHEIGHT).LeftHalf();
                Widgets.DrawHighlightIfMouseover(rect6);
                Widgets.CheckboxLabeled(rect6, $" - {PawnCapacityDefOf.Consciousness.LabelCap}", ref _useDynamicReloadTime_Consciousness);
                listingStandard.Gap(2f);

                Rect rect7 = listingStandard.GetRect(CELLHEIGHT).LeftHalf();
                Widgets.DrawHighlightIfMouseover(rect7);
                Widgets.CheckboxLabeled(rect7, $" - {"Quality".Translate()}", ref _useDynamicReloadTime_Quality);
                listingStandard.Gap(2f);
            }
            listingStandard.End();
            Text.Anchor = TextAnchor.UpperLeft;
        }
        void DrawWeaponInfoWindow(Rect rectWeaponsList, Rect rectBottom)
        {
            var mousePos = Event.current.mousePosition;
            Rect rectWeaponInfo = new Rect(mousePos.x + 10f, mousePos.y, 250f, 250f);
            if (rectWeaponInfo.Overlaps(rectBottom))
            {
                rectWeaponInfo.y -= rectWeaponInfo.height;
            }
            Widgets.DrawBoxSolidWithOutline(rectWeaponInfo, new Color(21f / 255f, 25f / 255f, 29f / 255f, 1f), Color.black);
            rectWeaponInfo.x += 5f;
            rectWeaponInfo.width -= 5f;

            try
            {
                var item = _selectedWeapon;

                var projectile = item.Verbs[0]?.defaultProjectile?.projectile;
                int damage = projectile?.GetDamageAmount(1f) ?? -1;
                float stoppingPower = projectile?.stoppingPower ?? -1f;
                float armorPenetration = Math.Max(0f, (projectile?.GetArmorPenetration(1f) ?? -1f) * 100f);
                int burstShotCount = item.Verbs[0].burstShotCount;
                int fireRate = burstShotCount == 1 ? 0 : Mathf.Max(0, (int)(60f * (60f / item.Verbs[0].ticksBetweenBurstShots)));
                float range = item.Verbs[0].range;
                float accClose = item.statBases.GetStatValueFromList(StatDefOf.AccuracyTouch, 0f) * 100f;
                float accShort = item.statBases.GetStatValueFromList(StatDefOf.AccuracyShort, 0f) * 100f;
                float accMedium = item.statBases.GetStatValueFromList(StatDefOf.AccuracyMedium, 0f) * 100f;
                float accLong = item.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0f) * 100f;
                float coolDown = item.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0f);
                float warmup = item.Verbs[0].warmupTime;

                string keys = $"{"Damage".Translate()}\n" +
                    $"{"StoppingPower".Translate()}\n" +
                    $"{"ArmorPenetration".Translate()}\n" +
                    $"{"BurstShotFireRate".Translate()}\n" +
                    $"{"BurstShotCount".Translate()}\n" +
                    $"{"Range".Translate()}\n" +
                    $"{StatDefOf.AccuracyTouch.label}\n" +
                    $"{StatDefOf.AccuracyShort.label}\n" +
                    $"{StatDefOf.AccuracyMedium.label}\n" +
                    $"{StatDefOf.AccuracyLong.label}\n" +
                    $"{StatDefOf.RangedWeapon_Cooldown.label}\n" +
                    $"{"RangedWarmupTime".Translate()}";

                string values = $"{damage}\n" +
                    $"{stoppingPower}\n" +
                    $"{armorPenetration}%\n" +
                    $"{fireRate} rpm\n" +
                    $"{burstShotCount}\n" +
                    $"{range}\n" +
                    $"{accClose}%\n" +
                    $"{accShort}%\n" +
                    $"{accMedium}%\n" +
                    $"{accLong}%\n" +
                    $"{coolDown} s\n" +
                    $"{warmup} s";

                GenUI.SplitHorizontally(rectWeaponInfo, Text.LineHeight, out Rect top, out Rect bottom);
                GenUI.SplitVertically(bottom, rectWeaponInfo.width * 0.7f, out Rect key, out Rect value);
                GenUI.SplitHorizontally(bottom, bottom.height - Text.LineHeight * 2, out bottom, out Rect rectModName);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(top, item.label);
                Widgets.Label(rectModName, $"({item?.modContentPack?.Name ?? "?"})");

                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(key, keys);
                Widgets.Label(value, values);
            }
            catch
            {
                _selectedWeapon = null;
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }
        static IEnumerable<Widgets.DropdownMenuElement<ThingDef>> GenerateAmmoCategoryMenu(ThingDef def)
        {
            using (List<ThingDef>.Enumerator enumerator = AmmoDefs.ToList<ThingDef>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ThingDef ammo = enumerator.Current;
                    var dropdownMenuElement = default(Widgets.DropdownMenuElement<ThingDef>);
                    TaggedString? taggedString = (ammo != null) ? new TaggedString?(ammo.LabelCap) : null;
                    dropdownMenuElement.option = new FloatMenuOption((taggedString != null) ? taggedString.GetValueOrDefault() : null, delegate () { _reloadDataDictLocal[def.defName].ammoDef = ammo.defName; }, Widgets.GetIconFor(ammo), Color.white);
                    dropdownMenuElement.payload = ammo;
                    yield return dropdownMenuElement;
                }
            }
            yield break;
        }
        static IEnumerable<Widgets.DropdownMenuElement<ModContentPack>> GenerateModContentCategoryMenu(ModContentPack def)
        {
            HashSet<ModContentPack> mods = new HashSet<ModContentPack>();
            foreach (var item in _reloadableWeapons)
            {
                if (item.modContentPack == null)
                    continue;

                mods.Add(item.modContentPack);
            }

            var nonSelected = default(Widgets.DropdownMenuElement<ModContentPack>);
            nonSelected.option = new FloatMenuOption("All", delegate () { _modFilter = null; });
            yield return nonSelected;

            using (var enumerator = mods.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var mod = enumerator.Current;
                    var dropdownMenuElement = default(Widgets.DropdownMenuElement<ModContentPack>);
                    dropdownMenuElement.option = new FloatMenuOption(mod.Name, delegate () { _modFilter = mod; });
                    dropdownMenuElement.payload = mod;
                    yield return dropdownMenuElement;
                }
            }

            yield break;
        }
    }
}   
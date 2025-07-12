using System;
using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Dormitories
{
    [StaticConstructorOnStartup]
    static class Dormitories
    {
        static DormitoriesSettings settings;
        static Dormitories()
        {
            settings = LoadedModManager.GetMod<DormitoriesMod>().GetSettings<DormitoriesSettings>();
            Harmony harmony = new Harmony("rimworld.lucifer.dormitories");
            harmony.PatchAll();

            int hospitalityPatchSuccess = 0;

            try
            {
                ((Action)(() =>
                {
                    if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Hospitality"))
                    {
                        harmony.Patch(AccessTools.Method(typeof(Hospitality.Patches.Toils_LayDown_Patch.ApplyBedThoughts), nameof(Hospitality.Patches.Toils_LayDown_Patch.ApplyBedThoughts.Replacement)),
                            postfix: new HarmonyMethod(typeof(DormitoryHarmonyModPatches), nameof(DormitoryHarmonyModPatches.Patch_Replacement_Hospitality_PostFix)));
                        hospitalityPatchSuccess++;
                    }
                }))();
            }
            catch (TypeLoadException ex) { }

            if (hospitalityPatchSuccess == 1)
            {
                Log.Message("[Dormitories] Dormitories successfully integrated with Hospitality!");
            }
        }
    }

    public class DormitoriesSettings : ModSettings
    {
        public int maximumBeds = 3;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.maximumBeds, "maximumBeds", 3);
            base.ExposeData();
        }
    }

    public class DormitoriesMod : Mod
    {
        DormitoriesSettings settings;

        public DormitoriesMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<DormitoriesSettings>();
        }

        public override string SettingsCategory() => "DormitoriesSettingsCategoryLabel".Translate();

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Rect rect = canvas;
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(canvas);
            ls.Label("SettingsLabelTip".Translate());
            ls.GapLine();
            ls.ColumnWidth = (canvas.width - 40f) * 0.34f;
            Text.Anchor = (TextAnchor)4;
            ls.Label("SettingsMaximumBedsLabel".Translate());
            ls.NewColumn();
            ls.ColumnWidth = (canvas.width - 40f) * 0.66f;
            ls.Gap(34f);
            ls.IntAdjuster(ref settings.maximumBeds, 1, 2);
            bool buttonpressed = ls.ButtonTextSound("SettingsResetButton".Translate(), null, 0.4f);
            ls.End();
            base.DoSettingsWindowContents(canvas);

            if (buttonpressed)
            {
                settings.maximumBeds = 3;
                buttonpressed = false;
            }
        }
    }

    public static class Extensions
    {
        public static bool ButtonTextSound(this Listing_Standard ls, string label, string highlightTag = null, float widthPct = 1f)
        {
            Rect rect = ls.GetRect(30f, widthPct);
            bool result = false;
            if (!ls.BoundingRectCached.HasValue || rect.Overlaps(ls.BoundingRectCached.Value))
            {
                result = Widgets.ButtonText(rect, label);
                if (highlightTag != null)
                {
                    UIHighlighter.HighlightOpportunity(rect, highlightTag);
                }
            }
            ls.Gap(ls.verticalSpacing);
            if (result) SoundDefOf.Click.PlayOneShotOnCamera();
            return result;
        }
    }


    /// <summary>
    /// Gives pawns unique thoughts for sleeping in dormitories
    /// </summary>
    [HarmonyPatch(typeof(Toils_LayDown))]
    [HarmonyPatch("ApplyBedThoughts")]
    static class ApplyBedThoughts_Patch
    {
        static void Prefix(Pawn actor)
        {
            if (actor.needs.mood == null)
            {
                return;
            }

            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DormitoriesDefOf.SleptInDormitory);
        }

        static void Postfix(Pawn actor)
        {
            if (actor.needs.mood != null)
            {
                Building_Bed building_Bed = actor.CurrentBed();

                if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    if (building_Bed.GetRoom().Role == DormitoriesDefOf.Dormitory && DormitoriesDefOf.SleptInDormitory != null)
                    {
                        ThoughtDef thoughtDef = DormitoriesDefOf.SleptInDormitory;
                        int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                        if (thoughtDef.stages[scoreStageIndex] != null)
                        {
                            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
                        }
                    }
                } 
            }
        }
    }

    /// <summary>
    /// This will either overwrite or be overwritten by Hospitality. Doesn't make much difference.
    /// </summary>
    [HarmonyPatch(typeof(Room))]
    [HarmonyPatch("Owners")]
    public static class Owners_Patch
    {
        public static bool Owners_Prefix(Room __instance, ref IEnumerable<Pawn> __result)
        {
            if (__instance.TouchesMapEdge || __instance.IsHuge || (__instance.Role != RoomRoleDefOf.Bedroom && __instance.Role != RoomRoleDefOf.PrisonCell && __instance.Role != RoomRoleDefOf.Barracks && __instance.Role != RoomRoleDefOf.PrisonBarracks && __instance.Role != DormitoriesDefOf.Dormitory))
            {
                __result = GimmieDatEnumerable();
                return false;
            }
            return true;
        }

        static IEnumerable<Pawn> GimmieDatEnumerable()
        {
            yield break;
        }
    }
}
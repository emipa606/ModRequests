using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VetFoodRestriction
{
    [HarmonyPatch(typeof(MainTabWindow_Animals), "DoWindowContents")]
    internal class MainTabWindow_AnimalsPatch
    {
        [HarmonyPostfix]
        public static void AddFoodRestrictionLink(Rect rect)
        {
            if (Widgets.ButtonText(new Rect(rect.x + 260f, rect.y, Mathf.Min(rect.width, 260f), 32f), VFRSaveRestriction.Instance.restrictionName))
            {
                Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(VFRSaveRestriction.Instance.foodRestiction));
            }
        }
    }
}

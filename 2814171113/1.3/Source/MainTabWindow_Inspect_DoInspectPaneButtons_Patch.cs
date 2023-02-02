using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RenameGun
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "DoInspectPaneButtons")]
    public static class MainTabWindow_Inspect_DoInspectPaneButtons_Patch
    {
        public static void Postfix(Rect rect, ref float lineEndWidth)
        {
            Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
            if (singleSelectedThing != null && singleSelectedThing.def.IsWeapon)
            {
                var renameRect = new Rect(rect.width - 48f - 30, 0f, 30f, 30f);
                if (Widgets.ButtonImage(renameRect, TexButton.Rename))
                {
                    Find.WindowStack.Add(new Dialog_RenameGun(singleSelectedThing));
                }
            }
        }
    }
}

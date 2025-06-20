using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Therapy.Patches
{
    internal static class NeedsCardUtility_Patch
    {
        // To show an icon when a thought is being healed
        [HarmonyPatch(typeof(NeedsCardUtility), nameof(NeedsCardUtility.DrawThoughtGroup))]
        public class DrawThoughtGroup
        {
            [HarmonyPrefix]
            public static void DrawTherapyIcon(Rect rect, Thought group, Pawn pawn)
            {
                if (pawn.CurJob == null) return;
                if (pawn.CurJob.def != MainUtility.patientJobDef) return;

                if (!(pawn.jobs.curDriver is JobDriver_ReceiveTherapy driver)) return;

                if (driver.CurrentTreatedMemory == null || driver.CurrentTreatedMemory.def != group.def) return;
                TooltipHandler.TipRegion(rect, new TipSignal( "CounseledTooltip".Translate((driver.CurrentHealAmount / 10).ToStringWithSign()), 72916));

                GUI.DrawTexture(new Rect(rect.x + 235 + 32, rect.y + 1, 8, 16), Mod_Therapy.therapyIcon);
            }
        }

        // TODO: Do nicely one day
        // To show how urgent therapy is
        //[HarmonyPatch(typeof(NeedsCardUtility), "DoNeedsMoodAndThoughts")]
        //public class DoNeedsMoodAndThoughts
        //{
        //    [HarmonyPrefix]
        //    public static void DrawTherapyNeed(Rect rect, Pawn pawn)
        //    {
        //        float need = pawn.GetTherapyNeed();
        //        Rect barRect = new Rect(rect.x + 50, rect.y + 500, 250, 20);
        //        Widgets.Label(barRect, "Therapy required: " + need);
        //    }
        //}
    }
}
using System;
using Verse;
using Harmony;
using System.Reflection;
using RimWorld;
using Verse.AI;

namespace PredatorHuntAlert {
    [StaticConstructorOnStartup]
    class Main {
        static Main() {
            var harmony = HarmonyInstance.Create("com.tammybee.predatorhuntalert");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(JobGiver_GetFood))]
    [HarmonyPatch("TryGiveJob")]
    class JobGiver_GetFood_TryGiveJob_Patch {

        [HarmonyPostfix]
        static void AlertTargetedByPredator(JobGiver_GetFood __instance, Job __result, Pawn pawn) {
            Pawn victim = Util.GetVictim(__result);

            if (victim != null) {
                if (PredatorHuntAlertMod.Settings.alertType == PredatorHuntAlertSettings.AlertType.Letter) {
                    SendLetter(pawn, victim);
                } else if (PredatorHuntAlertMod.Settings.alertType == PredatorHuntAlertSettings.AlertType.Message) {
                    SendMessage(pawn, victim);
                }
            }
        }

        //メッセージ(左上の通知)を表示する
        private static void SendMessage(Pawn predator, Pawn victim) {
            Pawn target = Util.ResolveFocusTarget(predator, victim);
            String text = "PredatorHuntAlert.MessageTargetedByPredator".Translate(victim.LabelShort,predator.LabelShort);
            Messages.Message(text, target, MessageTypeDefOf.NegativeEvent);
        }

        //レター(画面右下の四角い通知)を送信する
        private static void SendLetter(Pawn predator, Pawn victim) {
            Pawn target = Util.ResolveFocusTarget(predator, victim);
            string label = "PredatorHuntAlert.LetterLabelTargetedByPredator".Translate();
            string text = "PredatorHuntAlert.LetterTextTargetedByPredator".Translate(victim.LabelShort,predator.LabelShort);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatSmall, target, null);
        }
    }
    
    //競合するので削除（捕食対象が変わった際、何故か呼び出されない）
    [HarmonyPatch(typeof(JobDriver_PredatorHunt))]
    [HarmonyPatch("CheckWarnPlayer")]
    class JobDriver_PredatorHunt_CheckWarnPlayer_Patch {

        [HarmonyPrefix]
        static bool Replace(JobDriver_PredatorHunt __instance) {
            return false;
        }
    }

    [HarmonyPatch(typeof(Alert))]
    [HarmonyPatch("DrawAt")]
    class Patch_Alert_DrawAt {
        public static bool inDrawAt = false;

        static void Prefix() {
            inDrawAt = true;
        }

        static void Postfix() {
            inDrawAt = false;
        }
    }

    [HarmonyPatch(typeof(CameraJumper))]
    [HarmonyPatch("TryJumpAndSelect")]
    class Patch_CameraJumper_TryJumpAndSelect {
        public static int count = 0;

        static void Prefix() {
            if (Patch_Alert_DrawAt.inDrawAt) {
                count++;
            }
        }
    }
}

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace NoobertNebulous
{
    [HarmonyPatch(typeof(CharacterCardUtility), "DoTopStack")]
    public static class CharacterCardUtility_DoTopStack_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var appendInspectStringsFromQuestPartsInfo = typeof(QuestUtility).GetMethods(AccessTools.all)
                .FirstOrDefault(x => x.Name == "AppendInspectStringsFromQuestParts" && x.GetParameters().Count() == 3 
                && x.GetParameters().First().ParameterType == typeof(Action<string, Quest>));
            foreach (var code in codes)
            {
                yield return code;
                if (code.Calls(appendInspectStringsFromQuestPartsInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CharacterCardUtility_DoTopStack_Patch), "AddVidtuberInfo"));
                }
            }
        }
        public static void AddVidtuberInfo(Pawn pawn)
        {
            if (pawn.IsVidtuber())
            {
                var channel = pawn.GetVidtubeChannel();
                var subscriberCount = channel.subscribers.ToString();
                var textSize = Text.CalcSize(subscriberCount).x;
                CharacterCardUtility.tmpStackElements.Add(new GenUI.AnonymousStackElement
                {
                    drawer = delegate (Rect r)
                    {
                        Color color2 = GUI.color;
                        GUI.color = UIHelper.StackElementBackground;
                        GUI.DrawTexture(r, BaseContent.WhiteTex);
                        GUI.color = color2;

                        var iconRect = new Rect(r.x, r.y, 22, 22);
                        GUI.DrawTexture(iconRect, Window_Vidtube.SubscriberIcon);
                        var labelRect = new Rect(iconRect.xMax + 5, r.y, textSize, r.height);
                        Widgets.Label(labelRect, subscriberCount);
                        TooltipHandler.TipRegion(r, "NN.BiotabSummary".Translate());
                    },
                    width = 22 + 5 + textSize + 5
                });
            }
        }

    }
}

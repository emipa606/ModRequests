using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(FactionDialogMaker), "FactionDialogFor")]
    public static class FactionDialogMaker_FactionDialogFor_Patch
    {
        public static void Postfix(Pawn negotiator, Faction faction, ref DiaNode __result)
        {
            if (faction == Faction.OfEmpire)
            {
                var node = new DiaOption("IF.GetHonorQuestForSilver".Translate());
                if (!TradeUtility.ColonyHasEnoughSilver(Find.CurrentMap, 5000))
                {
                    node.Disable("IF.GetHonorQuestForSilverNotEnoughMoney".Translate());
                };
                node.action = delegate
                {
                    var count = 0;
                    while (count < 50)
                    {
                        count++;
                        try
                        {
                            var slate = new Slate();
                            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(Find.World));
                            var questDef = Core.imperialQuestsWithRoyalFavors.Where(x => x.CanRun(slate)).RandomElement();
                            Quest quest = QuestGen.Generate(questDef, slate);
                            var rewardChoice = quest.PartsListForReading.OfType<QuestPart_Choice>().FirstOrDefault();
                            if (rewardChoice != null && rewardChoice.choices.Any(x => x.rewards.Any(x => x is Reward_RoyalFavor favor && favor.faction == Faction.OfEmpire)))
                            {
                                Find.QuestManager.Add(quest);
                                if (!quest.hidden && quest.root.sendAvailableLetter)
                                {
                                    QuestUtility.SendLetterQuestAvailable(quest);
                                }
                                TradeUtility.LaunchSilver(Find.CurrentMap, 5000);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                };
                node.resolveTree = true;
                var disconnectOption = __result.options.FirstIndexOf(x => x.text == "(" + "Disconnect".Translate() + ")");
                if (disconnectOption >= 0)
                {
                    __result.options.Insert(disconnectOption, node);
                }
                else
                {
                    __result.options.Add(node);
                }
            }
        }
    }
}

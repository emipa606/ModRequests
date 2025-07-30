using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ImperialFunctionality
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        public static List<QuestScriptDef> imperialQuestsWithRoyalFavors = new List<QuestScriptDef>();
        static Core()
        {
            foreach (var royalTitle in FactionDefOf.Empire.RoyalTitlesAllInSeniorityOrderForReading)
            {
                if (royalTitle.seniority >= IF_DefOf.VFEE_Despot.seniority)
                {
                    royalTitle.grantedAbilities ??= new List<AbilityDef>();
                    royalTitle.grantedAbilities.Add(IF_DefOf.IF_FormEmpireAlliance);
                }
                if (royalTitle.seniority >= IF_DefOf.VFEE_Archduke.seniority)
                {
                    royalTitle.grantedAbilities ??= new List<AbilityDef>();
                    royalTitle.grantedAbilities.Add(IF_DefOf.IF_RoyalInvitation);
                }
            }
            var slate = new Slate();
            foreach (var quest in DefDatabase<QuestScriptDef>.AllDefs)
            {
                if (quest.canGiveRoyalFavor && HasQuestRoyalFavorReward(quest.root, slate))
                {
                    imperialQuestsWithRoyalFavors.Add(quest);
                }
            }
        }

        public static bool HasQuestRoyalFavorReward(QuestNode node, Slate slate)
        {
            if (node is QuestNode_GiveRewards giveRewards && giveRewards.parms.GetValue(slate).allowRoyalFavor)
            {
                return true;
            }
            var nodesField = AccessTools.Field(node.GetType(), "nodes");
            if (nodesField != null)
            {
                var listOfNodes = nodesField.GetValue(node);
                if (listOfNodes is List<QuestNode> questNodes)
                {
                    foreach (var curNode in questNodes)
                    {
                        if (HasQuestRoyalFavorReward(curNode, slate))
                        {
                            return true;
                        }
                    }
                }
            }
            var nodeField = AccessTools.Field(node.GetType(), "node");
            if (nodeField != null)
            {
                var questNode = nodeField.GetValue(node);
                if (questNode is QuestNode node2)
                {
                    if (HasQuestRoyalFavorReward(node2, slate))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

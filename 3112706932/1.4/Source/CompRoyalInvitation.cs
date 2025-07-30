using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ImperialFunctionality
{
    public class CompProperties_RoyalInvitation : CompProperties_AbilityEffect
    {
        public CompProperties_RoyalInvitation()
        {
            this.compClass = typeof(CompRoyalInvitation);
        }
    }
    public class CompRoyalInvitation : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var quests= new List<QuestScriptDef> { IF_DefOf.VFEE_ArtExhibit, IF_DefOf.VFEE_GrandBall };
            var questDef = quests.RandomElement();
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, Mathf.Max(questDef.rootMinPoints, StorytellerUtility.DefaultThreatPointsNow(Find.World)));
            QuestUtility.SendLetterQuestAvailable(quest);
        }
    }
}

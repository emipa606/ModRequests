using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;


namespace DIL_PositiveConnections
{
    public class InteractionWorker_SkillShare : InteractionWorker
    {
        private const float BaseSelectionWeight = 0.0005f;
        private const float NonColonyPawnFactor = 0.05f;
        private const int SkillDifferenceDivisor = 10;
        private PositiveConnectionsModSettings modSettings = Mod_PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {

            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            // Get the highest skill of the initiator using Verse's MaxBy
            SkillRecord highestSkill = Verse.GenCollection.MaxBy(initiator.skills.skills, s => s.Level);

            // If the recipient's skill level in the same area is less, allow teaching
            if (highestSkill != null && recipient.skills.GetSkill(highestSkill.def).Level < highestSkill.Level)
            {
                // Weight is based on the difference in skill levels
                float weight = (highestSkill.Level - recipient.skills.GetSkill(highestSkill.def).Level) / SkillDifferenceDivisor * initiator.needs.mood.CurLevel * BaseSelectionWeight;

                // If either the initiator or the recipient is not a colonist, reduce the weight
                if (initiator.Faction != Faction.OfPlayer || recipient.Faction != Faction.OfPlayer)
                {
                    weight *= NonColonyPawnFactor;
                }

                return weight * modSettings.BaseInteractionFrequency;
            }
            return 0f;
        }


        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Get the highest skill of the initiator using Verse's MaxBy
            SkillRecord highestSkill = Verse.GenCollection.MaxBy(initiator.skills.skills, s => s.Level);

            if (highestSkill != null)
            {
                // Increase the recipient's corresponding skill level
                recipient.skills.GetSkill(highestSkill.def).Learn(200f);

                // Generate the teaching message
                string teachingMessage = $"{initiator.Name} taught {recipient.Name} about {highestSkill.def.label}.";

                if(!modSettings.DisableAllMessages)
                {
                    // Feedback
                    Messages.Message(teachingMessage, recipient, MessageTypeDefOf.PositiveEvent);

                }
                

                // Increase the recipient's mood
                recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPositiveConnections.DIL_ReceivedTeaching,initiator);
                
            }

            // Clear out required 'out' parameters
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
        }


    }

}


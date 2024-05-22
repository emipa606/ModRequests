using System;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_SharedPassion : InteractionWorker
    {

        private PositiveConnectionsModSettings modSettings = Mod_PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();
        private const float BaseSelectionWeight = 0.0025f;
        private const int PassionLevelFactor = 2;  // Increase selection weight based on passion level

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {

            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            // Both pawns should have a shared skill with burning passion
            var sharedPassion = initiator.skills.skills
                .FirstOrDefault(s => recipient.skills.skills.Any(rs => rs.def == s.def && (int)rs.passion > 0 && (int)s.passion > 0));

            if (sharedPassion == null)
            {
                return 0f;
            }

            float baseWeight = (initiator.needs.mood.CurLevel + recipient.needs.mood.CurLevel) / 2 * BaseSelectionWeight;

            if (initiator.Faction == recipient.Faction)
            {
                // Increase the weight based on passion level
                return baseWeight * ((int)sharedPassion.passion * PassionLevelFactor) * modSettings.BaseInteractionFrequency;
            }
            else
            {
                return baseWeight * 0.05f * modSettings.BaseInteractionFrequency; // 1/20 as likely
            }
        }


        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Find the shared passion
            var sharedPassion = initiator.skills.skills
                .FirstOrDefault(s => recipient.skills.skills.Any(rs => rs.def == s.def && (int)rs.passion > 0 && (int)s.passion > 0));

            // Generate message and gain memory of the shared activity
            if (sharedPassion != null)
            {
                //string passionMessage = $"{initiator.Name.ToStringShort} and {recipient.Name.ToStringShort} enjoyed {sharedPassion.def.skillLabel} together";
                string passionMessage = PositiveConnectionsUtility.GenerateSharedPassionMessage(initiator, recipient, sharedPassion);

                if(!modSettings.DisableAllMessages) {

                    Messages.Message(passionMessage, recipient, MessageTypeDefOf.PositiveEvent);
                }
               


                Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SharedPassionActivity"));
                memory.moodPowerFactor = 1f;
                initiator.needs.mood.thoughts.memories.TryGainMemory(memory,recipient);
                recipient.needs.mood.thoughts.memories.TryGainMemory(memory,initiator);
            }

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Update relationship between initiator and recipient
            Faction factionA = initiator.Faction;
            Faction factionB = recipient.Faction;

            if (factionA != null && factionB != null && factionA != factionB)
            {
                PositiveConnectionsUtility.ChangeFactionRelations(factionA, factionB, 10);
            }
        }
    }

}

